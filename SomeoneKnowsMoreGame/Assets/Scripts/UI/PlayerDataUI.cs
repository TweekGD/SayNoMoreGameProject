using System;
using UnityEngine;

[Serializable]
public struct PlayerDataUI
{
    public uint netId;
    public string playerName;
    public Sprite avatarSprite;
    public bool isHost;
    public MirrorSteamworksVoice mirrorSteamworksVoice;

    public PlayerDataUI(uint id, string name, Sprite avatar, bool host, MirrorSteamworksVoice voice)
    {
        netId = id;
        playerName = name;
        avatarSprite = avatar;
        isHost = host;
        mirrorSteamworksVoice = voice;
    }
}