using UnityEngine;

// RQF-08: punto seguro. Al tocar el farol se enciende y ahí reaparece Aketzali.
[RequireComponent(typeof(Collider2D))]
public class Checkpoint : MonoBehaviour
{
    [SerializeField] private SpriteRenderer spriteRenderer;
    [SerializeField] private Sprite onSprite;
    [SerializeField] private Transform respawnPoint;
    [SerializeField] private HUDController hud;
    [SerializeField] private GameEvent activatedEvent;

    private bool activated;

    private void OnTriggerEnter2D(Collider2D other)
    {
        var lives = other.GetComponentInParent<PlayerLives>();
        if (lives == null || activated) return;

        activated = true;
        spriteRenderer.sprite = onSprite;
        lives.SetCheckpoint(respawnPoint != null ? respawnPoint.position : transform.position);
        if (hud != null) hud.ShowNotice("Punto seguro activado", 2f);
        if (activatedEvent != null) activatedEvent.Raise();
    }
}
