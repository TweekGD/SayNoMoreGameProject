using System;
using UnityEngine;
using Mirror;

namespace PlayerSystem
{
    public class PlayerController : NetworkBehaviour
    {
        [Header("Speed Settings")]
        [SerializeField] private float playerWalkSpeed = 2.5f;
        [SerializeField] private float playerRunSpeed = 5f;
        [SerializeField] private float playerCrouchSpeed = 1.25f;
        [SerializeField] private float smoothInputSpeed = 0.15f;
        [SerializeField] private float smoothChangeSpeedValue = 5f;

        [Header("Crouching Settings")]
        [SerializeField] private LayerMask crouchStandLayers;
        [SerializeField] private float crouchTransitionSpeed = 4f;
        [SerializeField] private float crouchHeight = 1f;

        [Header("Jump Settings")]
        [SerializeField] private float airControlFactor = 0.5f;
        [SerializeField] private float playerGravity = -29.8f;
        [SerializeField] private float jumpHeight = 0.8f;

        private float startHeight;
        private float targetHeight;
        private CharacterController characterController;
        private InputState inputState;
        private Vector3 playerVelocity;
        private Vector2 smoothInputVelocity;
        private Vector3 moveDirection;
        private bool wishCrouch;
        private float currentHeightVelocity;
        public Vector2 CurrentInputVector { get; private set; }
        public Vector2 CurrentInputVectorInt { get; private set; }
        public float CurrentMovementSpeed { get; private set; }
        public Vector2 MoveVector { get; private set; }
        public bool IsSprinting { get; private set; }
        public bool IsGrounded { get; private set; }
        public bool IsCrouching { get; private set; }

        public event Action OnPlayerJump;
        public event Action OnPlayerFallDown;
        private void Awake() 
        {
            characterController = GetComponent<CharacterController>();
            inputState = GetComponent<InputState>();
        }

        private void Start()
        {
            startHeight = characterController.height;
            targetHeight = startHeight;

            IntializeActionMap();
        }
        private void Update()
        {
            if (!isLocalPlayer) { return; } 

            GroundCheck();
            HandleMovementAndJump();
            HandleCrouch();
        }
        private void IntializeActionMap()
        {
            InputManager.Instance.ChangeActionMap("Player");
            inputState.AddLockCursor("InitPlayer");
        }

        private void GroundCheck()
        {
            bool wasGrounded = IsGrounded;
            IsGrounded = characterController.isGrounded;

            if (IsGrounded && !wasGrounded && playerVelocity.y < -5f)
                OnPlayerFallDown?.Invoke();
        }

        private void HandleMovementAndJump()
        {
            MoveVector = inputState.MoveIsLocked ? Vector2.zero : InputManager.Instance.MoveInput;

            IsSprinting = !inputState.MoveIsLocked &&
                        InputManager.Instance.SprintInput != 0 &&
                        MoveVector.y > 0f &&
                        !IsCrouching;

            CurrentInputVector = Vector2.SmoothDamp(CurrentInputVector, MoveVector, ref smoothInputVelocity, smoothInputSpeed);
            CurrentInputVectorInt = new Vector2(Mathf.Round(CurrentInputVector.x), Mathf.Round(CurrentInputVector.y));

            Vector3 rightMove = transform.right * CurrentInputVector.x;
            Vector3 forwardMove = transform.forward * CurrentInputVector.y;

            float targetSpeed = IsCrouching ? playerCrouchSpeed :
                IsSprinting ? playerRunSpeed : playerWalkSpeed;

            CurrentMovementSpeed = Mathf.Lerp(CurrentMovementSpeed, targetSpeed, Time.deltaTime * smoothChangeSpeedValue);

            Vector3 movementVector = Vector3.ClampMagnitude(rightMove + forwardMove, 1f) * CurrentMovementSpeed;

            if (IsGrounded)
                moveDirection = movementVector;
            else
                moveDirection = Vector3.Lerp(moveDirection, movementVector, airControlFactor * Time.deltaTime);

            if (IsGrounded)
            {
                if (playerVelocity.y < -2f) playerVelocity.y = -2f;

                if (!inputState.MoveIsLocked &&
                    InputManager.Instance.JumpInput &&
                    !IsCrouching)
                {
                    playerVelocity.y = Mathf.Sqrt(jumpHeight * -3f * playerGravity);
                    OnPlayerJump?.Invoke();
                }
            }

            playerVelocity.y += playerGravity * Time.deltaTime;

            if (characterController.collisionFlags == CollisionFlags.Above && playerVelocity.y > 0)
                playerVelocity.y = 0f;

            if (characterController.enabled)
                characterController.Move((moveDirection + playerVelocity) * Time.deltaTime);
        }

        private void HandleCrouch()
        {
            if (!inputState.MoveIsLocked && InputManager.Instance.CrouchInput)
                wishCrouch = !wishCrouch;

            float checkDistance = startHeight - crouchHeight;
            bool canStand = !Physics.SphereCast(transform.position, characterController.radius,
                Vector3.up, out _, checkDistance, crouchStandLayers, QueryTriggerInteraction.Ignore);

            if (wishCrouch)
            {
                targetHeight = crouchHeight;
                IsCrouching = true;
            }
            else if (canStand)
            {
                targetHeight = startHeight;
                IsCrouching = false;
            }

            characterController.height = Mathf.SmoothDamp(characterController.height, targetHeight,
                ref currentHeightVelocity, crouchTransitionSpeed * Time.deltaTime);
        }
    }
}