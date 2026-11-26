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

    private void Enqueue(string title, string body, Sprite image)
    {
        var message = new Message { title = title, body = body, image = image };

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
