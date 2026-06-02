using UnityEngine;
using UnityEngine.InputSystem;

/// <summary>
/// Switches to the silhouette/shadow projection camera while the
/// Quest Y button is held, and reverts to the main camera on release.
///
/// Setup:
///  1. Attach this script to any persistent GameObject (e.g. GameManager).
///  2. Assign mainCamera (your normal player camera).
///  3. Assign silhouetteCamera (the shadow projection camera from ShadowScorer).
/// </summary>
public class ShadowCameraSwitch : MonoBehaviour
{
    [Header("References")]
    [Tooltip("Your normal player/game camera.")]
    public Camera mainCamera;

    [Tooltip("The orthographic silhouette camera used by ShadowScorer.")]
    public Camera silhouetteCamera;

    // Y button on the Quest right controller
    private InputAction _yButton;
    private bool _isShowingSilhouette = false;

    private void Awake()
    {
        // Bind to the Quest right controller Y button
        _yButton = new InputAction(
            name: "SilhouetteView",
            binding: "<XRController>{LeftHand}/secondaryButton" // Y button
        );

        _yButton.Enable();
    }

    private void OnDestroy()
    {
        _yButton?.Disable();
        _yButton?.Dispose();
    }

    private void Update()
    {
        bool holding = _yButton.ReadValue<float>() > 0.5f;

        if (holding && !_isShowingSilhouette)
        {
            _isShowingSilhouette = true;
            mainCamera.enabled = false;
            silhouetteCamera.enabled = true;
        }
        else if (!holding && _isShowingSilhouette)
        {
            _isShowingSilhouette = false;
            silhouetteCamera.enabled = false;
            mainCamera.enabled = true;
        }
    }
}
