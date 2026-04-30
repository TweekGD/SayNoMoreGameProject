using Mirror;
using System.Collections;
using UnityEditor;
using UnityEngine;

public class RoundManager : NetworkBehaviour
{
    public static RoundManager Instance { get; private set; }

    [Header("Configuration")]
    [SerializeField] private RoundData[] rounds;

    [Header("Settings")]
    [SerializeField] private float spawnDelay = 0.2f;

    [SyncVar] private int currentRoundIndex = -1;
    [SyncVar] private float timeRemaining;
    [SyncVar] private bool isRunning;

    public float TimeRemaining => timeRemaining;
    public bool IsRunning => isRunning;

    private BaseMinigame activeMinigame;
    private Coroutine roundCoroutine;

    private bool panelFinished = false;

    private void Awake()
    {
        if (Instance != null && Instance != this) { Destroy(gameObject); return; }
        Instance = this;
        DontDestroyOnLoad(gameObject);
    }

    [Server]
    public void StartRounds()
    {
        if (roundCoroutine != null) StopCoroutine(roundCoroutine);
        roundCoroutine = StartCoroutine(RoundsCoroutine());
    }

    [Server]
    private IEnumerator RoundsCoroutine()
    {
        isRunning = true;
        for (int i = 0; i < rounds.Length; i++) yield return StartCoroutine(RunRound(i));
        isRunning = false;
        RpcOnAllRoundsFinished();
    }

    [Server]
    private IEnumerator RunRound(int index)
    {
        currentRoundIndex = index;
        RoundData data = rounds[index];
        timeRemaining = 0f;
        panelFinished = false;

        if (data.minigamePrefab != null)
        {
            GameObject go = Instantiate(data.minigamePrefab.gameObject);
            NetworkServer.Spawn(go);
            activeMinigame = go.GetComponent<BaseMinigame>();
            yield return new WaitForSeconds(spawnDelay);
            activeMinigame.OnRoundStart();
            activeMinigame.RpcOnRoundStart();
        }

        RpcOnRoundStarted(index, data.roundName, data.duration);

        yield return new WaitUntil(() => panelFinished);

        timeRemaining = data.duration;

        while (timeRemaining > 0f)
        {
            yield return null;
            float delta = Time.deltaTime;
            timeRemaining -= delta;
            activeMinigame?.OnRoundTick(delta);
        }

        if (activeMinigame != null)
        {
            activeMinigame.OnRoundEnd();
            activeMinigame.RpcOnRoundEnd();
            NetworkServer.Destroy(activeMinigame.gameObject);
            activeMinigame = null;
        }

        RpcOnRoundEnded(index);
    }

    [ClientRpc]
    private void RpcOnRoundStarted(int index, string roundName, float duration)
    {
        NetworkIdentity localPlayer = NetworkClient.localPlayer;
        if (localPlayer == null) return;

        PlayerComponentData playerComponentData = localPlayer.GetComponent<PlayerComponentData>();
        if (playerComponentData == null) return;

        RoundData data = rounds[index];
        string text = $"Round #{index}\r\n<size=180%><color=#FFAC00>\"{roundName}\"</color></size>\r\n{data.description}";

        playerComponentData.RoundDescriptionPanel.StartDescriptionPanel(text, () =>
        {
            CmdNotifyPanelFinished();
        });
    }

    [Command(requiresAuthority = false)]
    private void CmdNotifyPanelFinished()
    {
        panelFinished = true;
    }

    [ClientRpc] private void RpcOnRoundEnded(int index) { }
    [ClientRpc] private void RpcOnAllRoundsFinished() { }
}

#if UNITY_EDITOR
[CustomEditor(typeof(RoundManager))]
public class RoundManagerEditor : Editor
{
    public override void OnInspectorGUI()
    {
        DrawDefaultInspector();

        RoundManager script = (RoundManager)target;
        GUI.enabled = Application.isPlaying;

        if (GUILayout.Button("Start Rounds", GUILayout.Height(40)))
        {
            script.StartRounds();
        }

        if (!Application.isPlaying)
        {
            EditorGUILayout.HelpBox("Launch the game (play mode) for the button to become active.", MessageType.Info);
        }

        GUI.enabled = true;
    }
}
#endif