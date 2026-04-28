using Mirror;
using UnityEngine;

public class PlayerCameraRotation : NetworkBehaviour
{
    [SerializeField] private Transform cameraTransform;
    [SerializeField] private Transform bodyTransform;
    [SerializeField] private Camera playerCamera;
    public static Camera PlayerCamera { get; private set; }
    private InputState inputState;
    private Vector2 cameraRotation;
    private void Awake()
    {
        inputState = GetComponent<InputState>();
    }
    private void Start()
    {
        if (!isLocalPlayer) { playerCamera.gameObject.SetActive(false); }
    }
    public override void OnStartLocalPlayer()
    {
        PlayerCamera = playerCamera;
    }
    private void OnDestroy()
    {
        if (isLocalPlayer) { PlayerCamera = null; }
    }
    private void Update()
    {
        if (!isLocalPlayer) { return; }

        CameraRotation();
    }
    private void CameraRotation()
    {
        float sensitivityValue = SettingsManager.Instance != null ? SettingsManager.Instance.Sensitivity : 1f;

        Cursor.lockState = inputState.CursorIsLocked ? CursorLockMode.Locked : CursorLockMode.None;
        Cursor.visible = !inputState.CursorIsLocked;

        if (!inputState.CameraIsLocked)
        {
            cameraRotation.x += InputManager.Instance.LookInput.x * sensitivityValue;
            cameraRotation.y += InputManager.Instance.LookInput.y * sensitivityValue;

            cameraRotation.y = Mathf.Clamp(cameraRotation.y, -90f, 90f);
        }

        bodyTransform.rotation = Quaternion.Euler(0f, cameraRotation.x, 0f);
        cameraTransform.localRotation = Quaternion.Euler(-cameraRotation.y, 0f, 0f);
    }
}
