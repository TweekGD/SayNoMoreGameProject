using UnityEngine;

public class InteractivePanelUI : MonoBehaviour
{
    [SerializeField] private PlayerInteract playerInteract;
    [SerializeField] private float fadeSpeed = 1f;
    [SerializeField] private GameObject keyBindPrefab;
    [SerializeField] private Transform keyBindParent;
    [SerializeField] private CanvasGroup canvasGroup;
    public bool InteractivePanelIsActive { get; private set; }
    private void Awake()
    {
        playerInteract.OnInteracted += UpdateKeyBindInfo;
    }
    private void OnDisable()
    {
        playerInteract.OnInteracted -= UpdateKeyBindInfo;
    }
    private void Update()
    {
        ToggleInteractivePanel(playerInteract.OnInteractebleCollider);
    }
    public void ToggleInteractivePanel(bool active)
    {
        InteractivePanelIsActive = active;

        canvasGroup.alpha += InteractivePanelIsActive ? Time.deltaTime * fadeSpeed : -Time.deltaTime * fadeSpeed;
    }
    public void UpdateKeyBindInfo(bool value, KeyBind[] keyBinds)
    {
        if (!value) { return; }

        DestroyOldKeyBind();

        if (keyBinds == null || keyBindParent == null) { return; }

        CreateNewBindInfo(keyBinds);
    }
    private void CreateNewBindInfo(KeyBind[] keyBinds)
    {
        foreach (KeyBind keyBind in keyBinds)
        {
            GameObject newKeyBind = Instantiate(keyBindPrefab, keyBindParent);
            newKeyBind.GetComponent<KeyBindData>().UpdateBindInfo(keyBind.keyBind, keyBind.keyDescription);
        }
    }
    private void DestroyOldKeyBind()
    {
        for (int i = 0; i < keyBindParent.childCount; i++)
        {
            Destroy(keyBindParent.GetChild(i).gameObject);
        }
    }
}
