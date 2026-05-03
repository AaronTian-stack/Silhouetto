using System.Collections;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

[RequireComponent(typeof(CanvasGroup))]
public class SceneFadeTransition : MonoBehaviour
{
    [SerializeField, Min(0f)] private float fadeDuration = 0.35f;
    [SerializeField] private Image fadeImage;

    private CanvasGroup canvasGroup;
    private Coroutine fadeRoutine;

    private void Awake()
    {
        canvasGroup = GetComponent<CanvasGroup>();

        if (fadeImage == null)
        {
            fadeImage = GetComponent<Image>();
        }
    }

    private void Start()
    {
        if (canvasGroup == null)
        {
            return;
        }

        canvasGroup.alpha = 1f;
        canvasGroup.blocksRaycasts = true;
        canvasGroup.interactable = false;
        fadeRoutine = StartCoroutine(FadeTo(0f));
    }

    public void LoadSceneWithFade(int sceneBuildIndex)
    {
        if (!isActiveAndEnabled)
        {
            SceneManager.LoadScene(sceneBuildIndex);
            return;
        }

        if (fadeRoutine != null)
        {
            StopCoroutine(fadeRoutine);
        }

        fadeRoutine = StartCoroutine(LoadSceneRoutine(sceneBuildIndex));
    }

    private IEnumerator LoadSceneRoutine(int sceneBuildIndex)
    {
        yield return FadeTo(1f);
        SceneManager.LoadScene(sceneBuildIndex);
        fadeRoutine = null;
    }

    private IEnumerator FadeTo(float targetAlpha)
    {
        if (canvasGroup == null)
        {
            yield break;
        }

        float startAlpha = canvasGroup.alpha;
        if (Mathf.Approximately(startAlpha, targetAlpha) || fadeDuration <= 0f)
        {
            canvasGroup.alpha = targetAlpha;
            canvasGroup.blocksRaycasts = targetAlpha > 0f;
            yield break;
        }

        float elapsed = 0f;
        while (elapsed < fadeDuration)
        {
            elapsed += Time.unscaledDeltaTime;
            float t = Mathf.Clamp01(elapsed / fadeDuration);
            canvasGroup.alpha = Mathf.Lerp(startAlpha, targetAlpha, t);
            canvasGroup.blocksRaycasts = canvasGroup.alpha > 0f;
            yield return null;
        }

        canvasGroup.alpha = targetAlpha;
        canvasGroup.blocksRaycasts = targetAlpha > 0f;
    }
}