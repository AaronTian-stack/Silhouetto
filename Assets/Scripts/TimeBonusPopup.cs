using System.Collections;
using TMPro;
using UnityEngine;

[RequireComponent(typeof(TextMeshProUGUI))]
public class TimeBonusPopup : MonoBehaviour
{
    [Header("Animation")]
    [SerializeField, Min(0f)] private float fadeInDuration = 0.12f;
    [SerializeField, Min(0f)] private float holdDuration = 0.45f;
    [SerializeField, Min(0f)] private float fadeOutDuration = 0.35f;
    [SerializeField] private float riseDistance = 28f;
    [SerializeField, Min(0f)] private float peakFontSizeMultiplier = 1.1f;

    [Header("Text")]
    [SerializeField] private string prefix = "+";
    [SerializeField] private string suffix = "s";

    private TextMeshProUGUI popupText;
    private RectTransform rectTransform;

    private Vector3 startLocalPosition;
    private float startFontSize;
    private Color startColor;
    private Coroutine animationRoutine;

    private void Awake()
    {
        popupText = GetComponent<TextMeshProUGUI>();
        rectTransform = GetComponent<RectTransform>();

        startLocalPosition = rectTransform.localPosition;
        startFontSize = popupText.fontSize;
        startColor = popupText.color;
        popupText.color = new Color(startColor.r, startColor.g, startColor.b, 0f);
    }

    public void Show(float secondsAdded)
    {
        popupText.text = FormatBonus(secondsAdded);

        if (animationRoutine != null)
        {
            StopCoroutine(animationRoutine);
        }

        animationRoutine = StartCoroutine(PlayRoutine());
    }

    private IEnumerator PlayRoutine()
    {
        rectTransform.localPosition = startLocalPosition;
        popupText.fontSize = startFontSize;
        popupText.color = new Color(startColor.r, startColor.g, startColor.b, 0f);

        float totalDuration = fadeInDuration + holdDuration + fadeOutDuration;

        float elapsed = 0f;
        while (elapsed < totalDuration)
        {
            elapsed += Time.deltaTime;

            float alpha;
            if (elapsed <= fadeInDuration)
            {
                alpha = Mathf.Clamp01(elapsed / fadeInDuration);
            }
            else if (elapsed <= fadeInDuration + holdDuration)
            {
                alpha = 1f;
            }
            else
            {
                float fadeOutElapsed = elapsed - fadeInDuration - holdDuration;
                alpha = 1f - Mathf.Clamp01(fadeOutElapsed / fadeOutDuration);
            }

            float riseT = Mathf.Clamp01(elapsed / totalDuration);
            float yOffset = Mathf.Lerp(0f, riseDistance, riseT);
            float fontSizeT = Mathf.Clamp01(elapsed / Mathf.Max(fadeInDuration, 0.0001f));
            float fontSizeMultiplier = fontSizeT < 0.5f
                ? Mathf.Lerp(1f, peakFontSizeMultiplier, fontSizeT * 2f)
                : Mathf.Lerp(peakFontSizeMultiplier, 1f, (fontSizeT - 0.5f) * 2f);

            var color = startColor;
            color.a = alpha;
            popupText.color = color;
            rectTransform.localPosition = startLocalPosition + Vector3.up * yOffset;
            popupText.fontSize = startFontSize * fontSizeMultiplier;

            yield return null;
        }

        popupText.color = new Color(startColor.r, startColor.g, startColor.b, 0f);
        rectTransform.localPosition = startLocalPosition;
        popupText.fontSize = startFontSize;
        animationRoutine = null;
    }

    private string FormatBonus(float secondsAdded)
    {
        bool wholeNumber = Mathf.Approximately(secondsAdded, Mathf.Round(secondsAdded));
        return wholeNumber
            ? $"{prefix}{Mathf.RoundToInt(secondsAdded)}{suffix}"
            : $"{prefix}{secondsAdded:0.0}{suffix}";
    }
}
