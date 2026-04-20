using TMPro;
using UnityEngine;

public class ScoreTextSetter : MonoBehaviour
{
    private TextMeshProUGUI scoreText;
    void Start()
    {
        scoreText = GetComponent<TextMeshProUGUI>();
    }

    void Update()
    {
        scoreText.text = LevelManager.Score.ToString();
    }
}
