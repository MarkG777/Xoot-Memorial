using UnityEngine;

// Guarda en la computadora qué niveles ya se pasaron y el mejor puntaje de cada uno.
public static class LevelSaves
{
    private const string CompletedKey = "xoot.completado.";
    private const string ScoreKey = "xoot.mejorpuntaje.";
    private const string ReviewKey = "xoot.modorevision";

    // Modo revisión: abre todos los niveles (por ejemplo, para que el profesor pruebe cualquiera)
    public static bool ReviewMode
    {
        get => PlayerPrefs.GetInt(ReviewKey, 0) == 1;
        set
        {
            PlayerPrefs.SetInt(ReviewKey, value ? 1 : 0);
            PlayerPrefs.Save();
        }
    }

    public static bool IsCompleted(string sceneName)
    {
        return PlayerPrefs.GetInt(CompletedKey + sceneName, 0) == 1;
    }

    public static int BestScore(string sceneName)
    {
        return PlayerPrefs.GetInt(ScoreKey + sceneName, 0);
    }

    // levelScore: puntos conseguidos solo en ese nivel
    public static void MarkCompleted(string sceneName, int levelScore)
    {
        PlayerPrefs.SetInt(CompletedKey + sceneName, 1);
        if (levelScore > BestScore(sceneName)) PlayerPrefs.SetInt(ScoreKey + sceneName, levelScore);
        PlayerPrefs.Save();
    }
}
