using AOT;
using FMOD;
using FMOD.Studio;
using FMODUnity;
using Mirror;
using Steamworks;
using System;
using System.Collections.Generic;
using System.Runtime.InteropServices;
using UnityEngine;

public class MirrorSteamworksVoice : NetworkBehaviour
{
    [SerializeField] private EventReference _voiceEventReference;

    private float _voiceVolume = 1f;
    private float _volumeSmoothing = 0.1f;
    private float _volumeDecaySpeed = 5f;

    private float currentVolume;

    private MicrophoneMode _microphoneMode = MicrophoneMode.PushToTalk;

    private const int CompressedBufferSize = 20000;
    private const int DecompressedBufferSize = 22050 * 4;

    private readonly byte[] _compressedBuffer = new byte[CompressedBufferSize];
    private readonly byte[] _decompressedBuffer = new byte[DecompressedBufferSize];

    private readonly Queue<float> _pcmQueue = new();
    private GCHandle _pcmQueueHandle;
    private GCHandle _soundHandle;

    private uint _sampleRate;
    private Sound _streamSound;
    private EventInstance _voiceInstance;

    private static SOUND_PCMREAD_CALLBACK _pcmReadCallback;
    private static SOUND_PCMSETPOS_CALLBACK _pcmSetPosCallback;

    private bool voiceEnabled;
    private bool toggleActive;
    private float prevVoiceInput;
    public bool VoiceEnabled => voiceEnabled;
    public float CurrentVolume => currentVolume;
    public enum MicrophoneMode { Toggle, PushToTalk }

    private void Start()
    {
        _sampleRate = SteamUser.GetVoiceOptimalSampleRate();

        if (!isLocalPlayer)
            InitFmodPlayback();

        LoadSavedSettings();
    }
    private void OnEnable()
    {
        if (SettingsManager.Instance != null)
            SettingsManager.Instance.OnMicrophoneModeChanged += LoadSavedSettings;
        else
            UnityEngine.Debug.LogWarning("SettingsManager not init!");
    }
    private void OnDisable()
    {
        if (SettingsManager.Instance != null)
            SettingsManager.Instance.OnMicrophoneModeChanged -= LoadSavedSettings;
        else
            UnityEngine.Debug.LogWarning("SettingsManager not init!");
    }
    private void LoadSavedSettings()
    {
        if (SettingsManager.Instance != null)
        {
            string micMode = SettingsManager.Instance.MicrophoneMode;

            SetMicrophoneMode(micMode == "Toggle" ? MicrophoneMode.Toggle : MicrophoneMode.PushToTalk);
        }
    }
    public void SetMicrophoneMode(MicrophoneMode mode)
    {
        if (mode == _microphoneMode) return;

        if (voiceEnabled)
        {
            voiceEnabled = false;
            SteamUser.StopVoiceRecording();
        }

        _microphoneMode = mode;
        toggleActive = false;
        prevVoiceInput = 0f;
    }

    public void SetVoiceVolume(float volume)
    {
        _voiceVolume = Mathf.Clamp(volume, 0f, 2f);

        if (_voiceInstance.isValid())
            _voiceInstance.setVolume(_voiceVolume);
    }

    public void SetVolumeSmoothing(float smoothing)
    {
        _volumeSmoothing = Mathf.Clamp01(smoothing);
    }

    public void SetVolumeDecaySpeed(float speed)
    {
        _volumeDecaySpeed = Mathf.Max(0.1f, speed);
    }

    private void InitFmodPlayback()
    {
        _pcmReadCallback = PcmReadCallback;
        _pcmSetPosCallback = PcmSetPosCallback;

        _pcmQueueHandle = GCHandle.Alloc(_pcmQueue);

        CREATESOUNDEXINFO exinfo = new CREATESOUNDEXINFO();
        exinfo.cbsize = Marshal.SizeOf(typeof(CREATESOUNDEXINFO));
        exinfo.numchannels = 1;
        exinfo.format = SOUND_FORMAT.PCM16;
        exinfo.defaultfrequency = (int)_sampleRate;
        exinfo.length = _sampleRate * sizeof(short) * 5;
        exinfo.decodebuffersize = _sampleRate / 10;
        exinfo.pcmreadcallback = _pcmReadCallback;
        exinfo.pcmsetposcallback = _pcmSetPosCallback;
        exinfo.userdata = GCHandle.ToIntPtr(_pcmQueueHandle);

        RuntimeManager.CoreSystem.createSound(
            (string)null,
            MODE.LOOP_NORMAL | MODE.OPENUSER | MODE.CREATESTREAM,
            ref exinfo,
            out _streamSound
        );

        _streamSound.setUserData(GCHandle.ToIntPtr(_pcmQueueHandle));

        _voiceInstance = RuntimeManager.CreateInstance(_voiceEventReference);

        _soundHandle = GCHandle.Alloc(_streamSound, GCHandleType.Normal);
        _voiceInstance.setUserData(GCHandle.ToIntPtr(_soundHandle));
        _voiceInstance.setCallback(VoiceEventCallback,
            EVENT_CALLBACK_TYPE.CREATE_PROGRAMMER_SOUND |
            EVENT_CALLBACK_TYPE.DESTROY_PROGRAMMER_SOUND |
            EVENT_CALLBACK_TYPE.DESTROYED);

        RuntimeManager.AttachInstanceToGameObject(_voiceInstance, gameObject);
        _voiceInstance.start();
    }

    [MonoPInvokeCallback(typeof(EVENT_CALLBACK))]
    private static RESULT VoiceEventCallback(EVENT_CALLBACK_TYPE type, IntPtr instancePtr, IntPtr paramPtr)
    {
        EventInstance instance = new EventInstance(instancePtr);
        instance.getUserData(out IntPtr userDataPtr);
        if (userDataPtr == IntPtr.Zero) return RESULT.OK;

        GCHandle handle = GCHandle.FromIntPtr(userDataPtr);

        switch (type)
        {
            case EVENT_CALLBACK_TYPE.CREATE_PROGRAMMER_SOUND:
                {
                    PROGRAMMER_SOUND_PROPERTIES props =
                        (PROGRAMMER_SOUND_PROPERTIES)Marshal.PtrToStructure(
                            paramPtr, typeof(PROGRAMMER_SOUND_PROPERTIES));
                    FMOD.Sound sound = (FMOD.Sound)handle.Target;
                    props.sound = sound.handle;
                    props.subsoundIndex = -1;
                    Marshal.StructureToPtr(props, paramPtr, false);
                    break;
                }
            case EVENT_CALLBACK_TYPE.DESTROY_PROGRAMMER_SOUND:
                break;
            case EVENT_CALLBACK_TYPE.DESTROYED:
                handle.Free();
                break;
        }
        return RESULT.OK;
    }

    [MonoPInvokeCallback(typeof(SOUND_PCMREAD_CALLBACK))]
    private static RESULT PcmReadCallback(IntPtr soundRaw, IntPtr data, uint dataLen)
    {
        Sound sound = new Sound(soundRaw);
        sound.getUserData(out IntPtr ptr);

        if (ptr == IntPtr.Zero) return RESULT.OK;

        var queue = (Queue<float>)GCHandle.FromIntPtr(ptr).Target;

        int samples = (int)(dataLen / sizeof(short));
        short[] pcm = new short[samples];

        for (int i = 0; i < samples; i++)
            pcm[i] = queue.TryDequeue(out float f) ? (short)(f * short.MaxValue) : (short)0;

        Marshal.Copy(pcm, 0, data, samples);

        return RESULT.OK;
    }

    [MonoPInvokeCallback(typeof(SOUND_PCMSETPOS_CALLBACK))]
    private static RESULT PcmSetPosCallback(IntPtr soundRaw, int subsound, uint position, TIMEUNIT postype) => RESULT.OK;

    private static float CalculateRms(byte[] buffer, uint byteCount)
    {
        float sum = 0f;
        int count = 0;
        for (uint i = 0; i + 1 < byteCount; i += 2)
        {
            short s = (short)((buffer[i + 1] << 8) | buffer[i]);
            float sample = s / (float)short.MaxValue;
            sum += sample * sample;
            count++;
        }
        return count > 0 ? Mathf.Sqrt(sum / count) : 0f;
    }

    private void HandleToggleMode(float voiceInput)
    {
        bool pressed = voiceInput > 0f && prevVoiceInput <= 0f;

        if (pressed)
        {
            toggleActive = !toggleActive;

            if (toggleActive)
            {
                voiceEnabled = true;
                SteamUser.StartVoiceRecording();
            }
            else
            {
                voiceEnabled = false;
                SteamUser.StopVoiceRecording();
            }
        }
    }

    private void HandlePushToTalkMode(float voiceInput)
    {
        if (!voiceEnabled && voiceInput > 0f)
        {
            voiceEnabled = true;
            SteamUser.StartVoiceRecording();
        }
        else if (voiceEnabled && voiceInput <= 0f)
        {
            voiceEnabled = false;
            SteamUser.StopVoiceRecording();
        }
    }

    private void Update()
    {
        if (!isLocalPlayer)
        {
            currentVolume = Mathf.MoveTowards(currentVolume, 0f, _volumeDecaySpeed * Time.deltaTime);
            return;
        }

        if (InputManager.Instance == null) return;

        float voiceInput = InputManager.Instance.VoiceChatInput;

        if (_microphoneMode == MicrophoneMode.Toggle)
            HandleToggleMode(voiceInput);
        else
            HandlePushToTalkMode(voiceInput);

        prevVoiceInput = voiceInput;

        if (voiceEnabled)
            CaptureAndSendVoice();
        else
            currentVolume = Mathf.MoveTowards(currentVolume, 0f, _volumeDecaySpeed * Time.deltaTime);
    }

    private void CaptureAndSendVoice()
    {
        EVoiceResult available = SteamUser.GetAvailableVoice(out uint compressed);
        if (available != EVoiceResult.k_EVoiceResultOK || compressed == 0) return;

        EVoiceResult result = SteamUser.GetVoice(
            true, _compressedBuffer, CompressedBufferSize, out uint bytesWritten);
        if (result != EVoiceResult.k_EVoiceResultOK || bytesWritten == 0) return;

        SteamUser.DecompressVoice(
            _compressedBuffer, bytesWritten,
            _decompressedBuffer, DecompressedBufferSize,
            out uint decompressedBytes, _sampleRate);

        float rms = CalculateRms(_decompressedBuffer, decompressedBytes);
        currentVolume = Mathf.Lerp(currentVolume, rms, 1f - _volumeSmoothing);

        CmdSubmitVoice(new ArraySegment<byte>(_compressedBuffer, 0, (int)bytesWritten));
    }

    [Command(channel = Channels.Unreliable, requiresAuthority = true)]
    private void CmdSubmitVoice(ArraySegment<byte> voiceData) => RpcBroadcastVoice(voiceData);

    [ClientRpc(channel = Channels.Unreliable, includeOwner = false)]
    private void RpcBroadcastVoice(ArraySegment<byte> voiceData)
    {
        if (isLocalPlayer) return;

        byte[] voiceBytes = new byte[voiceData.Count];
        Buffer.BlockCopy(voiceData.Array, voiceData.Offset, voiceBytes, 0, voiceData.Count);

        EVoiceResult result = SteamUser.DecompressVoice(
            voiceBytes,
            (uint)voiceData.Count,
            _decompressedBuffer,
            DecompressedBufferSize,
            out uint bytesWritten,
            _sampleRate
        );

        if (result != EVoiceResult.k_EVoiceResultOK || bytesWritten == 0) return;

        float rms = CalculateRms(_decompressedBuffer, bytesWritten);
        currentVolume = Mathf.Lerp(currentVolume, rms, 1f - _volumeSmoothing);

        for (uint i = 0; i + 1 < bytesWritten; i += 2)
        {
            short pcmShort = (short)((_decompressedBuffer[i + 1] << 8) | _decompressedBuffer[i]);
            _pcmQueue.Enqueue(pcmShort / (float)short.MaxValue);
        }
    }

    private void OnDestroy()
    {
        if (_voiceInstance.isValid())
        {
            _voiceInstance.stop(FMOD.Studio.STOP_MODE.IMMEDIATE);
            _voiceInstance.release();
        }

        if (_pcmQueueHandle.IsAllocated) _pcmQueueHandle.Free();
        if (_soundHandle.IsAllocated) _soundHandle.Free();
    }
}