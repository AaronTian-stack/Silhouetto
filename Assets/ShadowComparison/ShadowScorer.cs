using System.Collections;
using UnityEngine;
using UnityEngine.Events;

/// <summary>
/// Compares the silhouette cast by a 3D object against a target texture
/// and produces a 0–1 match score using Intersection over Union (IoU).
/// </summary>
public class ShadowScorer : MonoBehaviour
{
    [Header("References")]
    [Tooltip("Orthographic camera positioned at the light, targeting the shadow receiver.")]
    public Camera silhouetteCamera;

    [Tooltip("RenderTexture the silhouette camera renders into. Also set as the camera's Target Texture.")]
    public RenderTexture captureRT;

    [Tooltip("Black-on-white Texture2D representing the goal shadow. Must have Read/Write enabled.")]
    public Texture2D targetSilhouette;

    [Header("Scoring")]
    [Tooltip("Pixel brightness threshold below which a pixel is considered 'shadow' (0–1).")]
    [Range(0f, 1f)]
    public float shadowThreshold = 0.5f;

    [Tooltip("How many times per second the score is recalculated.")]
    [Range(1f, 30f)]
    public float updatesPerSecond = 10f;

    [Tooltip("Score at which OnTargetReached fires (0–1).")]
    [Range(0f, 1f)]
    public float successThreshold = 0.85f;

    [Header("Events")]
    public UnityEvent<float> OnScoreChanged;   // fires every update with the new score
    public UnityEvent OnTargetReached;  // fires once when score >= successThreshold

    // ── Public read-only state ──────────────────────────────────────────────
    public float CurrentScore { get; private set; }
    public bool IsMatch { get; private set; }

    // ── Private ─────────────────────────────────────────────────────────────
    private Texture2D _captureBuffer;
    private Shader _silhouetteShader;
    private bool _successFired;

    // ────────────────────────────────────────────────────────────────────────

    private void Awake()
    {
        // Load the replacement shader by name.
        _silhouetteShader = Shader.Find("Custom/SilhouetteCapture");
        if (_silhouetteShader == null)
            Debug.LogError("[ShadowScorer] Could not find shader 'Custom/SilhouetteCapture'. " +
                           "Make sure SilhouetteCapture.shader is in your project.");

        // Create a CPU-side Texture2D to receive GPU readback.
        if (captureRT != null)
            _captureBuffer = new Texture2D(captureRT.width, captureRT.height,
                                           TextureFormat.RGBA32, false);

        ValidateTargetTexture();
    }

    private void OnEnable()
    {
        _successFired = false;
        StartCoroutine(ScoreLoop());
    }

    private void OnDisable()
    {
        StopAllCoroutines();
    }

    // ── Main loop ────────────────────────────────────────────────────────────

    private IEnumerator ScoreLoop()
    {
        var wait = new WaitForSeconds(1f / updatesPerSecond);
        while (true)
        {
            yield return wait;
            CurrentScore = CalculateScore();
            OnScoreChanged?.Invoke(CurrentScore);

            bool nowMatch = CurrentScore >= successThreshold;
            if (nowMatch && !_successFired)
            {
                _successFired = true;
                IsMatch = true;
                OnTargetReached?.Invoke();
            }
            else if (!nowMatch)
            {
                _successFired = false;
                IsMatch = false;
            }
        }
    }

    // ── Score calculation ────────────────────────────────────────────────────

    /// <summary>
    /// Renders the silhouette camera, reads back the pixels, and returns
    /// an IoU score against the target silhouette texture.
    /// </summary>
    public float CalculateScore()
    {
        if (!ValidateReferences()) return 0f;

        // 1. Render the silhouette camera using the replacement shader.
        silhouetteCamera.RenderWithShader(_silhouetteShader, "RenderType");

        // 2. Read back pixels from the RenderTexture.
        var prevActive = RenderTexture.active;
        RenderTexture.active = captureRT;
        _captureBuffer.ReadPixels(new Rect(0, 0, captureRT.width, captureRT.height), 0, 0);
        _captureBuffer.Apply();
        RenderTexture.active = prevActive;

        // 3. Compare pixel arrays.
        Color[] captured = _captureBuffer.GetPixels();
        Color[] target = targetSilhouette.GetPixels();

        if (captured.Length != target.Length)
        {
            Debug.LogWarning("[ShadowScorer] captureRT and targetSilhouette must be the same resolution.");
            return 0f;
        }

        int intersection = 0;
        int union = 0;

        for (int i = 0; i < captured.Length; i++)
        {
            bool inCaptured = captured[i].r < shadowThreshold; // black = shadow
            bool inTarget = target[i].r < shadowThreshold;

            if (inCaptured && inTarget) intersection++;
            if (inCaptured || inTarget) union++;
        }

        return union == 0 ? 1f : (float)intersection / union;
    }

    // ── Helpers ──────────────────────────────────────────────────────────────

    private bool ValidateReferences()
    {
        if (silhouetteCamera == null) { Debug.LogWarning("[ShadowScorer] silhouetteCamera not assigned."); return false; }
        if (captureRT == null) { Debug.LogWarning("[ShadowScorer] captureRT not assigned."); return false; }
        if (targetSilhouette == null) { Debug.LogWarning("[ShadowScorer] targetSilhouette not assigned."); return false; }
        if (_silhouetteShader == null) return false;
        if (_captureBuffer == null) return false;
        return true;
    }

    private void ValidateTargetTexture()
    {
        if (targetSilhouette == null) return;
        // Check the texture is readable (isReadable is internal; we try and catch).
        try { targetSilhouette.GetPixels(); }
        catch
        {
            Debug.LogError("[ShadowScorer] targetSilhouette is not readable. " +
                           "Enable 'Read/Write' in its Texture Import Settings.");
        }
    }

    private void OnDestroy()
    {
        if (_captureBuffer != null) Destroy(_captureBuffer);
    }

#if UNITY_EDITOR
    // Quick in-editor test — select the GameObject and press the button in the Inspector.
    [ContextMenu("Calculate Score Now")]
    private void EditorCalculateScore()
    {
        if (_silhouetteShader == null)
            _silhouetteShader = Shader.Find("Custom/SilhouetteCapture");
        if (_captureBuffer == null && captureRT != null)
            _captureBuffer = new Texture2D(captureRT.width, captureRT.height, TextureFormat.RGBA32, false);

        float score = CalculateScore();
        Debug.Log($"[ShadowScorer] Score: {score:P1}");
    }
#endif
}
