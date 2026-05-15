using UnityEngine;

[ExecuteAlways]
public class ParallaxLayer : MonoBehaviour
{
    [SerializeField] private Transform cameraTransform;

    [Header("0 = near / world fixed, 1 = far / follows camera completely")]
    [SerializeField, Range(0f, 1f)]
    private float parallaxFactor = 0.5f;

    [Header("Auto Z Position")]
    [SerializeField] private bool autoSetZ = true;

    private const float nearZ = 0f;
    private const float farZ = 50f;

    private Vector3 initialLayerPosition;
    private Vector3 initialCameraPosition;
    private bool initialized;

    private void OnEnable()
    {
        Initialize();
        ApplyZPosition();
    }

    private void LateUpdate()
    {
        if (cameraTransform == null)
        {
            Initialize();
            if (cameraTransform == null) return;
        }

        Vector3 cameraOffset = cameraTransform.position - initialCameraPosition;

        Vector3 pos = initialLayerPosition + new Vector3(
            cameraOffset.x * parallaxFactor,
            cameraOffset.y * parallaxFactor,
            0f
        );

        transform.position = pos;

        ApplyZPosition();
    }

    private void OnValidate()
    {
        if (!Application.isPlaying)
        {
            ApplyZPosition();
        }
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

        initialLayerPosition = transform.position;
        initialCameraPosition = cameraTransform.position;
        initialized = true;
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
}