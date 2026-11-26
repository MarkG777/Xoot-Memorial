using UnityEngine;

// Dato histórico de un lugar. Cada cofre muestra uno después de su pista.
[CreateAssetMenu(fileName = "Dato", menuName = "Xoot/Dato del lugar")]
public class PlaceFactData : ScriptableObject
{
    [SerializeField] private string place = "Querétaro";
    [SerializeField] private string date = "";
    [SerializeField, TextArea(2, 5)] private string text = "";

    [Tooltip("De dónde salió el dato. No se muestra en el juego")]
    [SerializeField] private string source = "";

    public string Title => "Dato de " + place + " · " + date;
    public string Text => text;
    public string Source => source;
}
