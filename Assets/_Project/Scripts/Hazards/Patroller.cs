using UnityEngine;

// Comportamiento de patrulla: camina de un punto a otro, hace una pausa y regresa (alacrán de Guanajuato).
// El daño lo hace el componente Hazard del mismo objeto.
public class Patroller : MonoBehaviour
{
    [SerializeField] private SpriteRenderer spriteRenderer;
    [SerializeField] private SpriteFlipbook walkAnimation;

    [Header("Recorrido (distancias desde donde empieza)")]
    [SerializeField] private float leftDistance = 2f;
    [SerializeField] private float rightDistance = 2f;
    [SerializeField] private float speed = 1.6f;
    [SerializeField] private float pauseTime = 0.7f;
    [SerializeField] private bool startToTheLeft;

    private float startX;
    private float direction = 1f;
    private float pauseTimer;

    private void Awake()
    {
        startX = transform.position.x;
        direction = startToTheLeft ? -1f : 1f;
        spriteRenderer.flipX = direction < 0f;
    }

    private void Update()
    {
        if (pauseTimer > 0f)
        {
            pauseTimer -= Time.deltaTime;
            if (pauseTimer <= 0f)
            {
                spriteRenderer.flipX = direction < 0f;
                SetWalking(true);
            }

            return;
        }

        float limit = direction > 0f ? startX + rightDistance : startX - leftDistance;
        Vector3 position = transform.position;
        position.x = Mathf.MoveTowards(position.x, limit, speed * Time.deltaTime);
        transform.position = position;

        // Al llegar a un extremo se detiene un momento y da la vuelta
        if (Mathf.Approximately(position.x, limit))
        {
            direction = -direction;
            pauseTimer = pauseTime;
            SetWalking(false);
        }
    }

    private void SetWalking(bool walking)
    {
        if (walkAnimation != null) walkAnimation.enabled = walking;
    }

    // Muestra el recorrido en la vista de escena para colocarlo fácilmente
    private void OnDrawGizmosSelected()
    {
        float center = Application.isPlaying ? startX : transform.position.x;
        var from = new Vector3(center - leftDistance, transform.position.y + 0.1f, 0f);
        var to = new Vector3(center + rightDistance, transform.position.y + 0.1f, 0f);
        Gizmos.color = Color.yellow;
        Gizmos.DrawLine(from, to);
        Gizmos.DrawWireSphere(from, 0.12f);
        Gizmos.DrawWireSphere(to, 0.12f);
    }
}
