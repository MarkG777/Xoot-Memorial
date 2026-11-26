using UnityEngine;

// Ficha informativa de un animal de la región.
[CreateAssetMenu(fileName = "Animal", menuName = "Xoot/Ficha de animal")]
public class AnimalData : ScriptableObject
{
    [SerializeField] private string commonName = "Alacrán";
    [SerializeField] private string species = "";
    [SerializeField, TextArea(3, 8)] private string facts = "";
    [SerializeField] private Sprite picture;

    public string CommonName => commonName;
    public string Species => species;
    public string Facts => facts;
    public Sprite Picture => picture;
}
