using System;
using UnityEngine;

[Serializable]
public struct PlayerPointsData
{
    public uint netId;
    public string playerName;
    public Sprite avatarSprite;
    public int playerPoints;

    public PlayerPointsData(uint id, string name, Sprite avatar, int points)
    {
        netId = id;
        playerName = name;
        avatarSprite = avatar;
        playerPoints = points;
    }
}