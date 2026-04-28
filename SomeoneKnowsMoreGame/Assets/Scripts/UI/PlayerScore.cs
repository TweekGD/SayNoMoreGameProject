using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class PlayerScore : MonoBehaviour
{
    [SerializeField] private TextMeshProUGUI playerNameText;
    [SerializeField] private Image avatarImage;
    [SerializeField] private TextMeshProUGUI scoreText;

    private uint _netID;
    public uint NetID => _netID;

    public void Setup(string playerName, Sprite avatarSprite, uint netID, int scoreValue)
    {
        if (playerName == null || avatarSprite == null) { return; }

        playerNameText.text = playerName;
        avatarImage.sprite = avatarSprite;
        scoreText.text = $"{scoreValue}";

        _netID = netID;
    }
}