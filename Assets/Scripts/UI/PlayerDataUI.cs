using System;
using UnityEngine;

[Serializable]
public struct PlayerData
{
    public uint netId;
    public string playerName;
    public Sprite avatarSprite;
    public bool isHost;
    public MirrorSteamworksVoice mirrorSteamworksVoice;

    public PlayerData(uint id, string name, Sprite avatar, bool host, MirrorSteamworksVoice voice)
    {
        netId = id;
        playerName = name;
        avatarSprite = avatar;
        isHost = host;
        mirrorSteamworksVoice = voice;
    }
}