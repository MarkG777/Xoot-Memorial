using UnityEngine;
using UnityEngine.InputSystem;

// RQF-01: mover (A/D o flechas) y saltar (Espacio). Salto simple, sin doble salto.
[RequireComponent(typeof(Rigidbody2D), typeof(Collider2D))]
public class PlayerController2D : MonoBehaviour
{
    [SerializeField] private float moveSpeed = 6f;
    [SerializeField] private float jumpSpeed = 11f;
    [SerializeField] private float groundCheckDistance = 0.1f;

    private readonly RaycastHit2D[] groundHits = new RaycastHit2D[4];
    private Rigidbody2D body;
    private Collider2D bodyCollider;
    private ContactFilter2D groundFilter;
    private float moveInput;
    private bool jumpRequested;

    private void Awake()
    {
        body = GetComponent<Rigidbody2D>();
        bodyCollider = GetComponent<Collider2D>();
        groundFilter = new ContactFilter2D { useTriggers = false };
    }

    private void Update()
    {
        var keyboard = Keyboard.current;
        if (keyboard == null) return;

        moveInput = 0f;
        if (keyboard.aKey.isPressed || keyboard.leftArrowKey.isPressed) moveInput -= 1f;
        if (keyboard.dKey.isPressed || keyboard.rightArrowKey.isPressed) moveInput += 1f;

        if (keyboard.spaceKey.wasPressedThisFrame) jumpRequested = true;
    }

    private void FixedUpdate()
    {
        var velocity = body.linearVelocity;
        velocity.x = moveInput * moveSpeed;

        if (jumpRequested && IsGrounded()) velocity.y = jumpSpeed;
        jumpRequested = false;

        body.linearVelocity = velocity;
    }

    private bool IsGrounded()
    {
        return bodyCollider.Cast(Vector2.down, groundFilter, groundHits, groundCheckDistance) > 0;
    }
}
