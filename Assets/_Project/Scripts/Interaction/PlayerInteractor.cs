using UnityEngine;
using UnityEngine.InputSystem;

// RQF-02: interactuar con E con el objeto cercano.
public class PlayerInteractor : MonoBehaviour
{
    [Tooltip("En el prototipo se deja vacío; en los niveles se asigna el LevelManager")]
    [SerializeField] private LevelManager levelManager;

    private IInteractable nearby;
    private Component nearbyComponent;

    private void Update()
    {
        var keyboard = Keyboard.current;
        var view = MessageView.Current;
        if (keyboard == null || view == null) return;

        if (levelManager == null)
        {
            // Prototipo sin LevelManager: E o Esc cierran el texto abierto
            if (view.IsReading)
            {
                if (keyboard.eKey.wasPressedThisFrame || keyboard.escapeKey.wasPressedThisFrame) view.HideMessage();
                return;
            }
        }
        else if (!levelManager.CanInteract)
        {
            return;
        }

        if (nearby != null && keyboard.eKey.wasPressedThisFrame)
        {
            nearby.Interact();
            if (nearby != null) view.SetPrompt(nearby.Prompt);
        }
    }

    private void OnTriggerEnter2D(Collider2D other)
    {
        var interactable = other.GetComponent<IInteractable>();
        if (interactable == null) return;

        nearby = interactable;
        nearbyComponent = other;
        MessageView.Current?.SetPrompt(interactable.Prompt);
    }

    private void OnTriggerExit2D(Collider2D other)
    {
        if (other != nearbyComponent) return;

        nearby = null;
        nearbyComponent = null;
        MessageView.Current?.SetPrompt(string.Empty);
    }
}
