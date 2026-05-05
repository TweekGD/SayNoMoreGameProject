using Mirror;
using System.Collections.Generic;
using UnityEngine;

public class UIPlayerList : NetworkBehaviour
{
    [Header("UI Elements")]
    [SerializeField] private Transform contentPanel;
    [SerializeField] private GameObject playerEntryPrefab;

    private Dictionary<uint, GameObject> entries = new Dictionary<uint, GameObject>();

    private void Start()
    {
        if (PlayerListManager.Instance != null)
            SubscribeToSyncList();
    }

    private void SubscribeToSyncList()
    {
        PlayerListManager.Instance.allPlayers.Callback += OnPlayersChanged;

        foreach (var player in PlayerListManager.Instance.allPlayers)
            AddPlayerEntry(player);
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
            entryScript.Setup(player.playerName, player.avatarSprite, player.netId, player.mirrorSteamworksVoice, player.isHost);

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
                entryScript.Setup(player.playerName, player.avatarSprite, player.netId, player.mirrorSteamworksVoice, player.isHost);
        }
    }

    private void OnDestroy()
    {
        if (PlayerListManager.Instance != null)
            PlayerListManager.Instance.allPlayers.Callback -= OnPlayersChanged;
    }
}