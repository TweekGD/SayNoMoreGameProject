using Mirror;
using System;
using UnityEngine;

public class PlayerInteract : NetworkBehaviour
{
    [SerializeField] private GameObject playerObject;
    [SerializeField] private Transform castTransform;
    [SerializeField] private InputState inputState;
    [SerializeField] private float maxDistance;
    [SerializeField] private Vector3 direction = Vector3.forward;
    [SerializeField] private LayerMask layer;
    public bool OnInteractebleCollider { get; private set; }
    public IInteractable LastTarget { get; private set; }
    public event Action<bool, KeyBind[]> OnInteracted;

    private void Update()
    {
        if (!isLocalPlayer) { return; }

        CheckForInteractable();

        if (inputState.CameraIsLocked || !inputState.CursorIsLocked) { return; }

        LastTarget?.Interact(playerObject);
    }
    private void CheckForInteractable()
    {
        bool hitDetected = Physics.Raycast(
            castTransform.position,
            castTransform.TransformDirection(direction),
            out RaycastHit hit,
            maxDistance,
            layer,
            QueryTriggerInteraction.Ignore
        );

        IInteractable newTarget = null;
        if (hitDetected && hit.collider != null)
        {
            hit.collider.TryGetComponent(out newTarget);
        }

        UpdateInteractionState(newTarget);
    }
    private void UpdateInteractionState(IInteractable newTarget)
    {
        if (LastTarget != newTarget)
        {
            if (LastTarget != null)
                LastTarget.OnInteractedEnd(playerObject);

            LastTarget = newTarget;

            if (LastTarget != null)
                LastTarget.OnInteractedStart(playerObject);
        }

        bool isInteracting = newTarget != null;

        KeyBind[] targetKeyBind = null;

        if (newTarget != null)
        {
            targetKeyBind = newTarget.KeyBind;
        }

        if (isInteracting != OnInteractebleCollider)
        {
            OnInteractebleCollider = isInteracting;
            OnInteracted?.Invoke(OnInteractebleCollider, targetKeyBind);
        }
    }
}
[Serializable]
public class KeyBind
{
    public string keyBind;
    public string keyDescription;
}
public interface IInteractable
{
    void Interact(GameObject player = null);
    void OnInteractedStart(GameObject player = null);
    void OnInteractedEnd(GameObject player = null);
    public KeyBind[] KeyBind { get; set; }
}