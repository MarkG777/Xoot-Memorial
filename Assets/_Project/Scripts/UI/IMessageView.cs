// Lo que necesita la interacción de cualquier HUD (el del prototipo o el del juego).
public interface IMessageView
{
    bool IsReading { get; }
    void SetPrompt(string text);
    void ShowMessage(string title, string body);
    void HideMessage();
}

public static class MessageView
{
    public static IMessageView Current { get; set; }
}
