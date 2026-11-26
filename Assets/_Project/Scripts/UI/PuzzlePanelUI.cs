using System;
using TMPro;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.UI;

// Panel de puzzles. Se resuelve con el mouse (o con los números del teclado). Equivocarse no penaliza.
public class PuzzlePanelUI : MonoBehaviour
{
    [SerializeField] private LevelManager levelManager;
    [SerializeField] private GameObject panel;
    [SerializeField] private TMP_Text titleText;
    [SerializeField] private TMP_Text questionText;
    [SerializeField] private TMP_Text answerText;
    [SerializeField] private TMP_Text feedbackText;

    [Header("Código")]
    [SerializeField] private GameObject keypad;
    [SerializeField] private Button[] digitButtons;
    [SerializeField] private Button eraseButton;

    [Header("Ordenar y elegir")]
    [SerializeField] private GameObject optionList;
    [SerializeField] private Button[] optionButtons;

    [Header("Dibujos (opcional)")]
    [SerializeField] private Image pictureImage;
    [SerializeField] private Image[] optionIcons;
    [SerializeField] private float pictureOptionsOffset = -45f;

    [Header("Salir")]
    [SerializeField] private Button closeButton;

    [Header("Eventos")]
    [SerializeField] private GameEvent puzzleSolvedEvent;
    [SerializeField] private GameEvent puzzleFailedEvent;

    private PuzzleData current;
    private Action solvedAction;
    private string typed = "";
    private int orderStep;
    private float optionListY;

    public bool IsOpen => panel.activeSelf;

    private void Awake()
    {
        for (int i = 0; i < digitButtons.Length; i++)
        {
            int digit = i;
            digitButtons[i].onClick.AddListener(() => TypeDigit(digit));
        }

        for (int i = 0; i < optionButtons.Length; i++)
        {
            int slot = i;
            optionButtons[i].onClick.AddListener(() => PressOption(slot));
        }

        optionListY = optionList.GetComponent<RectTransform>().anchoredPosition.y;
        eraseButton.onClick.AddListener(Erase);
        closeButton.onClick.AddListener(Close);
        panel.SetActive(false);
    }

    private void Update()
    {
        if (!IsOpen) return;

        // El LevelManager sale del estado Puzzle cuando se presiona Esc
        if (levelManager.State != LevelManager.LevelState.Puzzle)
        {
            panel.SetActive(false);
            return;
        }

        Keyboard keyboard = Keyboard.current;
        if (keyboard == null || current.Type != PuzzleData.PuzzleType.Code) return;

        for (int i = 0; i <= 9; i++)
        {
            Key key = i == 0 ? Key.Digit0 : Key.Digit1 + (i - 1);
            Key numpad = Key.Numpad0 + i;
            if (keyboard[key].wasPressedThisFrame || keyboard[numpad].wasPressedThisFrame) TypeDigit(i);
        }

        if (keyboard.backspaceKey.wasPressedThisFrame) Erase();
    }

    public void Open(PuzzleData puzzle, Action onSolved)
    {
        if (!levelManager.EnterPuzzle()) return;

        current = puzzle;
        solvedAction = onSolved;
        titleText.text = puzzle.Title;
        questionText.text = puzzle.Question;
        feedbackText.text = "";

        bool isCode = puzzle.Type == PuzzleData.PuzzleType.Code;
        keypad.SetActive(isCode);
        optionList.SetActive(!isCode);

        ShowPictures(puzzle);
        ResetAnswer();
        panel.SetActive(true);
    }

    public void Close()
    {
        panel.SetActive(false);
        levelManager.ExitPuzzle();
    }

    // Si el puzzle trae dibujo, ocupa el lugar de la línea de respuesta y las opciones bajan un poco
    private void ShowPictures(PuzzleData puzzle)
    {
        bool hasPicture = pictureImage != null && puzzle.Picture != null;
        if (pictureImage != null)
        {
            pictureImage.gameObject.SetActive(hasPicture);
            pictureImage.sprite = puzzle.Picture;
        }

        answerText.gameObject.SetActive(!hasPicture);
        RectTransform list = optionList.GetComponent<RectTransform>();
        list.anchoredPosition = new Vector2(list.anchoredPosition.x, optionListY + (hasPicture ? pictureOptionsOffset : 0f));

        if (optionIcons == null) return;
        for (int i = 0; i < optionIcons.Length; i++)
        {
            int index = puzzle.Options != null && i < puzzle.Options.Length ? OptionIndex(i) : -1;
            Sprite icon = index >= 0 && puzzle.OptionPictures != null && index < puzzle.OptionPictures.Length ? puzzle.OptionPictures[index] : null;
            optionIcons[i].gameObject.SetActive(icon != null);
            optionIcons[i].sprite = icon;
        }
    }

    private void ResetAnswer()
    {
        typed = "";
        orderStep = 0;

        if (current.Type == PuzzleData.PuzzleType.Code)
        {
            RefreshCode();
            return;
        }

        answerText.text = current.Type == PuzzleData.PuzzleType.Order ? "Toca en orden, del primero al último" : "Elige una respuesta";

        for (int i = 0; i < optionButtons.Length; i++)
        {
            bool used = i < current.Options.Length;
            optionButtons[i].gameObject.SetActive(used);
            if (!used) continue;

            optionButtons[i].interactable = true;
            optionButtons[i].GetComponentInChildren<TMP_Text>().text = current.Options[OptionIndex(i)];
        }
    }

    private int OptionIndex(int slot)
    {
        int[] order = current.DisplayOrder;
        return order != null && order.Length == current.Options.Length ? order[slot] : slot;
    }

    private void TypeDigit(int digit)
    {
        if (typed.Length >= current.Code.Length) return;

        typed += digit.ToString();
        feedbackText.text = "";
        RefreshCode();

        if (typed.Length < current.Code.Length) return;

        if (typed == current.Code) Solve();
        else Fail("El código no abre. Puedes intentarlo otra vez.");
    }

    private void Erase()
    {
        if (typed.Length == 0) return;

        typed = typed.Substring(0, typed.Length - 1);
        RefreshCode();
    }

    private void RefreshCode()
    {
        string shown = "";
        for (int i = 0; i < current.Code.Length; i++)
            shown += (i < typed.Length ? typed[i].ToString() : "_") + " ";

        answerText.text = shown.Trim();
    }

    private void PressOption(int slot)
    {
        int index = OptionIndex(slot);
        feedbackText.text = "";

        if (current.Type == PuzzleData.PuzzleType.Choice)
        {
            if (index == current.CorrectChoice) Solve();
            else Fail("Eso no coincide con las pistas. Intenta otra vez.");
            return;
        }

        if (index != orderStep)
        {
            Fail("Ese no es el orden. Empieza de nuevo.");
            return;
        }

        optionButtons[slot].interactable = false;
        orderStep++;
        answerText.text = orderStep + " de " + current.Options.Length;
        if (orderStep >= current.Options.Length) Solve();
    }

    private void Fail(string message)
    {
        ResetAnswer();
        feedbackText.text = message;
        if (puzzleFailedEvent != null) puzzleFailedEvent.Raise();
    }

    private void Solve()
    {
        panel.SetActive(false);
        levelManager.ExitPuzzle();
        if (puzzleSolvedEvent != null) puzzleSolvedEvent.Raise();

        Action action = solvedAction;
        solvedAction = null;
        action?.Invoke();
    }
}
