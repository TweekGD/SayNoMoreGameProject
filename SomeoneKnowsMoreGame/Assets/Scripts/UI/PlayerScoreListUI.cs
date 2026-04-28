using Mirror;
using System.Collections.Generic;
using UnityEngine;

public class PlayerScoreListUI : NetworkBehaviour
{
    [Header("UI Elements")]
    [SerializeField] private Transform contentPanel;
    [SerializeField] private GameObject playerScorePrefab;

    private Dictionary<uint, GameObject> entries = new Dictionary<uint, GameObject>();

    private void Start()
    {
        if (PlayerPointsManager.Instance != null)
            SubscribeToSyncList();
    }

    private void SubscribeToSyncList()
    {
        PlayerPointsManager.Instance.allPlayers.Callback += OnPlayersChanged;

        foreach (var player in PlayerPointsManager.Instance.allPlayers)
            AddPlayerEntry(player);
    }

    private void OnPlayersChanged(SyncList<PlayerPointsData>.Operation op, int index, PlayerPointsData oldItem, PlayerPointsData newItem)
    {
        switch (op)
        {
            case SyncList<PlayerPointsData>.Operation.OP_ADD:
                AddPlayerEntry(newItem);
                break;
            case SyncList<PlayerPointsData>.Operation.OP_REMOVEAT:
                RemovePlayerEntry(oldItem.netId);
                break;
            case SyncList<PlayerPointsData>.Operation.OP_SET:
                UpdatePlayerEntry(newItem);
                break;
        }
    }

    private void AddPlayerEntry(PlayerPointsData player)
    {
        if (entries.ContainsKey(player.netId)) return;

        GameObject entry = Instantiate(playerScorePrefab, contentPanel);

        PlayerScore entryScript = entry.GetComponent<PlayerScore>();
        if (entryScript != null)
            entryScript.Setup(player.playerName, player.avatarSprite, player.netId, player.playerPoints);

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

    private void UpdatePlayerEntry(PlayerPointsData player)
    {
        if (entries.TryGetValue(player.netId, out GameObject entry))
        {
            PlayerScore entryScript = entry.GetComponent<PlayerScore>();
            if (entryScript != null)
                entryScript.Setup(player.playerName, player.avatarSprite, player.netId, player.playerPoints);
        }
    }

    private void OnDestroy()
    {
        if (PlayerPointsManager.Instance != null)
            PlayerPointsManager.Instance.allPlayers.Callback -= OnPlayersChanged;
    }
}