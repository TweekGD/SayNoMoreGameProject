using DG.Tweening;
using TMPro;
using UnityEngine;

public class TimerPanel : MonoBehaviour
{
    [SerializeField] private RectTransform timerPanel;
    [SerializeField] private TextMeshProUGUI timerText;
    [Header("Animation Settings")]
    [SerializeField] private Vector2 hidePos;
    [SerializeField] private Vector2 showPos;
    [SerializeField] private float animDuration = 1f;

    private bool isShowing = false;
    private Tween timerPanelTween;
    public bool IsShowing => isShowing;
    private void Update()
    {
        if (RoundManager.Instance == null) return;

        float currentTime = RoundManager.Instance.TimeRemaining;
        bool isRunning = RoundManager.Instance.IsRunning;

        if (isRunning && currentTime > 0f)
        {
            if (!isShowing) { Show(); }

            timerText.text = ConvertToTime((int)currentTime);
        }
        else if (isShowing)
        {
            Hide();
        }
    }
    public void Show()
    {
        if (isShowing) return;

        if (timerPanel == null) return;

        timerPanelTween?.Kill();

        isShowing = true;

        timerPanelTween = timerPanel.DOAnchorPos(showPos, animDuration).SetEase(Ease.OutSine);
    }
    public void Hide()
    {
        if (!isShowing) return;

        if (timerPanel == null) return;

        timerPanelTween?.Kill();

        isShowing = false;

        timerPanelTween = timerPanel.DOAnchorPos(hidePos, animDuration).SetEase(Ease.OutSine);
    }
    private string ConvertToTime(int time)
    {
        if (time < 0) time = 0;

        int minutes = time / 60;
        int seconds = time % 60;

        return $"{minutes:D2}:{seconds:D2}";
    }
    private void OnDestroy()
    {
        timerPanelTween?.Kill();
    }
    private void OnDisable()
    {
        timerPanelTween?.Kill();
    }
}