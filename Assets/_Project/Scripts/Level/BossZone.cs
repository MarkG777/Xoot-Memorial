using UnityEngine;

// Zona invisible que avisa al jefe: marca el inicio o el final de una fase.
[RequireComponent(typeof(Collider2D))]
public class BossZone : MonoBehaviour
{
    [SerializeField] private BossController boss;
    [SerializeField, Min(0)] private int phaseIndex;
    [SerializeField] private bool isGoal;

    private void OnTriggerEnter2D(Collider2D other)
    {
        if (other.GetComponentInParent<PlayerLives>() == null) return;

        if (isGoal) boss.ClearPhase(phaseIndex);
        else boss.EnterPhase(phaseIndex);
    }
}
