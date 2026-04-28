using Mirror;
using OutlineShader;
using System.Collections.Generic;
using UnityEngine;

public class ToggleFloorLamp : NetworkBehaviour, IInteractable
{
    [SerializeField] private GameObject lampLight;
    [SerializeField] private int materialNumber = 1;
    [SerializeField] private KeyBind[] keyBind;
    [SyncVar(hook = nameof(ActiveLamp))] private bool isActive = true;

    private MeshRenderer meshRenderer;
    private Material material;
    private Color emissiveColor;
    private Outline outlineObject;
    public KeyBind[] KeyBind { get => keyBind; set => keyBind = value; }

    private void Awake()
    {
        outlineObject = GetComponent<Outline>();
        meshRenderer = GetComponent<MeshRenderer>();

        if (outlineObject != null)
            outlineObject.enabled = false;

        if (meshRenderer != null)
        {
            List<Material> materials = new List<Material>();
            meshRenderer.GetMaterials(materials);
            material = materials[materialNumber];
            emissiveColor = material.GetColor("_EmissiveColor");

            if (emissiveColor.r == 0f && emissiveColor.g == 0f && emissiveColor.b == 0f
                && material.HasProperty("_UseEmissiveIntensity")
                && material.GetFloat("_UseEmissiveIntensity") == 1f)
            {
                Color ldr = material.GetColor("_EmissiveColorLDR");
                float intensity = material.GetFloat("_EmissiveIntensity");
                emissiveColor = new Color(
                    Mathf.GammaToLinearSpace(ldr.r),
                    Mathf.GammaToLinearSpace(ldr.g),
                    Mathf.GammaToLinearSpace(ldr.b)) * intensity;
            }
        }
    }

    private void Start()
    {
        ApplyLampState(isActive);
    }

    public void Interact(GameObject player)
    {
        if (InputManager.Instance != null && InputManager.Instance.InteractInput)
        {
            CmdToggleLamp();
        }
    }

    [Command(requiresAuthority = false)]
    private void CmdToggleLamp()
    {
        isActive = !isActive;
    }

    private void ActiveLamp(bool oldValue, bool newValue)
    {
        ApplyLampState(newValue);
    }

    private void ApplyLampState(bool state)
    {
        if (lampLight != null)
            lampLight.SetActive(state);

        if (material != null)
            material.SetColor("_EmissiveColor", state ? emissiveColor : Color.black);
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