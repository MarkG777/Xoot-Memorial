using UnityEngine;

// Reja que se abre y se cierra con tiempo: sube para dejar pasar y baja para bloquear el camino.
[RequireComponent(typeof(Rigidbody2D), typeof(BoxCollider2D))]
public class TimedGate : MonoBehaviour
{
    [SerializeField] private float openHeight = 2.6f;
    [SerializeField] private float openSeconds = 2.2f;
    [SerializeField] private float closedSeconds = 1.8f;
    [SerializeField] private float moveSpeed = 5f;
    [SerializeField] private float startOffset = 0f;

    [Header("Daño al caer encima")]
    [SerializeField] private bool hurtsWhenClosing = true;
    [Tooltip("Cuánto se angosta la zona de daño por cada lado, para que rozar la reja de lado no cuente")]
    [SerializeField] private float sideMargin = 0.05f;

    private Rigidbody2D body;
    private BoxCollider2D box;
    private Vector2 closedPosition;
    private float timer;

    private void Awake()
    {
        body = GetComponent<Rigidbody2D>();
        box = GetComponent<BoxCollider2D>();
        body.bodyType = RigidbodyType2D.Kinematic;
        closedPosition = body.position;
        timer = startOffset;
    }

    private void FixedUpdate()
    {
        timer += Time.fixedDeltaTime;
        float cycle = openSeconds + closedSeconds;
        bool open = Mathf.Repeat(timer, cycle) < openSeconds;

        Vector2 target = open ? closedPosition + Vector2.up * openHeight : closedPosition;
        Vector2 next = Vector2.MoveTowards(body.position, target, moveSpeed * Time.fixedDeltaTime);

        // Solo lastima mientras va bajando; cerrada y quieta es una pared
        if (hurtsWhenClosing && next.y < body.position.y) HurtPlayerBelow(next);

        body.MovePosition(next);
    }

    // Revisa la franja que está justo debajo de la reja en este paso
    private void HurtPlayerBelow(Vector2 nextPosition)
    {
        float bottom = nextPosition.y + box.offset.y - box.size.y / 2f;
        var center = new Vector2(nextPosition.x + box.offset.x, bottom + 0.05f);
        var size = new Vector2(box.size.x - sideMargin * 2f, 0.2f);

        foreach (Collider2D hit in Physics2D.OverlapBoxAll(center, size, 0f))
        {
            var lives = hit.GetComponentInParent<PlayerLives>();
            if (lives == null) continue;

            lives.LoseLife();
            return;
        }
    }
}
