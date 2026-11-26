using UnityEngine;
using UnityEngine.SceneManagement;

// Navegación entre escenas (como en la P8). Los botones llaman a estos métodos con On Click.
public class SceneFlow : MonoBehaviour
{
    [SerializeField] private string menuScene = "MainMenu";
    [SerializeField] private string firstScene = "Nivel1_Queretaro";
    [SerializeField] private string controlsScene = "";
    [SerializeField] private LevelProgress levelProgress;
    [SerializeField] private LevelManager levelManager;

    // Partida nueva: se borra el puntaje anterior
    public void Play()
    {
        GameSession.Clear();
        Load(firstScene);
    }

    public void OpenControls()
    {
        Load(controlsScene);
    }

    public void RestartLevel()
    {
        Load(SceneManager.GetActiveScene().name);
    }

    public void NextLevel()
    {
        string next = levelProgress != null && levelProgress.Data != null ? levelProgress.Data.NextSceneName : menuScene;
        Load(next);
    }

    public void ReturnToMenu()
    {
        Load(menuScene);
    }

    public void Resume()
    {
        if (levelManager != null) levelManager.Resume();
    }

    public void Quit()
    {
        Debug.Log("Salir del juego");
        Application.Quit();
    }

    private static void Load(string sceneName)
    {
        if (string.IsNullOrEmpty(sceneName)) return;

        Time.timeScale = 1f;
        SceneManager.LoadScene(sceneName);
    }
}
