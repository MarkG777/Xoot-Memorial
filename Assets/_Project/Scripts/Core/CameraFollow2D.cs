using UnityEngine;

// Cámara 2D que sigue al objetivo con suavizado.
public class CameraFollow2D : MonoBehaviour
{
    [SerializeField] private Transform target;
    [SerializeField] private float smoothTime = 0.15f;
    [SerializeField] private Vector2 offset = new Vector2(0f, 1.5f);

    private Vector3 velocity;

    private void LateUpdate()
    {
        if (target == null) return;

        var goal = new Vector3(target.position.x + offset.x, target.position.y + offset.y, transform.position.z);
        transform.position = Vector3.SmoothDamp(transform.position, goal, ref velocity, smoothTime);
    }
}
