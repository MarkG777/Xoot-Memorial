using UnityEngine;

// Animación sencilla de pocos cuadros (aleteo del murciélago, flotar del jefe).
public class SpriteFlipbook : MonoBehaviour
{
    [SerializeField] private SpriteRenderer spriteRenderer;
    [SerializeField] private Sprite[] frames;
    [SerializeField] private float framesPerSecond = 8f;
    [SerializeField] private float floatAmplitude = 0f;
    [SerializeField] private float floatFrequency = 1.5f;

    private Vector3 startLocalPosition;

    private void Awake()
    {
        startLocalPosition = spriteRenderer.transform.localPosition;
    }

    private void Update()
    {
        if (frames != null && frames.Length > 1)
            spriteRenderer.sprite = frames[Mathf.FloorToInt(Time.time * framesPerSecond) % frames.Length];

        if (floatAmplitude > 0f)
            spriteRenderer.transform.localPosition = startLocalPosition + Vector3.up * (Mathf.Sin(Time.time * floatFrequency) * floatAmplitude);
    }
}
