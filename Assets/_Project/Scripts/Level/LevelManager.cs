using System;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.SceneManagement;

// Coordina el estado del nivel: jugando, leyendo, en pausa, completado o sin vidas.
// Los demás componentes consultan IsPlaying antes de actuar (como el GameManager de la P5).
public class LevelManager : MonoBehaviour
{
    public enum LevelState
    {
        Playing,
        Reading,
        Paused,
        Completed,
        Lost,
        Puzzle
    }

    [SerializeField] private LevelProgress progress;
    [SerializeField] private ScoreManager scoreManager;
    [SerializeField] private PlayerLives playerLives;
    [SerializeField] private PlayerController2D playerController;
    [SerializeField] private HUDController hud;

    [Header("Eventos")]
    [SerializeField] private GameEvent levelCompletedEvent;
    [SerializeField] private GameEvent outOfLivesEvent;

    private LevelState state = LevelState.Playing;
    private int readingChangedFrame = -1;

    public LevelState State => state;
    public bool IsPlaying => state == LevelState.Playing;

    // Evita que la misma pulsación de E abra y cierre un mensaje en el mismo cuadro
    public bool CanInteract => IsPlaying && Time.frameCount != readingChangedFrame;

    public event Action<LevelState> StateChanged;

    private void OnEnable()
    {
        playerLives.OutOfLives += HandleOutOfLives;
        playerLives.LifeLost += HandleLifeLost;
    }

    private void OnDisable()
    {
        playerLives.OutOfLives -= HandleOutOfLives;
        playerLives.LifeLost -= HandleLifeLost;
        Time.timeScale = 1f;
    }

    private void Start()
    {
        Time.timeScale = 1f;

        LevelData data = progress.Data;
        if (data != null && !string.IsNullOrEmpty(data.IntroText))
        {
            hud.ShowMessage(data.LevelName + ": " + data.RegionName, data.IntroText);
        }

        // La carta sobre la leyenda queda en fila después de la bienvenida
        if (data != null && data.Legend != null)
        {
            hud.ShowMessage("Carta sobre " + data.Legend.LegendName, data.Legend.LetterText);
        }
    }

    private void Update()
    {
        Keyboard keyboard = Keyboard.current;
        if (keyboard == null) return;

        if (state == LevelState.Reading)
        {
            bool close = keyboard.eKey.wasPressedThisFrame || keyboard.enterKey.wasPressedThisFrame || keyboard.escapeKey.wasPressedThisFrame;
            if (close && Time.frameCount != readingChangedFrame) hud.HideMessage();
            return;
        }

        if (state == LevelState.Puzzle)
        {
            if (keyboard.escapeKey.wasPressedThisFrame) ExitPuzzle();
            return;
        }

        if (keyboard.escapeKey.wasPressedThisFrame && Time.frameCount != readingChangedFrame)
        {
            if (state == LevelState.Playing) Pause();
            else if (state == LevelState.Paused) Resume();
        }
    }

    // Lo llama el HUD cuando abre o cierra el panel de lectura; mientras se lee, el juego se detiene
    public void SetReading(bool reading)
    {
        if (reading && state == LevelState.Playing) SetState(LevelState.Reading);
        else if (!reading && state == LevelState.Reading) SetState(LevelState.Playing);

        readingChangedFrame = Time.frameCount;
    }

    // El panel de puzzles detiene el juego mientras se resuelve
    public bool EnterPuzzle()
    {
        if (state != LevelState.Playing) return false;

        SetState(LevelState.Puzzle);
        return true;
    }

    public void ExitPuzzle()
    {
        if (state != LevelState.Puzzle) return;

        SetState(LevelState.Playing);
        readingChangedFrame = Time.frameCount;
    }

    public void Pause()
    {
        if (state != LevelState.Playing) return;

        SetState(LevelState.Paused);
    }

    public void Resume()
    {
        if (state != LevelState.Paused) return;

        SetState(LevelState.Playing);
    }

    public void CompleteLevel()
    {
        if (state != LevelState.Playing) return;

        ScoreRules rules = scoreManager.Rules;
        scoreManager.AddScore(rules.LevelPoints + rules.PointsPerRemainingLife * playerLives.CurrentLives);

        // Queda guardado que este nivel se pasó, para poder repetirlo desde el menú
        LevelSaves.MarkCompleted(SceneManager.GetActiveScene().name, scoreManager.LevelScore);

        scoreManager.SaveToSession();

        SetState(LevelState.Completed);
        if (levelCompletedEvent != null) levelCompletedEvent.Raise();
    }

    private void HandleLifeLost()
    {
        scoreManager.RemoveScore(scoreManager.Rules.LifeLostPenalty);
    }

    private void HandleOutOfLives()
    {
        if (state == LevelState.Completed) return;

        SetState(LevelState.Lost);
        if (outOfLivesEvent != null) outOfLivesEvent.Raise();
    }

    private void SetState(LevelState newState)
    {
        state = newState;

        // En lectura, pausa y fin de nivel se congela el juego para que nada golpee al jugador
        Time.timeScale = state == LevelState.Playing ? 1f : 0f;
        playerController.InputEnabled = state == LevelState.Playing;

        StateChanged?.Invoke(state);
    }
}
