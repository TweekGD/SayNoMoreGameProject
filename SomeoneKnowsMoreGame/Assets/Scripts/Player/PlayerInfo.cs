using Mirror;
using Steamworks;
using UnityEngine;

public class PlayerInfo : NetworkBehaviour
{
    [SyncVar(hook = nameof(OnPlayerNameChanged))] public string playerName;
    [SyncVar] public Sprite avatarSprite;
    [SyncVar] public PlayerRoomsData playerRoomsData;

    [Header("Default Player Info")]
    public string defaultName = "Player";
    public Sprite defaultAvatar;
    public MirrorSteamworksVoice mirrorSteamworksVoice;

    private PlayerNameOverHead playerNameOverHead;

    private void Awake()
    {
        playerNameOverHead = GetComponentInChildren<PlayerNameOverHead>(true);
    }

    private void Start()
    {
        if (isLocalPlayer)
        {
            string name = SteamAPI.Init() ? SteamFriends.GetPersonaName() : defaultName + Random.Range(100, 999);
            Sprite avatar = SteamAPI.Init() ? Texture2DToSprite(LoadAvatar(SteamUser.GetSteamID())) : defaultAvatar;

            CmdSetPlayerInfo(name, avatar, mirrorSteamworksVoice);
        }
    }

    private void OnPlayerNameChanged(string oldName, string newName)
    {
        if (playerNameOverHead != null)
            playerNameOverHead.SetName(newName);
    }

    public Texture2D LoadAvatar(CSteamID steamID)
    {
        int avatarHandle = SteamFriends.GetLargeFriendAvatar(steamID);

        if (avatarHandle == -1)
            return null;

        if (SteamUtils.GetImageSize(avatarHandle, out uint width, out uint height))
        {
            Texture2D texture = new Texture2D((int)width, (int)height, TextureFormat.RGBA32, false);
            byte[] buffer = new byte[width * height * 4];

            if (SteamUtils.GetImageRGBA(avatarHandle, buffer, buffer.Length))
            {
                texture.LoadRawTextureData(buffer);
                texture.Apply();
                return texture;
            }
        }

        return null;
    }

    public Sprite Texture2DToSprite(Texture2D texture)
    {
        int width = texture.width;
        int height = texture.height;
        Texture2D transformed = new Texture2D(width, height, texture.format, false);
        Color[] original = texture.GetPixels();
        Color[] result = new Color[original.Length];

        for (int y = 0; y < height; y++)
        {
            for (int x = 0; x < width; x++)
            {
                int srcIndex = y * width + x;
                int newX = width - 1 - x;
                int newY = height - 1 - y;
                int dstIndex = newY * width + newX;
                result[dstIndex] = original[srcIndex];
            }
        }

        transformed.SetPixels(result);
        transformed.Apply();

        return Sprite.Create(transformed, new Rect(0, 0, width, height), new Vector2(0.5f, 0.5f));
    }

    [Command]
    private void CmdSetPlayerInfo(string name, Sprite avatar, MirrorSteamworksVoice _mirrorSteamworksVoice)
    {
        playerName = name;
        avatarSprite = avatar;

        if (PlayerListManager.Instance != null 
            && PlayerPointsManager.Instance != null
            && PlayerRoomsManager.Instance != null)
        {
            PlayerListManager.Instance.AddPlayer(netId, playerName, avatarSprite, _mirrorSteamworksVoice);
            PlayerPointsManager.Instance.AddPlayer(netId, playerName, avatarSprite, 0);
            PlayerRoomsManager.Instance.SetPlayerRoomsData(netId);
        }
    }
    [Server]
    public void ServerSetPlayerRoomsData(PlayerRoomsData _playerRoomsData) 
    {
        playerRoomsData = _playerRoomsData;
    }

    [Server]
    private void OnDestroy()
    {
        if (PlayerListManager.Instance != null)
            PlayerListManager.Instance.RemovePlayer(netId);

        if (PlayerPointsManager.Instance != null)
            PlayerPointsManager.Instance.RemovePlayer(netId);

        if (PlayerRoomsManager.Instance != null)
            PlayerRoomsManager.Instance.RemoveOccupiedRoom(netId);
    }

    public override void OnStopServer()
    {
        base.OnStopServer();

        if (PlayerListManager.Instance != null)
            PlayerListManager.Instance.RemovePlayer(netId);

        if (PlayerPointsManager.Instance != null)
            PlayerPointsManager.Instance.RemovePlayer(netId);

        if (PlayerRoomsManager.Instance != null)
            PlayerRoomsManager.Instance.RemoveOccupiedRoom(netId);
    }
}