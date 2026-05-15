using UnityEngine;
using UnityEngine.UI;

public class MicrophoneVoiceActivityUI : MonoBehaviour
{
    [Header("Components")]
    [SerializeField] private PlayerVoiceChat playerVoiceChat;
    [SerializeField] private Image voiceLevelImage;
    [SerializeField] private Image voiceActiveImage;
    [Header("Sprites")]
    [SerializeField] private Sprite voiceActiveSprite;
    [SerializeField] private Sprite voiceDisableSprite;

    private bool isActive;

    private void Start()
    {
        if (playerVoiceChat == null) { return; }

        voiceActiveImage.sprite = playerVoiceChat.VoiceEnabled ? voiceActiveSprite : voiceDisableSprite;
    }
    private void Update()
    {
        ChangeVoiceLevel();

        ChangeVoiceActiveImage(playerVoiceChat.VoiceEnabled);
    }
    private void ChangeVoiceLevel()
    {
        if (playerVoiceChat == null) { return; }

        voiceLevelImage.fillAmount = playerVoiceChat.CurrentVolume;
    }
    private void ChangeVoiceActiveImage(bool isVoiceActive)
    {
        if (playerVoiceChat == null) 
        {
            voiceActiveImage.sprite = voiceDisableSprite;
            return; 
        }

        if (isVoiceActive != isActive)
        {
            isActive = isVoiceActive;
            voiceActiveImage.sprite = playerVoiceChat.VoiceEnabled ? voiceActiveSprite : voiceDisableSprite;
        }
    }
}
