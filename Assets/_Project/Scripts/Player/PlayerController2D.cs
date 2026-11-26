using System;
using UnityEngine;
using UnityEngine.InputSystem;

// RQF-01: mover (A/D o flechas), correr (mantener Shift) y saltar (Espacio). Salto simple, sin doble salto.
[RequireComponent(typeof(Rigidbody2D), typeof(Collider2D))]
public class PlayerController2D : MonoBehaviour
{
    [Header("Datos (si se asigna, reemplaza a los valores de abajo)")]
    [SerializeField] private PlayerStats stats;

    [SerializeField] private float moveSpeed = 6f;
    [SerializeField] private float jumpSpeed = 11f;
    [SerializeField] private float groundCheckDistance = 0.1f;

    private readonly RaycastHit2D[] groundHits = new RaycastHit2D[6];
    private Rigidbody2D body;
    private Collider2D bodyCollider;
    private ContactFilter2D groundFilter;
    private float moveInput;
    private bool runHeld;
    private bool jumpHeld;
    private float jumpBufferTimer;
    private float coyoteTimer;
    private bool jumpCutAvailable;
    private bool inputEnabled = true;

    public bool IsGrounded { get; private set; }
    public float MoveInput => moveInput;
    public bool IsRunning => runHeld && Mathf.Abs(moveInput) > 0.1f;
    public Vector2 Velocity => body.linearVelocity;
    public float RunSpeed => stats != null ? stats.RunSpeed : moveSpeed;

    // El LevelManager y PlayerLives apagan los controles al leer, pausar o reaparecer
    public bool InputEnabled
    {
        get => inputEnabled;
        set
        {
            inputEnabled = value;
            if (!value)
            {
                moveInput = 0f;
                jumpBufferTimer = 0f;
            }
        }
    }

    public event Action Jumped;

    private void Awake()
    {
        body = GetComponent<Rigidbody2D>();
        bodyCollider = GetComponent<Collider2D>();
        groundFilter = new ContactFilter2D { useTriggers = false };
    }

    private void Update()
    {
        var keyboard = Keyboard.current;
        if (keyboard == null || !inputEnabled) return;

        moveInput = 0f;
        if (keyboard.aKey.isPressed || keyboard.leftArrowKey.isPressed) moveInput -= 1f;
        if (keyboard.dKey.isPressed || keyboard.rightArrowKey.isPressed) moveInput += 1f;

        runHeld = keyboard.leftShiftKey.isPressed || keyboard.rightShiftKey.isPressed;
        jumpHeld = keyboard.spaceKey.isPressed;

        // El salto se recuerda un instante por si se presiona justo antes de tocar el suelo
        if (keyboard.spaceKey.wasPressedThisFrame)
            jumpBufferTimer = stats != null ? stats.JumpBufferTime : 0.02f;
    }

    private void FixedUpdate()
    {
        IsGrounded = CheckGrounded();

        // Margen para saltar un instante después de dejar la orilla
        if (IsGrounded && body.linearVelocity.y <= 0.1f) coyoteTimer = stats != null ? stats.CoyoteTime : 0.02f;
        else coyoteTimer -= Time.fixedDeltaTime;

        float speed = moveSpeed;
        if (stats != null) speed = runHeld ? stats.RunSpeed : stats.WalkSpeed;

        var velocity = body.linearVelocity;
        velocity.x = moveInput * speed;

        if (jumpBufferTimer > 0f && coyoteTimer > 0f)
        {
            velocity.y = stats != null ? stats.JumpSpeed : jumpSpeed;
            jumpBufferTimer = 0f;
            coyoteTimer = 0f;
            jumpCutAvailable = true;
            Jumped?.Invoke();
        }

        // Si se suelta Espacio mientras sube, el salto queda más corto
        if (stats != null && jumpCutAvailable && !jumpHeld && velocity.y > 0f)
        {
            velocity.y *= stats.JumpCutMultiplier;
            jumpCutAvailable = false;
        }

        if (stats != null) velocity.y = Mathf.Max(velocity.y, -stats.MaxFallSpeed);

        jumpBufferTimer -= Time.fixedDeltaTime;
        body.linearVelocity = velocity;
    }

    public void ResetMotion()
    {
        moveInput = 0f;
        jumpBufferTimer = 0f;
        coyoteTimer = 0f;
        jumpCutAvailable = false;
        body.linearVelocity = Vector2.zero;
    }

    private bool CheckGrounded()
    {
        int count = bodyCollider.Cast(Vector2.down, groundFilter, groundHits, groundCheckDistance);
        for (int i = 0; i < count; i++)
        {
            // Solo cuenta como suelo una superficie que mira hacia arriba (no las paredes)
            if (groundHits[i].normal.y > 0.6f) return true;
        }

        return false;
    }
}
