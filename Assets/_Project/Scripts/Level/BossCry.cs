using TMPro;
using UnityEngine;

// El llanto de La Llorona: se oye a lo lejos durante el nivel y fuerte cuando ella aparece.
// El sonido viene del lado donde está y en pantalla se lee lo que dice.
public class BossCry : MonoBehaviour
{
    [SerializeField] private BossController boss;
    [SerializeField] private LevelManager levelManager;
    [SerializeField] private AudioSource source;
    [SerializeField] private AudioClip cryClip;
    [SerializeField] private TMP_Text cryText;
    [SerializeField] private Transform player;
    [SerializeField] private Transform bossBody;
    [SerializeField] private string phrase = "¡Ay, mis hijos!";

    [Header("A lo lejos (antes de que aparezca)")]
    [SerializeField] private float firstDistantCry = 6f;
    [SerializeField] private float distantEvery = 28f;
    [SerializeField, Range(0f, 1f)] private float distantVolume = 0.3f;

    [Header("Durante el encuentro")]
    [SerializeField] private float bossEvery = 10f;
    [SerializeField, Range(0f, 1f)] private float bossVolume = 0.9f;

    [SerializeField] private float textSeconds = 3.8f;

    private float timer;
    private float textTimer;
    private float textStrength = 1f;
    private bool wasPresent;

    private void Awake()
    {
        timer = firstDistantCry;
        SetTextAlpha(0f);
    }

    private void Update()
    {
        FadeText();

        if (!levelManager.IsPlaying) return;

        bool present = boss.HasAppeared && !boss.IsCalmed;
        if (present && !wasPresent) timer = 0.4f;
        wasPresent = present;

        // Cuando la leyenda se calma, el llanto termina
        if (boss.IsCalmed) return;

        timer -= Time.deltaTime;
        if (timer > 0f) return;

        timer = present ? bossEvery : distantEvery;
        Cry(present);
    }

    private void Cry(bool near)
    {
        if (player != null && bossBody != null)
            source.panStereo = Mathf.Clamp((bossBody.position.x - player.position.x) / 10f, -0.8f, 0.8f);

        source.PlayOneShot(cryClip, near ? bossVolume : distantVolume);

        cryText.text = near ? phrase : "(a lo lejos) ..." + phrase.ToLower();
        textStrength = near ? 1f : 0.65f;
        textTimer = textSeconds;
    }

    private void FadeText()
    {
        if (textTimer <= 0f) return;

        textTimer -= Time.deltaTime;
        float fadeIn = Mathf.Clamp01((textSeconds - textTimer) / 0.6f);
        float fadeOut = Mathf.Clamp01(textTimer / 1f);
        SetTextAlpha(Mathf.Min(fadeIn, fadeOut) * textStrength);

        // Un leve temblor, como la voz
        cryText.rectTransform.localScale = Vector3.one * (1f + Mathf.Sin(Time.time * 9f) * 0.025f);
    }

    private void SetTextAlpha(float alpha)
    {
        Color color = cryText.color;
        color.a = alpha;
        cryText.color = color;
    }
}
