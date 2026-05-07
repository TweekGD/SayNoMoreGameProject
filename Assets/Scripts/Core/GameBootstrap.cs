using UnityEngine;

[DefaultExecutionOrder(-99999)]
public class GameBootstrapper : MonoBehaviour
{
    [SerializeField] private AudioManager audioManager;
    [SerializeField] private SettingsManager settingsManager;
    [SerializeField] private InputManager inputManager;
    [SerializeField] private SteamLobby steamLobby;
    [SerializeField] private PlayerKickManager playerKickManager;

    private void Awake()
    {
        ServiceLocator.Register<IAudioManager>(audioManager);
        ServiceLocator.Register<ISettingsManager>(settingsManager);
        ServiceLocator.Register<IInputManager>(inputManager);
        ServiceLocator.Register<ISteamLobby>(steamLobby);
        ServiceLocator.Register<IPlayerKickManager>(playerKickManager);

        DontDestroyOnLoad(gameObject);
    }

    private void OnDestroy()
    {
        ServiceLocator.Clear();
    }
}