using Mirror;
using Steamworks;
using UnityEngine;
public class SteamLobby : MonoBehaviour, ISteamLobby
{
    private NetworkManager networkManager;
    protected Callback<LobbyCreated_t> lobbyCreated;
    protected Callback<GameLobbyJoinRequested_t> gameLobbyJoinRequested;
    protected Callback<LobbyEnter_t> lobbyEntered;
    private const string HostAddressKey = "HostAddress";
    public CSteamID LobbyID { get; private set; }

    private void Start()
    {
        networkManager = GetComponent<NetworkManager>();

        if (!SteamManager.Initialized)
            return;

        lobbyCreated = Callback<LobbyCreated_t>.Create(OnLobbyCreated);
        gameLobbyJoinRequested = Callback<GameLobbyJoinRequested_t>.Create(OnGameLobbyJoinRequested);
        lobbyEntered = Callback<LobbyEnter_t>.Create(OnLobbyEntered);
    }
    public void HostLobby()
    {
        if (SteamManager.Initialized)
            SteamMatchmaking.CreateLobby(ELobbyType.k_ELobbyTypeFriendsOnly, networkManager.maxConnections);
    }
    public void LeaveLobby()
    {
        if (SteamManager.Initialized)
        {
            if (LobbyID.IsValid())
                SteamMatchmaking.LeaveLobby(LobbyID);
            if (NetworkServer.active)
                networkManager.StopHost();
            else if (NetworkClient.active)
                networkManager.StopClient();

            LobbyID = default;
        }
    }
    private void OnLobbyCreated(LobbyCreated_t callback)
    {
        if (callback.m_eResult != EResult.k_EResultOK)
            return;

        LobbyID = new CSteamID(callback.m_ulSteamIDLobby);
        SteamMatchmaking.SetLobbyData(LobbyID, HostAddressKey, SteamUser.GetSteamID().ToString());
        networkManager.StartHost();
    }
    private void OnGameLobbyJoinRequested(GameLobbyJoinRequested_t callback)
    {
        SteamMatchmaking.JoinLobby(callback.m_steamIDLobby);
    }
    private void OnLobbyEntered(LobbyEnter_t callback)
    {
        LobbyID = new CSteamID(callback.m_ulSteamIDLobby);

        if (NetworkServer.active)
            return;

        string hostAddress = SteamMatchmaking.GetLobbyData(LobbyID, HostAddressKey);
        networkManager.networkAddress = hostAddress;
        networkManager.StartClient();
    }
    public void OpenSteamInvite()
    {
        if (!SteamManager.Initialized)
        {
            Debug.LogWarning("Steamworks not init!");
            return;
        }

        SteamFriends.ActivateGameOverlayInviteDialog(LobbyID);
    }
}