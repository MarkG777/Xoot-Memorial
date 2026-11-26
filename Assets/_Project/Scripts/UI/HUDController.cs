using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

// Tablero del juego: corazones arriba a la izquierda, llaves y puntaje. Solo muestra datos
// que mantienen otros componentes (como el HUDController de la P5).
public class HUDController : MonoBehaviour, IMessageView
{
    private struct Message
    {
        public string title;
        public string body;
        public Sprite image;
        public bool flash;
    }

    [Header("Fuentes de los datos")]
    [SerializeField] private LevelManager levelManager;
    [SerializeField] private PlayerLives playerLives;
    [SerializeField] private LevelProgress progress;
    [SerializeField] private ScoreManager scoreManager;

    [Header("Tablero")]
    [SerializeField] private Image[] hearts;
    [SerializeField] private Sprite heartFull;
    [SerializeField] private Sprite heartEmpty;
    [SerializeField] private Image[] keyIcons;
    [SerializeField] private Sprite keyFull;
    [SerializeField] private Sprite keyEmpty;
    [SerializeField] private TMP_Text scoreText;
    [SerializeField] private TMP_Text levelText;

    [Header("Avisos y lectura")]
    [SerializeField] private GameObject promptPanel;
    [SerializeField] private TMP_Text promptText;
    [SerializeField] private GameObject messagePanel;
    [SerializeField] private TMP_Text messageTitle;
    [SerializeField] private TMP_Text messageBody;
    [SerializeField] private Image messageImage;

    [Header("RQF-05: fundido a blanco al mostrar un recuerdo")]
    [SerializeField, Min(0.1f)] private float flashSeconds = 1.2f;

    private Image whiteFlash;

    private readonly Queue<Message> pending = new Queue<Message>();
    private float noticeTimer;

    public bool IsReading => messagePanel.activeSelf;

    private void Awake()
    {
        MessageView.Current = this;
        messagePanel.SetActive(false);
        promptPanel.SetActive(false);
    }

    private void OnEnable()
    {
        playerLives.LivesChanged += HandleLivesChanged;
        progress.KeysChanged += HandleKeysChanged;
        scoreManager.ScoreChanged += HandleScoreChanged;
    }

    private void OnDisable()
    {
        playerLives.LivesChanged -= HandleLivesChanged;
        progress.KeysChanged -= HandleKeysChanged;
        scoreManager.ScoreChanged -= HandleScoreChanged;
    }

    private void Start()
    {
        HandleLivesChanged(playerLives.CurrentLives);
        HandleKeysChanged(progress.Keys, progress.RequiredKeys);
        HandleScoreChanged(scoreManager.Score);

        if (progress.Data != null && levelText != null)
            levelText.text = progress.Data.LevelName + " · " + progress.Data.RegionName;
    }

    private void Update()
    {
        // Los avisos temporales se quitan solos
        if (noticeTimer > 0f)
        {
            noticeTimer -= Time.unscaledDeltaTime;
            if (noticeTimer <= 0f) promptPanel.SetActive(false);
        }
    }

    public void SetPrompt(string text)
    {
        noticeTimer = 0f;
        promptText.text = text;
        promptPanel.SetActive(!string.IsNullOrEmpty(text));
    }

    // Aviso corto en la parte de abajo (por ejemplo, "Te faltan 2 llaves")
    public void ShowNotice(string text, float seconds)
    {
        SetPrompt(text);
        noticeTimer = seconds;
    }

    public void ShowMessage(string title, string body)
    {
        Enqueue(title, body, null);
    }

    public void ShowMemory(string title, string body, Sprite image)
    {
        Enqueue(title, body, image);
    }

    // Como ShowMemory, pero entra con un fundido a blanco: solo para los recuerdos
    public void ShowRecollection(string title, string body, Sprite image)
    {
        Enqueue(title, body, image, true);
    }

    public void HideMessage()
    {
        if (pending.Count > 0)
        {
            Show(pending.Dequeue());
            if (levelManager != null) levelManager.SetReading(true);
            return;
        }

        messagePanel.SetActive(false);
        if (levelManager != null) levelManager.SetReading(false);
    }

    private void Enqueue(string title, string body, Sprite image, bool flash = false)
    {
        var message = new Message { title = title, body = body, image = image, flash = flash };

        if (IsReading)
        {
            pending.Enqueue(message);
            return;
        }

        Show(message);
        if (levelManager != null) levelManager.SetReading(true);
    }

    private void Show(Message message)
    {
        messageTitle.text = message.title;
        messageBody.text = message.body;

        if (messageImage != null)
        {
            messageImage.gameObject.SetActive(message.image != null);
            messageImage.sprite = message.image;
        }

        messagePanel.SetActive(true);
        if (message.flash) StartCoroutine(FlashRoutine());
    }

    private IEnumerator FlashRoutine()
    {
        if (whiteFlash == null) whiteFlash = CreateWhiteFlash();
        whiteFlash.transform.SetAsLastSibling();
        whiteFlash.enabled = true;

        float time = 0f;
        while (time < flashSeconds)
        {
            whiteFlash.color = new Color(1f, 1f, 1f, 1f - time / flashSeconds);
            // Sin escala de tiempo: el juego está detenido mientras se lee
            time += Time.unscaledDeltaTime;
            yield return null;
        }

        whiteFlash.enabled = false;
    }

    // Un rectángulo blanco a pantalla completa que se crea la primera vez
    private Image CreateWhiteFlash()
    {
        var go = new GameObject("FundidoBlanco", typeof(RectTransform), typeof(Image));
        var rect = (RectTransform)go.transform;
        rect.SetParent(messagePanel.transform.parent, false);
        rect.anchorMin = Vector2.zero;
        rect.anchorMax = Vector2.one;
        rect.offsetMin = Vector2.zero;
        rect.offsetMax = Vector2.zero;

        var image = go.GetComponent<Image>();
        image.raycastTarget = false;
        return image;
    }

    private void HandleLivesChanged(int lives)
    {
        for (int i = 0; i < hearts.Length; i++)
            hearts[i].sprite = i < lives ? heartFull : heartEmpty;
    }

    private void HandleKeysChanged(int keys, int required)
    {
        for (int i = 0; i < keyIcons.Length; i++)
        {
            keyIcons[i].gameObject.SetActive(i < required);
            keyIcons[i].sprite = i < keys ? keyFull : keyEmpty;
        }
    }

    private void HandleScoreChanged(int score)
    {
        scoreText.text = "Puntos: " + score;
    }
}
