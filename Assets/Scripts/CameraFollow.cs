using UnityEngine;

public class CameraFollowDeadZone : MonoBehaviour
{
    public Transform target;                    // Mario's transform.
    public Vector3 offset = new Vector3(0, 0, -10); // Ensure the camera stays at the proper Z distance.
    public Vector2 deadZoneSize = new Vector2(2f, 1.5f); // Width & height of the dead zone.

    void LateUpdate()
    {
        if (target == null)
            return;
        
        // Calculate the desired target position including offset.
        Vector3 targetPos = target.position + offset;
        Vector3 currentPos = transform.position;

        // Calculate differences between targetPos and current camera position.
        float deltaX = targetPos.x - currentPos.x;
        float deltaY = targetPos.y - currentPos.y;

        // Horizontal adjustment: only move if outside dead zone.
        if (Mathf.Abs(deltaX) > deadZoneSize.x)
        {
            if (deltaX > 0)
                currentPos.x = targetPos.x - deadZoneSize.x;
            else
                currentPos.x = targetPos.x + deadZoneSize.x;
        }

        // Vertical adjustment: only move if outside dead zone.
        if (Mathf.Abs(deltaY) > deadZoneSize.y)
        {
            if (deltaY > 0)
                currentPos.y = targetPos.y - deadZoneSize.y;
            else
                currentPos.y = targetPos.y + deadZoneSize.y;
        }

        // Keep the Z value fixed.
        currentPos.z = offset.z;

        // --- Additional Vertical Clamping ---
        // Get the orthographic size of the camera (half of the camera's height in world units).
        Camera cam = Camera.main;
        float orthoSize = cam.orthographicSize;

        // We want Mario (target.position.y) to always be at least 2 units above the bottom edge of the camera.
        // The camera's bottom edge is at (cameraPos.y - orthoSize). So we require:
        //     target.position.y - (cameraPos.y - orthoSize) >= 2
        // Rearranged, that gives: cameraPos.y <= target.position.y + orthoSize - 2.
        float maxCamY = target.position.y + orthoSize - 2f;
        if (currentPos.y > maxCamY)
        {
            currentPos.y = maxCamY;
        }

        // Ensure the camera doesn't scroll below y=0.
        // The bottom edge is at (cameraPos.y - orthoSize), so we need: cameraPos.y - orthoSize >= 0,
        // i.e. cameraPos.y >= orthoSize.
        float minCamY = orthoSize;
        if (currentPos.y < minCamY)
        {
            currentPos.y = minCamY;
        }

        transform.position = currentPos;
    }
}
