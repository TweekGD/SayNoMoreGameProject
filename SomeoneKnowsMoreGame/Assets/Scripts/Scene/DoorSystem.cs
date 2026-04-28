using FMODUnity;
using Mirror;
using OutlineShader;
using System.Collections;
using UnityEngine;

public class DoorSystem : NetworkBehaviour, IInteractable
{
    [SerializeField] private Transform rootDoor;
    [SerializeField] private Transform soundPos;
    [SerializeField] private Vector3 startRot;
    [SerializeField] private Vector3 endRot;
    [SerializeField] private float rotSpeed = 1f;
    [SerializeField] private float knockDelay = 2f;
    [SerializeField] private EventReference doorOpenSound;
    [SerializeField] private EventReference doorCloseSound;
    [SerializeField] private EventReference doorKnockSound;
    [SerializeField] private EventReference doorLockSound;
    [SerializeField] private EventReference openLockedDoorSound;
    [SerializeField] private bool startOpened;
    [SerializeField] private bool startLocked;
    [SerializeField] private KeyBind[] keys;

    [SyncVar(hook = nameof(OnIsOpenChanged))]
    private bool _isOpen;
    [SyncVar(hook = nameof(OnDoorLocked))]
    private bool _isLocked;

    private Coroutine rotateDoorCoroutine;
    private Coroutine knockSoundCoroutine;

    private Outline outlineObject;
    private ShakeController shakeController;
    public bool IsOpen => _isOpen;
    public bool IsLocked => _isLocked;
    public readonly SyncList<uint> accessId = new SyncList<uint>();
    public KeyBind[] KeyBind { get => keys; set => keys = value; }
    private void Awake()
    {
        outlineObject = GetComponent<Outline>();
        shakeController = GetComponent<ShakeController>();

        if (outlineObject != null)
            outlineObject.enabled = false;

        accessId.Callback += OnAccessListChanged;
    }
    private void OnDestroy()
    {
        accessId.Callback -= OnAccessListChanged;
    }
    private void Start()
    {
        if (isServer)
        {
            _isOpen = startOpened;
            _isLocked = startLocked;

            rootDoor.localRotation = startOpened ? Quaternion.Euler(endRot) : Quaternion.Euler(startRot);
        }
    }
    public void Interact(GameObject player)
    {
        if (InputManager.Instance != null)
        {
            if (InputManager.Instance.InteractInput)
                CmdToggleDoor();

            if (InputManager.Instance.KnockInput)
                CmdKnockDoor();

            if (InputManager.Instance.DoorLockInput)
                CmdToggleLock();
        }
    }
    [Command(requiresAuthority = false)]
    private void CmdToggleDoor()
    {
        if (_isLocked) 
        {
            if (!openLockedDoorSound.IsNull)
            {
                RpcOpenLockedDoorSound();
                RpcShakeDoor();
            }

            return; 
        }

        if (rotateDoorCoroutine != null) return;

        _isOpen = !_isOpen;
    }
    [Command(requiresAuthority = false)]
    private void CmdKnockDoor()
    {
        if (knockSoundCoroutine != null) return;

        knockSoundCoroutine = StartCoroutine(ServerKnockDelay());
    }
    [Command(requiresAuthority = false)]
    private void CmdToggleLock(NetworkConnectionToClient sender = null)
    {
        if (sender == null || sender.identity == null) return;

        uint playerNetId = sender.identity.netId;

        if (!accessId.Contains(playerNetId)) return;

        _isLocked = !_isLocked;
    }
    [Server]
    public void ServerAddPlayerAccess(uint netId)
    {
        if (!accessId.Contains(netId))
            accessId.Add(netId);
    }

    [Server]
    public void ServerRemovePlayerAccess(uint netId)
    {
        accessId.Remove(netId);
    }
    private IEnumerator ServerKnockDelay()
    {
        RpcPlayKnockSound();
        RpcShakeDoor();

        yield return new WaitForSeconds(knockDelay);

        knockSoundCoroutine = null;
    }
    [ClientRpc]
    private void RpcPlayKnockSound()
    {
        if (!doorKnockSound.IsNull && soundPos != null)
        {
            AudioManager.Instance.PlayOneShot(doorKnockSound, soundPos.position);
        }
    }
    [ClientRpc]
    private void RpcOpenLockedDoorSound()
    {
        if (!doorKnockSound.IsNull && soundPos != null)
        {
            AudioManager.Instance.PlayOneShot(openLockedDoorSound, soundPos.position);
        }
    }
    [ClientRpc]
    private void RpcShakeDoor()
    {
        if (shakeController != null)
        {
            shakeController.Shake();
        }
    }
    private void OnIsOpenChanged(bool oldValue, bool newValue)
    {
        if (rotateDoorCoroutine != null)
        {
            StopCoroutine(rotateDoorCoroutine);
            rotateDoorCoroutine = null;
        }

        Vector3 targetRot = newValue ? endRot : startRot;
        rotateDoorCoroutine = StartCoroutine(RotateDoor(targetRot, newValue));
    }
    private void OnDoorLocked(bool oldValue, bool newValue)
    {
        if (!doorLockSound.IsNull && soundPos != null) 
        {
            AudioManager.Instance.PlayOneShot(doorLockSound, soundPos.position);
        }
    }
    private IEnumerator RotateDoor(Vector3 targetRotation, bool isOpen)
    {
        if (isOpen && !doorOpenSound.IsNull)
        {
            AudioManager.Instance.PlayOneShot(doorOpenSound, rootDoor.position);
        }

        Quaternion startRotation = rootDoor.localRotation;
        Quaternion targetRotQuat = Quaternion.Euler(targetRotation);
        float progress = 0f;

        while (progress < 1f)
        {
            progress = Mathf.Clamp01(progress + Time.deltaTime * rotSpeed);
            rootDoor.localRotation = Quaternion.Lerp(startRotation, targetRotQuat, progress);
            yield return null;
        }

        rootDoor.localRotation = targetRotQuat;

        if (!isOpen && !doorCloseSound.IsNull)
        {
            Vector3 playPos = soundPos != null ? soundPos.position : rootDoor.position;
            AudioManager.Instance.PlayOneShot(doorCloseSound, playPos);
        }

        rotateDoorCoroutine = null;
    }
    private void OnAccessListChanged(SyncList<uint>.Operation op, int index, uint oldItem, uint newItem)
    {
        UpdateOwnerOutline();
    }
    private void UpdateOwnerOutline()
    {
        if (NetworkClient.localPlayer == null) return;

        uint localNetId = NetworkClient.localPlayer.netId;
        bool isOwner = accessId.Contains(localNetId);

        outlineObject.OutlineMode = Outline.Mode.OutlineAll;
        outlineObject.enabled = isOwner;
    }
    public void OnInteractedStart(GameObject player)
    {
        if (outlineObject != null)
        {
            outlineObject.OutlineMode = Outline.Mode.OutlineVisible;
            outlineObject.enabled = true;
        }
    }
    public void OnInteractedEnd(GameObject player)
    {
        if (outlineObject != null)
        {
            outlineObject.OutlineMode = Outline.Mode.OutlineVisible;
            outlineObject.enabled = false;
        }
    }
}