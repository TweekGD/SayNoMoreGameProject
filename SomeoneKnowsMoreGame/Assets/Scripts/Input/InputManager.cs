using UnityEngine;
using UnityEngine.InputSystem;

public class InputManager : MonoBehaviour
{
    public InputActionAsset playerControls;

    #region Player Action
    private InputAction moveAction;
    private InputAction lookAction;
    private InputAction jumpAction;
    private InputAction sprintAction;
    private InputAction interactAction;
    private InputAction crouchAction;
    private InputAction fireAction;
    private InputAction dropItemAction;
    private InputAction pauseAction;
    private InputAction voiceChatAction;
    private InputAction knockAction;
    private InputAction doorLockAction;
    #endregion

    #region Player Input
    public Vector2 MoveInput {  get; private set; }
    public Vector2 LookInput { get; private set; }
    public bool JumpInput { get; private set; }
    public float SprintInput { get; private set; }
    public bool InteractInput { get; private set; }
    public bool CrouchInput { get; private set; }
    public float FireInput { get; private set; }
    public bool DropItemInput { get; private set; }
    public bool PauseInput { get; private set; }
    public float VoiceChatInput { get; private set; }
    public bool KnockInput { get; private set; }
    public bool DoorLockInput { get; private set; }
    #endregion

    public static InputManager Instance { get; private set; }

    private void Awake()
    {
        if (Instance == null) 
        {
            Instance = this;
            DontDestroyOnLoad(gameObject);
        }
        else
        {
            Destroy(gameObject);
        }

        moveAction = playerControls.FindActionMap("Player").FindAction("Move");
        lookAction = playerControls.FindActionMap("Player").FindAction("Look");
        jumpAction = playerControls.FindActionMap("Player").FindAction("Jump");
        sprintAction = playerControls.FindActionMap("Player").FindAction("Sprint");
        interactAction = playerControls.FindActionMap("Player").FindAction("Interact");
        crouchAction = playerControls.FindActionMap("Player").FindAction("Crouch");
        fireAction = playerControls.FindActionMap("Player").FindAction("Fire");
        dropItemAction = playerControls.FindActionMap("Player").FindAction("Drop Current Item");
        pauseAction = playerControls.FindActionMap("Player").FindAction("Pause");
        voiceChatAction = playerControls.FindActionMap("Player").FindAction("Voice Chat");
        knockAction = playerControls.FindActionMap("Player").FindAction("Knock");
        doorLockAction = playerControls.FindActionMap("Player").FindAction("Lock Door");

        moveAction.Enable();
        lookAction.Enable();
        jumpAction.Enable();
        sprintAction.Enable();
        interactAction.Enable();
        crouchAction.Enable();
        fireAction.Enable();
        dropItemAction.Enable();
        pauseAction.Enable();
        voiceChatAction.Enable();
        knockAction.Enable();
        doorLockAction.Enable();

        RegisterPlayerInputAction();
    }
    private void OnDestroy()
    {
        moveAction.Disable();
        lookAction.Disable();
        jumpAction.Disable();
        sprintAction.Disable();
        interactAction.Disable();
        crouchAction.Disable();
        fireAction.Disable();
        dropItemAction.Disable();
        pauseAction.Disable();
        voiceChatAction.Disable();
        knockAction.Disable();
        doorLockAction.Disable();
    }
    private void Update()
    {
        UpdatePlayerInput();
    }
    private void UpdatePlayerInput() 
    {
        JumpInput = jumpAction.triggered;
        InteractInput = interactAction.triggered;
        CrouchInput = crouchAction.triggered;
        DropItemInput = dropItemAction.triggered;
        PauseInput = pauseAction.triggered;
        KnockInput = knockAction.triggered;
        DoorLockInput = doorLockAction.triggered;
    }
    private void RegisterPlayerInputAction() 
    {
        moveAction.performed += context => MoveInput = context.ReadValue<Vector2>();
        moveAction.canceled += context => MoveInput = Vector2.zero;

        lookAction.performed += context => LookInput = context.ReadValue<Vector2>();
        lookAction.canceled += context => LookInput = Vector2.zero;

        sprintAction.performed += context => SprintInput = context.ReadValue<float>();
        sprintAction.canceled += context => SprintInput = 0f;

        fireAction.performed += context => FireInput = context.ReadValue<float>();
        fireAction.canceled += context => FireInput = 0f;

        voiceChatAction.performed += context => VoiceChatInput = context.ReadValue<float>();
        voiceChatAction.canceled += context => VoiceChatInput = 0f;
    }
    public void ChangeActionMap(string actionMap)
    {
        foreach (var playerActionMap in playerControls.actionMaps)
        {
            if (playerActionMap.name == "UI") { return; }

            if (playerActionMap.name == actionMap)
            {
                playerControls.FindActionMap(playerActionMap.name).Enable();
            }
            else
            {
                playerControls.FindActionMap(playerActionMap.name).Disable();
            }
        }
    }
}
