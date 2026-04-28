using System;
using System.Collections;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class RoundDescriptionPanel : MonoBehaviour
{
    [SerializeField] private GameMenuPanelManager gameMenuPanelManager;
    [SerializeField] private TextMeshProUGUI descriptionText;
    [SerializeField] private Image progressBarImage;
    [SerializeField] private InputState inputState;
    [SerializeField] private float charDelay = 0.05f;
    [SerializeField] private float startPrintingDelay = 0.5f;
    [SerializeField] private float fadeDuration = 0.6f;
    [SerializeField] private float progressBarFillDuration = 2f;
    public float CharDelay => charDelay;
    public float StartPrintingDelay => startPrintingDelay;
    public float FadeDuration => fadeDuration;
    public float ProgressBarFillDuration => progressBarFillDuration;
    public bool PanelIsActive { get; private set; }

    private Coroutine printingTextCoroutine;
    private Coroutine progressBarCoroutine;

    private FadeScreenEffect fadeScreenEffect;

    private void Awake()
    {
        fadeScreenEffect = GetComponent<FadeScreenEffect>();

        if (descriptionText != null)
            descriptionText.text = "";

        if (progressBarImage != null)
            progressBarImage.fillAmount = 0f;
    }

    public void StartDescriptionPanel(string description, Action onComplete = null, Action onFadeIn = null)
    {
        if (PanelIsActive) return;

        if (string.IsNullOrEmpty(description)) return;

        if (fadeScreenEffect == null) return;

        if (inputState == null) 
        {
            Debug.LogWarning("Component InputState not init!");
            return;
        }

        if (gameMenuPanelManager.MenuPanelIsActive)
        {
            gameMenuPanelManager.ToggleMenuPanel();
        }

        PanelIsActive = true;
        StopAllPanelCoroutines();

        if (descriptionText != null) descriptionText.text = "";
        if (progressBarImage != null) progressBarImage.fillAmount = 0f;

        fadeScreenEffect.StartFadeEffect(1f, fadeDuration, () =>
        {
            ToggleLockInput(true);
            inputState.AddLockMenu("Round Description");

            onFadeIn?.Invoke();

            printingTextCoroutine = StartCoroutine(PrintText(description, () =>
            {
                progressBarCoroutine = StartCoroutine(FillProgressBar(() =>
                {
                    ToggleLockInput(false);
                    inputState.RemoveLockMenu("Round Description");

                    fadeScreenEffect.StartFadeEffect(0f, fadeDuration, () =>
                    {
                        if (descriptionText != null) descriptionText.text = "";
                        if (progressBarImage != null) progressBarImage.fillAmount = 0f;

                        PanelIsActive = false;

                        onComplete?.Invoke();
                    });
                }));
            }));
        });
    }

    private void StopAllPanelCoroutines()
    {
        if (printingTextCoroutine != null) StopCoroutine(printingTextCoroutine);
        if (progressBarCoroutine != null) StopCoroutine(progressBarCoroutine);
    }

    private IEnumerator FillProgressBar(Action onComplete = null)
    {
        float elapsed = 0f;
        while (elapsed < progressBarFillDuration)
        {
            elapsed += Time.deltaTime;
            if (progressBarImage != null)
                progressBarImage.fillAmount = elapsed / progressBarFillDuration;
            yield return null;
        }
        if (progressBarImage != null) progressBarImage.fillAmount = 1f;
        onComplete?.Invoke();
    }

    private void ToggleLockInput(bool value)
    {
        if (value)
        {
            inputState.AddLockMovement("Round Description");
            inputState.AddLockCamera("Round Description");
        }
        else 
        {
            inputState.RemoveLockMovement("Round Description");
            inputState.RemoveLockCamera("Round Description");
        }
    }
    private IEnumerator PrintText(string text, Action onComplete = null)
    {
        yield return new WaitForSeconds(startPrintingDelay);
        int i = 0;
        while (i < text.Length)
        {
            if (text[i] == '<')
            {
                int closeIndex = text.IndexOf('>', i);
                if (closeIndex != -1)
                {
                    descriptionText.text += text.Substring(i, closeIndex - i + 1);
                    i = closeIndex + 1;
                    continue;
                }
            }
            descriptionText.text += text[i];
            i++;
            yield return new WaitForSeconds(charDelay);
        }
        onComplete?.Invoke();
    }
}