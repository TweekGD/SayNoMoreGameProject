using DG.Tweening;
using Mirror;
using Steamworks;
using System;
using UnityEngine;
using UnityEngine.UI;

public class GameMenuPanelManager : NetworkBehaviour
{
    [SerializeField] private RectTransform interfacePanel;
    [SerializeField] private RectTransform menuPanel;
    [SerializeField] private RectTransform menuButtonsPanel;
    [SerializeField] private RectTransform settingsPanel;
    [SerializeField] private RectTransform playerList;
    [SerializeField] private InputState inputState;
    [SerializeField] private Button inviteFriendsButton;
    [SerializeField] private Button leaveLobbyButton;
    [SerializeField] private CanvasGroup menuCanvasGroup;
    [Header("Animation Parameters")]
    [SerializeField] private Vector2 openPos;
    [SerializeField] private Vector2 closePos;
    [SerializeField] private float duration = 1f;

    private Tween menuButtonsPanelTween;
    private Tween settingsPanelTween;
    private Tween playerListPanelTween;
    private Tween menuFadeTween;

    private bool menuButtonsPanelIsActive;
    private bool menuPanelIsActive;
    private bool settingsPanelIsActive;
    private bool playerListIsActive;

    public bool MenuButtonsPanelIsActive => menuButtonsPanelIsActive;
    public bool MenuPanelIsActive => menuPanelIsActive;
    public bool SettingsPanelIsActive => settingsPanelIsActive;
    public bool PlayerListIsActive => playerListIsActive;

    private IInputManager inputManager;
    private ISteamLobby steamLobby;

    private void Awake()
    {
        inputManager = ServiceLocator.Get<IInputManager>();
        steamLobby = ServiceLocator.Get<ISteamLobby>();
    }
    private void Start()
    {
        if (!isLocalPlayer)
        {
            gameObject.SetActive(false);
            return;
        }

        menuCanvasGroup.alpha = 0f;

        interfacePanel.gameObject.SetActive(true);
        menuPanel.gameObject.SetActive(false);
        playerList.gameObject.SetActive(false);
        settingsPanel.gameObject.SetActive(false);
        menuButtonsPanel.gameObject.SetActive(false);

        playerList.anchoredPosition = closePos;
        settingsPanel.anchoredPosition = closePos;
        menuButtonsPanel.anchoredPosition = closePos;

        leaveLobbyButton.onClick.RemoveAllListeners();
        leaveLobbyButton.onClick.AddListener(LeaveLobby);

        inviteFriendsButton.onClick.RemoveAllListeners();
        inviteFriendsButton.onClick.AddListener(InviteFriends);
    }

    private void Update()
    {
        if (!isLocalPlayer) return;

        InputMenuPanel();
    }

    private void InputMenuPanel()
    {
        if (inputManager.GetInput<float>("Pause") > 0f)
        {
            ToggleMenuPanel();
        }
    }

    public void ToggleMenuPanel()
    {
        menuPanelIsActive = !menuPanelIsActive;

        if (menuPanelIsActive)
        {
            ChangeInputState(true);

            interfacePanel.gameObject.SetActive(false);
            menuPanel.gameObject.SetActive(true);

            menuFadeTween?.Kill();
            menuFadeTween = menuCanvasGroup.DOFade(1f, duration).SetEase(Ease.OutSine);

            if (!playerListIsActive)
                PlayerListPanelAnimation(true);
            if (!menuButtonsPanelIsActive)
                MenuButtonsPanelAnimation(true);
        }
        else
        {
            menuFadeTween?.Kill();
            menuFadeTween = menuCanvasGroup.DOFade(0f, duration).SetEase(Ease.OutSine);

            ChangeInputState(false);

            int closedCount = 0;
            bool fadeCompleted = false;
            Action tryFinish = () =>
            {
                if (fadeCompleted && closedCount == 0)
                {
                    menuPanel.gameObject.SetActive(false);
                    interfacePanel.gameObject.SetActive(true);
                }
            };

            menuFadeTween.OnComplete(() =>
            {
                fadeCompleted = true;
                tryFinish();
            });

            Action onSubClosed = () =>
            {
                closedCount--;
                if (closedCount == 0) tryFinish();
            };

            if (playerList.gameObject.activeSelf)
            {
                closedCount++;
                PlayerListPanelAnimation(false, onSubClosed);
            }
            if (settingsPanel.gameObject.activeSelf)
            {
                closedCount++;
                SettingsPanelAnimation(false, onSubClosed);
            }
            if (menuButtonsPanel.gameObject.activeSelf)
            {
                closedCount++;
                MenuButtonsPanelAnimation(false, onSubClosed);
            }

            if (closedCount == 0) tryFinish();
        }
    }

    public void ToggleSettingsPanel()
    {
        if (playerListIsActive) PlayerListPanelAnimation(false);
        SettingsPanelAnimation(!settingsPanelIsActive);
    }

    public void TogglePlayerListPanel()
    {
        if (settingsPanelIsActive) SettingsPanelAnimation(false);
        PlayerListPanelAnimation(!playerListIsActive);
    }

    private void SettingsPanelAnimation(bool isOpen, Action onComplete = null)
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
                .OnComplete(() =>
                {
                    settingsPanel.gameObject.SetActive(false);
                    onComplete?.Invoke();
                });
        }
    }

    private void PlayerListPanelAnimation(bool isOpen, Action onComplete = null)
    {
        playerListPanelTween?.Kill();
        playerListIsActive = isOpen;

        if (isOpen)
        {
            if (!playerList.gameObject.activeSelf)
            {
                playerList.anchoredPosition = closePos;
                playerList.gameObject.SetActive(true);
            }
            playerListPanelTween = playerList.DOAnchorPos(openPos, duration).SetEase(Ease.OutSine);
        }
        else
        {
            playerListPanelTween = playerList.DOAnchorPos(closePos, duration).SetEase(Ease.OutSine)
                .OnComplete(() =>
                {
                    playerList.gameObject.SetActive(false);
                    onComplete?.Invoke();
                });
        }
    }

    private void MenuButtonsPanelAnimation(bool isOpen, Action onComplete = null)
    {
        menuButtonsPanelTween?.Kill();
        menuButtonsPanelIsActive = isOpen;

        if (isOpen)
        {
            if (!menuButtonsPanel.gameObject.activeSelf)
            {
                menuButtonsPanel.anchoredPosition = openPos;
                menuButtonsPanel.gameObject.SetActive(true);
            }
            menuButtonsPanelTween = menuButtonsPanel.DOAnchorPos(closePos, duration).SetEase(Ease.OutSine);
        }
        else
        {
            menuButtonsPanelTween = menuButtonsPanel.DOAnchorPos(openPos, duration).SetEase(Ease.OutSine)
                .OnComplete(() =>
                {
                    menuButtonsPanel.gameObject.SetActive(false);
                    onComplete?.Invoke();
                });
        }
    }

    private void ChangeInputState(bool isLocked)
    {
        if (isLocked)
        {
            inputState.AddLockCamera("Pause");
            inputState.AddLockMovement("Pause");
            inputState.RemoveLockCursor("InitPlayer");
        }
        else
        {
            inputState.RemoveLockCamera("Pause");
            inputState.RemoveLockMovement("Pause");
            inputState.AddLockCursor("InitPlayer");
        }
    }

    private void LeaveLobby()
    {
        steamLobby?.LeaveLobby();
    }

    private void InviteFriends()
    {
        steamLobby?.OpenSteamInvite();
    }

    private void OnDestroy()
    {
        menuButtonsPanelTween?.Kill();
        settingsPanelTween?.Kill();
        playerListPanelTween?.Kill();
        menuFadeTween?.Kill();
    }

    private void OnDisable()
    {
        menuButtonsPanelTween?.Kill();
        settingsPanelTween?.Kill();
        playerListPanelTween?.Kill();
        menuFadeTween?.Kill();
    }
}