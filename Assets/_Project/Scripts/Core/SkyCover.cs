using UnityEngine;

// Estira el cielo para que cubra toda la pantalla también en ventanas más anchas que 16:9 (por ejemplo, maximizada).
[RequireComponent(typeof(SpriteRenderer))]
public class SkyCover : MonoBehaviour
{
    [SerializeField] private Camera targetCamera;
    [SerializeField] private float extraMargin = 1.02f;

    private SpriteRenderer sky;
    private Vector3 baseScale;

    private void Awake()
    {
        sky = GetComponent<SpriteRenderer>();
        baseScale = transform.localScale;
        if (targetCamera == null) targetCamera = GetComponentInParent<Camera>();
    }

    private void LateUpdate()
    {
        if (targetCamera == null || sky.sprite == null) return;

        float viewHeight = targetCamera.orthographicSize * 2f;
        float viewWidth = viewHeight * targetCamera.aspect;
        Vector2 size = sky.sprite.bounds.size;

        // Solo crece: en 16:9 se queda como estaba
        float grow = Mathf.Max(1f,
            viewWidth * extraMargin / (size.x * baseScale.x),
            viewHeight * extraMargin / (size.y * baseScale.y));

        transform.localScale = new Vector3(baseScale.x * grow, baseScale.y * grow, baseScale.z);
    }
}
