using TMPro;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.UI;

// Herramienta de editor: contenido y escena del Nivel 3 (Puebla).
public static partial class XootBuilder
{
    private static void FolderlessPuzzle(string file, System.Action<PuzzleData> setup)
    {
        CreateData<PuzzleData>(Data + "Puzzles/" + file + ".asset", setup);
    }

    [MenuItem("Xoot/22. Crear contenido del Nivel 3")]
    public static void CreateLevel3Content()
    {
        ImportArt();

        FolderlessPuzzle("Q7_Azulejo", p =>
        {
            Set(p, "type", (int)PuzzleData.PuzzleType.Choice);
            Set(p, "title", "El baúl de talavera");
            Set(p, "question", "Los azulejos siguen un patrón. ¿Cuál falta en el cuadro con la pregunta?");
            Set(p, "options", new[] { "Flor azul", "Estrella amarilla", "Rombo verde" });
            Set(p, "correctChoice", 1);
            Set(p, "picture", S("azulejo_patron"));
            Set(p, "optionPictures", new[] { S("azulejo_0"), S("azulejo_1"), S("azulejo_2") });
        });
        FolderlessPuzzle("Q8_Anio", p =>
        {
            Set(p, "type", (int)PuzzleData.PuzzleType.Code);
            Set(p, "title", "La caja del campanario");
            Set(p, "question", "Pide un año de 4 números. ¿Qué año estaba escrito junto al dibujo de los volcanes?");
            Set(p, "code", "1862");
        });
        FolderlessPuzzle("Q9_Volcanes", p =>
        {
            Set(p, "type", (int)PuzzleData.PuzzleType.Order);
            Set(p, "title", "La leyenda desordenada");
            Set(p, "question", "Las hojas de la leyenda se revolvieron. Ponlas en orden (la carta del inicio te ayuda).");
            Set(p, "options", new[] { "Se enamoran", "Él parte a la guerra", "Él la vela con su antorcha" });
            Set(p, "displayOrder", new[] { 2, 0, 1 });
        });

        CreateData<LevelData>(Data + "Nivel3_Puebla.asset", level =>
        {
            Set(level, "levelName", "Nivel 3");
            Set(level, "regionName", "Puebla");
            Set(level, "introText", "«Desde las azoteas se ven los dos volcanes. Mi amigo quería dibujarlos.»\n\nAquí viven los teporingos: no los pises. Las rocas brillan en el suelo antes de caer.");
            Set(level, "requiredKeys", 3);
            Set(level, "memoryFragmentText", "En una azotea, entre cúpulas de azulejo, alguien dibuja dos montañas...");
            Set(level, "memoryText", "Tu amigo te enseña su dibujo de los volcanes y dice: «Si un día no me ves, míralos y acuérdate de mí». Esta vez la niebla no logra borrar del todo su sonrisa.\n\n«Ya casi te recuerdo.»");
            Set(level, "completedText", "El volcán descansa y ya recuperaste tres recuerdos de tu amigo.\n\nContinuará: el viaje sigue hacia Oaxaca.");
            Set(level, "nextSceneName", "MainMenu");
        });
        SetLevelLegend("Nivel3_Puebla", "Volcanes");
        var legend = Legend("Volcanes");
        Set(legend, "picture", S("volcanes_jefe_0"));
        EditorUtility.SetDirty(legend);

        CreateData<AnimalData>(Data + "Animal_Teporingo.asset", animal =>
        {
            Set(animal, "commonName", "Teporingo");
            Set(animal, "species", "Romerolagus diazi");
            Set(animal, "facts", "Es un conejito de los volcanes que solo existe en México; vive en el Popocatépetl, el Iztaccíhuatl y montañas cercanas. Está en peligro de extinción: hay que cuidarlo. ¡No lo pises!");
            Set(animal, "picture", S("teporingo_0"));
        });

        CreateLevel3Prefabs();
        AssetDatabase.SaveAssets();
    }

    private static void CreateLevel3Prefabs()
    {
        if (!PrefabExists("Teporingo"))
        {
            var rabbit = new GameObject("Teporingo");
            var body = rabbit.AddComponent<Rigidbody2D>();
            body.bodyType = RigidbodyType2D.Kinematic;
            Box(rabbit, new Vector2(0.6f, 0.36f), new Vector2(0.03f, 0.23f), true);
            var visual = Renderer(rabbit, "Visual", S("teporingo_0"), 9);
            rabbit.AddComponent<Hazard>();
            var flip = rabbit.AddComponent<SpriteFlipbook>();
            Set(flip, "spriteRenderer", visual);
            Set(flip, "frames", new[] { S("teporingo_0"), S("teporingo_1") });
            Set(flip, "framesPerSecond", 3.5f);
            var patrol = rabbit.AddComponent<Patroller>();
            Set(patrol, "spriteRenderer", visual);
            Set(patrol, "walkAnimation", flip);

            var label = new GameObject("Especie");
            label.transform.SetParent(rabbit.transform, false);
            label.transform.localPosition = new Vector3(0f, 1.15f, 0f);
            var text = label.AddComponent<TextMeshPro>();
            text.fontSize = 3.2f;
            text.fontStyle = FontStyles.Bold;
            text.alignment = TextAlignmentOptions.Center;
            text.color = new Color(1f, 0.9f, 0.55f);
            text.textWrappingMode = TextWrappingModes.NoWrap;
            text.rectTransform.sizeDelta = new Vector2(6f, 0.6f);
            text.GetComponent<MeshRenderer>().sortingOrder = 14;
            var info = rabbit.AddComponent<AnimalInfo>();
            Set(info, "animal", AssetDatabase.LoadAssetAtPath<AnimalData>(Data + "Animal_Teporingo.asset"));
            Set(info, "speciesLabel", text);
            SavePrefab(rabbit, "Teporingo");
        }

        if (!PrefabExists("RocaVolcan"))
        {
            var rock = new GameObject("RocaVolcan");
            var body = rock.AddComponent<Rigidbody2D>();
            body.bodyType = RigidbodyType2D.Kinematic;
            var circle = rock.AddComponent<CircleCollider2D>();
            circle.radius = 0.22f;
            circle.isTrigger = true;
            var drop = Renderer(rock, "Visual", S("roca"), 11);
            var mark = Renderer(rock, "Aviso", S("roca_aviso"), 8);
            rock.AddComponent<Hazard>();
            var script = rock.AddComponent<TearDrop>();
            Set(script, "dropRenderer", drop);
            Set(script, "warningMark", mark);
            Set(script, "dropCollider", circle);
            SavePrefab(rock, "RocaVolcan");
        }

        if (!PrefabExists("LanzaRocas"))
        {
            var spawner = new GameObject("LanzaRocas");
            var script = spawner.AddComponent<TearSpawner>();
            Set(script, "tearPrefab", PrefabAsset("RocaVolcan").GetComponent<TearDrop>());
            SavePrefab(spawner, "LanzaRocas");
        }

        if (!PrefabExists("NubeDeCeniza"))
        {
            var cloud = new GameObject("NubeDeCeniza");
            var body = cloud.AddComponent<Rigidbody2D>();
            body.bodyType = RigidbodyType2D.Kinematic;
            var circle = cloud.AddComponent<CircleCollider2D>();
            circle.radius = 0.28f;
            circle.offset = new Vector2(-0.12f, 0f);
            circle.isTrigger = true;
            var visual = Renderer(cloud, "Visual", S("ceniza_0"), 11);
            cloud.AddComponent<Hazard>();
            var bat = cloud.AddComponent<Bat>();
            Set(bat, "spriteRenderer", visual);
            Set(bat, "waveAmplitude", 0.1f);
            var flip = cloud.AddComponent<SpriteFlipbook>();
            Set(flip, "spriteRenderer", visual);
            Set(flip, "frames", new[] { S("ceniza_0"), S("ceniza_1") });
            Set(flip, "framesPerSecond", 4f);
            SavePrefab(cloud, "NubeDeCeniza");
        }
    }

    private static void Teporingo(LevelContext level, AnimalGuide guide, string name, float x, float floorY, float reach, bool startLeft)
    {
        var rabbit = Spawn("Teporingo", new Vector2(x, floorY), level.map, name);
        Link<Patroller>(rabbit, ("leftDistance", reach), ("rightDistance", reach), ("speed", 0.85f), ("pauseTime", 1.5f), ("startToTheLeft", startLeft));
        Link<AnimalInfo>(rabbit, ("guide", guide), ("player", level.player.transform));
    }

    private static TearSpawner Rocks(LevelContext level, AudioSource cue, float minX, float maxX, float startY, float floorY, float interval)
    {
        var spawner = Spawn("LanzaRocas", new Vector2((minX + maxX) / 2f, startY), level.map);
        return Link<TearSpawner>(spawner, ("levelManager", level.manager), ("player", level.player.transform),
            ("minX", minX), ("maxX", maxX), ("startY", startY), ("floorY", floorY), ("interval", interval), ("warningTime", 1.1f),
            ("fallSpeed", 5.2f), ("aimAtPlayerChance", 0.3f), ("cueSource", cue), ("cueClip", Clip("aviso_roca")));
    }

    [MenuItem("Xoot/23. Construir Nivel 3 (Puebla)")]
    public static void BuildLevel3()
    {
        const string name = "Nivel3_Puebla";
        if (!CanBuild(name)) return;

        var level = BeginLevel("Nivel3_Puebla", new Vector2(2f, 1.05f), 32f, 40f, "musica_puebla", Color.white,
            Pixel + "bg_volcanes.png", "bg_puebla");
        var map = level.map;

        // Volcanes al fondo, casi quietos para que se sientan lejos
        var camera = Camera.main.transform;
        var volcanoes = Decoration(null, "VolcanesLejanos", S("bg_volcanes"), new Vector3(15f, 2.75f, 0f), new Vector3(1.7f, 1.7f, 1f), -95, new Color(1f, 1f, 1f, 0.95f));
        var far = volcanoes.gameObject.AddComponent<ParallaxLayer>();
        Set(far, "cameraTransform", camera);
        Set(far, "followX", 0.85f);
        Set(far, "followY", 0.93f);

        var guide = new GameObject("GuiaDeFauna").AddComponent<AnimalGuide>();
        Set(guide, "hud", level.hud);
        Set(guide, "levelManager", level.manager);

        // Zona 1: la calle, con una sola fosa y dos teporingos
        Ground(map, 0f, 13f, 1f, 3f);
        Ground(map, 13f, 15.4f, -0.4f, 1.6f);
        Spikes(map, 14.2f, -0.4f, 2.4f);
        Ground(map, 15.4f, 32f, 1f, 3f);
        Teporingo(level, guide, "Teporingo 1", 8f, 1f, 1.5f, false);
        Teporingo(level, guide, "Teporingo 2", 21.5f, 1f, 2f, true);

        Platform(map, 28.5f, 2.8f, 4f);
        Platform(map, 24.5f, 4.4f, 3f);
        Platform(map, 29f, 6f, 4.5f);
        Chest(level, 30.2f, 6f, "Q7_Azulejo", "Pista 1: el dibujo",
            "En el baúl hay un dibujo de dos montañas. A un lado dice: «Puebla, 1862: el año en que la ciudad se defendió».", "Cofre 1 (talavera)");
        SafePoint(level, 27.7f, 6f);

        // Zona 2: las azoteas
        Platform(map, 24.5f, 7.6f, 3f);
        Platform(map, 20f, 9.2f, 3f);
        Platform(map, 15f, 10.4f, 7f);
        SafePoint(level, 17.8f, 10.4f);
        Teporingo(level, guide, "Teporingo 3", 14f, 10.4f, 1.4f, false);
        Platform(map, 9.5f, 11.8f, 3f);
        Spawn("PlataformaQueCae", new Vector2(6f, 13f), map);
        Platform(map, 2.8f, 14.6f, 4.6f);
        Chest(level, 1.4f, 14.6f, "Q8_Anio", "Pista 2: la carta",
            "En la caja hay una carta: «Me gusta la leyenda de los volcanes porque nadie la ha podido olvidar. Ojalá a nadie le pase con las personas».", "Cofre 2 (campanario)");
        SafePoint(level, 4.3f, 14.6f);

        // Zona 3: las cúpulas
        Platform(map, 7.5f, 16.2f, 3f);
        Platform(map, 12.5f, 17.6f, 3f);
        Platform(map, 18f, 18.8f, 5f);
        SafePoint(level, 16.2f, 18.8f);
        Gate(level, 18.2f, 18.8f, 0.4f);
        Platform(map, 23.5f, 20.2f, 3f);
        Platform(map, 28.5f, 21.8f, 5f);
        SafePoint(level, 29.6f, 21.8f);
        Platform(map, 23.5f, 23.4f, 3f);
        SafePoint(level, 23.5f, 23.4f);
        Spawn("PlataformaQueCae", new Vector2(19f, 24.6f), map);
        Platform(map, 14f, 25.8f, 5f);
        Teporingo(level, guide, "Teporingo 4", 13.4f, 25.8f, 1.1f, true);
        Platform(map, 8.5f, 27.2f, 3f);
        Platform(map, 3f, 28.6f, 5f);
        Chest(level, 1.4f, 28.6f, "Q9_Volcanes", "Pista 3: la grabación",
            "En el cofre hay una cinta: «Cuando mires los volcanes, di en voz alta que todavía te acuerdas de mí».", "Cofre 3 (cúpula)");
        SafePoint(level, 4.4f, 28.6f);

        // Tramo del jefe: patrones lentos y de gran tamaño
        var bossObject = new GameObject("Popocatepetl e Iztaccihuatl");
        level.boss = bossObject.AddComponent<BossController>();
        var bossSprite = Decoration(bossObject.transform, "Silueta", S("volcanes_jefe_0"), new Vector3(20f, 35.4f, 0f), new Vector3(1.8f, 1.8f, 1f), 4, new Color(1f, 1f, 1f, 0.94f));
        var flip = bossSprite.gameObject.AddComponent<SpriteFlipbook>();
        Set(flip, "spriteRenderer", bossSprite);
        Set(flip, "frames", new[] { S("volcanes_jefe_0"), S("volcanes_jefe_1") });
        Set(flip, "framesPerSecond", 3f);
        Set(flip, "floatAmplitude", 0.2f);

        Platform(map, 7.2f, 30.2f, 2f);
        Platform(map, 13.5f, 31.4f, 8f);
        SafePoint(level, 10.2f, 31.4f);
        Platform(map, 22.2f, 31.4f, 7f);
        SafePoint(level, 19.2f, 31.4f);
        SafePoint(level, 25.1f, 31.4f);
        Spawn("PlataformaQueCae", new Vector2(27.5f, 32.2f), map);
        Platform(map, 30.5f, 33f, 3f);

        var cue = CueSource();
        var firstRocks = Rocks(level, cue, 11.6f, 16.2f, 36.8f, 31.4f, 1.5f);
        var lastRocks = Rocks(level, cue, 26.4f, 28.8f, 37.4f, 32.2f, 2.3f);
        var ash = Bats(level, new Vector2(27f, 32.2f), 3f, 3f, 3.2f, new Vector2(-0.12f, 0.12f));
        ash.name = "LanzaCeniza";
        Set(ash, "batPrefab", PrefabAsset("NubeDeCeniza").GetComponent<Bat>());
        Set(ash, "cueSource", cue);
        Set(ash, "cueClip", Clip("aviso_ceniza"));
        Set(ash, "player", level.player.transform);

        Zone(level, 10.9f, 31.4f, 0, false);
        Zone(level, 16.8f, 31.4f, 0, true);
        Zone(level, 19.9f, 31.4f, 1, false);
        Zone(level, 24.5f, 31.4f, 1, true);
        Zone(level, 25.5f, 31.4f, 2, false);
        Zone(level, 29.5f, 33f, 2, true);

        var door = Spawn("PuertaSalida", new Vector2(31f, 33f), map);
        Link<ExitDoor>(door, ("progress", level.progress), ("levelManager", level.manager), ("hud", level.hud), ("boss", level.boss));

        Set(level.boss, "bossName", "Popocatépetl e Iztaccíhuatl");
        Set(level.boss, "introText", "Las azoteas tiemblan. Son Popocatépetl e Iztaccíhuatl, pero el olvido enredó su historia y el volcán lanza rocas.\n\nNo hay que pelear: todo se mueve despacio. Míralo y esquívalo.");
        Set(level.boss, "calmedText", "Al recordar su historia completa, el volcán se calma y los dos vuelven a descansar.");
        Set(level.boss, "progress", level.progress);
        Set(level.boss, "hud", level.hud);
        Set(level.boss, "bossSprite", bossSprite);
        Set(level.boss, "bossAppearedEvent", Ev("JefeAparece"));
        Set(level.boss, "phaseClearedEvent", Ev("FaseSuperada"));

        var so = new SerializedObject(level.boss);
        var phases = so.FindProperty("phases");
        phases.arraySize = 3;
        Phase(phases, 0, "las rocas encendidas", "Cuentan que la princesa Iztaccíhuatl y el guerrero Popocatépetl se querían, pero él tuvo que partir a la guerra.", new BatSpawner[0]);
        Phase(phases, 1, "las nubes de ceniza", "A ella le dijeron que él había muerto, y la tristeza la venció.", new[] { ash });
        Phase(phases, 2, "el temblor de las azoteas", "Él la llevó a la montaña y se quedó velándola con su antorcha. Desde entonces son los dos volcanes.", new BatSpawner[0]);
        PhaseTears(phases, 0, firstRocks);
        PhaseTears(phases, 2, lastRocks);
        so.ApplyModifiedPropertiesWithoutUndo();

        ApplyTheme("pue_");
        AddPostProcessing();
        AddPlaceFacts();
        AddFog();
        SaveScene(name);
    }

    // Conecta el Nivel 2 con el Nivel 3 sin reconstruirlo
    [MenuItem("Xoot/24. Conectar el Nivel 2 con Puebla")]
    public static void ConnectLevel3()
    {
        var data = AssetDatabase.LoadAssetAtPath<LevelData>(Data + "Nivel2_Guanajuato.asset");
        Set(data, "nextSceneName", "Nivel3_Puebla");
        EditorUtility.SetDirty(data);
        AssetDatabase.SaveAssets();
    }
}
