using TMPro;
using UnityEngine;

public class KeyBindData : MonoBehaviour
{
    [SerializeField] private TextMeshProUGUI keyBindText;
    public void UpdateBindInfo(string bind,string newText)
    {
        keyBindText.text = $"<color=#FFAC00><size=120%>{bind}</size></color> - {newText}";
    }
}
