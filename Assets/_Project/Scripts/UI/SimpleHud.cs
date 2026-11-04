using UnityEngine;
using UnityEngine.UI;

// HUD mínimo del prototipo: aviso de interacción, contador de pistas y panel de lectura.
// Se construye solo en tiempo de ejecución para no depender de ningún asset.
public class SimpleHud : MonoBehaviour
{
    public static SimpleHud Instance { get; private set; }

    private Text promptText;
    private Text counterText;
    private GameObject messagePanel;
    private Text messageText;

    public bool IsReading => messagePanel != null && messagePanel.activeSelf;

    private void Awake()
    {
        Instance = this;
        Build();
    }

    public void SetPrompt(string text)
    {
        promptText.text = text;
        promptText.gameObject.SetActive(!string.IsNullOrEmpty(text));
    }

    public void SetCounter(int found, int total)
    {
        counterText.text = "Pistas: " + found + "/" + total;
    }

    public void ShowMessage(string text)
    {
        messageText.text = text;
        messagePanel.SetActive(true);
    }

    public void HideMessage()
    {
        messagePanel.SetActive(false);
    }

    private void Build()
    {
        var canvasObject = new GameObject("HudCanvas");
        canvasObject.transform.SetParent(transform, false);
        var canvas = canvasObject.AddComponent<Canvas>();
        canvas.renderMode = RenderMode.ScreenSpaceOverlay;
        var scaler = canvasObject.AddComponent<CanvasScaler>();
        scaler.uiScaleMode = CanvasScaler.ScaleMode.ScaleWithScreenSize;
        scaler.referenceResolution = new Vector2(1280, 720);

        var font = Resources.GetBuiltinResource<Font>("LegacyRuntime.ttf");

        counterText = CreateText(canvasObject.transform, "Contador", font, 28, TextAnchor.UpperLeft);
        Anchor(counterText.rectTransform, new Vector2(0, 1), new Vector2(0, 1), new Vector2(20, -20), new Vector2(400, 50));

        promptText = CreateText(canvasObject.transform, "Aviso", font, 28, TextAnchor.MiddleCenter);
        Anchor(promptText.rectTransform, new Vector2(0.5f, 0), new Vector2(0.5f, 0), new Vector2(0, 80), new Vector2(700, 60));
        promptText.gameObject.SetActive(false);

        messagePanel = new GameObject("PanelLectura");
        messagePanel.transform.SetParent(canvasObject.transform, false);
        var background = messagePanel.AddComponent<Image>();
        background.color = new Color(0f, 0f, 0f, 0.85f);
        Anchor(background.rectTransform, new Vector2(0.5f, 0.5f), new Vector2(0.5f, 0.5f), Vector2.zero, new Vector2(900, 320));

        messageText = CreateText(messagePanel.transform, "Texto", font, 30, TextAnchor.MiddleCenter);
        Anchor(messageText.rectTransform, new Vector2(0.5f, 0.5f), new Vector2(0.5f, 0.5f), Vector2.zero, new Vector2(840, 260));
        messagePanel.SetActive(false);

        SetCounter(0, 0);
    }

    private static Text CreateText(Transform parent, string objectName, Font font, int size, TextAnchor alignment)
    {
        var textObject = new GameObject(objectName);
        textObject.transform.SetParent(parent, false);
        var text = textObject.AddComponent<Text>();
        text.font = font;
        text.fontSize = size;
        text.alignment = alignment;
        text.color = Color.white;
        text.horizontalOverflow = HorizontalWrapMode.Wrap;
        text.verticalOverflow = VerticalWrapMode.Overflow;
        return text;
    }

    private static void Anchor(RectTransform rect, Vector2 anchor, Vector2 pivot, Vector2 position, Vector2 size)
    {
        rect.anchorMin = anchor;
        rect.anchorMax = anchor;
        rect.pivot = pivot;
        rect.anchoredPosition = position;
        rect.sizeDelta = size;
    }
}
