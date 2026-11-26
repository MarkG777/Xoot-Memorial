using UnityEngine;
using UnityEngine.Rendering;
using UnityEngine.Rendering.Universal;

// DE (post-processing): el nivel empieza apagado por la niebla del olvido y recupera
// su color con cada llave que se encuentra.
[RequireComponent(typeof(Volume))]
public class MemoryColor : MonoBehaviour
{
    [SerializeField] private LevelProgress progress;
    [SerializeField] private float startSaturation = -45f;
    [SerializeField] private float endSaturation = 10f;
    [SerializeField] private float changePerSecond = 30f;

    private ColorAdjustments colorAdjustments;
    private float targetSaturation;

    private void Awake()
    {
        // profile (no sharedProfile) crea una copia: el archivo original no se modifica al jugar
        GetComponent<Volume>().profile.TryGet(out colorAdjustments);
        targetSaturation = startSaturation;
        if (colorAdjustments != null) colorAdjustments.saturation.Override(startSaturation);
    }

    private void OnEnable()
    {
        if (progress != null) progress.KeysChanged += HandleKeysChanged;
    }

    private void OnDisable()
    {
        if (progress != null) progress.KeysChanged -= HandleKeysChanged;
    }

    private void Update()
    {
        if (colorAdjustments == null) return;

        // Sin escala de tiempo: el color cambia aunque haya un mensaje abierto
        float value = Mathf.MoveTowards(colorAdjustments.saturation.value, targetSaturation, changePerSecond * Time.unscaledDeltaTime);
        colorAdjustments.saturation.value = value;
    }

    private void HandleKeysChanged(int keys, int required)
    {
        targetSaturation = Mathf.Lerp(startSaturation, endSaturation, required > 0 ? (float)keys / required : 1f);
    }
}
