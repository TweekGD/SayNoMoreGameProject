using Mirror;
using System.Collections.Generic;
using UnityEngine;

public class PlayerKickManager : NetworkBehaviour, IPlayerKickManager
{
    private readonly HashSet<string> blacklist = new HashSet<string>();

    public override void OnStartServer()
    {
        NetworkServer.OnConnectedEvent += OnServerConnect;
    }

    public override void OnStopServer()
    {
        NetworkServer.OnConnectedEvent -= OnServerConnect;
    }

    private void OnServerConnect(NetworkConnectionToClient conn)
    {
        string address = conn.address;

        if (blacklist.Contains(address))
        {
            //RpcNotifyKicked(conn);
            StartCoroutine(DelayedDisconnect(conn));
        }
    }

    private System.Collections.IEnumerator DelayedDisconnect(NetworkConnectionToClient conn)
    {
        yield return null;
        conn.Disconnect();
    }

    [Server]
    public void KickPlayer(uint netId)
    {
        if (!NetworkServer.active) return;

        foreach (var conn in NetworkServer.connections.Values)
        {
            if (conn.identity != null && conn.identity.netId == netId)
            {
                string address = conn.address;
                blacklist.Add(address);
                Debug.Log($"[KickManager] Player kicked and blacklisted: netId={netId}, address={address}");

                //RpcNotifyKicked(conn);
                StartCoroutine(DelayedDisconnect(conn));
                break;
            }
        }
    }

    [Server]
    public void RemoveFromBlacklist(string address)
    {
        if (blacklist.Remove(address))
            Debug.Log($"[KickManager] Removed from blacklist: {address}");
    }

    [Server]
    public void ClearBlacklist()
    {
        blacklist.Clear();
        Debug.Log("[KickManager] Blacklist cleared");
    }

    public bool IsBlacklisted(string address) => blacklist.Contains(address);

    //[TargetRpc]
    //private void RpcNotifyKicked(NetworkConnection target)
    //{
    //    Debug.Log("[KickManager] You have been kicked and blacklisted by the host.");
    //    KickedNotification.Show();
    //}
}