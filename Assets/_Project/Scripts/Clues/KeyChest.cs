using System.Collections;
using UnityEngine;

// RQF-03: cada cofre guarda una de las 3 llaves del nivel y una pista de la historia.
[RequireComponent(typeof(Collider2D))]
public class KeyChest : MonoBehaviour, IInteractable
{
    [Header("Pista")]
    [SerializeField] private string clueTitle = "Pista";
    [SerializeField, TextArea(2, 6)] private string clueText = "";

    [Header("Dato histórico (opcional): sale después de la pista")]
    [SerializeField] private PlaceFactData fact;

    [Header("Puzzle (vacío = el cofre se abre directo)")]
    [SerializeField] private PuzzleData puzzle;
    [SerializeField] private PuzzlePanelUI puzzlePanel;

    [Header("Cofre")]
    [SerializeField] private SpriteRenderer spriteRenderer;
    [SerializeField] private Sprite openSprite;
    [SerializeField] private SpriteRenderer keyPopup;
    [SerializeField] private ParticleSystem openEffect;

    [Header("Nivel")]
    [SerializeField] private LevelProgress progress;
    [SerializeField] private ScoreManager scoreManager;
    [SerializeField] private HUDController hud;
    [SerializeField] private GameEvent chestOpenedEvent;

    public bool IsOpen { get; private set; }
    public string Prompt => IsOpen ? "E: leer la pista otra vez" : "E: abrir el cofre";
    public PuzzleData Puzzle => puzzle;

    public void Interact()
    {
        if (IsOpen)
        {
            hud.ShowMessage(clueTitle, clueText);
            ShowFact();
            return;
        }

        if (puzzle != null && puzzlePanel != null)
        {
            puzzlePanel.Open(puzzle, SolvedAndOpen);
            return;
        }

        Open();
    }

    private void SolvedAndOpen()
    {
        scoreManager.AddScore(scoreManager.Rules.PuzzlePoints);
        Open();
    }

    public void Open()
    {
        if (IsOpen) return;

        IsOpen = true;
        spriteRenderer.sprite = openSprite;
        if (openEffect != null) openEffect.Play();
        if (chestOpenedEvent != null) chestOpenedEvent.Raise();

        // El texto de la pista se muestra primero; el recuerdo (si toca) queda en fila después
        string found = "¡Encontraste una llave! (" + (progress.Keys + 1) + " de " + progress.RequiredKeys + ")\n\n" + clueText;
        hud.ShowMessage(clueTitle, found);
        ShowFact();

        scoreManager.AddScore(scoreManager.Rules.KeyPoints);
        progress.AddKey();

        if (keyPopup != null) StartCoroutine(KeyPopupRoutine());
    }

    private void ShowFact()
    {
        if (fact != null) hud.ShowMessage(fact.Title, fact.Text);
    }

    private IEnumerator KeyPopupRoutine()
    {
        keyPopup.gameObject.SetActive(true);
        Vector3 start = keyPopup.transform.localPosition;
        float time = 0f;

        while (time < 1.2f)
        {
            // Tiempo sin escala porque el juego se detiene mientras se lee la pista
            time += Time.unscaledDeltaTime;
            float t = Mathf.Clamp01(time / 1.2f);
            keyPopup.transform.localPosition = start + Vector3.up * (1.2f * t);
            keyPopup.color = new Color(1f, 1f, 1f, 1f - Mathf.Clamp01((t - 0.6f) / 0.4f));
            yield return null;
        }

        keyPopup.gameObject.SetActive(false);
        keyPopup.transform.localPosition = start;
    }
}
