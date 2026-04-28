using Mirror;
using UnityEngine;

public class PlayerPointsManager : NetworkBehaviour
{
    public static PlayerPointsManager Instance;

    public readonly SyncList<PlayerPointsData> allPlayers = new SyncList<PlayerPointsData>();

    private void Awake()
    {
        if (Instance == null)
            Instance = this;
        else
            Destroy(gameObject);
    }

    [Server]
    public void AddPlayer(uint netId, string playerName, Sprite avatarSprite, int playerPoints)
    {
        PlayerPointsData newPlayer = new PlayerPointsData(netId, playerName, avatarSprite, playerPoints);
        allPlayers.Add(newPlayer);
    }

    [Server]
    public void RemovePlayer(uint netId)
    {
        for (int i = 0; i < allPlayers.Count; i++)
        {
            if (allPlayers[i].netId == netId)
            {
                PlayerPointsData removed = allPlayers[i];
                allPlayers.RemoveAt(i);
                break;
            }
        }
    }
    [Server]
    public void AddPoint(uint netId, int value) 
    {
        for (int i = 0; i < allPlayers.Count; i++)
        {
            if (allPlayers[i].netId == netId)
            {
                PlayerPointsData playerPointsData = allPlayers[i];
                playerPointsData.playerPoints += value;
                break;
            }
        }
    }
    [Server]
    public void RemovePoint(uint netId, int value)
    {
        for (int i = 0; i < allPlayers.Count; i++)
        {
            if (allPlayers[i].netId == netId)
            {
                PlayerPointsData playerPointsData = allPlayers[i];
                playerPointsData.playerPoints -= value;
                break;
            }
        }
    }
}
