using UnityEngine;

// Datos de un nivel (región, textos del recuerdo y siguiente escena). Un archivo por nivel.
[CreateAssetMenu(fileName = "LevelData", menuName = "Xoot/Datos de nivel")]
public class LevelData : ScriptableObject
{
    [SerializeField] private string levelName = "Nivel 1";
    [SerializeField] private string regionName = "Querétaro";
    [SerializeField, TextArea(2, 5)] private string introText = "";
    [SerializeField, Min(1)] private int requiredKeys = 3;

    [Header("Recuerdo")]
    [SerializeField, TextArea(2, 6)] private string memoryFragmentText = "";
    [SerializeField, TextArea(2, 6)] private string memoryText = "";

    [Header("Al terminar")]
    [SerializeField, TextArea(2, 4)] private string completedText = "";
    [SerializeField] private string nextSceneName = "MainMenu";

    public string LevelName => levelName;
    public string RegionName => regionName;
    public string IntroText => introText;
    public int RequiredKeys => requiredKeys;
    public string MemoryFragmentText => memoryFragmentText;
    public string MemoryText => memoryText;
    public string CompletedText => completedText;
    public string NextSceneName => nextSceneName;
}
