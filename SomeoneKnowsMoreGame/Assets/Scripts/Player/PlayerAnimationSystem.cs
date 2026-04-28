using Mirror;
using PlayerSystem;
using UnityEngine;

public class PlayerAnimationSystem : NetworkBehaviour
{
    [SerializeField] private PlayerController playerController;
    [SerializeField] private float sprintSmoothSpeed = 1f;

    private Animator playerAnimator;
    private float sprintMultiplyer;
    private void Awake()
    {
        playerAnimator = GetComponent<Animator>();
    }
    private void Update()
    {
        if (!isLocalPlayer) { return; }

        float moveX = playerController.CurrentInputVector.x;
        float moveY = playerController.CurrentInputVector.y;

        float targetSpeed = playerController.IsSprinting ? 2f : 1f;
        sprintMultiplyer = Mathf.Lerp(sprintMultiplyer, targetSpeed, Time.deltaTime * sprintSmoothSpeed);

        playerAnimator.SetFloat("MoveX", moveX);
        playerAnimator.SetFloat("MoveY", moveY * sprintMultiplyer);

        playerAnimator.SetBool("Floating", !playerController.IsGrounded);

        playerAnimator.SetBool("Crouching", playerController.IsCrouching);
    }
}
