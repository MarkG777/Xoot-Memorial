using UnityEngine;

// Información de una leyenda: la carta que se lee al empezar el nivel y la ficha del menú.
[CreateAssetMenu(fileName = "Leyenda", menuName = "Xoot/Leyenda")]
public class LegendData : ScriptableObject
{
    [SerializeField] private string legendName = "";
    [SerializeField] private string regionName = "";
    [SerializeField, TextArea(4, 10)] private string letterText = "";
    [SerializeField, TextArea(6, 20)] private string menuText = "";
    [SerializeField] private Sprite picture;

    public string LegendName => legendName;
    public string RegionName => regionName;
    public string LetterText => letterText;
    public string MenuText => menuText;
    public Sprite Picture => picture;
}
