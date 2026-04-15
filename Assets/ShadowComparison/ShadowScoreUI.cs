using UnityEngine;
using UnityEngine.UI;
using TMPro;

/// <summary>
/// Wires ShadowScorer's score into a UI slider and label.
/// Attach to a UI GameObject and link up the references.
/// Subscribe ShadowScorer.OnScoreChanged → this.UpdateDisplay.
/// </summary>
public class ShadowScoreUI : MonoBehaviour
{
    [Header("References")]
    public ShadowScorer scorer;

    [Tooltip("Slider showing 0–1 match progress.")]
    public Slider scoreSlider;

    [Tooltip("Label showing e.g. '73%' or 'MATCHED!'")]
    public TMP_Text scoreLabel;

    [Tooltip("Image/panel that flashes on success.")]
    public GameObject successIndicator;

    [Header("Colors")]
    public Color lowColor = Color.red;
    public Color highColor = Color.green;

    private Image _fillImage;

    private void Awake()
    {
        if (scoreSlider != null)
        {
            scoreSlider.minValue = 0f;
            scoreSlider.maxValue = 1f;
            scoreSlider.interactable = false;
            _fillImage = scoreSlider.fillRect?.GetComponent<Image>();
        }

        if (successIndicator != null)
            successIndicator.SetActive(false);

        // Wire events
        if (scorer != null)
        {
            scorer.OnScoreChanged.AddListener(UpdateDisplay);
            scorer.OnTargetReached.AddListener(ShowSuccess);
        }
    }

    private void OnDestroy()
    {
        if (scorer != null)
        {
            scorer.OnScoreChanged.RemoveListener(UpdateDisplay);
            scorer.OnTargetReached.RemoveListener(ShowSuccess);
        }
    }

    /// <summary>Called by ShadowScorer.OnScoreChanged.</summary>
    public void UpdateDisplay(float score)
    {
        if (scoreSlider != null)
            scoreSlider.value = score;

        if (_fillImage != null)
            _fillImage.color = Color.Lerp(lowColor, highColor, score);

        if (scoreLabel != null)
            scoreLabel.text = scorer.IsMatch
                ? "MATCHED!"
                : $"{score * 100f:F0}%";

        if (successIndicator != null && !scorer.IsMatch)
            successIndicator.SetActive(false);
    }

    private void ShowSuccess()
    {
        if (successIndicator != null)
            successIndicator.SetActive(true);

        if (scoreLabel != null)
            scoreLabel.text = "MATCHED!";
    }
}
