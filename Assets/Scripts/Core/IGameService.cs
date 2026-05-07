using FMODUnity;
using System;
using UnityEngine;

public interface IGameService { }

public interface IAudioManager : IGameService
{
    public void InitializeAmbience(EventReference ambienceEventReference);
    public void InitializeMusic(EventReference musicEventReference);
    public void SetAmbienceParameter(string parameterName, float parameterValue, bool ignoreSpeed = false);
    public void PlayOneShot(EventReference sound, Vector3 worldPos);
}

public interface ISettingsManager : IGameService
{
    public event Action OnParametersChanged;
    public T GetParameter<T>(string name) where T : class;
    public T GetParametersValue<T>(string name);
}

public interface IInputManager : IGameService
{
    public T GetInput<T>(string inputName);
}

public interface ISteamLobby : IGameService
{
    public void HostLobby();
    public void LeaveLobby();
    public void OpenSteamInvite();
}

public interface IPlayerKickManager : IGameService
{
    public void KickPlayer(uint netId);
}