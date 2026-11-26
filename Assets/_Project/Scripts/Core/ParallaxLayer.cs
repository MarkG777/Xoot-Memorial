using UnityEngine;

// Fondo con profundidad: la capa se mueve solo una parte de lo que se mueve la cámara.
public class ParallaxLayer : MonoBehaviour
{
    [SerializeField] private Transform cameraTransform;
    [SerializeField, Range(0f, 1f)] private float followX = 0.7f;
    [SerializeField, Range(0f, 1f)] private float followY = 0.85f;

    private Vector3 startPosition;
    private Vector3 cameraStart;

    private void Start()
    {
        startPosition = transform.position;
        cameraStart = cameraTransform.position;
    }

    private void LateUpdate()
    {
        Vector3 delta = cameraTransform.position - cameraStart;
        transform.position = startPosition + new Vector3(delta.x * followX, delta.y * followY, 0f);
    }
}
