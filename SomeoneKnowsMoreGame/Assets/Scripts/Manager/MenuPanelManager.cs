using DG.Tweening;
using UnityEngine;

public class MenuPanelManager : MonoBehaviour
{
    [SerializeField] private RectTransform lobbyPanel;
    [SerializeField] private RectTransform settingsPanel;
    [Header("Animation Settings")]
    [SerializeField] private Vector2 openPos;
    [SerializeField] private Vector2 closePos;
    [SerializeField] private float duration = 1f;

    private bool settingsPanelIsActive;
    private bool lobbyPanelIsActive;

    private Tween settingsPanelTween;
    private Tween lobbyPanelTween;
    private void Start()
    {
        lobbyPanel.anchoredPosition = closePos;
        settingsPanel.anchoredPosition = closePos;

        lobbyPanel.gameObject.SetActive(false);
        settingsPanel.gameObject.SetActive(false);
    }
    public void ToggleSettingsPanel()
    {
        bool wasLobbyActive = lobbyPanelIsActive;

        if (wasLobbyActive) LobbyPanelAnimation(false);

        SettingsPanelAnimation(!settingsPanelIsActive);
    }
    public void ToggleLobbyPanel()
    {
        bool wasSettingsActive = settingsPanelIsActive;

        if (wasSettingsActive) SettingsPanelAnimation(false);

        LobbyPanelAnimation(!lobbyPanelIsActive);
    }
    private void SettingsPanelAnimation(bool isOpen)
    {
        settingsPanelTween?.Kill();
        settingsPanelIsActive = isOpen;

        if (isOpen)
        {
            if (!settingsPanel.gameObject.activeSelf)
            {
                settingsPanel.anchoredPosition = closePos;
                settingsPanel.gameObject.SetActive(true);
            }
            settingsPanelTween = settingsPanel.DOAnchorPos(openPos, duration).SetEase(Ease.OutSine);
        }
        else
        {
            settingsPanelTween = settingsPanel.DOAnchorPos(closePos, duration).SetEase(Ease.OutSine)
                .OnComplete(() => settingsPanel.gameObject.SetActive(false));
        }
    }
    private void LobbyPanelAnimation(bool isOpen)
    {
        lobbyPanelTween?.Kill();
        lobbyPanelIsActive = isOpen;

        if (isOpen)
        {
            if (!lobbyPanel.gameObject.activeSelf)
            {
                lobbyPanel.anchoredPosition = closePos;
                lobbyPanel.gameObject.SetActive(true);
            }
            lobbyPanelTween = lobbyPanel.DOAnchorPos(openPos, duration).SetEase(Ease.OutSine);
        }
        else
        {
            lobbyPanelTween = lobbyPanel.DOAnchorPos(closePos, duration).SetEase(Ease.OutSine)
                .OnComplete(() => lobbyPanel.gameObject.SetActive(false));
        }
    }
    public void GameQuit()
    {
        Application.Quit();
    }
    private void OnDestroy()
    {
        lobbyPanelTween?.Kill();
        settingsPanelTween?.Kill();
    }
    private void OnDisable()
    {
        lobbyPanelTween?.Kill();
        settingsPanelTween?.Kill();
    }
}