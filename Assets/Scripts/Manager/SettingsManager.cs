using System;
using System.Collections.Generic;
using System.IO;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class SettingsManager : MonoBehaviour
{
    public static SettingsManager Instance { get; private set; }

    private readonly string[] languages = { "English", "Russian", "Spain" };
    private readonly string[] microphoneModes = { "PushToTalk", "Toggle" };
    private readonly string[] screenModes = { "Fullscreen", "Windowed", "Borderless" };
    private static readonly int[] fpsSteps = { 30, 60, 90, 120, 144, 180, 240, 360, 480 };

    private int languageIndex;
    private float sensitivityParameter;
    private float masterVolumeParameter;
    private float musicVolumeParameter;
    private float sfxVolumeParameter;
    private float uiVolumeParameter;
    private float voiceVolumeParameter;
    private int microphoneModeIndex;
    private int screenResolutionIndex;
    private int screenModeIndex;
    private float fpsLimitParameter;
    private int fpsStepIndex;
    private int vSyncParameter;

    private string SavePath => Path.Combine(Application.persistentDataPath, "settings.json");

    [Serializable]
    private class SettingsData
    {
        public int languageIndex = 0;
        public float sensitivity = 1f;
        public float masterVolume = 1f;
        public float musicVolume = 1f;
        public float sfxVolume = 1f;
        public float uiVolume = 1f;
        public float voiceVolume = 1f;
        public int microphoneModeIndex = 0;
        public string screenResolution = "1920x1080";
        public int screenModeIndex = 0;
        public int fpsLimit = 120;
        public int vSync = 0;
    }

    public List<Resolution> FilteredResolutions { get; } = new List<Resolution>();
    public List<string> AllDevices { get; } = new List<string>();

    public event Action OnLanguageChanged;
    public event Action OnSensitivityChanged;
    public event Action OnMasterVolumeChanged;
    public event Action OnMusicVolumeChanged;
    public event Action OnSFXVolumeChanged;
    public event Action OnUIVolumeChanged;
    public event Action OnVoiceVolumeChanged;
    public event Action OnMicrophoneDeviceChanged;
    public event Action OnMicrophoneModeChanged;
    public event Action OnScreenResolutionChanged;
    public event Action OnScreenModeChanged;
    public event Action OnFpsLimitChanged;
    public event Action OnVSyncChanged;

    public string Language => languages[languageIndex];
    public float Sensitivity => sensitivityParameter;
    public float MasterVolume => masterVolumeParameter;
    public float MusicVolume => musicVolumeParameter;
    public float SFXVolume => sfxVolumeParameter;
    public float UIVolume => uiVolumeParameter;
    public float VoiceVolume => voiceVolumeParameter;
    public string MicrophoneMode => microphoneModes[microphoneModeIndex];
    public string ScreenResolution => FilteredResolutions.Count > 0
        ? FilteredResolutions[screenResolutionIndex].width + "x" + FilteredResolutions[screenResolutionIndex].height
        : "1920x1080";
    public string ScreenMode => screenModes[screenModeIndex];
    public float FpsLimit => fpsLimitParameter;
    public int VSync => vSyncParameter;

    public int LanguageIndex => languageIndex;
    public int MicrophoneModeIndex => microphoneModeIndex;
    public int ScreenResolutionIndex => screenResolutionIndex;
    public int ScreenModeIndex => screenModeIndex;
    public int LanguageCount => languages.Length;
    public int MicrophoneModeCount => microphoneModes.Length;
    public int ScreenModeCount => screenModes.Length;
    public int FpsStepIndex => fpsStepIndex;
    public int FpsStepsCount => fpsSteps.Length;
    public string[] FpsStepLabels
    {
        get
        {
            string[] labels = new string[fpsSteps.Length];
            for (int i = 0; i < fpsSteps.Length; i++)
                labels[i] = fpsSteps[i].ToString();
            return labels;
        }
    }

    private FMOD.System coreSystem;

    private void Awake()
    {
        if (Instance == null)
            Instance = this;

        coreSystem = FMODUnity.RuntimeManager.CoreSystem;

        PopulateResolutionList();
        PopulateMicrophoneList();

        LoadSettings();
        ApplyAllSettings();
    }

    private int GetClosestFpsIndex(int target)
    {
        int closest = 0;
        int minDiff = int.MaxValue;
        for (int i = 0; i < fpsSteps.Length; i++)
        {
            int diff = Math.Abs(fpsSteps[i] - target);
            if (diff < minDiff)
            {
                minDiff = diff;
                closest = i;
            }
        }
        return closest;
    }

    private void LoadSettings()
    {
        SettingsData data = new SettingsData();

        if (File.Exists(SavePath))
        {
            try
            {
                string json = File.ReadAllText(SavePath);
                data = JsonUtility.FromJson<SettingsData>(json) ?? data;
            }
            catch (Exception e)
            {
                Debug.LogWarning($"[SettingsManager] Failed to load settings: {e.Message}");
            }
        }

        languageIndex = Mathf.Clamp(data.languageIndex, 0, languages.Length - 1);
        sensitivityParameter = Mathf.Clamp(data.sensitivity, 0.05f, 1f);
        masterVolumeParameter = Mathf.Clamp(data.masterVolume, 0f, 2f);
        musicVolumeParameter = Mathf.Clamp(data.musicVolume, 0f, 2f);
        sfxVolumeParameter = Mathf.Clamp(data.sfxVolume, 0f, 2f);
        uiVolumeParameter = Mathf.Clamp(data.uiVolume, 0f, 2f);
        voiceVolumeParameter = Mathf.Clamp(data.voiceVolume, 0f, 2f);
        microphoneModeIndex = Mathf.Clamp(data.microphoneModeIndex, 0, microphoneModes.Length - 1);
        screenModeIndex = Mathf.Clamp(data.screenModeIndex, 0, screenModes.Length - 1);
        vSyncParameter = Mathf.Clamp(data.vSync, 0, 1);

        int fpsIdx = Array.IndexOf(fpsSteps, data.fpsLimit);
        if (fpsIdx < 0)
            fpsIdx = GetClosestFpsIndex(data.fpsLimit);
        fpsStepIndex = Mathf.Clamp(fpsIdx, 0, fpsSteps.Length - 1);
        fpsLimitParameter = fpsSteps[fpsStepIndex];

        int resIdx = FilteredResolutions.FindIndex(r => r.width + "x" + r.height == data.screenResolution);
        screenResolutionIndex = Mathf.Clamp(resIdx, 0, Mathf.Max(0, FilteredResolutions.Count - 1));
    }

    private void SaveSettings()
    {
        SettingsData data = new SettingsData
        {
            languageIndex = languageIndex,
            sensitivity = sensitivityParameter,
            masterVolume = masterVolumeParameter,
            musicVolume = musicVolumeParameter,
            sfxVolume = sfxVolumeParameter,
            uiVolume = uiVolumeParameter,
            voiceVolume = voiceVolumeParameter,
            microphoneModeIndex = microphoneModeIndex,
            screenResolution = ScreenResolution,
            screenModeIndex = screenModeIndex,
            fpsLimit = (int)fpsLimitParameter,
            vSync = vSyncParameter
        };

        try
        {
            File.WriteAllText(SavePath, JsonUtility.ToJson(data, true));
        }
        catch (Exception e)
        {
            Debug.LogError($"[SettingsManager] Failed to save settings: {e.Message}");
        }
    }

    public void PopulateMicrophoneList()
    {
        AllDevices.Clear();
        coreSystem.getRecordNumDrivers(out int numDrivers, out _);
        if (numDrivers == 0) return;

        for (int i = 0; i < numDrivers; i++)
        {
            coreSystem.getRecordDriverInfo(i, out string name, 256, out _, out _, out _, out _, out _);
            AllDevices.Add(name);
        }
    }

    private void PopulateResolutionList()
    {
        FilteredResolutions.Clear();
        Resolution[] all = Screen.resolutions;
        HashSet<string> seen = new HashSet<string>();

        for (int i = all.Length - 1; i >= 0; i--)
        {
            string key = all[i].width + "x" + all[i].height;
            if (seen.Add(key))
                FilteredResolutions.Add(all[i]);
        }
    }

    private int WrapIndex(int current, int direction, int count) =>
        (current + direction + count) % count;

    public void SetLanguageFromDropdown(int index)
    {
        languageIndex = Mathf.Clamp(index, 0, languages.Length - 1);
        SaveSettings();
        OnLanguageChanged?.Invoke();
    }

    public void SetSensitivityFromSlider(float value)
    {
        sensitivityParameter = Mathf.Clamp(value, 0.05f, 1f);
        SaveSettings();
        OnSensitivityChanged?.Invoke();
    }

    public void SetMasterVolumeFromSlider(float value)
    {
        masterVolumeParameter = Mathf.Clamp(value, 0f, 2f);
        AudioListener.volume = masterVolumeParameter;
        SaveSettings();
        OnMasterVolumeChanged?.Invoke();
    }

    public void SetMusicVolumeFromSlider(float value)
    {
        musicVolumeParameter = Mathf.Clamp(value, 0f, 2f);
        SaveSettings();
        OnMusicVolumeChanged?.Invoke();
    }

    public void SetSFXVolumeFromSlider(float value)
    {
        sfxVolumeParameter = Mathf.Clamp(value, 0f, 2f);
        SaveSettings();
        OnSFXVolumeChanged?.Invoke();
    }

    public void SetUIVolumeFromSlider(float value)
    {
        uiVolumeParameter = Mathf.Clamp(value, 0f, 2f);
        SaveSettings();
        OnUIVolumeChanged?.Invoke();
    }

    public void SetVoiceVolumeFromSlider(float value)
    {
        voiceVolumeParameter = Mathf.Clamp(value, 0f, 2f);
        SaveSettings();
        OnVoiceVolumeChanged?.Invoke();
    }

    public void SetFpsLimitFromSlider(float value)
    {
        fpsStepIndex = Mathf.Clamp(Mathf.RoundToInt(value), 0, fpsSteps.Length - 1);
        fpsLimitParameter = fpsSteps[fpsStepIndex];
        Application.targetFrameRate = (int)fpsLimitParameter;
        SaveSettings();
        OnFpsLimitChanged?.Invoke();
    }

    public void StepMicrophoneMode(int direction)
    {
        microphoneModeIndex = WrapIndex(microphoneModeIndex, direction, microphoneModes.Length);
        SaveSettings();
        OnMicrophoneModeChanged?.Invoke();
    }

    public void SetScreenResolutionFromDropdown(int index)
    {
        if (FilteredResolutions.Count == 0) return;
        screenResolutionIndex = Mathf.Clamp(index, 0, FilteredResolutions.Count - 1);
        ApplyScreenSettings();
        SaveSettings();
        OnScreenResolutionChanged?.Invoke();
    }

    public void SetScreenModeFromDropdown(int index)
    {
        screenModeIndex = Mathf.Clamp(index, 0, screenModes.Length - 1);
        ApplyScreenSettings();
        SaveSettings();
        OnScreenModeChanged?.Invoke();
    }

    public void StepVSync(int direction)
    {
        vSyncParameter = WrapIndex(vSyncParameter, direction, 2);
        QualitySettings.vSyncCount = vSyncParameter;
        SaveSettings();
        OnVSyncChanged?.Invoke();
    }

    private void ApplyScreenSettings()
    {
        if (FilteredResolutions.Count == 0) return;
        Resolution res = FilteredResolutions[screenResolutionIndex];
        FullScreenMode mode = screenModeIndex switch
        {
            1 => FullScreenMode.Windowed,
            2 => FullScreenMode.FullScreenWindow,
            _ => FullScreenMode.ExclusiveFullScreen
        };
        Screen.SetResolution(res.width, res.height, mode);
    }

    private void ApplyAllSettings()
    {
        AudioListener.volume = masterVolumeParameter;
        QualitySettings.vSyncCount = vSyncParameter;
        Application.targetFrameRate = (int)fpsLimitParameter;
        ApplyScreenSettings();
    }

    public void ResetToDefaults()
    {
        languageIndex = 0;
        sensitivityParameter = 1f;
        masterVolumeParameter = 1f;
        musicVolumeParameter = 1f;
        sfxVolumeParameter = 1f;
        uiVolumeParameter = 1f;
        voiceVolumeParameter = 1f;
        microphoneModeIndex = 0;
        screenModeIndex = 0;
        screenResolutionIndex = 0;
        vSyncParameter = 0;
        fpsStepIndex = Array.IndexOf(fpsSteps, 120);
        fpsLimitParameter = fpsSteps[fpsStepIndex];

        SaveSettings();
        ApplyAllSettings();

        OnLanguageChanged?.Invoke();
        OnSensitivityChanged?.Invoke();
        OnMasterVolumeChanged?.Invoke();
        OnMusicVolumeChanged?.Invoke();
        OnSFXVolumeChanged?.Invoke();
        OnUIVolumeChanged?.Invoke();
        OnVoiceVolumeChanged?.Invoke();
        OnMicrophoneDeviceChanged?.Invoke();
        OnMicrophoneModeChanged?.Invoke();
        OnScreenResolutionChanged?.Invoke();
        OnScreenModeChanged?.Invoke();
        OnFpsLimitChanged?.Invoke();
        OnVSyncChanged?.Invoke();
    }
}
