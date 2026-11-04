using UnityEngine;

// RQF-03: objeto-pista. Al examinarlo muestra su texto y se cuenta una sola vez.
[RequireComponent(typeof(Collider2D))]
public class ClueItem : MonoBehaviour
{
    [SerializeField, TextArea] private string clueText = "Una foto antigua. Alguien borró los rostros.";
    [SerializeField] private Color collectedColor = new Color(0.5f, 0.5f, 0.5f, 1f);

    private static int totalClues;
    private static int foundClues;
    private bool collected;

    private void Awake()
    {
        totalClues++;
    }

    private void Start()
    {
        SimpleHud.Instance?.SetCounter(foundClues, totalClues);
    }

    private void OnDestroy()
    {
        totalClues--;
        if (collected) foundClues--;
    }

    public void Interact()
    {
        SimpleHud.Instance?.ShowMessage(clueText);
        if (collected) return;

        collected = true;
        foundClues++;
        var sprite = GetComponent<SpriteRenderer>();
        if (sprite != null) sprite.color = collectedColor;
        SimpleHud.Instance?.SetCounter(foundClues, totalClues);
    }
}
