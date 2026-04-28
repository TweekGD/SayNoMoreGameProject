using Mirror;
using UnityEngine;

public abstract class BaseMinigame : NetworkBehaviour
{
    [Server]
    public virtual void OnRoundStart()
    {
        Debug.Log($"[BaseMinigame] OnRoundStart: {gameObject.name}");
    }

    [Server]
    public virtual void OnRoundEnd()
    {
        Debug.Log($"[BaseMinigame] OnRoundEnd: {gameObject.name}");
    }

    [Server]
    public virtual void OnRoundTick(float deltaTime) { }

    [ClientRpc]
    public virtual void RpcOnRoundStart() { }

    [ClientRpc]
    public virtual void RpcOnRoundEnd() { }
}
