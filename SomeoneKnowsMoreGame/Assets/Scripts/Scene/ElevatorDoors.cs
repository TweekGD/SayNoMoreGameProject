using System.Collections;
using UnityEngine;
using Mirror;
using FMODUnity;

public class ElevatorDoors : NetworkBehaviour
{
    [SerializeField] private Transform leftDoor;
    [SerializeField] private Transform rightDoor;
    [SerializeField] private Vector3 leftClosedPos;
    [SerializeField] private Vector3 rightClosedPos;
    [SerializeField] private Vector3 leftOpenPos;
    [SerializeField] private Vector3 rightOpenPos;
    [SerializeField] private float animationDelay = 2f;
    [SerializeField] private float openDelay = 2f;
    [SerializeField] private float moveSpeed = 2f;
    [SerializeField] private EventReference elevatorSound;

    [SyncVar(hook = nameof(OnIsOpenChanged))]
    private bool isOpen = false;
    private bool isMoving = false;

    private void Awake()
    {
        if (leftDoor != null) leftDoor.localPosition = leftClosedPos;
        if (rightDoor != null) rightDoor.localPosition = rightClosedPos;
    }

    public override void OnStartServer()
    {
        StartCoroutine(ServerDelayedOpen());
    }

    private IEnumerator ServerDelayedOpen()
    {
        yield return null;
        if (!isOpen)
        {
            isOpen = true;
        }
    }

    public override void OnStartClient()
    {
        if (isOpen && !isMoving)
        {
            if (leftDoor != null) leftDoor.localPosition = leftOpenPos;
            if (rightDoor != null) rightDoor.localPosition = rightOpenPos;
        }
    }

    private void OnIsOpenChanged(bool oldValue, bool newValue)
    {
        if (newValue && !isMoving)
        {
            StartCoroutine(AnimateOpen());
        }
    }

    [Command(requiresAuthority = false)]
    public void CmdOpenDoors()
    {
        if (isOpen) return;
        isOpen = true;
    }

    private IEnumerator AnimateOpen()
    {
        isMoving = true;

        yield return new WaitForSeconds(animationDelay);

        if (!elevatorSound.IsNull)
        {
            AudioManager.Instance.PlayOneShot(elevatorSound, transform.position);
        }

        yield return new WaitForSeconds(openDelay);

        Vector3 startLeft = leftDoor.localPosition;
        Vector3 startRight = rightDoor.localPosition;

        float distanceLeft = Vector3.Distance(leftClosedPos, leftOpenPos);
        float distanceRight = Vector3.Distance(rightClosedPos, rightOpenPos);
        float duration = Mathf.Max(distanceLeft, distanceRight) / moveSpeed;
        if (duration <= 0f) duration = 0.5f;

        float elapsed = 0f;
        while (elapsed < duration)
        {
            elapsed += Time.deltaTime;
            float t = Mathf.Clamp01(elapsed / duration);

            if (leftDoor != null) leftDoor.localPosition = Vector3.Lerp(startLeft, leftOpenPos, t);
            if (rightDoor != null) rightDoor.localPosition = Vector3.Lerp(startRight, rightOpenPos, t);

            yield return null;
        }

        if (leftDoor != null) leftDoor.localPosition = leftOpenPos;
        if (rightDoor != null) rightDoor.localPosition = rightOpenPos;

        isMoving = false;
    }
}