using Mirror;
using UnityEngine;

public class PlayerCameraRotation : NetworkBehaviour
{
    [SerializeField] private Transform cameraTransform;
    [SerializeField] private Transform bodyTransform;
    [SerializeField] private Camera playerCamera;
    [SerializeField] private float maxAngleX = 45f;
    [SerializeField] private float maxAngleY = 90f;
    [SerializeField] private float smoothTime = 0.05f;

    public static Camera PlayerCamera { get; private set; }

    private InputState inputState;
    private Vector2 cameraRotation;
    private Vector2 currentInput;
    private Vector2 smoothedInput;
    private Vector2 inputVelocity;

    private void Awake()
    {
        inputState = GetComponent<InputState>();
    }

    private void Start()
    {
        if (!isLocalPlayer) { playerCamera.gameObject.SetActive(false); }

        if (inputState != null) { inputState.AddLockCursor("InitPlayer"); }
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

        if (inputState != null)
        {
            Cursor.lockState = inputState.CursorIsLocked ? CursorLockMode.Locked : CursorLockMode.None;
            Cursor.visible = !inputState.CursorIsLocked;
        }

        if (!inputState.CameraIsLocked)
        {
            currentInput = InputManager.Instance.LookInput * sensitivityValue;

            smoothedInput.x = Mathf.SmoothDamp(smoothedInput.x, currentInput.x, ref inputVelocity.x, smoothTime);
            smoothedInput.y = Mathf.SmoothDamp(smoothedInput.y, currentInput.y, ref inputVelocity.y, smoothTime);

            cameraRotation.x += smoothedInput.x;
            cameraRotation.y += smoothedInput.y;

            cameraRotation.x = Mathf.Clamp(cameraRotation.x, -maxAngleX, maxAngleX);
            cameraRotation.y = Mathf.Clamp(cameraRotation.y, -maxAngleY, maxAngleY);
        }

        cameraTransform.localRotation = Quaternion.Euler(-cameraRotation.y, cameraRotation.x, 0f);
    }
}
