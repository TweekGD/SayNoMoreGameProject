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

    [SerializeField] private ArrowControl language;
    [SerializeField] private ArrowControl sensitivity;
    [SerializeField] private ArrowControl masterVolume;
    [SerializeField] private ArrowControl musicVolume;
    [SerializeField] private ArrowControl sfxVolume;
    [SerializeField] private ArrowControl uiVolume;
    [SerializeField] private ArrowControl voiceVolume;
    [SerializeField] private ArrowControl microphoneMode;
    [SerializeField] private ArrowControl screenResolution;
    [SerializeField] private ArrowControl screenMode;
    [SerializeField] private ArrowControl fpsLimit;
    [SerializeField] private ArrowControl vSync;
    [SerializeField] private Button resetButton;

    private SettingsManager sm;

    private void Start()
    {
        sm = SettingsManager.Instance;

        if (sm == null)
        {
            Debug.LogError("SettingsManager.Instance is null.");
            return;
        }

        Bind(language.leftButton, () => sm.StepLanguage(-1));
        Bind(language.rightButton, () => sm.StepLanguage(1));

        Bind(sensitivity.leftButton, () => sm.StepSensitivity(-1));
        Bind(sensitivity.rightButton, () => sm.StepSensitivity(1));

        Bind(masterVolume.leftButton, () => sm.StepMasterVolume(-1));
        Bind(masterVolume.rightButton, () => sm.StepMasterVolume(1));

        Bind(musicVolume.leftButton, () => sm.StepMusicVolume(-1));
        Bind(musicVolume.rightButton, () => sm.StepMusicVolume(1));

        Bind(sfxVolume.leftButton, () => sm.StepSFXVolume(-1));
        Bind(sfxVolume.rightButton, () => sm.StepSFXVolume(1));

        Bind(uiVolume.leftButton, () => sm.StepUIVolume(-1));
        Bind(uiVolume.rightButton, () => sm.StepUIVolume(1));

        Bind(voiceVolume.leftButton, () => sm.StepVoiceVolume(-1));
        Bind(voiceVolume.rightButton, () => sm.StepVoiceVolume(1));

        Bind(microphoneMode.leftButton, () => sm.StepMicrophoneMode(-1));
        Bind(microphoneMode.rightButton, () => sm.StepMicrophoneMode(1));

        Bind(screenResolution.leftButton, () => sm.StepScreenResolution(-1));
        Bind(screenResolution.rightButton, () => sm.StepScreenResolution(1));

        Bind(screenMode.leftButton, () => sm.StepScreenMode(-1));
        Bind(screenMode.rightButton, () => sm.StepScreenMode(1));

        Bind(fpsLimit.leftButton, () => sm.StepFpsLimit(-1));
        Bind(fpsLimit.rightButton, () => sm.StepFpsLimit(1));

        Bind(vSync.leftButton, () => sm.StepVSync(-1));
        Bind(vSync.rightButton, () => sm.StepVSync(1));

        if (resetButton) resetButton.onClick.AddListener(sm.ResetToDefaults);

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

    private static void Bind(Button btn, System.Action action)
    {
        if (btn) btn.onClick.AddListener(() => action());
    }

    private static void SetText(TextMeshProUGUI label, string value)
    {
        if (label) label.text = value;
    }

    private static string ToPercent(float value) => Mathf.RoundToInt(value * 100f) + "%";

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

    private void RefreshLanguage() => SetText(language.valueText, sm.Language);
    private void RefreshSensitivity() => SetText(sensitivity.valueText, sm.Sensitivity.ToString("F2"));
    private void RefreshMasterVolume() => SetText(masterVolume.valueText, ToPercent(sm.MasterVolume));
    private void RefreshMusicVolume() => SetText(musicVolume.valueText, ToPercent(sm.MusicVolume));
    private void RefreshSFXVolume() => SetText(sfxVolume.valueText, ToPercent(sm.SFXVolume));
    private void RefreshUIVolume() => SetText(uiVolume.valueText, ToPercent(sm.UIVolume));
    private void RefreshVoiceVolume() => SetText(voiceVolume.valueText, ToPercent(sm.VoiceVolume));
    private void RefreshMicrophoneMode() => SetText(microphoneMode.valueText, sm.MicrophoneMode);
    private void RefreshScreenResolution() => SetText(screenResolution.valueText, sm.ScreenResolution);
    private void RefreshScreenMode() => SetText(screenMode.valueText, sm.ScreenMode);
    private void RefreshFpsLimit() => SetText(fpsLimit.valueText, ((int)sm.FpsLimit).ToString());
    private void RefreshVSync() => SetText(vSync.valueText, sm.VSync == 1 ? "On" : "Off");
}