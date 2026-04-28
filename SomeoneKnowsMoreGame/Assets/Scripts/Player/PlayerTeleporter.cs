using Mirror;
using UnityEngine;

public class PlayerTeleporter : NetworkBehaviour
{
    private CharacterController characterController;
    private void Awake()
    {
        characterController = GetComponent<CharacterController>();
    }
    public void TeleportPlayer(Vector3 newPosition)
    {
        CmdTeleportTo(newPosition);
    }
    [Command]
    private void CmdTeleportTo(Vector3 newPosition)
    {
        RpcTeleportTo(newPosition);
    }
    [TargetRpc]
    private void RpcTeleportTo(Vector3 newPosition)
    {
        characterController.enabled = false;
        transform.position = newPosition;
        characterController.enabled = true;

        Debug.Log($"Player: {netId} was teleported");
    }
}
