using TMPro;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.Rendering;
using UnityEngine.Rendering.Universal;

// Herramienta de editor: efectos visuales (DE), contenido y escena del Nivel 2 (Guanajuato).
public static partial class XootBuilder
{
    // ---------------------------------------------------------------- DE: partículas
    private static Material ParticleMaterial()
    {
        string path = Pixel + "Particula.mat";
        var material = AssetDatabase.LoadAssetAtPath<Material>(path);
        if (material != null) return material;

        material = new Material(Shader.Find("Universal Render Pipeline/2D/Sprite-Unlit-Default"));
        material.mainTexture = AssetDatabase.LoadAssetAtPath<Texture2D>(Pixel + "particula.png");
        AssetDatabase.CreateAsset(material, path);
        return material;
    }

    private static ParticleSystem Effect(Transform parent, string name, Vector3 local, Color color, int count,
        float speed, float size, float life, float gravity, float radius, bool unscaled)
    {
        var go = new GameObject(name);
        go.transform.SetParent(parent, false);
        go.transform.localPosition = local;
        var particles = go.AddComponent<ParticleSystem>();
        particles.Stop(true, ParticleSystemStopBehavior.StopEmittingAndClear);

        var main = particles.main;
        main.duration = 0.5f;
        main.loop = false;
        main.playOnAwake = false;
        main.startLifetime = new ParticleSystem.MinMaxCurve(life * 0.6f, life);
        main.startSpeed = new ParticleSystem.MinMaxCurve(speed * 0.4f, speed);
        main.startSize = new ParticleSystem.MinMaxCurve(size * 0.6f, size);
        main.startColor = color;
        main.gravityModifier = gravity;
        main.simulationSpace = ParticleSystemSimulationSpace.World;
        main.useUnscaledTime = unscaled;
        main.maxParticles = 60;

        var emission = particles.emission;
        emission.rateOverTime = 0f;
        emission.SetBursts(new[] { new ParticleSystem.Burst(0f, (short)count) });

        var shape = particles.shape;
        shape.shapeType = ParticleSystemShapeType.Circle;
        shape.radius = radius;

        var fade = particles.colorOverLifetime;
        fade.enabled = true;
        var gradient = new Gradient();
        gradient.SetKeys(new[] { new GradientColorKey(Color.white, 0f), new GradientColorKey(Color.white, 1f) },
            new[] { new GradientAlphaKey(1f, 0f), new GradientAlphaKey(1f, 0.5f), new GradientAlphaKey(0f, 1f) });
        fade.color = gradient;

        var shrink = particles.sizeOverLifetime;
        shrink.enabled = true;
        shrink.size = new ParticleSystem.MinMaxCurve(1f, AnimationCurve.Linear(0f, 1f, 1f, 0.3f));

        var renderer = go.GetComponent<ParticleSystemRenderer>();
        renderer.sharedMaterial = ParticleMaterial();
        renderer.sortingOrder = 13;
        return particles;
    }

    private static void EditPrefab(string name, System.Action<GameObject> change)
    {
        string path = Prefabs + name + ".prefab";
        var root = PrefabUtility.LoadPrefabContents(path);
        change(root);
        PrefabUtility.SaveAsPrefabAsset(root, path);
        PrefabUtility.UnloadPrefabContents(root);
    }

    // Agrega los efectos a los prefabs existentes sin rehacerlos (las escenas conservan sus ajustes)
    [MenuItem("Xoot/9. Agregar partículas a los prefabs")]
    public static void AddEffects()
    {
        EditPrefab("Jugador", root =>
        {
            if (root.transform.Find("FX_Polvo") != null) return;
            var dust = Effect(root.transform, "FX_Polvo", new Vector3(0f, 0.08f, 0f), new Color(0.92f, 0.84f, 0.76f, 0.85f), 9, 1.6f, 0.17f, 0.4f, 0.1f, 0.22f, false);
            var hit = Effect(root.transform, "FX_Golpe", new Vector3(0f, 0.7f, 0f), new Color(1f, 0.28f, 0.34f), 22, 4.5f, 0.2f, 0.6f, 0.8f, 0.2f, false);
            Set(root.GetComponent<PlayerAnimation2D>(), "dustEffect", dust);
            Set(root.GetComponent<PlayerLives>(), "hurtEffect", hit);
        });

        EditPrefab("Cofre", root =>
        {
            if (root.transform.Find("FX_Brillo") != null) return;
            var glow = Effect(root.transform, "FX_Brillo", new Vector3(0f, 0.7f, 0f), new Color(1f, 0.85f, 0.35f), 28, 3.2f, 0.18f, 1f, 0.35f, 0.35f, true);
            Set(root.GetComponent<KeyChest>(), "openEffect", glow);
        });

        EditPrefab("PuntoSeguro", root =>
        {
            if (root.transform.Find("FX_Luz") != null) return;
            var light = Effect(root.transform, "FX_Luz", new Vector3(0f, 1.05f, 0f), new Color(1f, 0.9f, 0.55f), 16, 1.8f, 0.14f, 0.8f, -0.15f, 0.2f, false);
            Set(root.GetComponent<Checkpoint>(), "activateEffect", light);
        });

        EditPrefab("PuertaSalida", root =>
        {
            if (root.transform.Find("FX_Puerta") != null) return;
            var shine = Effect(root.transform, "FX_Puerta", new Vector3(0f, 1.1f, 0f), new Color(1f, 0.95f, 0.7f), 40, 3.5f, 0.2f, 1.1f, -0.1f, 0.6f, true);
            Set(root.GetComponent<ExitDoor>(), "openEffect", shine);
        });

        AssetDatabase.SaveAssets();
    }

    // ---------------------------------------------------------------- DE: post-processing
    private static VolumeProfile FogProfile()
    {
        string path = Data + "PostNiebla.asset";
        var profile = AssetDatabase.LoadAssetAtPath<VolumeProfile>(path);
        if (profile != null) return profile;

        profile = ScriptableObject.CreateInstance<VolumeProfile>();
        AssetDatabase.CreateAsset(profile, path);

        var bloom = profile.Add<Bloom>(false);
        bloom.threshold.Override(0.85f);
        bloom.intensity.Override(0.7f);
        bloom.scatter.Override(0.6f);
        var vignette = profile.Add<Vignette>(false);
        vignette.intensity.Override(0.28f);
        vignette.smoothness.Override(0.45f);
        var color = profile.Add<ColorAdjustments>(false);
        color.saturation.Override(-45f);
        color.contrast.Override(8f);

        foreach (VolumeComponent component in profile.components)
        {
            component.name = component.GetType().Name;
            AssetDatabase.AddObjectToAsset(component, profile);
        }

        EditorUtility.SetDirty(profile);
        AssetDatabase.SaveAssets();
        return profile;
    }

    [MenuItem("Xoot/10. Agregar post-processing al nivel abierto")]
    public static void AddPostProcessing()
    {
        var progress = Object.FindAnyObjectByType<LevelProgress>();
        var camera = Camera.main;
        if (progress == null || camera == null) return;

        var cameraData = camera.GetUniversalAdditionalCameraData();
        cameraData.renderPostProcessing = true;

        if (Object.FindAnyObjectByType<MemoryColor>() == null)
        {
            var go = new GameObject("PostProcesado");
            var volume = go.AddComponent<Volume>();
            volume.isGlobal = true;
            volume.sharedProfile = FogProfile();
            Set(go.AddComponent<MemoryColor>(), "progress", progress);
        }

        EditorSceneManager.MarkSceneDirty(progress.gameObject.scene);
    }

    // ---------------------------------------------------------------- Contenido del Nivel 2
    [MenuItem("Xoot/11. Crear contenido del Nivel 2")]
    public static void CreateLevel2Content()
    {
        ImportArt();

        CreateData<AnimalData>(Data + "Animal_Alacran.asset", animal =>
        {
            Set(animal, "commonName", "Alacrán");
            Set(animal, "species", "Centruroides infamatus");
            Set(animal, "facts", "Es una de las especies de alacrán de Guanajuato y sale sobre todo de noche. Su picadura puede ser peligrosa: en la vida real ve al médico. ¡Sáltalo!");
            Set(animal, "picture", S("alacran_0"));
        });

        CreateData<PuzzleData>(Data + "Puzzles/Q4_Callejones.asset", p =>
        {
            Set(p, "type", (int)PuzzleData.PuzzleType.Order);
            Set(p, "title", "El baúl del minero");
            Set(p, "question", "La etiqueta dice: «Primero subimos al mirador, luego cruzamos el callejón y al final bajamos al túnel». Ordena las fotos del paseo.");
            Set(p, "options", new[] { "El mirador", "El callejón", "El túnel" });
            Set(p, "displayOrder", new[] { 1, 2, 0 });
        });
        CreateData<PuzzleData>(Data + "Puzzles/Q5_Buzon.asset", p =>
        {
            Set(p, "type", (int)PuzzleData.PuzzleType.Code);
            Set(p, "title", "El buzón del callejón");
            Set(p, "question", "Tiene una cerradura de 4 números. ¿Qué fecha estaba escrita al reverso de la fotografía?");
            Set(p, "code", "2809");
        });
        CreateData<PuzzleData>(Data + "Puzzles/Q6_Eco.asset", p =>
        {
            Set(p, "type", (int)PuzzleData.PuzzleType.Choice);
            Set(p, "title", "La nota incompleta");
            Set(p, "question", "«Si quieres encontrarme, escucha en ___, donde el eco repite tu nombre».");
            Set(p, "options", new[] { "el mercado", "la plaza", "los túneles", "el teatro" });
            Set(p, "correctChoice", 2);
        });

        CreateData<LevelData>(Data + "Nivel2_Guanajuato.asset", level =>
        {
            Set(level, "levelName", "Nivel 2");
            Set(level, "regionName", "Guanajuato");
            Set(level, "introText", "«Los callejones suben y suben... y alguien llora a lo lejos.»\n\nCuidado con los alacranes. Cuando oigas el llanto, fíjate de qué lado viene.");
            Set(level, "requiredKeys", 3);
            Set(level, "memoryFragmentText", "Un callejón lleno de risas... Tu amigo te enseña una foto, pero la niebla no te deja verla.");
            Set(level, "memoryText", "En lo alto de un callejón, tu amigo te dice: «Cuando no me veas, escucha». El eco repite su voz hasta que la niebla la apaga.\n\n«Estuvo aquí. No lo voy a olvidar.»");
            Set(level, "completedText", "El llanto se apaga y los callejones recuperan sus colores. El viaje continúa en Puebla.");
            Set(level, "nextSceneName", "MainMenu");
        });

        CreateLevel2Prefabs();
        AssetDatabase.SaveAssets();
    }

    private static bool PrefabExists(string name) => System.IO.File.Exists(Prefabs + name + ".prefab");

    private static GameObject PrefabAsset(string name) => AssetDatabase.LoadAssetAtPath<GameObject>(Prefabs + name + ".prefab");

    private static void CreateLevel2Prefabs()
    {
        if (!PrefabExists("Alacran"))
        {
            var scorpion = new GameObject("Alacran");
            var body = scorpion.AddComponent<Rigidbody2D>();
            body.bodyType = RigidbodyType2D.Kinematic;
            Box(scorpion, new Vector2(0.72f, 0.34f), new Vector2(0f, 0.2f), true);
            var visual = Renderer(scorpion, "Visual", S("alacran_0"), 9);
            scorpion.AddComponent<Hazard>();
            var flip = scorpion.AddComponent<SpriteFlipbook>();
            Set(flip, "spriteRenderer", visual);
            Set(flip, "frames", new[] { S("alacran_0"), S("alacran_1") });
            Set(flip, "framesPerSecond", 6f);
            var patrol = scorpion.AddComponent<Patroller>();
            Set(patrol, "spriteRenderer", visual);
            Set(patrol, "walkAnimation", flip);

            var label = new GameObject("Especie");
            label.transform.SetParent(scorpion.transform, false);
            label.transform.localPosition = new Vector3(0f, 1.05f, 0f);
            var text = label.AddComponent<TextMeshPro>();
            text.fontSize = 3.2f;
            text.fontStyle = FontStyles.Italic | FontStyles.Bold;
            text.alignment = TextAlignmentOptions.Center;
            text.color = new Color(1f, 0.9f, 0.55f);
            text.textWrappingMode = TextWrappingModes.NoWrap;
            text.rectTransform.sizeDelta = new Vector2(6f, 0.6f);
            text.GetComponent<MeshRenderer>().sortingOrder = 14;
            var info = scorpion.AddComponent<AnimalInfo>();
            Set(info, "animal", AssetDatabase.LoadAssetAtPath<AnimalData>(Data + "Animal_Alacran.asset"));
            Set(info, "speciesLabel", text);
            SavePrefab(scorpion, "Alacran");
        }

        if (!PrefabExists("Lagrima"))
        {
            var tear = new GameObject("Lagrima");
            var body = tear.AddComponent<Rigidbody2D>();
            body.bodyType = RigidbodyType2D.Kinematic;
            var circle = tear.AddComponent<CircleCollider2D>();
            circle.radius = 0.18f;
            circle.isTrigger = true;
            var drop = Renderer(tear, "Visual", S("lagrima"), 11);
            var mark = Renderer(tear, "Aviso", S("lagrima_aviso"), 8);
            tear.AddComponent<Hazard>();
            var script = tear.AddComponent<TearDrop>();
            Set(script, "dropRenderer", drop);
            Set(script, "warningMark", mark);
            Set(script, "dropCollider", circle);
            SavePrefab(tear, "Lagrima");
        }

        if (!PrefabExists("LanzaLagrimas"))
        {
            var spawner = new GameObject("LanzaLagrimas");
            var script = spawner.AddComponent<TearSpawner>();
            Set(script, "tearPrefab", PrefabAsset("Lagrima").GetComponent<TearDrop>());
            SavePrefab(spawner, "LanzaLagrimas");
        }

        if (!PrefabExists("EcoDelLlanto"))
        {
            var echo = new GameObject("EcoDelLlanto");
            var body = echo.AddComponent<Rigidbody2D>();
            body.bodyType = RigidbodyType2D.Kinematic;
            var circle = echo.AddComponent<CircleCollider2D>();
            circle.radius = 0.28f;
            circle.offset = new Vector2(-0.15f, 0f);
            circle.isTrigger = true;
            var visual = Renderer(echo, "Visual", S("eco_0"), 11);
            echo.AddComponent<Hazard>();
            var bat = echo.AddComponent<Bat>();
            Set(bat, "spriteRenderer", visual);
            Set(bat, "waveAmplitude", 0.12f);
            var flip = echo.AddComponent<SpriteFlipbook>();
            Set(flip, "spriteRenderer", visual);
            Set(flip, "frames", new[] { S("eco_0"), S("eco_1") });
            Set(flip, "framesPerSecond", 6f);
            SavePrefab(echo, "EcoDelLlanto");
        }
    }

    // ---------------------------------------------------------------- ayudas del Nivel 2
    private static void Scorpion(LevelContext level, AnimalGuide guide, float x, float floorY, float left, float right, float speed, bool startLeft)
    {
        var scorpion = Spawn("Alacran", new Vector2(x, floorY), level.map);
        Link<Patroller>(scorpion, ("leftDistance", left), ("rightDistance", right), ("speed", speed), ("startToTheLeft", startLeft));
        Link<AnimalInfo>(scorpion, ("guide", guide), ("player", level.player.transform));
    }

    private static AudioSource CueSource()
    {
        var go = new GameObject("AvisosSonoros");
        var source = go.AddComponent<AudioSource>();
        source.playOnAwake = false;
        source.spatialBlend = 0f;
        return source;
    }

    private static TearSpawner Tears(LevelContext level, AudioSource cue, float minX, float maxX, float startY, float floorY, float interval, float speed)
    {
        var spawner = Spawn("LanzaLagrimas", new Vector2((minX + maxX) / 2f, startY), level.map);
        return Link<TearSpawner>(spawner, ("levelManager", level.manager), ("player", level.player.transform),
            ("minX", minX), ("maxX", maxX), ("startY", startY), ("floorY", floorY), ("interval", interval), ("fallSpeed", speed),
            ("cueSource", cue), ("cueClip", Clip("aviso_lagrima")));
    }

    private static void ApplyTheme(string prefix)
    {
        var swaps = new System.Collections.Generic.Dictionary<string, string>
        {
            { "tile_top", "top" }, { "tile_fill", "fill" }, { "platform", "platform" }, { "platform_cracked", "platform_cracked" },
            { "chest_closed", "chest_closed" }, { "door_closed", "door_closed" }
        };

        foreach (var renderer in Object.FindObjectsByType<SpriteRenderer>(FindObjectsInactive.Include))
        {
            if (renderer.sprite == null || !swaps.TryGetValue(renderer.sprite.name, out string target)) continue;

            renderer.sprite = S(prefix + target);
            PrefabUtility.RecordPrefabInstancePropertyModifications(renderer);
        }

        foreach (var chest in Object.FindObjectsByType<KeyChest>()) Set(chest, "openSprite", S(prefix + "chest_open"));
        foreach (var door in Object.FindObjectsByType<ExitDoor>()) Set(door, "openSprite", S(prefix + "door_open"));
    }

    // ---------------------------------------------------------------- Nivel 2: Guanajuato
    [MenuItem("Xoot/12. Construir Nivel 2 (Guanajuato)")]
    public static void BuildLevel2()
    {
        const string name = "Nivel2_Guanajuato";
        if (!CanBuild(name)) return;

        var level = BeginLevel("Nivel2_Guanajuato", new Vector2(2f, 1.05f), 32f, 43f, "musica_guanajuato", Color.white,
            "Assets/_Project/Art/Cinematica/escena_10.png", "bg_guanajuato");
        var map = level.map;

        var guideObject = new GameObject("GuiaDeFauna");
        var guide = guideObject.AddComponent<AnimalGuide>();
        Set(guide, "hud", level.hud);
        Set(guide, "levelManager", level.manager);

        // Zona 1: calle baja con fosas, un túnel y los primeros alacranes
        Ground(map, 0f, 9f, 1f, 3f);
        Ground(map, 9f, 11.4f, -0.4f, 1.6f);
        Spikes(map, 10.2f, -0.4f, 2.4f);
        Ground(map, 11.4f, 20f, 1f, 3f);
        Ground(map, 20f, 22.4f, -0.4f, 1.6f);
        Spikes(map, 21.2f, -0.4f, 2.4f);
        Ground(map, 22.4f, 32f, 1f, 3f);
        Ground(map, 12.8f, 18.4f, 5.2f, 0.8f).name = "Techo del túnel bajo";
        Scorpion(level, guide, 16f, 1f, 2.2f, 1.8f, 1.3f, true);
        Scorpion(level, guide, 26f, 1f, 1.8f, 1.8f, 1.5f, true);

        Platform(map, 29.5f, 2.8f, 3f);
        Platform(map, 25.8f, 4.4f, 2.6f);
        Platform(map, 29.6f, 6f, 4f);
        Chest(level, 30.8f, 6f, "Q4_Callejones", "Pista 1: la fotografía del callejón",
            "En el baúl hay una fotografía: tu amigo sonríe en un callejón mientras un alacrán cruza el empedrado. Al reverso dice: «28 · 09, junto a la Alhóndiga».", "Cofre 1 (baúl minero)");
        SafePoint(level, 28.3f, 6f);

        // Zona 2: callejones verticales
        Platform(map, 25f, 7.6f, 2.6f);
        Platform(map, 21.5f, 9f, 2.4f);
        Platform(map, 25f, 10.6f, 2.4f);
        Platform(map, 21f, 12.2f, 2.4f);
        SafePoint(level, 21f, 12.2f);
        Platform(map, 16f, 13.2f, 5f);
        Scorpion(level, guide, 15.8f, 13.2f, 1.4f, 1.4f, 1.2f, false);
        Spawn("PlataformaQueCae", new Vector2(11f, 14.4f), map);
        Spawn("PlataformaQueCae", new Vector2(7.5f, 15.8f), map);
        Platform(map, 3f, 17f, 5f);
        Chest(level, 1.4f, 17f, "Q5_Buzon", "Pista 2: la carta del buzón",
            "En el buzón hay una carta sin enviar: «Me gusta caminar por los túneles. Ahí el eco repite tu nombre y siento que no se me va a olvidar».", "Cofre 2 (buzón del callejón)");
        SafePoint(level, 4.6f, 17f);

        // Zona 3: el túnel largo y las azoteas
        Platform(map, 8f, 18.6f, 2.4f);
        Ground(map, 10.2f, 20.6f, 19.8f, 0.8f).name = "Piso del túnel";
        Ground(map, 11.8f, 19.4f, 24f, 0.8f).name = "Techo del túnel";
        SafePoint(level, 10.8f, 19.8f);
        Scorpion(level, guide, 14f, 19.8f, 1.2f, 1.2f, 1.2f, true);
        Scorpion(level, guide, 17.8f, 19.8f, 1f, 1.4f, 1.5f, false);
        Platform(map, 23.5f, 21.2f, 2.4f);
        Platform(map, 27.8f, 22.8f, 3f);
        Platform(map, 23.5f, 24.4f, 2.4f);
        Platform(map, 28.5f, 26f, 5f);
        SafePoint(level, 29.6f, 26f);
        Platform(map, 23f, 27.6f, 2.6f);
        Platform(map, 17.5f, 28.8f, 5f);
        Gate(level, 17.4f, 28.8f, 0.6f);
        Spawn("PlataformaQueCae", new Vector2(12.6f, 30f), map);
        Platform(map, 8f, 31.2f, 2.4f);
        Platform(map, 3f, 32.4f, 5f);
        Chest(level, 1.4f, 32.4f, "Q6_Eco", "Pista 3: la grabación del eco",
            "En el baúl hay una cinta: «La mujer que llora no quiere hacerte daño. El olvido torció su historia: escúchala completa».", "Cofre 3 (baúl del eco)");
        SafePoint(level, 4.4f, 32.4f);

        // Tramo del jefe: La Llorona en 3 fases
        var bossObject = new GameObject("La Llorona");
        level.boss = bossObject.AddComponent<BossController>();
        var bossSprite = Decoration(bossObject.transform, "Silueta", S("llorona_0"), new Vector3(20f, 39.2f, 0f), new Vector3(1.4f, 1.4f, 1f), 4, new Color(1f, 1f, 1f, 0.9f));
        var flip = bossSprite.gameObject.AddComponent<SpriteFlipbook>();
        Set(flip, "spriteRenderer", bossSprite);
        Set(flip, "frames", new[] { S("llorona_0"), S("llorona_1") });
        Set(flip, "framesPerSecond", 2f);
        Set(flip, "floatAmplitude", 0.4f);

        Platform(map, 7.2f, 34f, 2f);
        Platform(map, 13.5f, 35.2f, 8f);
        SafePoint(level, 10.2f, 35.2f);
        Platform(map, 22.2f, 35.2f, 7f);
        SafePoint(level, 19.2f, 35.2f);
        SafePoint(level, 25.1f, 35.2f);
        Spawn("PlataformaQueCae", new Vector2(27.5f, 36f), map);
        Platform(map, 30.5f, 36.8f, 3f);

        var cue = CueSource();
        var rain = Tears(level, cue, 11.6f, 16.2f, 41f, 35.2f, 0.95f, 7f);
        var lastRain = Tears(level, cue, 26.4f, 28.8f, 41.5f, 36f, 1.5f, 6.5f);
        var echoes = Bats(level, new Vector2(27f, 35.95f), 2.1f, 4f, 2.4f, new Vector2(-0.15f, 0.15f));
        Set(echoes, "batPrefab", PrefabAsset("EcoDelLlanto").GetComponent<Bat>());
        Set(echoes, "cueSource", cue);
        Set(echoes, "cueClip", Clip("aviso_llanto"));
        Set(echoes, "player", level.player.transform);
        echoes.name = "LanzaEcos";

        Zone(level, 10.9f, 35.2f, 0, false);
        Zone(level, 16.8f, 35.2f, 0, true);
        Zone(level, 19.9f, 35.2f, 1, false);
        Zone(level, 24.5f, 35.2f, 1, true);
        Zone(level, 25.5f, 35.2f, 2, false);
        Zone(level, 29.5f, 36.8f, 2, true);

        var door = Spawn("PuertaSalida", new Vector2(31f, 36.8f), map);
        Link<ExitDoor>(door, ("progress", level.progress), ("levelManager", level.manager), ("hud", level.hud), ("boss", level.boss));

        Set(level.boss, "bossName", "La Llorona");
        Set(level.boss, "introText", "Un llanto baja por los callejones. Según cuenta la leyenda, es La Llorona, pero el Ente del Olvido torció su historia.\n\nNo hay que pelear: escucha de qué lado viene y esquiva sus lágrimas.");
        Set(level.boss, "calmedText", "Al escuchar su historia completa, el llanto se vuelve silencio y La Llorona deja libre la puerta.");
        Set(level.boss, "progress", level.progress);
        Set(level.boss, "hud", level.hud);
        Set(level.boss, "bossSprite", bossSprite);
        Set(level.boss, "bossAppearedEvent", Ev("JefeAparece"));
        Set(level.boss, "phaseClearedEvent", Ev("FaseSuperada"));

        var so = new SerializedObject(level.boss);
        var phases = so.FindProperty("phases");
        phases.arraySize = 3;
        Phase(phases, 0, "la lluvia de lágrimas", "Cuentan que en muchos pueblos de México se escucha por las noches el llanto de una mujer junto al agua.", new BatSpawner[0]);
        Phase(phases, 1, "los ecos del llanto", "Dicen que busca lo que más quería y perdió, y por eso recorre las calles llamando.", new[] { echoes });
        Phase(phases, 2, "las azoteas rotas", "Su historia ha pasado de abuelos a nietos. Escucharla con respeto calma su llanto.", new BatSpawner[0]);
        PhaseTears(phases, 0, rain);
        PhaseTears(phases, 2, lastRain);
        so.ApplyModifiedPropertiesWithoutUndo();

        ApplyTheme("gto_");
        AddPostProcessing();
        AddLloronaCry();
        EaseOpenLevel();
        AddPlaceFacts();
        SaveScene(name);
    }

    private static void PhaseTears(SerializedProperty phases, int index, TearSpawner spawner)
    {
        var list = phases.GetArrayElementAtIndex(index).FindPropertyRelative("tearSpawners");
        list.arraySize = 1;
        list.GetArrayElementAtIndex(0).objectReferenceValue = spawner;
    }

    // Conecta el Nivel 1 con el Nivel 2 y le agrega el post-processing sin reconstruirlo
    [MenuItem("Xoot/13. Actualizar Nivel 1 (efectos y siguiente nivel)")]
    public static void UpdateLevel1()
    {
        var data = AssetDatabase.LoadAssetAtPath<LevelData>(Data + "Nivel1_Queretaro.asset");
        Set(data, "nextSceneName", "Nivel2_Guanajuato");
        EditorUtility.SetDirty(data);

        EditorSceneManager.OpenScene(Scenes + "Nivel1_Queretaro.unity");
        AddPostProcessing();
        EditorSceneManager.SaveOpenScenes();
        AssetDatabase.SaveAssets();
    }
}
