using FMOD.Studio;
using FMODUnity;
using System.Collections.Generic;
using UnityEngine;

public class AudioManager : MonoBehaviour
{
    public float masterVolume = 1;
    public float musicVolume = 1;
    public float SFXVolume = 1;
    public float UIVolume = 1;
    public float voiceChatVolume = 1;

    private Bus masterBus;
    private Bus musicBus;
    private Bus SFXBus;
    private Bus UIBus;
    private Bus voiceChatBus;

    private List<EventInstance> eventInstances;
    private List<StudioEventEmitter> eventEmitters;

    public EventInstance ambienceEventInstance;
    public EventInstance musicEventInstance;

    private SettingsManager settingsManager;
    public static AudioManager Instance { get; private set; }
    private void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
        }
        else
        {
            Destroy(gameObject);
        }

        DontDestroyOnLoad(gameObject);

        eventInstances = new List<EventInstance>();
        eventEmitters = new List<StudioEventEmitter>();

        masterBus = RuntimeManager.GetBus("bus:/");
        SFXBus = RuntimeManager.GetBus("bus:/SFX");
        musicBus = RuntimeManager.GetBus("bus:/Music");
        UIBus = RuntimeManager.GetBus("bus:/UI");
        voiceChatBus = RuntimeManager.GetBus("bus:/Voice Chat");
    }
    private void Start()
    {
        ChangeVolumes();
    }
    private void Update()
    {
        ChangeVolumes();
    }
    private void ChangeVolumes()
    {
        GetSavedVolumes();
        UpdateVolumeValue();
    }
    private void GetSavedVolumes()
    {
        if (settingsManager == null) 
        {
            settingsManager = SettingsManager.Instance;

            if (settingsManager == null)
            {
                return;
            }
        }

        masterVolume = settingsManager.MasterVolume;
        musicVolume = settingsManager.MusicVolume;
        SFXVolume = settingsManager.SFXVolume;
        UIVolume = settingsManager.UIVolume;
        voiceChatVolume = settingsManager.VoiceVolume;
    }
    private void UpdateVolumeValue()
    {
        masterBus.setVolume(masterVolume);
        musicBus.setVolume(musicVolume);
        SFXBus.setVolume(SFXVolume);
        UIBus.setVolume(UIVolume);
        voiceChatBus.setVolume(voiceChatVolume);
    }

    public void InitializeAmbience(EventReference ambienceEventReference)
    {
        ambienceEventInstance = CreateInstance(ambienceEventReference);
        ambienceEventInstance.start();
    }

    public void InitializeMusic(EventReference musicEventReference)
    {
        musicEventInstance = CreateInstance(musicEventReference);
        musicEventInstance.start();
    }

    public void SetAmbienceParameter(string parameterName, float parameterValue, bool ignoreSpeed = false)
    {
        ambienceEventInstance.setParameterByName(parameterName, parameterValue, ignoreSpeed);
    }

    public void PlayOneShot(EventReference sound, Vector3 worldPos)
    {
        RuntimeManager.PlayOneShot(sound, worldPos);
    }

    public EventInstance CreateInstance(EventReference eventReference)
    {
        EventInstance eventInstance = RuntimeManager.CreateInstance(eventReference);
        eventInstances.Add(eventInstance);
        return eventInstance;
    }

    public StudioEventEmitter InitializeEventEmitter(EventReference eventReference, GameObject emitterGameObject)
    {
        StudioEventEmitter emitter = emitterGameObject.GetComponent<StudioEventEmitter>();
        emitter.EventReference = eventReference;
        eventEmitters.Add(emitter);
        return emitter;
    }

    public void CleanUp()
    {
        foreach (EventInstance eventInstance in eventInstances)
        {
            eventInstance.stop(FMOD.Studio.STOP_MODE.IMMEDIATE);
            eventInstance.release();
        }

        foreach (StudioEventEmitter emitter in eventEmitters)
        {
            emitter.Stop();
        }
    }

    private void OnDestroy()
    {
        CleanUp();
    }
}