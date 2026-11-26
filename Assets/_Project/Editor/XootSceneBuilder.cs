using System.Collections.Generic;
using TMPro;
using Unity.Cinemachine;
using UnityEditor;
using UnityEditor.Events;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

// Herramienta de editor: arma las escenas. Si una escena ya existe pide confirmación,
// para no borrar cambios hechos a mano.
public static partial class XootBuilder
{
    private static bool CanBuild(string sceneName)
    {
        string path = Scenes + sceneName + ".unity";
        if (!System.IO.File.Exists(path) || Application.isBatchMode) return true;

        return EditorUtility.DisplayDialog("Xoot", "La escena " + sceneName + " ya existe. ¿Reconstruirla y perder los cambios hechos a mano?", "Reconstruir", "Cancelar");
    }

    private static GameObject Spawn(string prefab, Vector2 position, Transform parent, string name = null)
    {
        var asset = AssetDatabase.LoadAssetAtPath<GameObject>(Prefabs + prefab + ".prefab");
        var instance = (GameObject)PrefabUtility.InstantiatePrefab(asset);
        instance.transform.SetParent(parent, false);
        instance.transform.position = position;
        if (name != null) instance.name = name;
        return instance;
    }

    // Bloque sólido definido por sus orillas: izquierda, derecha, altura del piso y grosor
    private static GameObject Ground(Transform parent, float left, float right, float topY, float height)
    {
        float width = right - left;
        var block = Spawn("Bloque", new Vector2((left + right) / 2f, topY), parent);
        var box = block.GetComponent<BoxCollider2D>();
        box.size = new Vector2(width, height);
        box.offset = new Vector2(0f, -height / 2f);
        var top = block.transform.Find("Orilla").GetComponent<SpriteRenderer>();
        top.size = new Vector2(width, Mathf.Min(1f, height));
        top.transform.localPosition = new Vector3(0f, -Mathf.Min(1f, height) / 2f, 0f);
        var fill = block.transform.Find("Relleno").GetComponent<SpriteRenderer>();
        float rest = Mathf.Max(0f, height - 1f);
        fill.gameObject.SetActive(rest > 0f);
        fill.size = new Vector2(width, rest);
        fill.transform.localPosition = new Vector3(0f, -1f - rest / 2f, 0f);
        return block;
    }

    private static GameObject Platform(Transform parent, float center, float topY, float width)
    {
        var platform = Spawn("Plataforma", new Vector2(center, topY), parent);
        platform.GetComponent<BoxCollider2D>().size = new Vector2(width, 0.5f);
        platform.transform.Find("Visual").GetComponent<SpriteRenderer>().size = new Vector2(width, 0.5f);
        return platform;
    }

    private static void Spikes(Transform parent, float center, float floorY, float width)
    {
        var spikes = Spawn("Pinchos", new Vector2(center, floorY), parent);
        spikes.GetComponent<BoxCollider2D>().size = new Vector2(width - 0.2f, 0.3f);
        spikes.transform.Find("Visual").GetComponent<SpriteRenderer>().size = new Vector2(width, 0.5f);
    }

    private static T Link<T>(GameObject target, params (string field, object value)[] values) where T : Component
    {
        var component = target.GetComponent<T>();
        foreach (var pair in values) Set(component, pair.field, pair.value);
        return component;
    }

    private static SpriteRenderer Decoration(Transform parent, string name, Sprite sprite, Vector3 position, Vector3 scale, int order, Color color)
    {
        var go = new GameObject(name);
        go.transform.SetParent(parent, false);
        go.transform.position = position;
        go.transform.localScale = scale;
        var renderer = go.AddComponent<SpriteRenderer>();
        renderer.sprite = sprite;
        renderer.sortingOrder = order;
        renderer.color = color;
        return renderer;
    }

    private class LevelContext
    {
        public Transform map;
        public GameObject player;
        public PlayerLives lives;
        public LevelManager manager;
        public LevelProgress progress;
        public ScoreManager score;
        public SceneFlow flow;
        public HUDController hud;
        public PuzzlePanelUI puzzlePanel;
        public BossController boss;
    }

    // Todo lo que comparten los niveles: jugador, sistemas, tablero, cámara, fondo y límites
    private static LevelContext BeginLevel(string dataName, Vector2 start, float width, float height, string music, Color tint, string memoryImagePath,
        string backdrop = "bg_aqueduct")
    {
        EditorSceneManager.NewScene(NewSceneSetup.EmptyScene, NewSceneMode.Single);

        // Los recursos se cargan después de crear la escena: al crearla, Unity descarga lo que no se usa
        var data = AssetDatabase.LoadAssetAtPath<LevelData>(Data + dataName + ".asset");
        var memoryImage = AssetDatabase.LoadAssetAtPath<Sprite>(memoryImagePath);
        var level = new LevelContext();
        level.map = new GameObject("Mapa").transform;

        level.player = Spawn("Jugador", start, null);
        level.lives = level.player.GetComponent<PlayerLives>();
        var controller = level.player.GetComponent<PlayerController2D>();

        var systems = new GameObject("Sistemas del nivel");
        level.progress = systems.AddComponent<LevelProgress>();
        Set(level.progress, "levelData", data);
        Set(level.progress, "keyCollectedEvent", Ev("LlaveObtenida"));
        level.score = systems.AddComponent<ScoreManager>();
        Set(level.score, "rules", AssetDatabase.LoadAssetAtPath<ScoreRules>(Data + "ReglasPuntaje.asset"));
        level.manager = systems.AddComponent<LevelManager>();
        level.flow = systems.AddComponent<SceneFlow>();
        Set(level.flow, "firstScene", "Intro");
        Set(level.flow, "levelProgress", level.progress);
        Set(level.flow, "levelManager", level.manager);

        level.hud = BuildHud(level.manager, level.lives, level.progress, level.score, level.flow, out level.puzzlePanel);
        CreateEventSystem();

        Set(level.manager, "progress", level.progress);
        Set(level.manager, "scoreManager", level.score);
        Set(level.manager, "playerLives", level.lives);
        Set(level.manager, "playerController", controller);
        Set(level.manager, "hud", level.hud);
        Set(level.manager, "levelCompletedEvent", Ev("NivelCompletado"));
        Set(level.manager, "outOfLivesEvent", Ev("SinCorazones"));
        Set(level.lives, "levelManager", level.manager);
        Set(level.player.GetComponent<PlayerInteractor>(), "levelManager", level.manager);

        var memory = systems.AddComponent<LevelMemory>();
        Set(memory, "progress", level.progress);
        Set(memory, "scoreManager", level.score);
        Set(memory, "hud", level.hud);
        Set(memory, "memoryImage", memoryImage);
        Set(memory, "memoryUnlockedEvent", Ev("RecuerdoDesbloqueado"));

        BuildAudio(controller, music);

        // Límites de la cámara
        var bounds = new GameObject("LimitesCamara") { layer = LayerMask.NameToLayer("Ignore Raycast") };
        var polygon = bounds.AddComponent<PolygonCollider2D>();
        polygon.isTrigger = true;
        polygon.points = new[] { new Vector2(-1f, -1.5f), new Vector2(width + 1f, -1.5f), new Vector2(width + 1f, height), new Vector2(-1f, height) };

        // DE: cámara inteligente con Cinemachine, Confiner 2D y sacudida
        var cameraObject = new GameObject("Main Camera") { tag = "MainCamera" };
        cameraObject.transform.position = new Vector3(start.x, start.y + 3f, -10f);
        var unityCamera = cameraObject.AddComponent<Camera>();
        unityCamera.orthographic = true;
        unityCamera.orthographicSize = 6f;
        unityCamera.clearFlags = CameraClearFlags.SolidColor;
        unityCamera.backgroundColor = new Color(0.12f, 0.13f, 0.27f);
        cameraObject.AddComponent<AudioListener>();
        cameraObject.AddComponent<CinemachineBrain>();

        var virtualObject = new GameObject("CinemachineCamera");
        virtualObject.transform.position = cameraObject.transform.position;
        var virtualCamera = virtualObject.AddComponent<CinemachineCamera>();
        virtualCamera.Lens.OrthographicSize = 6f;
        virtualCamera.Follow = level.player.transform;
        var composer = virtualObject.AddComponent<CinemachinePositionComposer>();
        composer.CameraDistance = 10f;
        composer.Damping = new Vector3(0.4f, 0.35f, 0f);
        composer.TargetOffset = new Vector3(0f, 1.6f, 0f);
        var confiner = virtualObject.AddComponent<CinemachineConfiner2D>();
        confiner.BoundingShape2D = polygon;
        virtualObject.AddComponent<CinemachineImpulseListener>();

        var shakeObject = new GameObject("SacudidaCamara");
        var source = shakeObject.AddComponent<CinemachineImpulseSource>();
        source.ImpulseDefinition.ImpulseDuration = 0.28f;
        var shake = shakeObject.AddComponent<CameraShake>();
        Set(shake, "lifeLostEvent", Ev("VidaPerdida"));
        Set(shake, "bossAppearedEvent", Ev("JefeAparece"));
        Set(shake, "phaseClearedEvent", Ev("FaseSuperada"));

        // Fondo: cielo fijo a la cámara y acueducto con profundidad
        var background = new GameObject("Fondo").transform;
        var sky = Decoration(cameraObject.transform, "Cielo", S("bg_sky"), Vector3.zero, new Vector3(1.3f, 1.3f, 1f), -100, tint);
        sky.transform.localPosition = new Vector3(0f, 0f, 20f);
        var arches = new GameObject("Acueducto");
        arches.transform.SetParent(background, false);
        arches.transform.position = new Vector3(width / 2f, 1.5f, 0f);
        var archRenderer = arches.AddComponent<SpriteRenderer>();
        archRenderer.sprite = S(backdrop);
        archRenderer.drawMode = SpriteDrawMode.Tiled;
        archRenderer.size = new Vector2(80f, 8f);
        archRenderer.sortingOrder = -90;
        archRenderer.color = tint;
        var parallax = arches.AddComponent<ParallaxLayer>();
        Set(parallax, "cameraTransform", cameraObject.transform);
        Set(parallax, "followX", 0.55f);
        Set(parallax, "followY", 0.8f);

        // Muros laterales y vacío bajo el mapa
        Ground(level.map, -2f, 0f, height + 2f, height + 5f).name = "Muro izquierdo";
        Ground(level.map, width, width + 2f, height + 2f, height + 5f).name = "Muro derecho";
        Spawn("Vacio", new Vector2(width / 2f, -5f), level.map);

        return level;
    }

    private static void BuildAudio(PlayerController2D player, string music)
    {
        var go = new GameObject("Audio");
        var musicSource = go.AddComponent<AudioSource>();
        musicSource.clip = Clip(music);
        musicSource.loop = true;
        musicSource.volume = 0.5f;
        musicSource.playOnAwake = true;
        var effects = go.AddComponent<AudioSource>();
        effects.playOnAwake = false;

        var manager = go.AddComponent<AudioManager>();
        Set(manager, "effectsSource", effects);
        Set(manager, "player", player);
        Set(manager, "jumpClip", Clip("salto"));

        var pairs = new[]
        {
            ("VidaPerdida", "vida_perdida"), ("CofreAbierto", "cofre"), ("LlaveObtenida", "llave"),
            ("PuzzleResuelto", "puzzle_bien"), ("PuzzleFallado", "puzzle_mal"), ("PuntoSeguro", "punto_seguro"),
            ("PuertaAbierta", "puerta"), ("NivelCompletado", "nivel_completado"), ("SinCorazones", "sin_corazones"),
            ("RecuerdoDesbloqueado", "recuerdo"), ("JefeAparece", "jefe"), ("FaseSuperada", "fase_superada")
        };

        var so = new SerializedObject(manager);
        var list = so.FindProperty("sounds");
        list.arraySize = pairs.Length;
        for (int i = 0; i < pairs.Length; i++)
        {
            var item = list.GetArrayElementAtIndex(i);
            item.FindPropertyRelative("gameEvent").objectReferenceValue = Ev(pairs[i].Item1);
            item.FindPropertyRelative("clip").objectReferenceValue = Clip(pairs[i].Item2);
            item.FindPropertyRelative("volume").floatValue = 0.75f;
        }

        so.ApplyModifiedPropertiesWithoutUndo();
    }

    private static void Chest(LevelContext level, float x, float floorY, string puzzle, string title, string clue, string name)
    {
        var chest = Spawn("Cofre", new Vector2(x, floorY), level.map, name);
        Link<KeyChest>(chest,
            ("progress", level.progress), ("scoreManager", level.score), ("hud", level.hud),
            ("puzzlePanel", level.puzzlePanel),
            ("puzzle", AssetDatabase.LoadAssetAtPath<PuzzleData>(Data + "Puzzles/" + puzzle + ".asset")),
            ("clueTitle", title), ("clueText", clue));
    }

    private static void SafePoint(LevelContext level, float x, float floorY)
    {
        var point = Spawn("PuntoSeguro", new Vector2(x, floorY), level.map);
        Link<Checkpoint>(point, ("hud", level.hud));
    }

    private static void Zone(LevelContext level, float x, float floorY, int phase, bool goal)
    {
        var zone = Spawn("ZonaJefe", new Vector2(x, floorY), level.map, (goal ? "Meta fase " : "Inicio fase ") + (phase + 1));
        Link<BossZone>(zone, ("boss", level.boss), ("phaseIndex", phase), ("isGoal", goal));
    }

    private static BatSpawner Bats(LevelContext level, Vector2 position, float interval, float speed, float life, Vector2 heights)
    {
        var spawner = Spawn("LanzaMurcielagos", position, level.map);
        return Link<BatSpawner>(spawner, ("levelManager", level.manager), ("interval", interval), ("batSpeed", speed),
            ("batLifeTime", life), ("direction", -1f), ("heightRange", heights));
    }

    private static void Gate(LevelContext level, float x, float floorY, float offset)
    {
        var gate = Spawn("RejaConTiempo", new Vector2(x, floorY + 1.5f), level.map);
        Link<TimedGate>(gate, ("startOffset", offset));
    }

    private static void SaveScene(string sceneName)
    {
        Folder(Scenes.TrimEnd('/'));
        EditorSceneManager.SaveScene(SceneManager.GetActiveScene(), Scenes + sceneName + ".unity");

        var list = new List<EditorBuildSettingsScene>(EditorBuildSettings.scenes);
        string path = Scenes + sceneName + ".unity";
        if (!list.Exists(s => s.path == path)) list.Add(new EditorBuildSettingsScene(path, true));

        // El menú siempre va primero y se quitan escenas que ya no existen
        list.RemoveAll(s => !System.IO.File.Exists(s.path) || !s.path.StartsWith(Scenes));
        list.Sort((a, b) => Order(a.path).CompareTo(Order(b.path)));
        EditorBuildSettings.scenes = list.ToArray();
    }

    private static int Order(string path)
    {
        if (path.EndsWith("MainMenu.unity")) return 0;
        if (path.EndsWith("Intro.unity")) return 1;
        if (path.Contains("Nivel1")) return 2;
        if (path.Contains("Nivel2")) return 3;
        if (path.Contains("Nivel3")) return 4;
        return 9;
    }

    // ---------------------------------------------------------------- Nivel 1: Querétaro
    [MenuItem("Xoot/5. Construir Nivel 1 (Querétaro)")]
    public static void BuildLevel1()
    {
        const string name = "Nivel1_Queretaro";
        if (!CanBuild(name)) return;

        var level = BeginLevel("Nivel1_Queretaro", new Vector2(2f, 1.05f), 32f, 36f, "musica_queretaro", Color.white,
            "Assets/_Project/Art/Cinematica/escena_16.png");
        var map = level.map;

        // Zona 1: la estación. Suelo con una fosa de pinchos para aprender a saltar
        Ground(map, 0f, 12f, 1f, 3f);
        Ground(map, 12f, 14.6f, -0.4f, 1.6f);
        Spikes(map, 13.3f, -0.4f, 2.6f);
        Ground(map, 14.6f, 32f, 1f, 3f);
        Platform(map, 22f, 2.8f, 3f);
        Platform(map, 27.5f, 4.6f, 5f);
        Chest(level, 29.2f, 4.6f, "Q1_OrdenarFotos", "Pista 1: la fotografía",
            "Dentro de la maleta hay una fotografía de la estación. Al reverso alguien marcó una fecha: 15 · 09.\n\n«Esta fecha... la conozco.»", "Cofre 1 (maleta)");
        Platform(map, 22.5f, 6.4f, 3f);
        Platform(map, 18f, 8.2f, 3f);
        Platform(map, 13f, 10f, 5f);
        SafePoint(level, 14.4f, 10f);

        // Zona 2: balcones. Aparecen las plataformas que se caen
        Spawn("PlataformaQueCae", new Vector2(8f, 11.4f), map);
        Spawn("PlataformaQueCae", new Vector2(4.8f, 12.8f), map);
        Platform(map, 2f, 14.4f, 3.5f);
        Chest(level, 1.1f, 14.4f, "Q2_CajaConsigna", "Pista 2: el registro de pasajeros",
            "En la caja de consigna hay un registro de pasajeros. La hoja de aquel día está arrancada a la mitad: solo se lee «...junto a los arcos».\n\n«Aquí hay algo más.»", "Cofre 2 (caja de consigna)");
        SafePoint(level, 3f, 14.4f);
        Platform(map, 6.8f, 16.2f, 3f);
        Platform(map, 11.3f, 18f, 3f);
        Platform(map, 19f, 19.4f, 9f);
        SafePoint(level, 15.4f, 19.4f);
        Gate(level, 19.5f, 19.4f, 0f);

        // Zona 3: las azoteas
        Platform(map, 26.5f, 21f, 3f);
        Platform(map, 22.3f, 22.8f, 3f);
        Platform(map, 15.8f, 23f, 5f);
        Spikes(map, 15.8f, 23f, 1f);
        Spawn("PlataformaQueCae", new Vector2(10.5f, 24f), map);
        Platform(map, 6.8f, 25f, 2f);
        Platform(map, 2.5f, 26f, 4.5f);
        Chest(level, 1.3f, 26f, "Q3_Registro", "Pista 3: la grabación",
            "En el cofre hay una grabadora vieja. Se escucha una voz conocida: «Si algún día me olvidan, búscame donde comenzó todo...». La cinta se corta con un ruido de niebla.", "Cofre 3 (andén)");
        SafePoint(level, 3.9f, 26f);

        // Tramo del jefe: 3 fases de izquierda a derecha hasta la puerta
        var bossObject = new GameObject("La Zacatecana");
        level.boss = bossObject.AddComponent<BossController>();
        var bossSprite = Decoration(bossObject.transform, "Silueta", S("zacatecana_0"), new Vector3(20f, 32.4f, 0f), new Vector3(1.4f, 1.4f, 1f), 4, new Color(1f, 1f, 1f, 0.92f));
        var flip = bossSprite.gameObject.AddComponent<SpriteFlipbook>();
        Set(flip, "spriteRenderer", bossSprite);
        Set(flip, "frames", new[] { S("zacatecana_0"), S("zacatecana_1") });
        Set(flip, "framesPerSecond", 2f);
        Set(flip, "floatAmplitude", 0.35f);

        Platform(map, 7f, 27.5f, 2f);
        Platform(map, 13.25f, 28.7f, 7.5f);
        SafePoint(level, 10f, 28.7f);
        Platform(map, 22f, 28.7f, 7f);
        SafePoint(level, 19f, 28.7f);
        Gate(level, 21.2f, 28.7f, 0f);
        Gate(level, 23.4f, 28.7f, 1.3f);
        SafePoint(level, 24.9f, 28.7f);
        Spawn("PlataformaQueCae", new Vector2(27.4f, 29.5f), map);
        Platform(map, 30.5f, 30.3f, 3f);

        var phase1 = Bats(level, new Vector2(18f, 29.55f), 1.9f, 4f, 2.5f, new Vector2(-0.25f, 0.2f));
        var phase3 = Bats(level, new Vector2(32f, 31.4f), 2.3f, 3.6f, 2.6f, new Vector2(-0.4f, 0.3f));

        Zone(level, 10.9f, 28.7f, 0, false);
        Zone(level, 16.5f, 28.7f, 0, true);
        Zone(level, 19.9f, 28.7f, 1, false);
        Zone(level, 24.3f, 28.7f, 1, true);
        Zone(level, 25.4f, 28.7f, 2, false);
        Zone(level, 29.4f, 30.3f, 2, true);

        var door = Spawn("PuertaSalida", new Vector2(31f, 30.3f), map);
        Link<ExitDoor>(door, ("progress", level.progress), ("levelManager", level.manager), ("hud", level.hud), ("boss", level.boss));

        Set(level.boss, "bossName", "La Zacatecana");
        Set(level.boss, "introText", "Una sombra con velo aparece sobre los arcos. Según cuenta la leyenda, es La Zacatecana, pero el Ente del Olvido torció su historia.\n\nNo hay que pelear: cruza los tres tramos sin tocar sus obstáculos.");
        Set(level.boss, "calmedText", "Al recordar su historia como Querétaro la cuenta, la sombra se calma y deja libre la puerta.");
        Set(level.boss, "progress", level.progress);
        Set(level.boss, "hud", level.hud);
        Set(level.boss, "bossSprite", bossSprite);
        Set(level.boss, "bossAppearedEvent", Ev("JefeAparece"));
        Set(level.boss, "phaseClearedEvent", Ev("FaseSuperada"));

        var so = new SerializedObject(level.boss);
        var phases = so.FindProperty("phases");
        phases.arraySize = 3;
        Phase(phases, 0, "los murciélagos", "Cuentan que a inicios del siglo XIX una mujer llegada de Zacatecas vivió con su esposo, un rico minero, en una casona del centro de Querétaro.", new[] { phase1 });
        Phase(phases, 1, "las rejas", "Dicen que un día su esposo desapareció y que ella guardó un secreto que nadie más conocía.", new BatSpawner[0]);
        Phase(phases, 2, "las azoteas rotas", "Hoy la casona es un museo y Querétaro sigue contando su leyenda. Recordarla le quita fuerza al olvido.", new[] { phase3 });
        so.ApplyModifiedPropertiesWithoutUndo();

        AddPostProcessing();
        SaveScene(name);
    }

    private static void Phase(SerializedProperty phases, int index, string name, string story, BatSpawner[] spawners)
    {
        var phase = phases.GetArrayElementAtIndex(index);
        phase.FindPropertyRelative("name").stringValue = name;
        phase.FindPropertyRelative("storyFragment").stringValue = story;
        var list = phase.FindPropertyRelative("spawners");
        list.arraySize = spawners.Length;
        for (int i = 0; i < spawners.Length; i++) list.GetArrayElementAtIndex(i).objectReferenceValue = spawners[i];
        phase.FindPropertyRelative("tearSpawners").arraySize = 0;
        phase.FindPropertyRelative("obstacles").arraySize = 0;
    }

    // ---------------------------------------------------------------- Menú principal
    [MenuItem("Xoot/6. Construir menú principal")]
    public static void BuildMainMenu()
    {
        if (!CanBuild("MainMenu")) return;

        EditorSceneManager.NewScene(NewSceneSetup.EmptyScene, NewSceneMode.Single);
        var cameraObject = new GameObject("Main Camera") { tag = "MainCamera" };
        var camera = cameraObject.AddComponent<Camera>();
        camera.orthographic = true;
        camera.clearFlags = CameraClearFlags.SolidColor;
        camera.backgroundColor = new Color(0.12f, 0.13f, 0.27f);
        cameraObject.AddComponent<AudioListener>();
        var music = cameraObject.AddComponent<AudioSource>();
        music.clip = Clip("musica_queretaro");
        music.loop = true;
        music.volume = 0.45f;

        var systems = new GameObject("SceneFlow");
        var flow = systems.AddComponent<SceneFlow>();
        Set(flow, "firstScene", "Intro");

        var canvas = UiCanvas("Menu");
        CreateEventSystem();
        var sky = UiImage(canvas.transform, "Cielo", S("bg_sky"), Color.white);
        Stretch(sky.gameObject);
        for (int i = 0; i < 3; i++)
        {
            var arch = UiImage(canvas.transform, "Acueducto" + (i + 1), S("bg_aqueduct"), Color.white);
            Place(arch.gameObject, new Vector2(0f, 0f), new Vector2(i * 512f, 0f), new Vector2(512f, 256f));
        }

        var title = UiText(canvas.transform, "Titulo", "XOOT MEMORIAL", 82f, Gold, TextAlignmentOptions.Center);
        title.fontStyle = FontStyles.Bold;
        Place(title.gameObject, new Vector2(0.5f, 1f), new Vector2(0f, -60f), new Vector2(1000f, 110f));
        var subtitle = UiText(canvas.transform, "Subtitulo", "Una aventura para recuperar lo que se está olvidando", 28f, Cream, TextAlignmentOptions.Center);
        Place(subtitle.gameObject, new Vector2(0.5f, 1f), new Vector2(0f, -170f), new Vector2(1000f, 44f));

        var hero = UiImage(canvas.transform, "Aketzali", S("aketzali_idle_0"), Color.white);
        hero.preserveAspect = true;
        Place(hero.gameObject, new Vector2(0f, 0f), new Vector2(170f, 60f), new Vector2(224f, 322f));

        var buttons = UiObject(canvas.transform, "Botones");
        Stretch(buttons);
        var play = UiButton(buttons.transform, "Jugar", "Jugar", new Vector2(0f, 20f), new Vector2(340f, 70f), 32f);
        var controls = UiButton(buttons.transform, "Controles", "Controles", new Vector2(0f, -70f), new Vector2(340f, 70f), 32f);
        var quit = UiButton(buttons.transform, "Salir", "Salir", new Vector2(0f, -160f), new Vector2(340f, 70f), 32f);

        Transform box;
        var window = UiWindow(canvas.transform, "PanelControles", new Vector2(780f, 500f), out box);
        Heading(box, "Controles", 185f);
        var help = UiText(box, "Texto",
            "A / D o flechas: moverte\nShift (mantener): correr\nEspacio: saltar (suéltalo pronto para un salto corto)\nE: abrir cofres y leer pistas\nEsc: pausa\n\nEncuentra las 3 llaves de cada nivel, calma a la leyenda y llega a la puerta. Tienes 3 corazones.",
            27f, Cream, TextAlignmentOptions.Center);
        Place(help.gameObject, new Vector2(0.5f, 0.5f), new Vector2(0f, 5f), new Vector2(700f, 300f));
        var back = UiButton(box, "Volver", "Volver", new Vector2(0f, -195f), new Vector2(260f, 60f));
        window.SetActive(false);

        OnClick(play, flow.Play);
        OnClick(quit, flow.Quit);
        UnityEventTools.AddBoolPersistentListener(controls.onClick, window.SetActive, true);
        UnityEventTools.AddBoolPersistentListener(back.onClick, window.SetActive, false);

        AddLevelSelect();
        AddLegendsMenu();
        SaveScene("MainMenu");
    }

    // ---------------------------------------------------------------- Secuencia inicial (RQF-06)
    // Un sonido por viñeta, siguiendo las notas del storyboard
    private static readonly string[] IntroSounds =
    {
        "in01_vaso", "in02_click", "in03_reloj", "in04_ring", "in05_ring", "in06_fastidio", "in07_grabadora", "in08_mensaje",
        "in09_sorpresa", "in10_recuerdo", "in11_pasos", "in12_corre", "in13_mensaje", "in14_colgar", "in15_lagrimas", "in16_postal"
    };

    private static readonly float[] IntroDurations =
    {
        3f, 3f, 3f, 3.4f, 3.4f, 3f, 3.4f, 4.2f,
        3.2f, 3.8f, 4f, 3.2f, 4.4f, 3.4f, 3.8f, 4.4f
    };

    private static AudioClip IntroSound(int index) => AssetDatabase.LoadAssetAtPath<AudioClip>(Audio + "Intro/" + IntroSounds[index] + ".wav");

    private static void SetIntroAudio(IntroComic intro)
    {
        var music = intro.gameObject.AddComponent<AudioSource>();
        music.clip = AssetDatabase.LoadAssetAtPath<AudioClip>(Audio + "Intro/musica_intro.wav");
        music.loop = true;
        music.volume = 0.4f;
        music.playOnAwake = true;
        Set(intro, "musicSource", music);
    }

    [MenuItem("Xoot/8. Poner sonido a la secuencia abierta")]
    public static void AddIntroSounds()
    {
        var intro = Object.FindAnyObjectByType<IntroComic>();
        if (intro == null) return;

        var so = new SerializedObject(intro);
        if (so.FindProperty("musicSource").objectReferenceValue == null)
        {
            SetIntroAudio(intro);
            so.Update();
        }

        var panels = so.FindProperty("panels");
        for (int i = 0; i < panels.arraySize && i < IntroSounds.Length; i++)
        {
            var panel = panels.GetArrayElementAtIndex(i);
            panel.FindPropertyRelative("sound").objectReferenceValue = IntroSound(i);
            panel.FindPropertyRelative("soundVolume").floatValue = 1f;
            panel.FindPropertyRelative("duration").floatValue = IntroDurations[i];
        }

        so.ApplyModifiedProperties();
        EditorSceneManager.MarkSceneDirty(intro.gameObject.scene);
    }



    [MenuItem("Xoot/7. Construir secuencia inicial")]
    public static void BuildIntro()
    {
        if (!CanBuild("Intro")) return;

        EditorSceneManager.NewScene(NewSceneSetup.EmptyScene, NewSceneMode.Single);
        var cameraObject = new GameObject("Main Camera") { tag = "MainCamera" };
        var camera = cameraObject.AddComponent<Camera>();
        camera.orthographic = true;
        camera.clearFlags = CameraClearFlags.SolidColor;
        camera.backgroundColor = Color.black;
        cameraObject.AddComponent<AudioListener>();

        var canvas = UiCanvas("Comic");
        var black = UiImage(canvas.transform, "Negro", null, Color.black);
        Stretch(black.gameObject);
        var holder = UiObject(canvas.transform, "Vineta");
        Stretch(holder);
        var group = holder.AddComponent<CanvasGroup>();
        var picture = UiImage(holder.transform, "Imagen", null, Color.white);
        picture.preserveAspect = true;
        Stretch(picture.gameObject);
        var skip = UiText(canvas.transform, "Saltar", "Enter o Espacio para saltar", 20f, new Color(1f, 1f, 1f, 0.75f), TextAlignmentOptions.BottomRight);
        Place(skip.gameObject, new Vector2(1f, 0f), new Vector2(-20f, 14f), new Vector2(420f, 34f));

        var comic = new GameObject("Secuencia");
        var audio = comic.AddComponent<AudioSource>();
        audio.playOnAwake = false;
        var intro = comic.AddComponent<IntroComic>();
        SetIntroAudio(intro);
        Set(intro, "panelImage", picture);
        Set(intro, "panelGroup", group);
        Set(intro, "audioSource", audio);
        Set(intro, "nextScene", "Nivel1_Queretaro");

        var so = new SerializedObject(intro);
        var panels = so.FindProperty("panels");
        panels.arraySize = 16;
        for (int i = 0; i < 16; i++)
        {
            var panel = panels.GetArrayElementAtIndex(i);
            panel.FindPropertyRelative("image").objectReferenceValue =
                AssetDatabase.LoadAssetAtPath<Sprite>("Assets/_Project/Art/Cinematica/escena_" + (i + 1).ToString("00") + ".png");
            panel.FindPropertyRelative("duration").floatValue = IntroDurations[i];
            panel.FindPropertyRelative("zoomStart").floatValue = 1f;
            panel.FindPropertyRelative("zoomEnd").floatValue = 1.05f;
            panel.FindPropertyRelative("sound").objectReferenceValue = IntroSound(i);
            panel.FindPropertyRelative("soundVolume").floatValue = 1f;
        }

        so.ApplyModifiedPropertiesWithoutUndo();
        SaveScene("Intro");
    }
}
