using Mirror;
using System;
using UnityEngine;

public class PlayerListManager : NetworkBehaviour, IPlayerListManager
{
    public readonly SyncList<PlayerData> allPlayers = new SyncList<PlayerData>();

    public event Action<SyncList<PlayerData>.Operation, int, PlayerData, PlayerData> OnPlayersChanged;

    private void OnEnable()
    {
        allPlayers.Callback += OnPlayersChanged;
    }

    private void OnDisable()
    {
        allPlayers.Callback -= OnPlayersChanged;
    }

    [Server]
    public void AddPlayer(uint netId, string playerName, Sprite avatarSprite)
    {
        bool isHost = NetworkServer.connections.TryGetValue(0, out NetworkConnectionToClient hostConn)
            && hostConn.identity != null
            && hostConn.identity.netId == netId;

        allPlayers.Add(new PlayerData(netId, playerName, avatarSprite, isHost));
    }

    [Server]
    public void RemovePlayer(uint netId)
    {
        int index = allPlayers.FindIndex(p => p.netId == netId);
        if (index < 0) return;

        allPlayers.RemoveAt(index);
    }

    public SyncList<PlayerData> GetAllPlayers() { return allPlayers; }
}
