using System.Collections;
using UnityEngine;

// Plataforma que tiembla al pisarla, se cae y vuelve a aparecer después de un tiempo.
[RequireComponent(typeof(Rigidbody2D), typeof(Collider2D))]
public class FallingPlatform : MonoBehaviour
{
    [SerializeField] private Transform visual;
    [SerializeField] private float shakeTime = 0.55f;
    [SerializeField] private float fallTime = 0.9f;
    [SerializeField] private float respawnTime = 2.5f;
    [SerializeField] private float fallAcceleration = 25f;

    private Rigidbody2D body;
    private Collider2D platformCollider;
    private SpriteRenderer[] renderers;
    private Vector2 startPosition;
    private bool busy;

    private void Awake()
    {
        body = GetComponent<Rigidbody2D>();
        platformCollider = GetComponent<Collider2D>();
        renderers = GetComponentsInChildren<SpriteRenderer>();
        body.bodyType = RigidbodyType2D.Kinematic;
        startPosition = body.position;
    }

    private void OnCollisionEnter2D(Collision2D collision)
    {
        if (busy || collision.collider.GetComponentInParent<PlayerLives>() == null) return;

        // Solo si el jugador la pisa desde arriba
        if (collision.collider.bounds.min.y < platformCollider.bounds.max.y - 0.15f) return;

        StartCoroutine(FallRoutine());
    }

    private IEnumerator FallRoutine()
    {
        busy = true;

        float time = 0f;
        while (time < shakeTime)
        {
            time += Time.deltaTime;
            visual.localPosition = new Vector3(Mathf.Sin(time * 70f) * 0.06f, 0f, 0f);
            yield return null;
        }

        visual.localPosition = Vector3.zero;

        float speed = 0f;
        time = 0f;
        while (time < fallTime)
        {
            yield return new WaitForFixedUpdate();
            time += Time.fixedDeltaTime;
            speed += fallAcceleration * Time.fixedDeltaTime;
            body.MovePosition(body.position + Vector2.down * speed * Time.fixedDeltaTime);
        }

        SetVisible(false);
        yield return new WaitForSeconds(respawnTime);

        body.position = startPosition;
        transform.position = startPosition;
        SetVisible(true);
        busy = false;
    }

    private void SetVisible(bool visible)
    {
        platformCollider.enabled = visible;
        foreach (SpriteRenderer sprite in renderers) sprite.enabled = visible;
    }
}
