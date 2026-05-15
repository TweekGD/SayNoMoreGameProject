using Mirror;
using UnityEngine;

public class CustomNetworkManager : NetworkManager
{
    public override void OnClientDisconnect()
    {
        base.OnClientDisconnect();

        ReturnToMenu();
    }

    public override void OnStopHost()
    {
        base.OnStopHost();

        ReturnToMenu();
    }

    public override void OnStopClient()
    {
        base.OnStopClient();

        ReturnToMenu();
    }

    private void ReturnToMenu()
    {
        UnlockCursor();
    }

    private void UnlockCursor()
    {
        Cursor.lockState = CursorLockMode.None;
        Cursor.visible = true;
    }
}