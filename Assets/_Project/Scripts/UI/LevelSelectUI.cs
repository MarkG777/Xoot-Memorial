using System;
using TMPro;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

// Menú de niveles: se puede repetir cualquier nivel ya pasado. Al pasar un nivel se abre el siguiente.
public class LevelSelectUI : MonoBehaviour
{
    [Serializable]
    public class Entry
    {
        public string title;
        public string sceneName;
        public Button button;
        public TMP_Text label;
        public TMP_Text status;
    }

    [SerializeField] private Entry[] levels;
    [SerializeField] private Button reviewButton;
    [SerializeField] private TMP_Text reviewLabel;

    private void Awake()
    {
        foreach (Entry level in levels)
        {
            Entry current = level;
            current.button.onClick.AddListener(() => PlayLevel(current.sceneName));
        }

        reviewButton.onClick.AddListener(ToggleReview);
    }

    private void OnEnable()
    {
        Refresh();
    }

    public void Refresh()
    {
        for (int i = 0; i < levels.Length; i++)
        {
            Entry level = levels[i];
            bool exists = Application.CanStreamedLevelBeLoaded(level.sceneName);
            bool completed = LevelSaves.IsCompleted(level.sceneName);
            bool unlocked = i == 0 || completed || LevelSaves.ReviewMode || LevelSaves.IsCompleted(levels[i - 1].sceneName);

            level.button.interactable = exists && unlocked;
            level.label.text = level.title;

            if (!exists) level.status.text = "En construcción";
            else if (completed) level.status.text = "Completado · mejor puntaje: " + LevelSaves.BestScore(level.sceneName);
            else if (unlocked) level.status.text = "Disponible";
            else level.status.text = "Bloqueado: pasa el nivel anterior";
        }

        reviewLabel.text = LevelSaves.ReviewMode ? "Modo revisión: activado" : "Desbloquear todos (modo revisión)";
    }

    private void ToggleReview()
    {
        LevelSaves.ReviewMode = !LevelSaves.ReviewMode;
        Refresh();
    }

    private static void PlayLevel(string sceneName)
    {
        // Al elegir un nivel, el puntaje empieza en cero
        GameSession.Clear();
        Time.timeScale = 1f;
        SceneManager.LoadScene(sceneName);
    }
}
