using System;
using System.Collections;
using UnityEngine;

// 3 vidas (base de la P3 y la P5). Al perder una, Aketzali reaparece en el último punto seguro
// sin perder las llaves. Al perder las tres se avisa al LevelManager.
[RequireComponent(typeof(PlayerController2D), typeof(Rigidbody2D))]
public class PlayerLives : MonoBehaviour
{
    [SerializeField] private PlayerStats stats;
    [SerializeField] private LevelManager levelManager;
    [SerializeField] private SpriteRenderer spriteRenderer;
    [SerializeField] private GameEvent lifeLostEvent;
    [SerializeField] private GameEvent respawnedEvent;

    private PlayerController2D controller;
    private Rigidbody2D body;
    private Vector3 checkpointPosition;
    private bool invulnerable;
    private bool hurting;

    public int CurrentLives { get; private set; }
    public int MaxLives => stats.MaxLives;
    public bool IsHurting => hurting;

    public event Action<int> LivesChanged;
    public event Action LifeLost;
    public event Action OutOfLives;
    public event Action Respawned;

    private void Awake()
    {
        controller = GetComponent<PlayerController2D>();
        body = GetComponent<Rigidbody2D>();
        CurrentLives = stats.MaxLives;
        checkpointPosition = transform.position;
    }

    public void SetCheckpoint(Vector3 position)
    {
        checkpointPosition = position;
    }

    // Lo llaman los peligros (pinchos, vacío, murciélagos)
    public void LoseLife()
    {
        if (invulnerable || hurting || CurrentLives <= 0) return;
        if (levelManager != null && !levelManager.IsPlaying) return;

        CurrentLives--;
        hurting = true;
        LivesChanged?.Invoke(CurrentLives);
        LifeLost?.Invoke();
        if (lifeLostEvent != null) lifeLostEvent.Raise();

        StartCoroutine(HurtRoutine());
    }

    private IEnumerator HurtRoutine()
    {
        controller.InputEnabled = false;
        controller.ResetMotion();
        body.simulated = false;

        yield return new WaitForSeconds(stats.RespawnDelay);

        if (CurrentLives <= 0)
        {
            OutOfLives?.Invoke();
            yield break;
        }

        transform.position = checkpointPosition;
        body.position = checkpointPosition;
        body.simulated = true;
        controller.ResetMotion();
        controller.InputEnabled = true;
        hurting = false;
        Respawned?.Invoke();
        if (respawnedEvent != null) respawnedEvent.Raise();

        // Parpadeo mientras es invulnerable para no perder dos vidas seguidas
        invulnerable = true;
        float time = 0f;
        while (time < stats.InvulnerableTime)
        {
            spriteRenderer.enabled = Mathf.FloorToInt(time * 10f) % 2 == 0;
            time += Time.deltaTime;
            yield return null;
        }

        spriteRenderer.enabled = true;
        invulnerable = false;
    }
}
