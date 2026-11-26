using TMPro;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;

// Herramienta de editor: fauna de Querétaro (Nivel 1) y ajuste de dificultad para que sea más fácil de pasar.
public static partial class XootBuilder
{
    private static void SnakePrefab()
    {
        if (PrefabExists("Cascabel")) return;

        var snake = new GameObject("Cascabel");
        var body = snake.AddComponent<Rigidbody2D>();
        body.bodyType = RigidbodyType2D.Kinematic;
        // La zona que quita vida es más chica que el dibujo para que sea fácil brincarla
        Box(snake, new Vector2(0.72f, 0.3f), new Vector2(0.05f, 0.2f), true);
        var visual = Renderer(snake, "Visual", S("cascabel_0"), 9);
        snake.AddComponent<Hazard>();
        var flip = snake.AddComponent<SpriteFlipbook>();
        Set(flip, "spriteRenderer", visual);
        Set(flip, "frames", new[] { S("cascabel_0"), S("cascabel_1") });
        Set(flip, "framesPerSecond", 4f);
        var patrol = snake.AddComponent<Patroller>();
        Set(patrol, "spriteRenderer", visual);
        Set(patrol, "walkAnimation", flip);

        var label = new GameObject("Especie");
        label.transform.SetParent(snake.transform, false);
        label.transform.localPosition = new Vector3(0f, 1.1f, 0f);
        var text = label.AddComponent<TextMeshPro>();
        text.fontSize = 3.2f;
        text.fontStyle = FontStyles.Italic | FontStyles.Bold;
        text.alignment = TextAlignmentOptions.Center;
        text.color = new Color(1f, 0.9f, 0.55f);
        text.textWrappingMode = TextWrappingModes.NoWrap;
        text.rectTransform.sizeDelta = new Vector2(6f, 0.6f);
        text.GetComponent<MeshRenderer>().sortingOrder = 14;
        var info = snake.AddComponent<AnimalInfo>();
        Set(info, "animal", AssetDatabase.LoadAssetAtPath<AnimalData>(Data + "Animal_Cascabel.asset"));
        Set(info, "speciesLabel", text);
        SavePrefab(snake, "Cascabel");
    }

    private static void Snake(Transform map, AnimalGuide guide, Transform player, string name, float x, float floorY, float reach, bool startLeft)
    {
        var snake = Spawn("Cascabel", new Vector2(x, floorY), map, name);
        // Lenta y con pausas largas para que se pueda pasar
        Link<Patroller>(snake, ("leftDistance", reach), ("rightDistance", reach), ("speed", 0.75f), ("pauseTime", 1.6f), ("startToTheLeft", startLeft));
        Link<AnimalInfo>(snake, ("guide", guide), ("player", player));
    }

    [MenuItem("Xoot/18. Agregar víboras de cascabel al Nivel 1 abierto")]
    public static void AddSnakes()
    {
        ImportArt();
        CreateData<AnimalData>(Data + "Animal_Cascabel.asset", animal =>
        {
            Set(animal, "commonName", "Víbora de cascabel");
            Set(animal, "species", "Crotalus aquilus");
            Set(animal, "facts", "Vive en Querétaro y el centro de México, y solo existe en este país. Es venenosa: en la vida real no te acerques; si muerde, ve al médico. ¡Sáltala!");
            Set(animal, "picture", S("cascabel_0"));
        });
        SnakePrefab();

        var hud = Object.FindAnyObjectByType<HUDController>();
        var manager = Object.FindAnyObjectByType<LevelManager>();
        var player = Object.FindAnyObjectByType<PlayerController2D>();
        var map = GameObject.Find("Mapa");
        if (hud == null || manager == null || player == null || map == null || GameObject.Find("Cascabel 1") != null) return;

        var guide = Object.FindAnyObjectByType<AnimalGuide>();
        if (guide == null)
        {
            guide = new GameObject("GuiaDeFauna").AddComponent<AnimalGuide>();
            Set(guide, "hud", hud);
            Set(guide, "levelManager", manager);
        }

        Snake(map.transform, guide, player.transform, "Cascabel 1", 8f, 1f, 1.4f, false);
        Snake(map.transform, guide, player.transform, "Cascabel 2", 20f, 1f, 2f, true);
        EditorSceneManager.MarkSceneDirty(map.scene);
    }

    // ---------------------------------------------------------------- dificultad
    // Más margen para saltar y obstáculos más lentos. No quita ningún obstáculo.
    [MenuItem("Xoot/19. Hacer el juego más fácil (jugador y prefabs)")]
    public static void EaseDifficulty()
    {
        var stats = AssetDatabase.LoadAssetAtPath<PlayerStats>(Data + "PlayerStats.asset");
        Set(stats, "coyoteTime", 0.15f);
        Set(stats, "jumpBufferTime", 0.16f);
        Set(stats, "invulnerableTime", 2.2f);
        EditorUtility.SetDirty(stats);

        EditPrefab("PlataformaQueCae", root =>
        {
            var platform = root.GetComponent<FallingPlatform>();
            Set(platform, "shakeTime", 0.9f);
            Set(platform, "respawnTime", 2f);
        });
        EditPrefab("RejaConTiempo", root =>
        {
            var gate = root.GetComponent<TimedGate>();
            Set(gate, "openSeconds", 3.2f);
            Set(gate, "closedSeconds", 1.5f);
        });
        AssetDatabase.SaveAssets();
    }

    // Ajusta en la escena abierta lo que cada nivel tiene configurado por separado
    [MenuItem("Xoot/20. Hacer más fácil el nivel abierto")]
    public static void EaseOpenLevel()
    {
        foreach (var bats in Object.FindObjectsByType<BatSpawner>())
        {
            bool echo = bats.name.Contains("Eco");
            bool first = bats.transform.position.x < 25f && !echo;
            Set(bats, "interval", echo ? 2.9f : first ? 2.7f : 3.1f);
            Set(bats, "batSpeed", echo ? 3.4f : first ? 3.3f : 3f);
            Set(bats, "batLifeTime", echo ? 2.8f : first ? 3f : 3.1f);
        }

        foreach (var tears in Object.FindObjectsByType<TearSpawner>())
        {
            bool last = tears.transform.position.x > 22f;
            Set(tears, "interval", last ? 2.1f : 1.4f);
            Set(tears, "warningTime", 0.95f);
            Set(tears, "fallSpeed", last ? 5.5f : 6f);
            Set(tears, "aimAtPlayerChance", 0.3f);
        }

        foreach (var patrol in Object.FindObjectsByType<Patroller>())
        {
            if (!patrol.name.StartsWith("Alacran")) continue;

            Set(patrol, "speed", 1f);
            Set(patrol, "pauseTime", 1.2f);
        }

        var any = Object.FindAnyObjectByType<LevelManager>();
        if (any != null) EditorSceneManager.MarkSceneDirty(any.gameObject.scene);
    }
}
