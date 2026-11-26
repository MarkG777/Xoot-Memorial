using UnityEngine;

// Cualquier peligro (pinchos, vacío, murciélago, sombra): al tocarlo se pierde una vida.
[RequireComponent(typeof(Collider2D))]
public class Hazard : MonoBehaviour
{
    private void OnTriggerEnter2D(Collider2D other)
    {
        Hit(other);
    }

    // Stay: si el jugador sigue dentro cuando termina la invulnerabilidad, vuelve a contar
    private void OnTriggerStay2D(Collider2D other)
    {
        Hit(other);
    }

    private void OnCollisionEnter2D(Collision2D collision)
    {
        Hit(collision.collider);
    }

    private static void Hit(Collider2D other)
    {
        var lives = other.GetComponentInParent<PlayerLives>();
        if (lives != null) lives.LoseLife();
    }
}
