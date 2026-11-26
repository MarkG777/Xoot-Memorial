using System.Collections.Generic;
using UnityEngine;

// Guía de fauna del nivel: muestra cada ficha una sola vez.
public class AnimalGuide : MonoBehaviour
{
    [SerializeField] private HUDController hud;
    [SerializeField] private LevelManager levelManager;

    private readonly HashSet<AnimalData> shown = new HashSet<AnimalData>();

    public void Present(AnimalData animal)
    {
        if (shown.Contains(animal) || !levelManager.IsPlaying) return;

        shown.Add(animal);
        string text = "Nombre común: <b>" + animal.CommonName + "</b>\nNombre científico: <i>" + animal.Species + "</i>\n\n" + animal.Facts;
        hud.ShowMemory("¡Cuidado! " + animal.CommonName, text, animal.Picture);
    }
}
