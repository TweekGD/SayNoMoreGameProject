using UnityEngine;

[CreateAssetMenu(fileName = "RoundData", menuName = "RoundSystem/RoundData")]
public class RoundData : ScriptableObject
{
    public string roundName;
    public float duration;
    public BaseMinigame minigamePrefab;
    [TextArea()] public string description;
}