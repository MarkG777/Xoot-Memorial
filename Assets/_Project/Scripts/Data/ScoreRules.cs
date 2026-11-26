using UnityEngine;

// Reglas del puntaje en un solo archivo de datos, para ajustarlas sin tocar código.
[CreateAssetMenu(fileName = "ScoreRules", menuName = "Xoot/Reglas de puntaje")]
public class ScoreRules : ScriptableObject
{
    [SerializeField] private int keyPoints = 100;
    [SerializeField] private int puzzlePoints = 200;
    [SerializeField] private int memoryPoints = 500;
    [SerializeField] private int levelPoints = 1000;
    [SerializeField] private int pointsPerRemainingLife = 250;
    [SerializeField] private int lifeLostPenalty = 50;

    public int KeyPoints => keyPoints;
    public int PuzzlePoints => puzzlePoints;
    public int MemoryPoints => memoryPoints;
    public int LevelPoints => levelPoints;
    public int PointsPerRemainingLife => pointsPerRemainingLife;
    public int LifeLostPenalty => lifeLostPenalty;
}
