using UnityEngine;

[ExecuteAlways]
public class ParallaxLayer : MonoBehaviour
{
    [SerializeField] private Transform cameraTransform;

    [Header("0 = near / world fixed, 1 = far / follows camera completely")]
    [SerializeField, Range(0f, 1f)]
    private float parallaxFactor = 0.5f;

    [Header("Reference Position")]
    [SerializeField]
    private bool useCurrentCameraPositionAsReferenceOnStart = true;

    [SerializeField]
    private Vector3 referenceCameraPosition;

    [Header("Auto Z Position")]
    [SerializeField]
    private bool autoSetZ = true;

    [SerializeField]
    private float nearZ = 0f;

    [SerializeField]
    private float farZ = 50f;

    private Vector3 placedLayerPosition;
    private bool initialized;

    private void OnEnable()
    {
        Initialize();
        ApplyZPosition();
    }

    private void LateUpdate()
    {
        if (!Application.isPlaying)
        {
            return;
        }

        if (cameraTransform == null)
        {
            Initialize();

            if (cameraTransform == null)
            {
                return;
            }
        }

        ApplyParallax();
    }

    private void OnValidate()
    {
        ApplyZPosition();
    }

    private void Initialize()
    {
        if (cameraTransform == null && Camera.main != null)
        {
            cameraTransform = Camera.main.transform;
        }

        if (cameraTransform == null)
        {
            return;
        }

        placedLayerPosition = transform.position;

        if (useCurrentCameraPositionAsReferenceOnStart)
        {
            referenceCameraPosition = cameraTransform.position;
        }

        initialized = true;
    }

    private void ApplyParallax()
    {
        if (!initialized)
        {
            Initialize();

            if (!initialized)
            {
                return;
            }
        }

        Vector3 cameraOffset = cameraTransform.position - referenceCameraPosition;

        Vector3 pos = placedLayerPosition + new Vector3(
            cameraOffset.x * parallaxFactor,
            cameraOffset.y * parallaxFactor,
            0f
        );

        transform.position = pos;

        ApplyZPosition();
    }

    private void ApplyZPosition()
    {
        if (!autoSetZ)
        {
            return;
        }

        Vector3 pos = transform.position;
        pos.z = Mathf.Lerp(nearZ, farZ, parallaxFactor);
        transform.position = pos;
    }

    [ContextMenu("Set Current Camera Position As Reference")]
    private void SetCurrentCameraPositionAsReference()
    {
        if (cameraTransform == null && Camera.main != null)
        {
            cameraTransform = Camera.main.transform;
        }

        if (cameraTransform == null)
        {
            return;
        }

        referenceCameraPosition = cameraTransform.position;
        placedLayerPosition = transform.position;
    }
}