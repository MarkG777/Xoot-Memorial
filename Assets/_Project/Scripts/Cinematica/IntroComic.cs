using System.Collections;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

// Secuencia inicial (RQF-06): muestra las viñetas del storyboard una tras otra, sin control del jugador.
// Cada viñeta entra con un fundido y se acerca un poco mientras dura; Enter, Espacio o Esc saltan la secuencia.
public class IntroComic : MonoBehaviour
{
    [System.Serializable]
    public class Panel
    {
        public Sprite image;
        [Min(0.5f)] public float duration = 3f;
        public float zoomStart = 1f;
        public float zoomEnd = 1.06f;
        public AudioClip sound;
    }

    [SerializeField] private Image panelImage;
    [SerializeField] private CanvasGroup panelGroup;
    [SerializeField] private AudioSource audioSource;
    [SerializeField] private Panel[] panels;
    [SerializeField] private float fadeTime = 0.4f;
    [SerializeField] private string nextScene = "Prototipo_Estacion";

    private bool skipRequested;

    private void Start()
    {
        StartCoroutine(PlaySequence());
    }

    private void Update()
    {
        Keyboard keyboard = Keyboard.current;
        if (keyboard == null)
        {
            return;
        }

        if (keyboard.enterKey.wasPressedThisFrame || keyboard.spaceKey.wasPressedThisFrame || keyboard.escapeKey.wasPressedThisFrame)
        {
            skipRequested = true;
        }
    }

    private IEnumerator PlaySequence()
    {
        foreach (Panel panel in panels)
        {
            if (skipRequested)
            {
                break;
            }

            panelImage.sprite = panel.image;
            panelImage.rectTransform.localScale = Vector3.one * panel.zoomStart;

            if (panel.sound != null && audioSource != null)
            {
                audioSource.PlayOneShot(panel.sound);
            }

            float time = 0f;
            while (time < panel.duration && !skipRequested)
            {
                time += Time.deltaTime;

                // Fundido de entrada al inicio y de salida al final de la viñeta
                float fadeIn = Mathf.Clamp01(time / fadeTime);
                float fadeOut = Mathf.Clamp01((panel.duration - time) / fadeTime);
                panelGroup.alpha = Mathf.Min(fadeIn, fadeOut);

                float progress = Mathf.Clamp01(time / panel.duration);
                panelImage.rectTransform.localScale = Vector3.one * Mathf.Lerp(panel.zoomStart, panel.zoomEnd, progress);
                yield return null;
            }
        }

        SceneManager.LoadScene(nextScene);
    }
}
