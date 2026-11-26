using UnityEngine;

// RQF-04: datos de un puzzle sencillo. Hay tres tipos reutilizables en todos los niveles.
[CreateAssetMenu(fileName = "Puzzle", menuName = "Xoot/Puzzle")]
public class PuzzleData : ScriptableObject
{
    public enum PuzzleType
    {
        Code,   // escribir un código de números (fechas, caja de consigna)
        Order,  // tocar las opciones en el orden correcto (ordenar fotografías o eventos)
        Choice  // elegir la respuesta correcta (completar un registro, patrón de azulejos)
    }

    [SerializeField] private PuzzleType type;
    [SerializeField] private string title = "Puzzle";
    [SerializeField, TextArea(2, 5)] private string question = "";

    [Tooltip("Code: los números correctos, por ejemplo 1509")]
    [SerializeField] private string code = "";

    [Tooltip("Order: escribir las opciones YA en el orden correcto. Choice: las respuestas posibles")]
    [SerializeField] private string[] options;

    [Tooltip("Order: en qué orden se muestran los botones (índices de las opciones)")]
    [SerializeField] private int[] displayOrder;

    [Tooltip("Choice: índice de la respuesta correcta")]
    [SerializeField] private int correctChoice;

    public PuzzleType Type => type;
    public string Title => title;
    public string Question => question;
    public string Code => code;
    public string[] Options => options;
    public int[] DisplayOrder => displayOrder;
    public int CorrectChoice => correctChoice;
}
