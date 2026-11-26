using System.Collections;
using UnityEngine;

// RQF-07: la puerta de salida. Se abre con las 3 llaves (y cuando el jefe ya se calmó, si hay jefe).
[RequireComponent(typeof(Collider2D))]
public class ExitDoor : MonoBehaviour
{
    [SerializeField] private LevelProgress progress;
    [SerializeField] private LevelManager levelManager;
    [SerializeField] private HUDController hud;
    [SerializeField] private BossController boss;
    [SerializeField] private SpriteRenderer spriteRenderer;
    [SerializeField] private Sprite openSprite;
    [SerializeField] private GameEvent doorOpenedEvent;
    [SerializeField] private ParticleSystem openEffect;

    private bool opened;

    private void OnTriggerEnter2D(Collider2D other)
    {
        if (opened || other.GetComponentInParent<PlayerLives>() == null) return;

        if (!progress.HasAllKeys)
        {
            int missing = progress.RequiredKeys - progress.Keys;
            hud.ShowNotice(missing == 1 ? "Te falta 1 llave" : "Te faltan " + missing + " llaves", 2.5f);
            return;
        }

        if (boss != null && !boss.IsCalmed)
        {
            hud.ShowNotice("La leyenda todavía bloquea la puerta", 2.5f);
            return;
        }

        opened = true;
        StartCoroutine(OpenRoutine());
    }

    private IEnumerator OpenRoutine()
    {
        spriteRenderer.sprite = openSprite;
        if (openEffect != null) openEffect.Play();
        if (doorOpenedEvent != null) doorOpenedEvent.Raise();

        yield return new WaitForSeconds(0.6f);
        levelManager.CompleteLevel();
    }
}
