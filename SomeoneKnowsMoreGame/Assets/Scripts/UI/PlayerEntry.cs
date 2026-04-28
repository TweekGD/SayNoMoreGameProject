using Mirror;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class PlayerEntry : MonoBehaviour
{
    [SerializeField] private TextMeshProUGUI playerNameText;
    [SerializeField] private Image avatarImage;
    [SerializeField] private GameObject switchVolumeObject;
    [SerializeField] private ArrowControl switchVolume;
    [SerializeField] private Button kickButton;
    [SerializeField] private TextMeshProUGUI hostText;

    private uint _netID;
    private MirrorSteamworksVoice _mirrorSteamworksVoice;
    private float currentVoiceVolume = 1f;
    public uint NetID => _netID;

    public void Setup(string playerName, Sprite avatarSprite, uint netID, MirrorSteamworksVoice mirrorSteamworksVoice, bool isHost = false)
    {
        if (playerName == null || avatarSprite == null) { return; }

        playerNameText.text = playerName;
        avatarImage.sprite = avatarSprite;
        _netID = netID;
        _mirrorSteamworksVoice = mirrorSteamworksVoice;

        bool isLocalPlayer = NetworkClient.localPlayer != null && NetworkClient.localPlayer.netId == _netID;
        bool amIHost = NetworkServer.active;

        hostText.text = isHost ? "Host" : isLocalPlayer ? "You" : "Client";;
        kickButton.gameObject.SetActive(amIHost && !isLocalPlayer);
        switchVolumeObject.SetActive(!isLocalPlayer);

        switchVolume.leftButton.onClick.RemoveAllListeners();
        switchVolume.rightButton.onClick.RemoveAllListeners();
        kickButton.onClick.RemoveAllListeners();

        switchVolume.leftButton.onClick.AddListener(DecreaseVolume);
        switchVolume.rightButton.onClick.AddListener(IncreaseVolume);
        kickButton.onClick.AddListener(KickPlayer);

        UpdateSwitchText(currentVoiceVolume);
    }

    private void OnDestroy()
    {
        switchVolume.leftButton.onClick.RemoveAllListeners();
        switchVolume.rightButton.onClick.RemoveAllListeners();
        kickButton.onClick.RemoveAllListeners();
    }

    private void DecreaseVolume()
    {
        currentVoiceVolume -= 0.1f;
        currentVoiceVolume = Mathf.Clamp(currentVoiceVolume, 0f, 2f);
        ChangePlayerVolume(currentVoiceVolume);
        UpdateSwitchText(currentVoiceVolume);
    }

    private void IncreaseVolume()
    {
        currentVoiceVolume += 0.1f;
        currentVoiceVolume = Mathf.Clamp(currentVoiceVolume, 0f, 2f);
        ChangePlayerVolume(currentVoiceVolume);
        UpdateSwitchText(currentVoiceVolume);
    }

    private void ChangePlayerVolume(float volume)
    {
        _mirrorSteamworksVoice.SetVoiceVolume(volume);
    }

    private void UpdateSwitchText(float value)
    {
        switchVolume.valueText.text = $"{value * 100:0}%";
    }
    private void KickPlayer()
    {
        if (!NetworkServer.active) return;

        if (KickManager.Instance != null)
            KickManager.Instance.KickPlayer(_netID);
    }

    [System.Serializable]
    private struct ArrowControl
    {
        public Button leftButton;
        public Button rightButton;
        public TextMeshProUGUI valueText;
    }
}
