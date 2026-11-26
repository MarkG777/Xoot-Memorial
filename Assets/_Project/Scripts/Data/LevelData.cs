using UnityEngine;

// Datos de un nivel (región, textos del recuerdo y siguiente escena). Un archivo por nivel.
[CreateAssetMenu(fileName = "LevelData", menuName = "Xoot/Datos de nivel")]
public class LevelData : ScriptableObject
{
    [SerializeField] private string levelName = "Nivel 1";
    [SerializeField] private string regionName = "Querétaro";
    [SerializeField, TextArea(2, 5)] private string introText = "";
    [SerializeField, Min(1)] private int requiredKeys = 3;

    [Header("Leyenda del nivel (carta que se lee al empezar)")]
    [SerializeField] private LegendData legend;

    [Header("Niebla del olvido: crece nivel tras nivel")]
    [SerializeField, Range(0f, 1f)] private float fogDensity = 0.6f;

    [Header("Recuerdo")]
    [SerializeField, TextArea(2, 6)] private string memoryFragmentText = "";
    [SerializeField, TextArea(2, 6)] private string memoryText = "";

    [Header("Al terminar")]
    [SerializeField, TextArea(2, 4)] private string completedText = "";
    [SerializeField] private string nextSceneName = "MainMenu";

    public string LevelName => levelName;
    public string RegionName => regionName;
    public string IntroText => introText;
    public LegendData Legend => legend;
    public int RequiredKeys => requiredKeys;
    public float FogDensity => fogDensity;
    public string MemoryFragmentText => memoryFragmentText;
    public string MemoryText => memoryText;
    public string CompletedText => completedText;
    public string NextSceneName => nextSceneName;
}
