using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class LobbySettingsManager : MonoBehaviour
{
    [System.Serializable]
    private struct ArrowControl
    {
        public Button leftButton;
        public Button rightButton;
        public TextMeshProUGUI valueText;
    }

    public enum LobbyType { Public, Private, FriendsOnly }

    public static LobbySettingsManager Instance { get; private set; }

    [SerializeField] private ArrowControl maxPlayersArrowControl;
    [SerializeField] private ArrowControl lobbyTypeArrowControl;
    [SerializeField] private Button startButton;

    private readonly string[] lobbyTypes = { "Public", "Private", "FriendsOnly" };
    private readonly int minPlayers = 4;
    private readonly int maxPlayersLimit = 8;

    private string _lobbyName;
    private string _lobbyPassword;
    private int _lobbyTypeIndex;
    private int _maxPlayers = 4;

    public string LobbyName => _lobbyName;
    public string LobbyPassword => _lobbyPassword;
    public LobbyType CurrentLobbyType => (LobbyType)_lobbyTypeIndex;
    public int MaxPlayers => _maxPlayers;

    private void Awake()
    {
        if (Instance == null)
            Instance = this;

        startButton.onClick.RemoveAllListeners();

        if (SteamLobby.Instance != null)
            startButton.onClick.AddListener(SteamLobby.Instance.HostLobby);
    }

    private void Start()
    {
        Bind(maxPlayersArrowControl.leftButton, () => StepMaxPlayers(-1));
        Bind(maxPlayersArrowControl.rightButton, () => StepMaxPlayers(1));

        Bind(lobbyTypeArrowControl.leftButton, () => StepLobbyType(-1));
        Bind(lobbyTypeArrowControl.rightButton, () => StepLobbyType(1));

        RefreshMaxPlayers();
        RefreshLobbyType();
    }

    private static void Bind(Button btn, System.Action action)
    {
        if (btn) btn.onClick.AddListener(() => action());
    }

    private static void SetText(TextMeshProUGUI label, string value)
    {
        if (label) label.text = value;
    }

    private void StepMaxPlayers(int direction)
    {
        _maxPlayers = Mathf.Clamp(_maxPlayers + direction, minPlayers, maxPlayersLimit);
        RefreshMaxPlayers();
    }

    private void StepLobbyType(int direction)
    {
        _lobbyTypeIndex = (_lobbyTypeIndex + direction + lobbyTypes.Length) % lobbyTypes.Length;
        RefreshLobbyType();
    }

    private void RefreshMaxPlayers() => SetText(maxPlayersArrowControl.valueText, $"{_maxPlayers} Players");
    private void RefreshLobbyType() => SetText(lobbyTypeArrowControl.valueText, lobbyTypes[_lobbyTypeIndex]);

    public void ChangeLobbyName(string value) => _lobbyName = value;
    public void ChangeLobbyPassword(string value) => _lobbyPassword = value;
}