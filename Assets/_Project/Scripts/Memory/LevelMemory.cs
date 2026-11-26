using UnityEngine;

// RQF-05: con 2 de 3 llaves aparece un fragmento del recuerdo; con las 3, el recuerdo completo.
public class LevelMemory : MonoBehaviour
{
    [SerializeField] private LevelProgress progress;
    [SerializeField] private ScoreManager scoreManager;
    [SerializeField] private HUDController hud;
    [SerializeField] private Sprite memoryImage;
    [SerializeField] private GameEvent memoryUnlockedEvent;

    private bool fragmentShown;
    private bool completeShown;

    private void OnEnable()
    {
        progress.KeysChanged += HandleKeysChanged;
    }

    private void OnDisable()
    {
        progress.KeysChanged -= HandleKeysChanged;
    }

    private void HandleKeysChanged(int keys, int required)
    {
        LevelData data = progress.Data;

        if (keys >= required && !completeShown)
        {
            completeShown = true;
            scoreManager.AddScore(scoreManager.Rules.MemoryPoints);
            hud.ShowRecollection("Recuerdo desbloqueado", data.MemoryText + "\n\nLa puerta de salida ya puede abrirse.", memoryImage);
            if (memoryUnlockedEvent != null) memoryUnlockedEvent.Raise();
        }
        else if (keys == required - 1 && !fragmentShown)
        {
            fragmentShown = true;
            hud.ShowRecollection("Un fragmento del recuerdo...", data.MemoryFragmentText, memoryImage);
        }
    }
}
