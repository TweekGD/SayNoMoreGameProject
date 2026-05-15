using UnityEngine;

public class MenuCameraRotation : MonoBehaviour
{
    [SerializeField] private Transform cameraTransform;
    [SerializeField] private float sensitivityValue = 1f;
    [SerializeField] private float cameraAngle = 10f;
    [SerializeField] private float smoothSpeed = 5f;

    private Vector2 cameraRotation;
    private IInputManager inputManager;
    private void Awake()
    {
        inputManager = ServiceLocator.Get<IInputManager>();
    }
    private void Start()
    {
        Cursor.lockState = CursorLockMode.None;
        Cursor.visible = true;
    }
    private void Update()
    {
        CameraRotation();
    }
    private void CameraRotation()
    {
        cameraRotation.x += inputManager.GetInput<Vector2>("LookInput").x * sensitivityValue;
        cameraRotation.y += inputManager.GetInput<Vector2>("LookInput").y * sensitivityValue;

        cameraRotation.x = Mathf.Clamp(cameraRotation.x, -cameraAngle, cameraAngle);
        cameraRotation.y = Mathf.Clamp(cameraRotation.y, -cameraAngle, cameraAngle);

        Quaternion targetRotation = Quaternion.Euler(-cameraRotation.y, cameraRotation.x, 0f);
        cameraTransform.localRotation = Quaternion.Slerp(cameraTransform.localRotation, targetRotation, smoothSpeed * Time.deltaTime);
    }
}