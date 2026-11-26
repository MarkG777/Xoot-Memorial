using Unity.Cinemachine;
using UnityEngine;

// DE: sacudida de cámara (Cinemachine Impulse) ante impactos. Escucha eventos en ScriptableObjects,
// así que no necesita conocer al jugador ni al jefe.
[RequireComponent(typeof(CinemachineImpulseSource))]
public class CameraShake : MonoBehaviour
{
    [SerializeField] private GameEvent lifeLostEvent;
    [SerializeField] private GameEvent bossAppearedEvent;
    [SerializeField] private GameEvent phaseClearedEvent;
    [SerializeField] private float hitForce = 0.45f;
    [SerializeField] private float bossForce = 0.3f;

    private CinemachineImpulseSource source;

    private void Awake()
    {
        source = GetComponent<CinemachineImpulseSource>();
    }

    private void OnEnable()
    {
        if (lifeLostEvent != null) lifeLostEvent.Raised += ShakeHit;
        if (bossAppearedEvent != null) bossAppearedEvent.Raised += ShakeBoss;
        if (phaseClearedEvent != null) phaseClearedEvent.Raised += ShakeBoss;
    }

    private void OnDisable()
    {
        if (lifeLostEvent != null) lifeLostEvent.Raised -= ShakeHit;
        if (bossAppearedEvent != null) bossAppearedEvent.Raised -= ShakeBoss;
        if (phaseClearedEvent != null) phaseClearedEvent.Raised -= ShakeBoss;
    }

    private void ShakeHit() => source.GenerateImpulseWithForce(hitForce);

    private void ShakeBoss() => source.GenerateImpulseWithForce(bossForce);
}
