using UnityEngine;

public class PlayerComponentData : MonoBehaviour
{
    [SerializeField] private RoundDescriptionPanel roundDescriptionPanel;
    [SerializeField] private TimerPanel timerPanel;
    [SerializeField] private PlayerTeleporter playerTeleporter;
    [SerializeField] private PlayerInfo playerInfo;
    public RoundDescriptionPanel RoundDescriptionPanel => roundDescriptionPanel;
    public TimerPanel TimerPanel => timerPanel;
    public PlayerTeleporter PlayerTeleporter => playerTeleporter;
    public PlayerInfo PlayerInfo => playerInfo;
}
