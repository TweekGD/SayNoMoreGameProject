using Mirror;
using Steamworks;
using UnityEngine;

public class PlayerInfo : NetworkBehaviour
{
    [Header("Default Player Info")]
    public string defaultName = "Player";
    public Sprite defaultAvatar;

    [SyncVar(hook = nameof(OnPlayerNameChanged))] private string playerName;
    [SyncVar] private Sprite avatarSprite;

    private PlayerNameOverHead playerNameOverHead;
    private IPlayerListManager playerListManager;
    public string PlayerName => playerName;
    public Sprite AvatarSprite => avatarSprite;

    private void Awake()
    {
        playerNameOverHead = GetComponentInChildren<PlayerNameOverHead>(true);
        playerListManager = ServiceLocator.Get<IPlayerListManager>();
    }

    private void Start()
    {
        if (isLocalPlayer)
        {
            string name = SteamAPI.Init() ? SteamFriends.GetPersonaName() : defaultName + Random.Range(100, 999);
            Sprite avatar = SteamAPI.Init() ? Texture2DToSprite(LoadAvatar(SteamUser.GetSteamID())) : defaultAvatar;

            CmdSetPlayerInfo(name, avatar);
            CmdAddPlayerToList(name, avatar);
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
    private void CmdSetPlayerInfo(string name, Sprite avatar)
    {
        playerName = name;
        avatarSprite = avatar;
    }

    [Command]
    private void CmdAddPlayerToList(string name, Sprite avatar)
    {
        playerListManager?.AddPlayer(netId, name, avatar);
    }

    private void RemovePlayerFromList()
    {
        playerListManager?.RemovePlayer(netId);
    }

    [Server]
    private void OnDestroy()
    {
        RemovePlayerFromList();
    }

    public override void OnStopServer()
    {
        base.OnStopServer();

        RemovePlayerFromList();
    }
}