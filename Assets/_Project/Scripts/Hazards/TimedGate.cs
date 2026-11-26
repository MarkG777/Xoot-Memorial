using UnityEngine;

// Reja que se abre y se cierra con tiempo: sube para dejar pasar y baja para bloquear el camino.
[RequireComponent(typeof(Rigidbody2D))]
public class TimedGate : MonoBehaviour
{
    [SerializeField] private float openHeight = 2.6f;
    [SerializeField] private float openSeconds = 2.2f;
    [SerializeField] private float closedSeconds = 1.8f;
    [SerializeField] private float moveSpeed = 5f;
    [SerializeField] private float startOffset = 0f;

    private Rigidbody2D body;
    private Vector2 closedPosition;
    private float timer;

    private void Awake()
    {
        body = GetComponent<Rigidbody2D>();
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
        body.MovePosition(Vector2.MoveTowards(body.position, target, moveSpeed * Time.fixedDeltaTime));
    }
}
