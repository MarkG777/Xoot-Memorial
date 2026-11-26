using System;
using UnityEngine;

// RQF-05: la niebla del olvido. Cada nivel empieza con más niebla que el anterior (LevelData)
// y se va aclarando con cada cofre que se abre.
public class ForgettingFog : MonoBehaviour
{
    [Serializable]
    public class Layer
    {
        public SpriteRenderer renderer;
        [Tooltip("Unidades por segundo; en negativo se mueve hacia el otro lado")]
        public float speed = 0.35f;
        [Range(0f, 1f)] public float weight = 0.7f;
    }

    [SerializeField] private LevelProgress progress;
    [SerializeField] private Layer[] layers;
    [Tooltip("Cuánta niebla queda con 0, 1, 2 y 3 llaves")]
    [SerializeField] private float[] amountPerKey = { 1f, 0.65f, 0.35f, 0f };
    [SerializeField] private float changePerSecond = 0.2f;
    [SerializeField] private float tileWidth = 24f;

    private float current;
    private float target;

    // Entre 0 (despejado) y 1 (toda la niebla del nivel)
    public float Amount => current;

    private void Awake()
    {
        current = target = AmountFor(0);
    }

    private void OnEnable()
    {
        if (progress != null) progress.KeysChanged += HandleKeysChanged;
    }

    private void OnDisable()
    {
        if (progress != null) progress.KeysChanged -= HandleKeysChanged;
    }

    private void LateUpdate()
    {
        // Con escala de tiempo: la niebla se levanta cuando se cierran los mensajes y así se alcanza a ver
        current = Mathf.MoveTowards(current, target, changePerSecond * Time.deltaTime);

        float density = progress != null && progress.Data != null ? progress.Data.FogDensity : 0.6f;
        foreach (Layer layer in layers)
        {
            Color color = layer.renderer.color;
            color.a = density * current * layer.weight;
            layer.renderer.color = color;
            layer.renderer.enabled = color.a > 0.005f;

            Vector3 position = layer.renderer.transform.localPosition;
            position.x = Mathf.Repeat(Time.time * layer.speed, tileWidth) - tileWidth * 0.5f;
            layer.renderer.transform.localPosition = position;
        }
    }

    private void HandleKeysChanged(int keys, int required)
    {
        target = AmountFor(keys);
    }

    private float AmountFor(int keys)
    {
        if (amountPerKey == null || amountPerKey.Length == 0) return 0f;
        return amountPerKey[Mathf.Clamp(keys, 0, amountPerKey.Length - 1)];
    }
}
