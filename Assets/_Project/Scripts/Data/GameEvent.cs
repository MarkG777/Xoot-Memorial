using System;
using UnityEngine;

// AU: canal de eventos en un archivo de datos. Quien avisa y quien escucha no se conocen
// entre sí; por ejemplo, el audio y la cámara escuchan "vida perdida" sin buscar al jugador.
[CreateAssetMenu(fileName = "GameEvent", menuName = "Xoot/Evento")]
public class GameEvent : ScriptableObject
{
    public event Action Raised;

    public void Raise()
    {
        Raised?.Invoke();
    }
}
