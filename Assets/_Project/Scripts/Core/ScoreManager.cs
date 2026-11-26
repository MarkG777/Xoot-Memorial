using System;
using UnityEngine;

// Administra el puntaje. No escribe textos: solo avisa con ScoreChanged (como en la P5).
public class ScoreManager : MonoBehaviour
{
    [SerializeField] private ScoreRules rules;

    public int Score { get; private set; }
    public ScoreRules Rules => rules;

    public event Action<int> ScoreChanged;

    private void Awake()
    {
        // Cada nivel arranca con lo acumulado; si se reinicia, se pierde solo lo de este intento
        Score = GameSession.TotalScore;
    }

    public void AddScore(int amount)
    {
        if (amount <= 0) return;

        Score += amount;
        ScoreChanged?.Invoke(Score);
    }

    public void RemoveScore(int amount)
    {
        if (amount <= 0) return;

        Score = Mathf.Max(0, Score - amount);
        ScoreChanged?.Invoke(Score);
    }

    // Se llama al terminar el nivel para conservar el puntaje en el siguiente
    public void SaveToSession()
    {
        GameSession.TotalScore = Score;
    }
}
