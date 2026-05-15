using System;
using UnityEngine;

[Serializable]
public struct PlayerData
{
    public uint netId;
    public string playerName;
    public Sprite avatarSprite;
    public bool isHost;

    public PlayerData(uint id, string name, Sprite avatar, bool host)
    {
        netId = id;
        playerName = name;
        avatarSprite = avatar;
        isHost = host;
    }
}