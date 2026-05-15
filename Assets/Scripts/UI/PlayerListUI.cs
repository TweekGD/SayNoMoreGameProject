using Mirror;
using System.Collections.Generic;
using UnityEngine;

public class UIPlayerList : NetworkBehaviour
{
    [Header("UI Elements")]
    [SerializeField] private Transform contentPanel;
    [SerializeField] private GameObject playerEntryPrefab;

    private Dictionary<uint, GameObject> entries = new Dictionary<uint, GameObject>();

    private IPlayerListManager playerListManager;

    private void Awake()
    {
        playerListManager = ServiceLocator.Get<IPlayerListManager>();
    }

    private void Start()
    {
        if (playerListManager != null)
            SubscribeToSyncList();
    }

    private void SubscribeToSyncList()
    {
        playerListManager.OnPlayersChanged += OnPlayersChanged;

        SyncList <PlayerData> playerList = playerListManager.GetAllPlayers();

        foreach (var player in playerList)
            AddPlayerEntry(player);
    }

    private void OnDestroy()
    {
        if (playerListManager != null)
            playerListManager.OnPlayersChanged -= OnPlayersChanged;
    }

    private void OnPlayersChanged(SyncList<PlayerData>.Operation op, int index, PlayerData oldItem, PlayerData newItem)
    {
        switch (op)
        {
            case SyncList<PlayerData>.Operation.OP_ADD:
                AddPlayerEntry(newItem);
                break;
            case SyncList<PlayerData>.Operation.OP_REMOVEAT:
                RemovePlayerEntry(oldItem.netId);
                break;
            case SyncList<PlayerData>.Operation.OP_SET:
                UpdatePlayerEntry(newItem);
                break;
        }
    }

    private void AddPlayerEntry(PlayerData player)
    {
        if (entries.ContainsKey(player.netId)) return;

        GameObject entry = Instantiate(playerEntryPrefab, contentPanel);

        PlayerEntry entryScript = entry.GetComponent<PlayerEntry>();
        if (entryScript != null)
            entryScript.Setup(player.playerName, player.avatarSprite, player.netId, player.isHost);

        entries[player.netId] = entry;
    }

    private void RemovePlayerEntry(uint netId)
    {
        if (entries.TryGetValue(netId, out GameObject entry))
        {
            Destroy(entry);
            entries.Remove(netId);
        }
    }

    private void UpdatePlayerEntry(PlayerData player)
    {
        if (entries.TryGetValue(player.netId, out GameObject entry))
        {
            PlayerEntry entryScript = entry.GetComponent<PlayerEntry>();
            if (entryScript != null)
                entryScript.Setup(player.playerName, player.avatarSprite, player.netId, player.isHost);
        }
    }
}