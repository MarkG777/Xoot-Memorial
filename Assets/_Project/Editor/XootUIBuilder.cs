using TMPro;
using UnityEditor;
using UnityEditor.Events;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.EventSystems;
using UnityEngine.InputSystem.UI;
using UnityEngine.UI;

// Herramienta de editor: construye el tablero (HUD) y los paneles con Canvas y TextMeshPro.
public static partial class XootBuilder
{
    private static Sprite UiSprite => AssetDatabase.GetBuiltinExtraResource<Sprite>("UI/Skin/UISprite.psd");

    private static RectTransform Rect(GameObject go) => go.GetComponent<RectTransform>();

    private static GameObject UiObject(Transform parent, string name)
    {
        var go = new GameObject(name, typeof(RectTransform));
        go.transform.SetParent(parent, false);
        go.layer = LayerMask.NameToLayer("UI");
        return go;
    }

    // Coloca un elemento respecto a un punto de anclaje de su padre
    private static void Place(GameObject go, Vector2 anchor, Vector2 position, Vector2 size, Vector2? pivot = null)
    {
        var rect = Rect(go);
        rect.anchorMin = anchor;
        rect.anchorMax = anchor;
        rect.pivot = pivot ?? anchor;
        rect.anchoredPosition = position;
        rect.sizeDelta = size;
    }

    private static void Stretch(GameObject go)
    {
        var rect = Rect(go);
        rect.anchorMin = Vector2.zero;
        rect.anchorMax = Vector2.one;
        rect.offsetMin = Vector2.zero;
        rect.offsetMax = Vector2.zero;
    }

    private static Image UiImage(Transform parent, string name, Sprite sprite, Color color, bool sliced = false)
    {
        var go = UiObject(parent, name);
        var image = go.AddComponent<Image>();
        image.sprite = sprite;
        image.color = color;
        if (sliced) image.type = Image.Type.Sliced;
        image.raycastTarget = false;
        return image;
    }

    private static TMP_Text UiText(Transform parent, string name, string text, float size, Color color, TextAlignmentOptions alignment)
    {
        var go = UiObject(parent, name);
        var label = go.AddComponent<TextMeshProUGUI>();
        label.text = text;
        label.fontSize = size;
        label.color = color;
        label.alignment = alignment;
        label.raycastTarget = false;
        label.textWrappingMode = TextWrappingModes.Normal;
        return label;
    }

    private static Button UiButton(Transform parent, string name, string text, Vector2 position, Vector2 size, float fontSize = 28f)
    {
        var image = UiImage(parent, name, UiSprite, ButtonColor, true);
        image.raycastTarget = true;
        Place(image.gameObject, new Vector2(0.5f, 0.5f), position, size);
        var button = image.gameObject.AddComponent<Button>();
        button.targetGraphic = image;
        var colors = button.colors;
        colors.highlightedColor = new Color(1f, 0.86f, 0.72f);
        colors.selectedColor = new Color(1f, 0.86f, 0.72f);
        colors.pressedColor = new Color(0.8f, 0.66f, 0.58f);
        colors.disabledColor = new Color(0.45f, 0.4f, 0.45f, 0.6f);
        button.colors = colors;
        var label = UiText(image.transform, "Texto", text, fontSize, new Color(0.14f, 0.07f, 0.1f), TextAlignmentOptions.Center);
        label.fontStyle = FontStyles.Bold;
        Stretch(label.gameObject);
        return button;
    }

    private static GameObject UiCanvas(string name)
    {
        var go = new GameObject(name, typeof(RectTransform));
        go.layer = LayerMask.NameToLayer("UI");
        var canvas = go.AddComponent<Canvas>();
        canvas.renderMode = RenderMode.ScreenSpaceOverlay;
        var scaler = go.AddComponent<CanvasScaler>();
        scaler.uiScaleMode = CanvasScaler.ScaleMode.ScaleWithScreenSize;
        scaler.referenceResolution = new Vector2(1280, 720);
        scaler.matchWidthOrHeight = 0.5f;
        go.AddComponent<GraphicRaycaster>();
        return go;
    }

    private static void CreateEventSystem()
    {
        var go = new GameObject("EventSystem");
        go.AddComponent<EventSystem>();
        go.AddComponent<InputSystemUIInputModule>();
    }

    private static GameObject UiWindow(Transform parent, string name, Vector2 size, out Transform box)
    {
        var dim = UiImage(parent, name, null, new Color(0f, 0f, 0f, 0.6f));
        dim.raycastTarget = true;
        Stretch(dim.gameObject);
        var border = UiImage(dim.transform, "Borde", UiSprite, ButtonColor, true);
        Place(border.gameObject, new Vector2(0.5f, 0.5f), Vector2.zero, size + new Vector2(8f, 8f));
        var inner = UiImage(border.transform, "Caja", UiSprite, PanelColor, true);
        Place(inner.gameObject, new Vector2(0.5f, 0.5f), Vector2.zero, size);
        box = inner.transform;
        return dim.gameObject;
    }

    private static void OnClick(Button button, UnityAction action)
    {
        UnityEventTools.AddPersistentListener(button.onClick, action);
    }

    // Construye el tablero completo y conecta sus referencias con los sistemas del nivel
    private static HUDController BuildHud(LevelManager manager, PlayerLives lives, LevelProgress progress,
        ScoreManager score, SceneFlow flow, out PuzzlePanelUI puzzlePanel)
    {
        var canvas = UiCanvas("HUD");
        var root = canvas.transform;

        // Tablero arriba a la izquierda: corazones, llaves y puntaje
        var board = UiImage(root, "Tablero", UiSprite, new Color(0.08f, 0.05f, 0.13f, 0.78f), true);
        Place(board.gameObject, new Vector2(0f, 1f), new Vector2(16f, -16f), new Vector2(252f, 150f));
        var hearts = new Image[3];
        for (int i = 0; i < 3; i++)
        {
            hearts[i] = UiImage(board.transform, "Corazon" + (i + 1), S("heart_full"), Color.white);
            Place(hearts[i].gameObject, new Vector2(0f, 1f), new Vector2(16f + i * 74f, -12f), new Vector2(63f, 56f));
        }

        var keys = new Image[3];
        for (int i = 0; i < 3; i++)
        {
            keys[i] = UiImage(board.transform, "Llave" + (i + 1), S("key_empty"), Color.white);
            Place(keys[i].gameObject, new Vector2(0f, 1f), new Vector2(20f + i * 74f, -76f), new Vector2(55f, 33f));
        }

        var scoreText = UiText(board.transform, "Puntaje", "Puntos: 0", 26f, Gold, TextAlignmentOptions.Left);
        Place(scoreText.gameObject, new Vector2(0f, 1f), new Vector2(20f, -112f), new Vector2(225f, 32f));

        var levelBox = UiImage(root, "NombreNivel", UiSprite, new Color(0.08f, 0.05f, 0.13f, 0.7f), true);
        Place(levelBox.gameObject, new Vector2(1f, 1f), new Vector2(-16f, -16f), new Vector2(320f, 44f));
        var levelText = UiText(levelBox.transform, "Texto", "Nivel", 24f, Cream, TextAlignmentOptions.Center);
        Stretch(levelText.gameObject);

        var prompt = UiImage(root, "Aviso", UiSprite, new Color(0.08f, 0.05f, 0.13f, 0.85f), true);
        Place(prompt.gameObject, new Vector2(0.5f, 0f), new Vector2(0f, 36f), new Vector2(620f, 52f));
        var promptText = UiText(prompt.transform, "Texto", "", 26f, Cream, TextAlignmentOptions.Center);
        Stretch(promptText.gameObject);

        // Panel de lectura (pistas, recuerdo y jefe)
        Transform box;
        var message = UiWindow(root, "PanelLectura", new Vector2(880f, 540f), out box);
        var title = UiText(box, "Titulo", "Título", 40f, Gold, TextAlignmentOptions.Center);
        title.fontStyle = FontStyles.Bold;
        Place(title.gameObject, new Vector2(0.5f, 1f), new Vector2(0f, -18f), new Vector2(820f, 52f));
        var column = UiObject(box, "Contenido");
        Place(column, new Vector2(0.5f, 1f), new Vector2(0f, -78f), new Vector2(820f, 400f));
        var layout = column.AddComponent<VerticalLayoutGroup>();
        layout.spacing = 12f;
        layout.childAlignment = TextAnchor.MiddleCenter;
        layout.childControlWidth = true;
        layout.childControlHeight = true;
        layout.childForceExpandWidth = true;
        layout.childForceExpandHeight = false;
        var picture = UiImage(column.transform, "Imagen", null, Color.white);
        picture.preserveAspect = true;
        var pictureSize = picture.gameObject.AddComponent<LayoutElement>();
        pictureSize.preferredHeight = 225f;
        var bodyText = UiText(column.transform, "Texto", "", 27f, Cream, TextAlignmentOptions.Center);
        var hint = UiText(box, "Continuar", "Presiona E para continuar", 20f, new Color(0.75f, 0.68f, 0.82f), TextAlignmentOptions.Center);
        Place(hint.gameObject, new Vector2(0.5f, 0f), new Vector2(0f, 14f), new Vector2(600f, 30f));

        // Puzzle
        var puzzle = UiWindow(root, "PanelPuzzle", new Vector2(760f, 580f), out box);
        var puzzleTitle = UiText(box, "Titulo", "Puzzle", 38f, Gold, TextAlignmentOptions.Center);
        puzzleTitle.fontStyle = FontStyles.Bold;
        Place(puzzleTitle.gameObject, new Vector2(0.5f, 1f), new Vector2(0f, -18f), new Vector2(700f, 50f));
        var question = UiText(box, "Pregunta", "", 25f, Cream, TextAlignmentOptions.Center);
        Place(question.gameObject, new Vector2(0.5f, 1f), new Vector2(0f, -74f), new Vector2(690f, 100f));
        var answer = UiText(box, "Respuesta", "", 34f, Gold, TextAlignmentOptions.Center);
        Place(answer.gameObject, new Vector2(0.5f, 1f), new Vector2(0f, -178f), new Vector2(690f, 44f));

        var keypad = UiObject(box, "Teclado");
        Place(keypad, new Vector2(0.5f, 0.5f), new Vector2(0f, -65f), new Vector2(400f, 270f));
        var digits = new Button[10];
        for (int d = 1; d <= 9; d++)
        {
            int row = (d - 1) / 3;
            int col = (d - 1) % 3;
            digits[d] = UiButton(keypad.transform, "Digito" + d, d.ToString(), new Vector2((col - 1) * 100f, 95f - row * 64f), new Vector2(88f, 56f));
        }

        digits[0] = UiButton(keypad.transform, "Digito0", "0", new Vector2(0f, -97f), new Vector2(88f, 56f));
        var erase = UiButton(keypad.transform, "Borrar", "Borrar", new Vector2(100f, -97f), new Vector2(88f, 56f), 20f);

        var list = UiObject(box, "Opciones");
        Place(list, new Vector2(0.5f, 0.5f), new Vector2(0f, -65f), new Vector2(520f, 270f));
        var optionButtons = new Button[4];
        for (int i = 0; i < 4; i++)
            optionButtons[i] = UiButton(list.transform, "Opcion" + (i + 1), "Opción", new Vector2(0f, 96f - i * 66f), new Vector2(480f, 56f), 26f);

        var feedback = UiText(box, "Aviso", "", 22f, new Color(1f, 0.55f, 0.55f), TextAlignmentOptions.Center);
        Place(feedback.gameObject, new Vector2(0.5f, 0f), new Vector2(0f, 76f), new Vector2(690f, 32f));
        var closeButton = UiButton(box, "Cerrar", "Salir (Esc)", new Vector2(0f, -250f), new Vector2(200f, 46f), 22f);

        // Pausa, sin corazones y nivel completado
        var pause = UiWindow(root, "PanelPausa", new Vector2(460f, 400f), out box);
        Heading(box, "Pausa", 130f);
        OnClick(UiButton(box, "Continuar", "Continuar", new Vector2(0f, 40f), new Vector2(320f, 62f)), flow.Resume);
        OnClick(UiButton(box, "Reiniciar", "Reiniciar nivel", new Vector2(0f, -40f), new Vector2(320f, 62f)), flow.RestartLevel);
        OnClick(UiButton(box, "Menu", "Menú principal", new Vector2(0f, -120f), new Vector2(320f, 62f)), flow.ReturnToMenu);

        var lost = UiWindow(root, "PanelSinCorazones", new Vector2(640f, 380f), out box);
        Heading(box, "Te quedaste sin corazones", 120f);
        var lostText = UiText(box, "Texto", "El olvido avanzó un poco. Vuelve a intentarlo.", 25f, Cream, TextAlignmentOptions.Center);
        Place(lostText.gameObject, new Vector2(0.5f, 0.5f), new Vector2(0f, 45f), new Vector2(560f, 60f));
        OnClick(UiButton(box, "Reintentar", "Reintentar nivel", new Vector2(0f, -40f), new Vector2(340f, 62f)), flow.RestartLevel);
        OnClick(UiButton(box, "Menu", "Menú principal", new Vector2(0f, -120f), new Vector2(340f, 62f)), flow.ReturnToMenu);

        var completed = UiWindow(root, "PanelNivelCompletado", new Vector2(720f, 440f), out box);
        Heading(box, "¡Nivel completado!", 160f);
        var completedBody = UiText(box, "Texto", "", 27f, Cream, TextAlignmentOptions.Center);
        Place(completedBody.gameObject, new Vector2(0.5f, 0.5f), new Vector2(0f, 45f), new Vector2(640f, 160f));
        OnClick(UiButton(box, "Continuar", "Continuar", new Vector2(0f, -80f), new Vector2(340f, 62f)), flow.NextLevel);
        OnClick(UiButton(box, "Menu", "Menú principal", new Vector2(0f, -158f), new Vector2(340f, 62f)), flow.ReturnToMenu);

        // El orden en la jerarquía decide qué panel queda encima
        message.transform.SetAsLastSibling();

        var hud = canvas.AddComponent<HUDController>();
        Set(hud, "levelManager", manager);
        Set(hud, "playerLives", lives);
        Set(hud, "progress", progress);
        Set(hud, "scoreManager", score);
        Set(hud, "hearts", hearts);
        Set(hud, "heartFull", S("heart_full"));
        Set(hud, "heartEmpty", S("heart_empty"));
        Set(hud, "keyIcons", keys);
        Set(hud, "keyFull", S("key"));
        Set(hud, "keyEmpty", S("key_empty"));
        Set(hud, "scoreText", scoreText);
        Set(hud, "levelText", levelText);
        Set(hud, "promptPanel", prompt.gameObject);
        Set(hud, "promptText", promptText);
        Set(hud, "messagePanel", message);
        Set(hud, "messageTitle", title);
        Set(hud, "messageBody", bodyText);
        Set(hud, "messageImage", picture);

        puzzlePanel = canvas.AddComponent<PuzzlePanelUI>();
        Set(puzzlePanel, "levelManager", manager);
        Set(puzzlePanel, "panel", puzzle);
        Set(puzzlePanel, "titleText", puzzleTitle);
        Set(puzzlePanel, "questionText", question);
        Set(puzzlePanel, "answerText", answer);
        Set(puzzlePanel, "feedbackText", feedback);
        Set(puzzlePanel, "keypad", keypad);
        Set(puzzlePanel, "digitButtons", digits);
        Set(puzzlePanel, "eraseButton", erase);
        Set(puzzlePanel, "optionList", list);
        Set(puzzlePanel, "optionButtons", optionButtons);
        Set(puzzlePanel, "closeButton", closeButton);
        Set(puzzlePanel, "puzzleSolvedEvent", Ev("PuzzleResuelto"));
        Set(puzzlePanel, "puzzleFailedEvent", Ev("PuzzleFallado"));

        var panels = canvas.AddComponent<LevelPanelsUI>();
        Set(panels, "levelManager", manager);
        Set(panels, "progress", progress);
        Set(panels, "scoreManager", score);
        Set(panels, "pausePanel", pause);
        Set(panels, "lostPanel", lost);
        Set(panels, "completedPanel", completed);
        Set(panels, "completedBody", completedBody);

        puzzle.SetActive(false);
        pause.SetActive(false);
        lost.SetActive(false);
        completed.SetActive(false);
        message.SetActive(false);
        prompt.gameObject.SetActive(false);

        return hud;
    }

    private static void Heading(Transform box, string text, float y)
    {
        var heading = UiText(box, "Titulo", text, 40f, Gold, TextAlignmentOptions.Center);
        heading.fontStyle = FontStyles.Bold;
        Place(heading.gameObject, new Vector2(0.5f, 0.5f), new Vector2(0f, y), new Vector2(660f, 56f));
    }
}
