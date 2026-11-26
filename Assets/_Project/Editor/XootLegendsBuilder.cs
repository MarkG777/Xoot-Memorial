using TMPro;
using UnityEditor;
using UnityEditor.Events;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.UI;

// Herramienta de editor: datos de las leyendas, sección del menú y llanto de La Llorona.
public static partial class XootBuilder
{
    private static LegendData Legend(string asset) => AssetDatabase.LoadAssetAtPath<LegendData>(Data + "Leyendas/" + asset + ".asset");

    [MenuItem("Xoot/15. Crear datos de las leyendas")]
    public static void CreateLegends()
    {
        Folder(Data + "Leyendas");

        CreateData<LegendData>(Data + "Leyendas/Zacatecana.asset", legend =>
        {
            Set(legend, "legendName", "La Zacatecana");
            Set(legend, "regionName", "Querétaro");
            Set(legend, "letterText", "«Para quien quiera recordar:\n\nEn el centro de Querétaro hay una casona de la que se cuenta una leyenda: la de una mujer llegada de Zacatecas y de un secreto que guardó bajo su techo.\n\nEl olvido torció su historia. Si la escuchas completa, la podrás calmar.»");
            Set(legend, "menuText", "Según cuentan en Querétaro, a inicios del siglo XIX vivió en una casona del centro una mujer llegada de Zacatecas, esposa de un hombre rico.\n\nLa leyenda habla de una traición, de la desaparición del esposo y de un secreto guardado en la casa. Las versiones cambian y los documentos no la confirman: se conserva de boca en boca.\n\nHoy la casona es el Museo Casa de la Zacatecana.");
            Set(legend, "picture", S("zacatecana_0"));
        });

        CreateData<LegendData>(Data + "Leyendas/Llorona.asset", legend =>
        {
            Set(legend, "legendName", "La Llorona");
            Set(legend, "regionName", "Guanajuato");
            Set(legend, "letterText", "«Para quien quiera recordar:\n\nEn todo México se cuenta que, de noche y cerca del agua, una mujer vestida de blanco llora buscando a sus hijos: ¡Ay, mis hijos!\n\nSi oyes su llanto en los callejones, fíjate de qué lado viene. No la quieras vencer: escucha su historia.»");
            Set(legend, "menuText", "La Llorona se cuenta en todo México y en otros países: una mujer vestida de blanco que de noche, cerca del agua, llora buscando a sus hijos.\n\nMuchos relacionan su origen con Cihuacóatl, diosa mexica. En los relatos que recogió fray Bernardino de Sahagún, antes de la Conquista se oía por las noches a una mujer llorar y decir: «Hijos míos, ¿adónde os llevaré?». Se recuerda como uno de los presagios de aquella época.");
            Set(legend, "picture", S("llorona_0"));
        });

        CreateData<LegendData>(Data + "Leyendas/Volcanes.asset", legend =>
        {
            Set(legend, "legendName", "Popocatépetl e Iztaccíhuatl");
            Set(legend, "regionName", "Puebla");
            Set(legend, "letterText", "«Para quien quiera recordar:\n\nDesde Puebla se ven dos volcanes. Cuentan que antes fueron una princesa y un guerrero que se querían, y que ni el tiempo pudo separarlos.\n\nSi olvidas su historia, solo verás montañas.»");
            Set(legend, "menuText", "Cuentan que la princesa Iztaccíhuatl se enamoró de Popocatépetl, un guerrero al servicio de su padre. Él partió a la guerra y, según muchas versiones, a ella le dijeron que había muerto; la tristeza la venció.\n\nCuando él regresó, la llevó a la montaña y se quedó velándola con una antorcha. Se convirtieron en dos volcanes: la «mujer dormida» y el Popo, que todavía echa humo. Existen varias versiones.");
        });

        SetLevelLegend("Nivel1_Queretaro", "Zacatecana");
        SetLevelLegend("Nivel2_Guanajuato", "Llorona");
        AssetDatabase.SaveAssets();
    }

    private static void SetLevelLegend(string levelAsset, string legendAsset)
    {
        var level = AssetDatabase.LoadAssetAtPath<LevelData>(Data + levelAsset + ".asset");
        if (level == null) return;

        Set(level, "legend", Legend(legendAsset));
        EditorUtility.SetDirty(level);
    }

    // ---------------------------------------------------------------- sección del menú
    [MenuItem("Xoot/16. Agregar sección Leyendas al menú abierto")]
    public static void AddLegendsMenu()
    {
        var canvas = GameObject.Find("Menu");
        if (canvas == null || canvas.transform.Find("PanelLeyendas") != null) return;

        var buttons = canvas.transform.Find("Botones");
        var open = UiButton(buttons, "Leyendas", "Leyendas", Vector2.zero, new Vector2(340f, 62f), 30f);

        // Los botones existentes solo se acomodan para que quepa el nuevo
        string[] order = { "Jugar", "Niveles", "Leyendas", "Controles", "Salir" };
        for (int i = 0; i < order.Length; i++)
        {
            var button = buttons.Find(order[i]);
            if (button == null) continue;

            var rect = button.GetComponent<RectTransform>();
            rect.anchoredPosition = new Vector2(0f, 55f - i * 76f);
            rect.sizeDelta = new Vector2(340f, 62f);
            button.SetSiblingIndex(i);
        }

        Transform box;
        var window = UiWindow(canvas.transform, "PanelLeyendas", new Vector2(1100f, 640f), out box);
        Heading(box, "Leyendas", 275f);

        string[] assets = { "Zacatecana", "Llorona", "Volcanes" };
        string[] captions = { "La Zacatecana", "La Llorona", "Popo e Izta" };
        var tabs = new Button[3];
        for (int i = 0; i < 3; i++)
            tabs[i] = UiButton(box, "Leyenda" + (i + 1), captions[i], new Vector2((i - 1) * 330f, 200f), new Vector2(300f, 54f), 25f);

        var title = UiText(box, "Nombre", "", 32f, Gold, TextAlignmentOptions.Center);
        title.fontStyle = FontStyles.Bold;
        Place(title.gameObject, new Vector2(0.5f, 0.5f), new Vector2(0f, 135f), new Vector2(1040f, 44f));

        var picture = UiImage(box, "Dibujo", null, Color.white);
        picture.preserveAspect = true;
        Place(picture.gameObject, new Vector2(0.5f, 0.5f), new Vector2(-400f, -55f), new Vector2(190f, 300f));

        var body = UiText(box, "Texto", "", 25f, Cream, TextAlignmentOptions.TopLeft);
        body.enableAutoSizing = true;
        body.fontSizeMin = 18f;
        body.fontSizeMax = 25f;
        Place(body.gameObject, new Vector2(0.5f, 0.5f), new Vector2(105f, -60f), new Vector2(780f, 320f));

        var back = UiButton(box, "Volver", "Volver", new Vector2(0f, -270f), new Vector2(240f, 56f));

        var legends = window.AddComponent<LegendsUI>();
        var so = new SerializedObject(legends);
        var list = so.FindProperty("entries");
        list.arraySize = 3;
        for (int i = 0; i < 3; i++)
        {
            var item = list.GetArrayElementAtIndex(i);
            item.FindPropertyRelative("legend").objectReferenceValue = Legend(assets[i]);
            item.FindPropertyRelative("button").objectReferenceValue = tabs[i];
        }

        so.ApplyModifiedPropertiesWithoutUndo();
        Set(legends, "titleText", title);
        Set(legends, "bodyText", body);
        Set(legends, "picture", picture);

        window.SetActive(false);
        UnityEventTools.AddBoolPersistentListener(open.onClick, window.SetActive, true);
        UnityEventTools.AddBoolPersistentListener(back.onClick, window.SetActive, false);
        EditorSceneManager.MarkSceneDirty(canvas.scene);
    }

    // ---------------------------------------------------------------- crédito del audio (licencia CC BY)
    [MenuItem("Xoot/21. Agregar créditos al menú abierto")]
    public static void AddCredits()
    {
        var canvas = GameObject.Find("Menu");
        if (canvas == null || canvas.transform.Find("Creditos") != null) return;

        var credits = UiText(canvas.transform, "Creditos",
            "Llanto: «La Llorona» de RaspberryTickle2 (freesound.org), licencia CC BY 3.0, con volumen ajustado",
            16f, new Color(1f, 0.96f, 0.9f, 0.85f), TextAlignmentOptions.BottomRight);
        Place(credits.gameObject, new Vector2(1f, 0f), new Vector2(-14f, 8f), new Vector2(900f, 26f));
        // Detrás de los paneles para que no se encime con ellos
        credits.transform.SetSiblingIndex(canvas.transform.Find("Botones").GetSiblingIndex());
        EditorSceneManager.MarkSceneDirty(canvas.scene);
    }

    // ---------------------------------------------------------------- llanto de La Llorona
    [MenuItem("Xoot/17. Agregar el llanto de La Llorona al nivel abierto")]
    public static void AddLloronaCry()
    {
        var boss = Object.FindAnyObjectByType<BossController>();
        var manager = Object.FindAnyObjectByType<LevelManager>();
        var hud = GameObject.Find("HUD");
        if (boss == null || manager == null || hud == null || Object.FindAnyObjectByType<BossCry>() != null) return;

        var text = UiText(hud.transform, "TextoLlanto", "", 46f, new Color(0.72f, 0.87f, 1f, 0f), TextAlignmentOptions.Center);
        text.fontStyle = FontStyles.Italic | FontStyles.Bold;
        Place(text.gameObject, new Vector2(0.5f, 1f), new Vector2(0f, -190f), new Vector2(900f, 70f));
        // Debajo de los paneles para no tapar las lecturas
        text.transform.SetSiblingIndex(0);

        var source = boss.gameObject.AddComponent<AudioSource>();
        source.playOnAwake = false;
        var cry = boss.gameObject.AddComponent<BossCry>();
        Set(cry, "boss", boss);
        Set(cry, "levelManager", manager);
        Set(cry, "source", source);
        Set(cry, "cryClip", Clip("llorona_raspberrytickle2"));
        Set(cry, "cryText", text);
        Set(cry, "player", Object.FindAnyObjectByType<PlayerController2D>().transform);
        Set(cry, "bossBody", boss.transform.Find("Silueta"));

        // El segundo fragmento de la historia menciona su frase
        var so = new SerializedObject(boss);
        var second = so.FindProperty("phases").GetArrayElementAtIndex(1).FindPropertyRelative("storyFragment");
        second.stringValue = "Dicen que busca a sus hijos, y por eso recorre las calles llamándolos: «¡Ay, mis hijos!».";
        so.ApplyModifiedPropertiesWithoutUndo();

        EditorSceneManager.MarkSceneDirty(boss.gameObject.scene);
    }
}
