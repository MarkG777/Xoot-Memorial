using UnityEngine;

// DE: manda al Animator la velocidad (Blend Tree Idle-Walk-Run), si está en el suelo y la velocidad vertical.
public class PlayerAnimation2D : MonoBehaviour
{
    [SerializeField] private PlayerController2D controller;
    [SerializeField] private PlayerLives lives;
    [SerializeField] private Animator animator;
    [SerializeField] private SpriteRenderer spriteRenderer;
    [SerializeField] private float damping = 0.08f;

    private static readonly int SpeedId = Animator.StringToHash("Speed");
    private static readonly int GroundedId = Animator.StringToHash("Grounded");
    private static readonly int VerticalId = Animator.StringToHash("VerticalSpeed");
    private static readonly int HurtId = Animator.StringToHash("Hurt");

    private void Update()
    {
        if (Time.timeScale <= 0f) return;

        bool hurt = lives != null && lives.IsHurting;
        Vector2 velocity = controller.Velocity;

        // 0 = reposo, 0.5 = caminar, 1 = correr
        float speed = 0f;
        if (Mathf.Abs(controller.MoveInput) > 0.1f) speed = controller.IsRunning ? 1f : 0.5f;

        animator.SetFloat(SpeedId, speed, damping, Time.deltaTime);
        animator.SetBool(GroundedId, controller.IsGrounded);
        animator.SetFloat(VerticalId, velocity.y);
        animator.SetBool(HurtId, hurt);

        if (controller.MoveInput > 0.1f) spriteRenderer.flipX = false;
        else if (controller.MoveInput < -0.1f) spriteRenderer.flipX = true;
    }
}
