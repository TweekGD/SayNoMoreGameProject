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
    private PlayerVoiceChat _playerVoiceChat;
    private float _currentVoiceVolume = 1f;
    public uint NetID => _netID;

    private IPlayerKickManager playerKickManager;
    private void Awake()
    {
        playerKickManager = ServiceLocator.Get<IPlayerKickManager>();
    }
    public void Setup(string playerName, Sprite avatarSprite, uint netID, bool isHost = false)
    {
        if (playerName == null || avatarSprite == null) { return; }

        playerNameText.text = playerName;
        avatarImage.sprite = avatarSprite;
        _netID = netID;
        //_mirrorSteamworksVoice = mirrorSteamworksVoice;

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

        UpdateSwitchText(_currentVoiceVolume);
    }

    private void OnDestroy()
    {
        switchVolume.leftButton.onClick.RemoveAllListeners();
        switchVolume.rightButton.onClick.RemoveAllListeners();
        kickButton.onClick.RemoveAllListeners();
    }

    private void DecreaseVolume()
    {
        _currentVoiceVolume -= 0.1f;
        _currentVoiceVolume = Mathf.Clamp(_currentVoiceVolume, 0f, 2f);
        ChangePlayerVolume(_currentVoiceVolume);
        UpdateSwitchText(_currentVoiceVolume);
    }

    private void IncreaseVolume()
    {
        _currentVoiceVolume += 0.1f;
        _currentVoiceVolume = Mathf.Clamp(_currentVoiceVolume, 0f, 2f);
        ChangePlayerVolume(_currentVoiceVolume);
        UpdateSwitchText(_currentVoiceVolume);
    }

    private void ChangePlayerVolume(float volume)
    {
        _playerVoiceChat.SetVoiceVolume(volume);
    }

    private void UpdateSwitchText(float value)
    {
        switchVolume.valueText.text = $"{value * 100:0}%";
    }
    private void KickPlayer()
    {
        if (!NetworkServer.active) return;

        if (playerKickManager != null)
            playerKickManager.KickPlayer(_netID);
    }

    [System.Serializable]
    private struct ArrowControl
    {
        public Button leftButton;
        public Button rightButton;
        public TextMeshProUGUI valueText;
    }
}
