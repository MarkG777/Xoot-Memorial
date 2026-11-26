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
