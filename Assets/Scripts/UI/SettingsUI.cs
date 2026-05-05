using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class SettingsUI : MonoBehaviour
{
    [System.Serializable]
    private struct ArrowControl
    {
        public Button leftButton;
        public Button rightButton;
        public TextMeshProUGUI valueText;
    }

    [System.Serializable]
    private struct SliderControl
    {
        public Slider slider;
        public TextMeshProUGUI valueText;
    }

    [SerializeField] private TMP_Dropdown languageDropdown;
    [SerializeField] private SliderControl sensitivity;
    [SerializeField] private SliderControl masterVolume;
    [SerializeField] private SliderControl musicVolume;
    [SerializeField] private SliderControl sfxVolume;
    [SerializeField] private SliderControl uiVolume;
    [SerializeField] private SliderControl voiceVolume;
    [SerializeField] private ArrowControl microphoneMode;
    [SerializeField] private TMP_Dropdown screenResolutionDropdown;
    [SerializeField] private TMP_Dropdown screenModeDropdown;
    [SerializeField] private SliderControl fpsLimit;
    [SerializeField] private ArrowControl vSync;
    [SerializeField] private Button resetButton;

    private SettingsManager sm;
    private bool isInitializing;

    private void Start()
    {
        sm = SettingsManager.Instance;

        if (sm == null)
        {
            Debug.LogError("SettingsManager.Instance is null.");
            return;
        }

        InitDropdowns();
        InitSliders();

        BindDropdowns();
        BindSliders();
        BindArrows();

        if (resetButton) resetButton.onClick.AddListener(OnReset);

        sm.OnLanguageChanged += RefreshLanguage;
        sm.OnSensitivityChanged += RefreshSensitivity;
        sm.OnMasterVolumeChanged += RefreshMasterVolume;
        sm.OnMusicVolumeChanged += RefreshMusicVolume;
        sm.OnSFXVolumeChanged += RefreshSFXVolume;
        sm.OnUIVolumeChanged += RefreshUIVolume;
        sm.OnVoiceVolumeChanged += RefreshVoiceVolume;
        sm.OnMicrophoneModeChanged += RefreshMicrophoneMode;
        sm.OnScreenResolutionChanged += RefreshScreenResolution;
        sm.OnScreenModeChanged += RefreshScreenMode;
        sm.OnFpsLimitChanged += RefreshFpsLimit;
        sm.OnVSyncChanged += RefreshVSync;

        RefreshAll();
    }

    private void OnEnable()
    {
        if (sm != null)
            RefreshAll();
    }

    private void OnDestroy()
    {
        if (sm == null) return;

        sm.OnLanguageChanged -= RefreshLanguage;
        sm.OnSensitivityChanged -= RefreshSensitivity;
        sm.OnMasterVolumeChanged -= RefreshMasterVolume;
        sm.OnMusicVolumeChanged -= RefreshMusicVolume;
        sm.OnSFXVolumeChanged -= RefreshSFXVolume;
        sm.OnUIVolumeChanged -= RefreshUIVolume;
        sm.OnVoiceVolumeChanged -= RefreshVoiceVolume;
        sm.OnMicrophoneModeChanged -= RefreshMicrophoneMode;
        sm.OnScreenResolutionChanged -= RefreshScreenResolution;
        sm.OnScreenModeChanged -= RefreshScreenMode;
        sm.OnFpsLimitChanged -= RefreshFpsLimit;
        sm.OnVSyncChanged -= RefreshVSync;
    }

    private void InitDropdowns()
    {
        if (languageDropdown)
        {
            languageDropdown.ClearOptions();
            for (int i = 0; i < sm.LanguageCount; i++)
                languageDropdown.options.Add(new TMP_Dropdown.OptionData(sm.Language));
            RebuildLanguageOptions();
        }

        if (screenResolutionDropdown)
        {
            screenResolutionDropdown.ClearOptions();
            foreach (Resolution res in sm.FilteredResolutions)
                screenResolutionDropdown.options.Add(new TMP_Dropdown.OptionData(res.width + "x" + res.height));
            screenResolutionDropdown.RefreshShownValue();
        }

        if (screenModeDropdown)
        {
            screenModeDropdown.ClearOptions();
            for (int i = 0; i < sm.ScreenModeCount; i++)
                screenModeDropdown.options.Add(new TMP_Dropdown.OptionData(GetScreenModeLabel(i)));
            screenModeDropdown.RefreshShownValue();
        }
    }

    private void RebuildLanguageOptions()
    {
        if (!languageDropdown) return;
        languageDropdown.ClearOptions();
        for (int i = 0; i < sm.LanguageCount; i++)
        {
            string[] labels = { "English", "Russian", "Spain" };
            languageDropdown.options.Add(new TMP_Dropdown.OptionData(i < labels.Length ? labels[i] : i.ToString()));
        }
        languageDropdown.RefreshShownValue();
    }

    private string GetScreenModeLabel(int index) => index switch
    {
        0 => "Fullscreen",
        1 => "Windowed",
        2 => "Borderless",
        _ => index.ToString()
    };

    private void InitSliders()
    {
        SetupSlider(sensitivity.slider, 0.05f, 1f, false);
        SetupSlider(masterVolume.slider, 0f, 2f, false);
        SetupSlider(musicVolume.slider, 0f, 2f, false);
        SetupSlider(sfxVolume.slider, 0f, 2f, false);
        SetupSlider(uiVolume.slider, 0f, 2f, false);
        SetupSlider(voiceVolume.slider, 0f, 2f, false);
        SetupSlider(fpsLimit.slider, 0f, sm.FpsStepsCount - 1, true);
    }

    private void SetupSlider(Slider slider, float min, float max, bool wholeNumbers)
    {
        if (!slider) return;
        slider.minValue = min;
        slider.maxValue = max;
        slider.wholeNumbers = wholeNumbers;
    }

    private void BindDropdowns()
    {
        if (languageDropdown)
            languageDropdown.onValueChanged.AddListener(OnLanguageDropdownChanged);

        if (screenResolutionDropdown)
            screenResolutionDropdown.onValueChanged.AddListener(OnScreenResolutionDropdownChanged);

        if (screenModeDropdown)
            screenModeDropdown.onValueChanged.AddListener(OnScreenModeDropdownChanged);
    }

    private void BindSliders()
    {
        if (sensitivity.slider)
            sensitivity.slider.onValueChanged.AddListener(OnSensitivitySliderChanged);

        if (masterVolume.slider)
            masterVolume.slider.onValueChanged.AddListener(OnMasterVolumeSliderChanged);

        if (musicVolume.slider)
            musicVolume.slider.onValueChanged.AddListener(OnMusicVolumeSliderChanged);

        if (sfxVolume.slider)
            sfxVolume.slider.onValueChanged.AddListener(OnSFXVolumeSliderChanged);

        if (uiVolume.slider)
            uiVolume.slider.onValueChanged.AddListener(OnUIVolumeSliderChanged);

        if (voiceVolume.slider)
            voiceVolume.slider.onValueChanged.AddListener(OnVoiceVolumeSliderChanged);

        if (fpsLimit.slider)
            fpsLimit.slider.onValueChanged.AddListener(OnFpsLimitSliderChanged);
    }

    private void BindArrows()
    {
        BindArrow(microphoneMode.leftButton, () => sm.StepMicrophoneMode(-1));
        BindArrow(microphoneMode.rightButton, () => sm.StepMicrophoneMode(1));

        BindArrow(vSync.leftButton, () => sm.StepVSync(-1));
        BindArrow(vSync.rightButton, () => sm.StepVSync(1));
    }

    private static void BindArrow(Button btn, System.Action action)
    {
        if (btn) btn.onClick.AddListener(() => action());
    }

    private void OnLanguageDropdownChanged(int index)
    {
        if (isInitializing) return;
        sm.SetLanguageFromDropdown(index);
    }

    private void OnScreenResolutionDropdownChanged(int index)
    {
        if (isInitializing) return;
        sm.SetScreenResolutionFromDropdown(index);
    }

    private void OnScreenModeDropdownChanged(int index)
    {
        if (isInitializing) return;
        sm.SetScreenModeFromDropdown(index);
    }

    private void OnSensitivitySliderChanged(float value)
    {
        if (isInitializing) return;
        sm.SetSensitivityFromSlider(value);
    }

    private void OnMasterVolumeSliderChanged(float value)
    {
        if (isInitializing) return;
        sm.SetMasterVolumeFromSlider(value);
    }

    private void OnMusicVolumeSliderChanged(float value)
    {
        if (isInitializing) return;
        sm.SetMusicVolumeFromSlider(value);
    }

    private void OnSFXVolumeSliderChanged(float value)
    {
        if (isInitializing) return;
        sm.SetSFXVolumeFromSlider(value);
    }

    private void OnUIVolumeSliderChanged(float value)
    {
        if (isInitializing) return;
        sm.SetUIVolumeFromSlider(value);
    }

    private void OnVoiceVolumeSliderChanged(float value)
    {
        if (isInitializing) return;
        sm.SetVoiceVolumeFromSlider(value);
    }

    private void OnFpsLimitSliderChanged(float value)
    {
        if (isInitializing) return;
        sm.SetFpsLimitFromSlider(value);
    }

    private void OnReset()
    {
        sm.ResetToDefaults();
        RefreshAll();
    }

    private void RefreshAll()
    {
        RefreshLanguage();
        RefreshSensitivity();
        RefreshMasterVolume();
        RefreshMusicVolume();
        RefreshSFXVolume();
        RefreshUIVolume();
        RefreshVoiceVolume();
        RefreshMicrophoneMode();
        RefreshScreenResolution();
        RefreshScreenMode();
        RefreshFpsLimit();
        RefreshVSync();
    }

    private void RefreshDropdown(TMP_Dropdown dropdown, int index)
    {
        if (!dropdown) return;
        isInitializing = true;
        dropdown.value = index;
        dropdown.RefreshShownValue();
        isInitializing = false;
    }

    private void RefreshSlider(Slider slider, float value)
    {
        if (!slider) return;
        isInitializing = true;
        slider.value = value;
        isInitializing = false;
    }

    private static void SetText(TextMeshProUGUI label, string value)
    {
        if (label) label.text = value;
    }

    private static string ToPercent(float value) => Mathf.RoundToInt(value * 100f) + "%";

    private void RefreshLanguage()
    {
        RefreshDropdown(languageDropdown, sm.LanguageIndex);
    }

    private void RefreshSensitivity()
    {
        RefreshSlider(sensitivity.slider, sm.Sensitivity);
        SetText(sensitivity.valueText, sm.Sensitivity.ToString("F2"));
    }

    private void RefreshMasterVolume()
    {
        RefreshSlider(masterVolume.slider, sm.MasterVolume);
        SetText(masterVolume.valueText, ToPercent(sm.MasterVolume));
    }

    private void RefreshMusicVolume()
    {
        RefreshSlider(musicVolume.slider, sm.MusicVolume);
        SetText(musicVolume.valueText, ToPercent(sm.MusicVolume));
    }

    private void RefreshSFXVolume()
    {
        RefreshSlider(sfxVolume.slider, sm.SFXVolume);
        SetText(sfxVolume.valueText, ToPercent(sm.SFXVolume));
    }

    private void RefreshUIVolume()
    {
        RefreshSlider(uiVolume.slider, sm.UIVolume);
        SetText(uiVolume.valueText, ToPercent(sm.UIVolume));
    }

    private void RefreshVoiceVolume()
    {
        RefreshSlider(voiceVolume.slider, sm.VoiceVolume);
        SetText(voiceVolume.valueText, ToPercent(sm.VoiceVolume));
    }

    private void RefreshMicrophoneMode()
    {
        SetText(microphoneMode.valueText, sm.MicrophoneMode);
    }

    private void RefreshScreenResolution()
    {
        RefreshDropdown(screenResolutionDropdown, sm.ScreenResolutionIndex);
    }

    private void RefreshScreenMode()
    {
        RefreshDropdown(screenModeDropdown, sm.ScreenModeIndex);
    }

    private void RefreshFpsLimit()
    {
        RefreshSlider(fpsLimit.slider, sm.FpsStepIndex);
        SetText(fpsLimit.valueText, ((int)sm.FpsLimit).ToString());
    }

    private void RefreshVSync()
    {
        SetText(vSync.valueText, sm.VSync == 1 ? "On" : "Off");
    }
}
