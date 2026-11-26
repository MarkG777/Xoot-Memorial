using TMPro;
using UnityEditor;
using UnityEditor.Events;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.UI;

// Herramienta de editor: agrega al menú principal el botón "Niveles" y su panel, sin rehacer el menú.
public static partial class XootBuilder
{
    [MenuItem("Xoot/14. Agregar menú de niveles al menú abierto")]
    public static void AddLevelSelect()
    {
        var canvas = GameObject.Find("Menu");
        if (canvas == null || canvas.transform.Find("PanelNiveles") != null) return;

        var buttons = canvas.transform.Find("Botones");

        // Se recorren los botones existentes para hacer espacio al botón nuevo
        MoveButton(buttons, "Jugar", 45f);
        MoveButton(buttons, "Controles", -125f);
        MoveButton(buttons, "Salir", -210f);
        var open = UiButton(buttons, "Niveles", "Niveles", new Vector2(0f, -40f), new Vector2(340f, 70f), 32f);
        open.transform.SetSiblingIndex(1);

        Transform box;
        var window = UiWindow(canvas.transform, "PanelNiveles", new Vector2(800f, 600f), out box);
        Heading(box, "Elige un nivel", 248f);

        string[] titles = { "Nivel 1 · Querétaro", "Nivel 2 · Guanajuato", "Nivel 3 · Puebla" };
        string[] scenes = { "Nivel1_Queretaro", "Nivel2_Guanajuato", "Nivel3_Puebla" };
        var levelButtons = new Button[3];
        var labels = new TMP_Text[3];
        var states = new TMP_Text[3];
        for (int i = 0; i < 3; i++)
        {
            float y = 160f - i * 112f;
            levelButtons[i] = UiButton(box, "Nivel" + (i + 1), titles[i], new Vector2(0f, y), new Vector2(520f, 62f), 30f);
            labels[i] = levelButtons[i].GetComponentInChildren<TMP_Text>();
            states[i] = UiText(box, "Estado" + (i + 1), "", 21f, Cream, TextAlignmentOptions.Center);
            Place(states[i].gameObject, new Vector2(0.5f, 0.5f), new Vector2(0f, y - 47f), new Vector2(700f, 30f));
        }

        var review = UiButton(box, "ModoRevision", "Desbloquear todos (modo revisión)", new Vector2(0f, -180f), new Vector2(470f, 48f), 21f);
        var back = UiButton(box, "Volver", "Volver", new Vector2(0f, -250f), new Vector2(240f, 56f));

        var select = window.AddComponent<LevelSelectUI>();
        var so = new SerializedObject(select);
        var list = so.FindProperty("levels");
        list.arraySize = 3;
        for (int i = 0; i < 3; i++)
        {
            var item = list.GetArrayElementAtIndex(i);
            item.FindPropertyRelative("title").stringValue = titles[i];
            item.FindPropertyRelative("sceneName").stringValue = scenes[i];
            item.FindPropertyRelative("button").objectReferenceValue = levelButtons[i];
            item.FindPropertyRelative("label").objectReferenceValue = labels[i];
            item.FindPropertyRelative("status").objectReferenceValue = states[i];
        }

        so.ApplyModifiedPropertiesWithoutUndo();
        Set(select, "reviewButton", review);
        Set(select, "reviewLabel", review.GetComponentInChildren<TMP_Text>());

        window.SetActive(false);
        UnityEventTools.AddBoolPersistentListener(open.onClick, window.SetActive, true);
        UnityEventTools.AddBoolPersistentListener(back.onClick, window.SetActive, false);
        EditorSceneManager.MarkSceneDirty(canvas.scene);
    }

    private static void MoveButton(Transform parent, string name, float y)
    {
        var rect = parent.Find(name).GetComponent<RectTransform>();
        rect.anchoredPosition = new Vector2(0f, y);
    }
}
