using System.Collections.Generic;
using UnityEngine;

public class InputState : MonoBehaviour
{
    public Dictionary<string, bool> moveDictionary = new Dictionary<string, bool>();
    public Dictionary<string, bool> cameraDictionary = new Dictionary<string, bool>();
    public Dictionary<string, bool> cursorDictionary = new Dictionary<string, bool>();
    public Dictionary<string, bool> menuDictionary = new Dictionary<string, bool>();

    private bool moveIsLocked = false;
    private bool cameraIsLocked = false;
    private bool cursorIsLocked = false;
    private bool menuIsLocked = false;

    public bool MoveIsLocked => moveIsLocked;
    public bool CameraIsLocked => cameraIsLocked;
    public bool CursorIsLocked => cursorIsLocked;
    public bool MenuIsLocked => menuIsLocked;
    private void UpdateLockState(Dictionary<string, bool> dictionary, out bool lockState)
    {
        lockState = false;

        foreach (var item in dictionary.Values)
        {
            if (item)
            {
                lockState = true;
                break;
            }
        }
    }
    private void UpdateMovementLockState()
    {
        UpdateLockState(moveDictionary, out moveIsLocked);
    }

    private void UpdateCameraLockState()
    {
        UpdateLockState(cameraDictionary, out cameraIsLocked);
    }

    private void UpdateCursorLockState()
    {
        UpdateLockState(cursorDictionary, out cursorIsLocked);
    }
    private void UpdateMenuLockState()
    {
        UpdateLockState(menuDictionary, out menuIsLocked);
    }

    public void AddLockMovement(string key)
    {
        moveDictionary[key] = true;
        UpdateMovementLockState();
    }

    public void RemoveLockMovement(string key)
    {
        moveDictionary.Remove(key);
        UpdateMovementLockState();
    }

    public void AddLockCamera(string key)
    {
        cameraDictionary[key] = true;
        UpdateCameraLockState();
    }
    public void AddLockMenu(string key)
    {
        menuDictionary[key] = true;
        UpdateMenuLockState();
    }
    public void RemoveLockMenu(string key)
    {
        menuDictionary.Remove(key);
        UpdateMenuLockState();
    }

    public void RemoveLockCamera(string key)
    {
        cameraDictionary.Remove(key);
        UpdateCameraLockState();
    }

    public void AddLockCursor(string key)
    {
        cursorDictionary[key] = true;
        UpdateCursorLockState();
    }

    public void RemoveLockCursor(string key)
    {
        cursorDictionary.Remove(key);
        UpdateCursorLockState();
    }
}
