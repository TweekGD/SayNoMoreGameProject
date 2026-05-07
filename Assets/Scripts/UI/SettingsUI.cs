//using TMPro;
//using UnityEngine;
//using UnityEngine.UI;

//public class SettingsUI : MonoBehaviour
//{
//    [System.Serializable]
//    private struct ArrowControl
//    {
//        public Button leftButton;
//        public Button rightButton;
//        public TextMeshProUGUI valueText;
//    }

//    [System.Serializable]
//    private struct SliderControl
//    {
//        public Slider slider;
//        public TextMeshProUGUI valueText;
//    }

//    [SerializeField] private TMP_Dropdown languageDropdown;
//    [SerializeField] private SliderControl sensitivity;
//    [SerializeField] private SliderControl masterVolume;
//    [SerializeField] private SliderControl musicVolume;
//    [SerializeField] private SliderControl sfxVolume;
//    [SerializeField] private SliderControl uiVolume;
//    [SerializeField] private SliderControl voiceVolume;
//    [SerializeField] private ArrowControl microphoneMode;
//    [SerializeField] private TMP_Dropdown screenResolutionDropdown;
//    [SerializeField] private TMP_Dropdown screenModeDropdown;
//    [SerializeField] private SliderControl fpsLimit;
//    [SerializeField] private ArrowControl vSync;
//    [SerializeField] private Button resetButton;

//    private ISettingsManager settingsManager;
//    private bool isInitializing;

//    private void Start()
//    {
//        settingsManager = ServiceLocator.Get<ISettingsManager>();

//        if (settingsManager == null)
//        {
//            Debug.LogError("SettingsManager.Instance is null.");
//            return;
//        }

//        InitDropdowns();
//        InitSliders();

//        BindDropdowns();
//        BindSliders();
//        BindArrows();

//        if (resetButton) resetButton.onClick.AddListener(OnReset);

//        settingsManager.GetParameter<IndexedParameter>("Language").OnChanged += RefreshLanguage;
//        settingsManager.OnSensitivityChanged += RefreshSensitivity;
//        settingsManager.OnMasterVolumeChanged += RefreshMasterVolume;
//        settingsManager.OnMusicVolumeChanged += RefreshMusicVolume;
//        settingsManager.OnSFXVolumeChanged += RefreshSFXVolume;
//        settingsManager.OnUIVolumeChanged += RefreshUIVolume;
//        settingsManager.OnVoiceVolumeChanged += RefreshVoiceVolume;
//        settingsManager.OnMicrophoneModeChanged += RefreshMicrophoneMode;
//        settingsManager.OnScreenResolutionChanged += RefreshScreenResolution;
//        settingsManager.OnScreenModeChanged += RefreshScreenMode;
//        settingsManager.OnFpsLimitChanged += RefreshFpsLimit;
//        settingsManager.OnVSyncChanged += RefreshVSync;

//        RefreshAll();
//    }

//    private void OnEnable()
//    {
//        if (settingsManager != null)
//            RefreshAll();
//    }

//    private void OnDestroy()
//    {
//        if (settingsManager == null) return;

//        settingsManager.OnLanguageChanged -= RefreshLanguage;
//        settingsManager.OnSensitivityChanged -= RefreshSensitivity;
//        settingsManager.OnMasterVolumeChanged -= RefreshMasterVolume;
//        settingsManager.OnMusicVolumeChanged -= RefreshMusicVolume;
//        settingsManager.OnSFXVolumeChanged -= RefreshSFXVolume;
//        settingsManager.OnUIVolumeChanged -= RefreshUIVolume;
//        settingsManager.OnVoiceVolumeChanged -= RefreshVoiceVolume;
//        settingsManager.OnMicrophoneModeChanged -= RefreshMicrophoneMode;
//        settingsManager.OnScreenResolutionChanged -= RefreshScreenResolution;
//        settingsManager.OnScreenModeChanged -= RefreshScreenMode;
//        settingsManager.OnFpsLimitChanged -= RefreshFpsLimit;
//        settingsManager.OnVSyncChanged -= RefreshVSync;
//    }

//    private void InitDropdowns()
//    {
//        if (languageDropdown)
//        {
//            languageDropdown.ClearOptions();
//            for (int i = 0; i < settingsManager.LanguageCount; i++)
//                languageDropdown.options.Add(new TMP_Dropdown.OptionData(settingsManager.Language));
//            RebuildLanguageOptions();
//        }

//        if (screenResolutionDropdown)
//        {
//            screenResolutionDropdown.ClearOptions();
//            foreach (Resolution res in sm.FilteredResolutions)
//                screenResolutionDropdown.options.Add(new TMP_Dropdown.OptionData(res.width + "x" + res.height));
//            screenResolutionDropdown.RefreshShownValue();
//        }

//        if (screenModeDropdown)
//        {
//            screenModeDropdown.ClearOptions();
//            for (int i = 0; i < sm.ScreenModeCount; i++)
//                screenModeDropdown.options.Add(new TMP_Dropdown.OptionData(GetScreenModeLabel(i)));
//            screenModeDropdown.RefreshShownValue();
//        }
//    }

//    private void RebuildLanguageOptions()
//    {
//        if (!languageDropdown) return;
//        languageDropdown.ClearOptions();
//        for (int i = 0; i < settingsManager.LanguageCount; i++)
//        {
//            string[] labels = { "English", "Russian", "Spain" };
//            languageDropdown.options.Add(new TMP_Dropdown.OptionData(i < labels.Length ? labels[i] : i.ToString()));
//        }
//        languageDropdown.RefreshShownValue();
//    }

//    private string GetScreenModeLabel(int index) => index switch
//    {
//        0 => "Fullscreen",
//        1 => "Windowed",
//        2 => "Borderless",
//        _ => index.ToString()
//    };

//    private void InitSliders()
//    {
//        SetupSlider(sensitivity.slider, 0.05f, 1f, false);
//        SetupSlider(masterVolume.slider, 0f, 2f, false);
//        SetupSlider(musicVolume.slider, 0f, 2f, false);
//        SetupSlider(sfxVolume.slider, 0f, 2f, false);
//        SetupSlider(uiVolume.slider, 0f, 2f, false);
//        SetupSlider(voiceVolume.slider, 0f, 2f, false);
//        SetupSlider(fpsLimit.slider, 0f, settingsManager.FpsStepsCount - 1, true);
//    }

//    private void SetupSlider(Slider slider, float min, float max, bool wholeNumbers)
//    {
//        if (!slider) return;
//        slider.minValue = min;
//        slider.maxValue = max;
//        slider.wholeNumbers = wholeNumbers;
//    }

//    private void BindDropdowns()
//    {
//        if (languageDropdown)
//            languageDropdown.onValueChanged.AddListener(OnLanguageDropdownChanged);

//        if (screenResolutionDropdown)
//            screenResolutionDropdown.onValueChanged.AddListener(OnScreenResolutionDropdownChanged);

//        if (screenModeDropdown)
//            screenModeDropdown.onValueChanged.AddListener(OnScreenModeDropdownChanged);
//    }

//    private void BindSliders()
//    {
//        if (sensitivity.slider)
//            sensitivity.slider.onValueChanged.AddListener(OnSensitivitySliderChanged);

//        if (masterVolume.slider)
//            masterVolume.slider.onValueChanged.AddListener(OnMasterVolumeSliderChanged);

//        if (musicVolume.slider)
//            musicVolume.slider.onValueChanged.AddListener(OnMusicVolumeSliderChanged);

//        if (sfxVolume.slider)
//            sfxVolume.slider.onValueChanged.AddListener(OnSFXVolumeSliderChanged);

//        if (uiVolume.slider)
//            uiVolume.slider.onValueChanged.AddListener(OnUIVolumeSliderChanged);

//        if (voiceVolume.slider)
//            voiceVolume.slider.onValueChanged.AddListener(OnVoiceVolumeSliderChanged);

//        if (fpsLimit.slider)
//            fpsLimit.slider.onValueChanged.AddListener(OnFpsLimitSliderChanged);
//    }

//    private void BindArrows()
//    {
//        BindArrow(microphoneMode.leftButton, () => settingsManager.StepMicrophoneMode(-1));
//        BindArrow(microphoneMode.rightButton, () => settingsManager.StepMicrophoneMode(1));

//        BindArrow(vSync.leftButton, () => settingsManager.StepVSync(-1));
//        BindArrow(vSync.rightButton, () => settingsManager.StepVSync(1));
//    }

//    private static void BindArrow(Button btn, System.Action action)
//    {
//        if (btn) btn.onClick.AddListener(() => action());
//    }

//    private void OnLanguageDropdownChanged(int index)
//    {
//        if (isInitializing) return;
//        sm.SetLanguageFromDropdown(index);
//    }

//    private void OnScreenResolutionDropdownChanged(int index)
//    {
//        if (isInitializing) return;
//        sm.SetScreenResolutionFromDropdown(index);
//    }

//    private void OnScreenModeDropdownChanged(int index)
//    {
//        if (isInitializing) return;
//        sm.SetScreenModeFromDropdown(index);
//    }

//    private void OnSensitivitySliderChanged(float value)
//    {
//        if (isInitializing) return;
//        sm.SetSensitivityFromSlider(value);
//    }

//    private void OnMasterVolumeSliderChanged(float value)
//    {
//        if (isInitializing) return;
//        sm.SetMasterVolumeFromSlider(value);
//    }

//    private void OnMusicVolumeSliderChanged(float value)
//    {
//        if (isInitializing) return;
//        sm.SetMusicVolumeFromSlider(value);
//    }

//    private void OnSFXVolumeSliderChanged(float value)
//    {
//        if (isInitializing) return;
//        sm.SetSFXVolumeFromSlider(value);
//    }

//    private void OnUIVolumeSliderChanged(float value)
//    {
//        if (isInitializing) return;
//        sm.SetUIVolumeFromSlider(value);
//    }

//    private void OnVoiceVolumeSliderChanged(float value)
//    {
//        if (isInitializing) return;
//        sm.SetVoiceVolumeFromSlider(value);
//    }

//    private void OnFpsLimitSliderChanged(float value)
//    {
//        if (isInitializing) return;
//        sm.SetFpsLimitFromSlider(value);
//    }

//    private void OnReset()
//    {
//        sm.ResetToDefaults();
//        RefreshAll();
//    }

//    private void RefreshAll()
//    {
//        RefreshLanguage();
//        RefreshSensitivity();
//        RefreshMasterVolume();
//        RefreshMusicVolume();
//        RefreshSFXVolume();
//        RefreshUIVolume();
//        RefreshVoiceVolume();
//        RefreshMicrophoneMode();
//        RefreshScreenResolution();
//        RefreshScreenMode();
//        RefreshFpsLimit();
//        RefreshVSync();
//    }

//    private void RefreshDropdown(TMP_Dropdown dropdown, int index)
//    {
//        if (!dropdown) return;
//        isInitializing = true;
//        dropdown.value = index;
//        dropdown.RefreshShownValue();
//        isInitializing = false;
//    }

//    private void RefreshSlider(Slider slider, float value)
//    {
//        if (!slider) return;
//        isInitializing = true;
//        slider.value = value;
//        isInitializing = false;
//    }

//    private static void SetText(TextMeshProUGUI label, string value)
//    {
//        if (label) label.text = value;
//    }

//    private static string ToPercent(float value) => Mathf.RoundToInt(value * 100f) + "%";

//    private void RefreshLanguage()
//    {
//        RefreshDropdown(languageDropdown, sm.LanguageIndex);
//    }

//    private void RefreshSensitivity()
//    {
//        RefreshSlider(sensitivity.slider, sm.Sensitivity);
//        SetText(sensitivity.valueText, sm.Sensitivity.ToString("F2"));
//    }

//    private void RefreshMasterVolume()
//    {
//        RefreshSlider(masterVolume.slider, sm.MasterVolume);
//        SetText(masterVolume.valueText, ToPercent(sm.MasterVolume));
//    }

//    private void RefreshMusicVolume()
//    {
//        RefreshSlider(musicVolume.slider, sm.MusicVolume);
//        SetText(musicVolume.valueText, ToPercent(sm.MusicVolume));
//    }

//    private void RefreshSFXVolume()
//    {
//        RefreshSlider(sfxVolume.slider, sm.SFXVolume);
//        SetText(sfxVolume.valueText, ToPercent(sm.SFXVolume));
//    }

//    private void RefreshUIVolume()
//    {
//        RefreshSlider(uiVolume.slider, sm.UIVolume);
//        SetText(uiVolume.valueText, ToPercent(sm.UIVolume));
//    }

//    private void RefreshVoiceVolume()
//    {
//        RefreshSlider(voiceVolume.slider, sm.VoiceVolume);
//        SetText(voiceVolume.valueText, ToPercent(sm.VoiceVolume));
//    }

//    private void RefreshMicrophoneMode()
//    {
//        SetText(microphoneMode.valueText, sm.MicrophoneMode);
//    }

//    private void RefreshScreenResolution()
//    {
//        RefreshDropdown(screenResolutionDropdown, sm.ScreenResolutionIndex);
//    }

//    private void RefreshScreenMode()
//    {
//        RefreshDropdown(screenModeDropdown, sm.ScreenModeIndex);
//    }

//    private void RefreshFpsLimit()
//    {
//        RefreshSlider(fpsLimit.slider, sm.FpsStepIndex);
//        SetText(fpsLimit.valueText, ((int)sm.FpsLimit).ToString());
//    }

//    private void RefreshVSync()
//    {
//        SetText(vSync.valueText, sm.VSync == 1 ? "On" : "Off");
//    }
//}
