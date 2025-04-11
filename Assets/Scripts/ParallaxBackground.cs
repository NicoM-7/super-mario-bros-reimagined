using UnityEngine;

public class ParallaxBackground : MonoBehaviour
{
    public Transform cameraTransform;         // Reference to your camera (assign in Inspector)
    [Range(0f, 1f)]
    public float parallaxFactor = 0.5f;         // How much slower the background moves relative to the camera

    private Vector3 previousCameraPosition;
    private Renderer bgRenderer;

    void Start()
    {
        if (cameraTransform == null)
            cameraTransform = Camera.main.transform;
        
        previousCameraPosition = cameraTransform.position;
        bgRenderer = GetComponent<Renderer>();
    }

    void LateUpdate()
    {
        // Calculate how much the camera moved since the last frame.
        Vector3 deltaMovement = cameraTransform.position - previousCameraPosition;
        // Update only the X offset (keep Y offset fixed)
        Vector2 offset = bgRenderer.material.mainTextureOffset;
        offset.x += deltaMovement.x * parallaxFactor;
        bgRenderer.material.mainTextureOffset = offset;
        
        previousCameraPosition = cameraTransform.position;
    }
}
