using TMPro;
using UnityEngine;

public class TimerTextSetter : MonoBehaviour
{
    private TextMeshProUGUI timerText;

    private void Start()
    {
        timerText = GetComponent<TextMeshProUGUI>();
    }

    private void Update()
    {
        float remainingSeconds = Mathf.Max(0f, LevelManager.RemainingTimeSeconds);
        int totalSeconds = Mathf.CeilToInt(remainingSeconds);
        int minutes = totalSeconds / 60;
        int seconds = totalSeconds % 60;
        timerText.text = $"{minutes:00}:{seconds:00}";
    }
}
