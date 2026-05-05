using UnityEngine;
using UnityEngine.UI;

public class MicrophoneVoiceActivityUI : MonoBehaviour
{
    [Header("Components")]
    [SerializeField] private MirrorSteamworksVoice mirrorSteamworksVoice;
    [SerializeField] private Image voiceLevelImage;
    [SerializeField] private Image voiceActiveImage;
    [Header("Sprites")]
    [SerializeField] private Sprite voiceActiveSprite;
    [SerializeField] private Sprite voiceDisableSprite;

    private bool isActive;

    private void Start()
    {
        if (mirrorSteamworksVoice == null) { return; }

        voiceActiveImage.sprite = mirrorSteamworksVoice.VoiceEnabled ? voiceActiveSprite : voiceDisableSprite;
    }
    private void Update()
    {
        ChangeVoiceLevel();

        ChangeVoiceActiveImage(mirrorSteamworksVoice.VoiceEnabled);
    }
    private void ChangeVoiceLevel()
    {
        if (mirrorSteamworksVoice == null) { return; }

        voiceLevelImage.fillAmount = mirrorSteamworksVoice.CurrentVolume;
    }
    private void ChangeVoiceActiveImage(bool isVoiceActive)
    {
        if (mirrorSteamworksVoice == null) 
        {
            voiceActiveImage.sprite = voiceDisableSprite;
            return; 
        }

        if (isVoiceActive != isActive)
        {
            isActive = isVoiceActive;
            voiceActiveImage.sprite = mirrorSteamworksVoice.VoiceEnabled ? voiceActiveSprite : voiceDisableSprite;
        }
    }
}
