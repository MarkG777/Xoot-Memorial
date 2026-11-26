using UnityEngine;

// AU: las estadísticas del jugador viven en un archivo de datos, no en un objeto de la escena.
[CreateAssetMenu(fileName = "PlayerStats", menuName = "Xoot/Datos del jugador")]
public class PlayerStats : ScriptableObject
{
    [Header("Movimiento")]
    [SerializeField] private float walkSpeed = 4f;
    [SerializeField] private float runSpeed = 6.5f;

    [Header("Salto")]
    [SerializeField] private float jumpSpeed = 13f;
    [SerializeField] private float coyoteTime = 0.1f;
    [SerializeField] private float jumpBufferTime = 0.12f;
    [SerializeField, Range(0.1f, 1f)] private float jumpCutMultiplier = 0.55f;
    [SerializeField] private float maxFallSpeed = 18f;

    [Header("Vidas")]
    [SerializeField, Min(1)] private int maxLives = 3;
    [SerializeField] private float respawnDelay = 0.8f;
    [SerializeField] private float invulnerableTime = 1.5f;

    public float WalkSpeed => walkSpeed;
    public float RunSpeed => runSpeed;
    public float JumpSpeed => jumpSpeed;
    public float CoyoteTime => coyoteTime;
    public float JumpBufferTime => jumpBufferTime;
    public float JumpCutMultiplier => jumpCutMultiplier;
    public float MaxFallSpeed => maxFallSpeed;
    public int MaxLives => maxLives;
    public float RespawnDelay => respawnDelay;
    public float InvulnerableTime => invulnerableTime;
}
