using UnityEngine;
using Mirror;

public class PlayerListManager : NetworkBehaviour
{
    public static PlayerListManager Instance;

    public readonly SyncList<PlayerData> allPlayers = new SyncList<PlayerData>();

    private void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
        }
        else
        {
            Destroy(gameObject);
        }
    }

    [Server]
    public void AddPlayer(uint netId, string playerName, Sprite avatarSprite, MirrorSteamworksVoice mirrorSteamworksVoice)
    {
        bool isHost = false;
        foreach (var conn in NetworkServer.connections.Values)
        {
            if (conn.identity != null && conn.identity.netId == netId && conn.connectionId == 0)
            {
                isHost = true;
                break;
            }
        }

        PlayerData newPlayer = new PlayerData(netId, playerName, avatarSprite, isHost, mirrorSteamworksVoice);
        allPlayers.Add(newPlayer);
        Debug.Log($"Player added to player list: {playerName} (ID={netId}, isHost={isHost})");
    }

    [Server]
    public void RemovePlayer(uint netId)
    {
        for (int i = 0; i < allPlayers.Count; i++)
        {
            if (allPlayers[i].netId == netId)
            {
                PlayerData removed = allPlayers[i];
                allPlayers.RemoveAt(i);
                Debug.Log($"Player removed in player list: {removed.playerName} (ID={netId})");
                break;
            }
        }
    }
}