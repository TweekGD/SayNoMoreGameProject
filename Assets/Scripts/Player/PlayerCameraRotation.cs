using Mirror;
using UnityEngine;

public class PlayerCameraRotation : NetworkBehaviour
{
    [SerializeField] private Transform cameraTransform;
    [SerializeField] private Transform bodyTransform;
    [SerializeField] private Camera playerCamera;
    [SerializeField] private float maxAngleY = 90f;
    [SerializeField] private float smoothTime = 0.05f;
    public static Camera PlayerCamera { get; private set; }

    private InputState inputState;
    private Vector2 cameraRotation;
    private Vector2 currentInput;
    private Vector2 smoothedInput;
    private Vector2 inputVelocity;

    private IInputManager inputManager;
    private ISettingsManager settingsManager;

    private void Awake()
    {
        inputManager = ServiceLocator.Get<IInputManager>();
        settingsManager = ServiceLocator.Get<ISettingsManager>();

        inputState = GetComponent<InputState>();
    }

    private void Start()
    {
        if (!isLocalPlayer) { playerCamera.gameObject.SetActive(false); }

        inputState?.AddLock(InputState.LockType.Cursor, "InitPlayer");
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
        if (inputManager == null) { return; }

        float sensitivityValue = settingsManager != null ? settingsManager.GetParametersValue<float>("Sensitivity") : 1f;

        if (inputState != null)
        {
            Cursor.lockState = inputState.IsLocked(InputState.LockType.Cursor) ? CursorLockMode.Locked : CursorLockMode.None;
            Cursor.visible = !inputState.IsLocked(InputState.LockType.Cursor);
        }

        if (!inputState.IsLocked(InputState.LockType.Camera))
        {
            currentInput = inputManager.GetInput<Vector2>("LookInput") * sensitivityValue;

            smoothedInput.x = Mathf.SmoothDamp(smoothedInput.x, currentInput.x, ref inputVelocity.x, smoothTime);
            smoothedInput.y = Mathf.SmoothDamp(smoothedInput.y, currentInput.y, ref inputVelocity.y, smoothTime);

            cameraRotation.x += smoothedInput.x;
            cameraRotation.y += smoothedInput.y;

            cameraRotation.y = Mathf.Clamp(cameraRotation.y, -maxAngleY, maxAngleY);
        }

        cameraTransform.localRotation = Quaternion.Euler(-cameraRotation.y, 0f, 0f);
        bodyTransform.rotation = Quaternion.Euler(0f, cameraRotation.x, 0f);
    }
}
