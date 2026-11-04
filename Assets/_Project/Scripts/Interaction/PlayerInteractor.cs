using UnityEngine;
using UnityEngine.InputSystem;

// RQF-02: interactuar con E con el objeto cercano. Si hay un texto abierto, E lo cierra.
public class PlayerInteractor : MonoBehaviour
{
    private ClueItem nearbyClue;

    private void Update()
    {
        var keyboard = Keyboard.current;
        var hud = SimpleHud.Instance;
        if (keyboard == null || hud == null) return;

        if (hud.IsReading)
        {
            if (keyboard.eKey.wasPressedThisFrame || keyboard.escapeKey.wasPressedThisFrame) hud.HideMessage();
            return;
        }

        if (nearbyClue != null && keyboard.eKey.wasPressedThisFrame) nearbyClue.Interact();
    }

    private void OnTriggerEnter2D(Collider2D other)
    {
        var clue = other.GetComponent<ClueItem>();
        if (clue == null) return;

        nearbyClue = clue;
        SimpleHud.Instance?.SetPrompt("Presiona E para examinar");
    }

    private void OnTriggerExit2D(Collider2D other)
    {
        if (other.GetComponent<ClueItem>() != nearbyClue) return;

        nearbyClue = null;
        SimpleHud.Instance?.SetPrompt(string.Empty);
    }
}
