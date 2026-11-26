using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;

// Mejoras que se agregan sobre las escenas ya construidas, sin rehacerlas.
public static partial class XootBuilder
{
    private const string Facts = "Assets/_Project/ScriptableObjects/Datos/";

    // ---------------------------------------------------------------- datos históricos
    // Solo el primer cofre de cada nivel lleva fecha: la tarjeta explica la fecha que trae su pista.
    [MenuItem("Xoot/26. Crear datos históricos de los cofres")]
    public static void CreatePlaceFacts()
    {
        Folder(Facts.TrimEnd('/'));

        Fact("Dato_QRO_Septiembre1810", "Querétaro", "septiembre de 1810",
            "La conspiración por la Independencia que se reunía en Querétaro fue descubierta. Josefa Ortiz de Domínguez, la Corregidora, logró avisar a los insurgentes.",
            "https://en.wikipedia.org/wiki/Josefa_Ortiz_de_Dom%C3%ADnguez ; CNDH, cronología de la conspiración");
        Fact("Dato_GTO_Alhondiga1810", "Guanajuato", "28 de septiembre de 1810",
            "Las tropas insurgentes de Miguel Hidalgo tomaron la Alhóndiga de Granaditas, un almacén de granos. Hoy es museo.",
            "https://lugares.inah.gob.mx/en/node/4245");
        Fact("Dato_PUE_Batalla1862", "Puebla", "5 de mayo de 1862",
            "En la Batalla de Puebla, el ejército mexicano al mando de Ignacio Zaragoza venció a las tropas francesas.",
            "https://www.britannica.com/event/Battle-of-Puebla");

        AssetDatabase.SaveAssets();
    }

    private static void Fact(string file, string place, string date, string text, string source)
    {
        CreateData<PlaceFactData>(Facts + file + ".asset", fact =>
        {
            Set(fact, "place", place);
            Set(fact, "date", date);
            Set(fact, "text", text);
            Set(fact, "source", source);
        });
    }

    [MenuItem("Xoot/27. Poner el dato histórico en el primer cofre del nivel abierto")]
    public static void AddPlaceFacts()
    {
        CreatePlaceFacts();
        var progress = Object.FindAnyObjectByType<LevelProgress>();
        if (progress == null || progress.Data == null) return;

        // Se usa el nombre de los datos del nivel porque una escena recién construida todavía no tiene nombre
        string level = progress.Data.name;
        string file = level.Contains("Queretaro") ? "Dato_QRO_Septiembre1810"
            : level.Contains("Guanajuato") ? "Dato_GTO_Alhondiga1810" : "Dato_PUE_Batalla1862";

        foreach (var chest in Object.FindObjectsByType<KeyChest>())
        {
            bool first = chest.name.StartsWith("Cofre 1");
            Set(chest, "fact", first ? AssetDatabase.LoadAssetAtPath<PlaceFactData>(Facts + file + ".asset") : null);
            PrefabUtility.RecordPrefabInstancePropertyModifications(chest);
            EditorSceneManager.MarkSceneDirty(chest.gameObject.scene);
        }
    }

    // ---------------------------------------------------------------- cielo en ventanas anchas
    [MenuItem("Xoot/29. Ajustar el cielo del nivel abierto a ventanas anchas")]
    public static void FitSky()
    {
        var camera = GameObject.FindWithTag("MainCamera");
        var sky = camera != null ? camera.transform.Find("Cielo") : null;
        if (sky == null || sky.GetComponent<SkyCover>() != null) return;

        sky.gameObject.AddComponent<SkyCover>();
        EditorSceneManager.MarkSceneDirty(sky.gameObject.scene);
    }

    // ---------------------------------------------------------------- niebla del olvido
    private const string FogTexture = "Assets/_Project/Art/Efectos/niebla.png";

    [MenuItem("Xoot/30. Poner la niebla del olvido en el nivel abierto")]
    public static void AddFog()
    {
        var sprite = ImportFog();
        var camera = GameObject.FindWithTag("MainCamera");
        var progress = Object.FindAnyObjectByType<LevelProgress>();
        if (sprite == null || camera == null || progress == null || progress.Data == null) return;

        // Cada nivel con un poco más que el anterior
        string level = progress.Data.name;
        float density = level.Contains("Queretaro") ? 0.62f : level.Contains("Guanajuato") ? 0.72f : 0.8f;
        Set(progress.Data, "fogDensity", density);
        EditorUtility.SetDirty(progress.Data);
        AssetDatabase.SaveAssets();

        KeepPlayerInFront();
        if (camera.transform.Find("Niebla") != null) return;

        var fog = new GameObject("Niebla");
        fog.transform.SetParent(camera.transform, false);
        // Delante del escenario y los personajes (la cámara está en z = -10)
        fog.transform.localPosition = new Vector3(0f, 0f, 10f);
        var script = fog.AddComponent<ForgettingFog>();

        var back = FogLayer(fog.transform, "Capa lenta", sprite, -0.2f, 11, false);
        var front = FogLayer(fog.transform, "Capa de enfrente", sprite, -0.5f, 12, true);

        var so = new SerializedObject(script);
        so.FindProperty("progress").objectReferenceValue = progress;
        var layers = so.FindProperty("layers");
        layers.arraySize = 2;
        SetFogLayer(layers.GetArrayElementAtIndex(0), back, 0.35f, 0.72f);
        SetFogLayer(layers.GetArrayElementAtIndex(1), front, -0.6f, 0.5f);
        so.ApplyModifiedPropertiesWithoutUndo();

        EditorSceneManager.MarkSceneDirty(fog.scene);
    }

    // Aketzali se dibuja delante de la niebla para que no se pierda de vista
    private static void KeepPlayerInFront()
    {
        var lives = Object.FindAnyObjectByType<PlayerLives>();
        if (lives == null) return;

        var renderer = lives.GetComponentInChildren<SpriteRenderer>();
        var so = new SerializedObject(renderer);
        so.FindProperty("m_SortingOrder").intValue = 13;
        so.ApplyModifiedPropertiesWithoutUndo();
        PrefabUtility.RecordPrefabInstancePropertyModifications(renderer);
        EditorSceneManager.MarkSceneDirty(lives.gameObject.scene);
    }

    private static SpriteRenderer FogLayer(Transform parent, string name, Sprite sprite, float height, int order, bool flip)
    {
        var go = new GameObject(name);
        go.transform.SetParent(parent, false);
        go.transform.localPosition = new Vector3(0f, height, 0f);
        var renderer = go.AddComponent<SpriteRenderer>();
        renderer.sprite = sprite;
        renderer.drawMode = SpriteDrawMode.Tiled;
        // Tres repeticiones de ancho: sobra para cualquier ventana mientras se desliza
        renderer.size = new Vector2(sprite.bounds.size.x * 3f, sprite.bounds.size.y);
        renderer.flipX = flip;
        renderer.sortingOrder = order;
        renderer.color = new Color(1f, 1f, 1f, 0f);
        return renderer;
    }

    private static void SetFogLayer(SerializedProperty item, SpriteRenderer renderer, float speed, float weight)
    {
        item.FindPropertyRelative("renderer").objectReferenceValue = renderer;
        item.FindPropertyRelative("speed").floatValue = speed;
        item.FindPropertyRelative("weight").floatValue = weight;
    }

    private static Sprite ImportFog()
    {
        AssetDatabase.Refresh();
        var importer = AssetImporter.GetAtPath(FogTexture) as TextureImporter;
        if (importer == null) return null;

        var settings = new TextureImporterSettings();
        importer.ReadTextureSettings(settings);
        if (settings.textureType != TextureImporterType.Sprite || settings.spriteMeshType != SpriteMeshType.FullRect || settings.wrapMode != TextureWrapMode.Repeat)
        {
            settings.textureType = TextureImporterType.Sprite;
            settings.spriteMode = (int)SpriteImportMode.Single;
            settings.spritePixelsPerUnit = 512f / 24f;
            settings.spriteMeshType = SpriteMeshType.FullRect;
            settings.wrapMode = TextureWrapMode.Repeat;
            settings.filterMode = FilterMode.Bilinear;
            settings.mipmapEnabled = false;
            settings.alphaIsTransparency = true;
            importer.SetTextureSettings(settings);
            importer.textureCompression = TextureImporterCompression.Uncompressed;
            importer.SaveAndReimport();
        }

        return AssetDatabase.LoadAssetAtPath<Sprite>(FogTexture);
    }

    // ---------------------------------------------------------------- final de Querétaro
    // Los murciélagos de la fase 3 hacían el nivel muy difícil: se apagan (no se borran)
    // y en su lugar queda una reja antes de la puerta.
    [MenuItem("Xoot/28. Cambiar el final de Querétaro (reja en vez de murciélagos)")]
    public static void ChangeQueretaroEnding()
    {
        var map = GameObject.Find("Mapa");
        var manager = Object.FindAnyObjectByType<LevelManager>();
        if (map == null || manager == null || map.transform.Find("Reja final") != null) return;

        foreach (var bats in Object.FindObjectsByType<BatSpawner>())
        {
            if (bats.transform.position.x < 25f) continue;

            bats.gameObject.name = "LanzaMurcielagos fase 3 (apagado)";
            bats.gameObject.SetActive(false);
        }

        var platform = FindAt(map.transform, "Plataforma", new Vector2(30.5f, 30.3f));
        var door = FindAt(map.transform, "PuertaSalida", new Vector2(31f, 30.3f));
        var goal = map.transform.Find("Meta fase 3");
        if (platform == null || door == null || goal == null)
        {
            Debug.LogWarning("El final de Querétaro no está como se esperaba; revisa la escena a mano.");
            return;
        }

        // La plataforma crece un poco para aterrizar con calma antes de la reja
        platform.position = new Vector3(30.3f, 30.3f, 0f);
        platform.GetComponent<BoxCollider2D>().size = new Vector2(3.4f, 0.5f);
        var visual = platform.Find("Visual").GetComponent<SpriteRenderer>();
        visual.size = new Vector2(3.4f, 0.5f);

        door.position = new Vector3(31.3f, 30.3f, 0f);
        goal.position = new Vector3(30.6f, 30.3f, 0f);

        var gate = Spawn("RejaConTiempo", new Vector2(30.1f, 31.8f), map.transform, "Reja final");

        var point = Spawn("PuntoSeguro", new Vector2(29f, 30.3f), map.transform, "PuntoSeguro final");
        Link<Checkpoint>(point, ("hud", Object.FindAnyObjectByType<HUDController>()));

        foreach (var changed in new Object[] { platform, platform.GetComponent<BoxCollider2D>(), visual, door, goal })
            PrefabUtility.RecordPrefabInstancePropertyModifications(changed);

        EditorSceneManager.MarkSceneDirty(gate.scene);
    }

    private static Transform FindAt(Transform parent, string prefix, Vector2 position)
    {
        foreach (Transform child in parent)
        {
            if (child.name.StartsWith(prefix) && Vector2.Distance(child.position, position) < 0.1f) return child;
        }

        return null;
    }
}
