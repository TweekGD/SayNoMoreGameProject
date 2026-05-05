using Mirror;
using UnityEngine;

public class PlayerAnimationSystem : NetworkBehaviour
{
    private Animator playerAnimator;
    private void Awake()
    {
        playerAnimator = GetComponent<Animator>();
    }
}
