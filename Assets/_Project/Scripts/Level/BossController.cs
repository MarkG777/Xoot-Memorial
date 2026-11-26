using System;
using UnityEngine;

// RQF-12: jefe-leyenda de evasión en 3 fases. Cada fase es un tramo antes de la puerta con una amenaza
// distinta. Superar una fase revela un fragmento de la historia; con las 3, la leyenda se calma.
public class BossController : MonoBehaviour
{
    [Serializable]
    public class Phase
    {
        public string name;
        [TextArea(2, 5)] public string storyFragment;
        public BatSpawner[] spawners;
        public TearSpawner[] tearSpawners;
        public GameObject[] obstacles;
    }

    [SerializeField] private string bossName = "La Zacatecana";
    [SerializeField, TextArea(2, 5)] private string introText = "";
    [SerializeField, TextArea(2, 5)] private string calmedText = "";
    [SerializeField] private Phase[] phases;
    [SerializeField] private LevelProgress progress;
    [SerializeField] private HUDController hud;
    [SerializeField] private SpriteRenderer bossSprite;
    [SerializeField] private GameEvent bossAppearedEvent;
    [SerializeField] private GameEvent phaseClearedEvent;

    private int currentPhase = -1;
    private int clearedPhases;
    private bool appeared;

    public bool IsCalmed => phases != null && clearedPhases >= phases.Length;
    public bool HasAppeared => appeared;
    public int ClearedPhases => clearedPhases;

    private void Awake()
    {
        SetActivePhase(-1);
        if (bossSprite != null) bossSprite.gameObject.SetActive(false);
    }

    // Lo llama cada BossZone cuando el jugador entra a su tramo
    public void EnterPhase(int index)
    {
        if (!progress.HasAllKeys || IsCalmed || index < clearedPhases) return;

        if (!appeared)
        {
            appeared = true;
            if (bossSprite != null) bossSprite.gameObject.SetActive(true);
            hud.ShowMessage(bossName, introText);
            if (bossAppearedEvent != null) bossAppearedEvent.Raise();
        }

        currentPhase = index;
        SetActivePhase(index);
        hud.ShowNotice("Fase " + (index + 1) + " de " + phases.Length + ": " + phases[index].name, 2.5f);
    }

    // Lo llama la meta de cada tramo
    public void ClearPhase(int index)
    {
        if (!appeared || index != clearedPhases) return;

        clearedPhases++;
        SetActivePhase(-1);
        if (phaseClearedEvent != null) phaseClearedEvent.Raise();

        string title = "Fragmento " + clearedPhases + " de " + phases.Length;
        hud.ShowMessage(title, phases[index].storyFragment);

        if (IsCalmed)
        {
            if (bossSprite != null) bossSprite.gameObject.SetActive(false);
            hud.ShowMessage(bossName + " se ha calmado", calmedText);
        }
    }

    private void SetActivePhase(int index)
    {
        if (phases == null) return;

        for (int i = 0; i < phases.Length; i++)
        {
            bool active = i == index;
            foreach (BatSpawner spawner in phases[i].spawners)
            {
                if (spawner == null) continue;
                spawner.Active = active;
                if (active) spawner.Prime();
            }

            foreach (TearSpawner spawner in phases[i].tearSpawners)
            {
                if (spawner == null) continue;
                spawner.Active = active;
                if (active) spawner.Prime();
            }

            foreach (GameObject obstacle in phases[i].obstacles)
            {
                if (obstacle != null && active) obstacle.SetActive(true);
            }
        }
    }
}
