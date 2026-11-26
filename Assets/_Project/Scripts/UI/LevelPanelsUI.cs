using TMPro;
using UnityEngine;

// Paneles de pausa, "sin corazones" y nivel completado. Solo muestra el panel que toca
// según el estado del LevelManager; los botones llaman a SceneFlow.
public class LevelPanelsUI : MonoBehaviour
{
    [SerializeField] private LevelManager levelManager;
    [SerializeField] private LevelProgress progress;
    [SerializeField] private ScoreManager scoreManager;
    [SerializeField] private GameObject pausePanel;
    [SerializeField] private GameObject lostPanel;
    [SerializeField] private GameObject completedPanel;
    [SerializeField] private TMP_Text completedBody;

    private void OnEnable()
    {
        levelManager.StateChanged += HandleStateChanged;
    }

    private void OnDisable()
    {
        levelManager.StateChanged -= HandleStateChanged;
    }

    private void Start()
    {
        HandleStateChanged(levelManager.State);
    }

    private void HandleStateChanged(LevelManager.LevelState state)
    {
        pausePanel.SetActive(state == LevelManager.LevelState.Paused);
        lostPanel.SetActive(state == LevelManager.LevelState.Lost);
        completedPanel.SetActive(state == LevelManager.LevelState.Completed);

        if (state == LevelManager.LevelState.Completed && completedBody != null)
        {
            string text = progress.Data != null ? progress.Data.CompletedText : "";
            completedBody.text = text + "\n\nPuntaje: " + scoreManager.Score;
        }
    }
}
