using System.Collections.Generic;
using TMPro;
using Unity.Cinemachine;
using UnityEditor;
using UnityEditor.Animations;
using UnityEditor.Events;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.InputSystem.UI;
using UnityEngine.UI;

// Herramienta de editor: crea los sprites importados, el Animator, los ScriptableObjects y los prefabs.
// Los datos que ya existen no se sobrescriben, para conservar lo que se ajuste en el Inspector.
public static partial class XootBuilder
{
    private const string Pixel = "Assets/_Project/Art/Pixel/";
    private const string Audio = "Assets/_Project/Audio/";
    private const string Prefabs = "Assets/_Project/Prefabs/";
    private const string Data = "Assets/_Project/ScriptableObjects/";
    private const string Anim = "Assets/_Project/Animations/";
    private const string Scenes = "Assets/_Project/Scenes/";

    private static readonly Color PanelColor = new Color(0.09f, 0.06f, 0.14f, 0.94f);
    private static readonly Color Gold = new Color(1f, 0.82f, 0.36f);
    private static readonly Color Cream = new Color(0.97f, 0.94f, 0.9f);
    private static readonly Color ButtonColor = new Color(0.79f, 0.46f, 0.42f);

    // ---------------------------------------------------------------- utilidades
    private static Sprite S(string name) => AssetDatabase.LoadAssetAtPath<Sprite>(Pixel + name + ".png");

    private static AudioClip Clip(string name) => AssetDatabase.LoadAssetAtPath<AudioClip>(Audio + name + ".wav");

    private static GameEvent Ev(string name) => AssetDatabase.LoadAssetAtPath<GameEvent>(Data + "Eventos/" + name + ".asset");

    private static void Folder(string path)
    {
        path = path.TrimEnd('/');
        if (AssetDatabase.IsValidFolder(path)) return;

        int slash = path.LastIndexOf('/');
        Folder(path.Substring(0, slash));
        AssetDatabase.CreateFolder(path.Substring(0, slash), path.Substring(slash + 1));
    }

    // Asigna campos privados [SerializeField]
    private static void Set(Object target, string field, object value)
    {
        var so = new SerializedObject(target);
        var property = so.FindProperty(field);
        if (property == null) throw new System.Exception(target.GetType().Name + " no tiene el campo " + field);

        Assign(property, value);
        so.ApplyModifiedPropertiesWithoutUndo();
    }

    private static void Assign(SerializedProperty property, object value)
    {
        if (value is System.Array array && !(value is string))
        {
            property.arraySize = array.Length;
            for (int i = 0; i < array.Length; i++) Assign(property.GetArrayElementAtIndex(i), array.GetValue(i));
            return;
        }

        if (value == null) property.objectReferenceValue = null;
        else if (value is Object obj) property.objectReferenceValue = obj;
        else if (value is int number) { if (property.propertyType == SerializedPropertyType.Enum) property.enumValueIndex = number; else property.intValue = number; }
        else if (value is float f) property.floatValue = f;
        else if (value is bool b) property.boolValue = b;
        else if (value is string s) property.stringValue = s;
        else if (value is Vector2 v) property.vector2Value = v;
        else if (value is System.Enum e) property.enumValueIndex = System.Convert.ToInt32(e);
        else throw new System.Exception("Tipo no soportado: " + value.GetType());
    }

    private static T CreateData<T>(string path, System.Action<T> setup) where T : ScriptableObject
    {
        var asset = AssetDatabase.LoadAssetAtPath<T>(path);
        if (asset != null) return asset;

        asset = ScriptableObject.CreateInstance<T>();
        AssetDatabase.CreateAsset(asset, path);
        setup?.Invoke(asset);
        EditorUtility.SetDirty(asset);
        return asset;
    }

    // ---------------------------------------------------------------- 1) arte
    [MenuItem("Xoot/1. Importar arte")]
    public static void ImportArt()
    {
        AssetDatabase.Refresh();
        var bottomPivot = new HashSet<string> { "chest_closed", "chest_open", "door_closed", "door_open", "lantern_off", "lantern_on", "spikes",
            "gto_chest_closed", "gto_chest_open", "gto_door_closed", "gto_door_open", "alacran_0", "alacran_1",
            "cascabel_0", "cascabel_1", "pue_chest_closed", "pue_chest_open", "pue_door_closed", "pue_door_open",
            "teporingo_0", "teporingo_1" };
        var tiled = new HashSet<string> { "tile_top", "tile_fill", "platform", "platform_cracked", "spikes", "bg_aqueduct", "white",
            "gto_top", "gto_fill", "gto_platform", "gto_platform_cracked", "bg_guanajuato",
            "pue_top", "pue_fill", "pue_platform", "pue_platform_cracked", "bg_puebla" };

        foreach (string guid in AssetDatabase.FindAssets("t:Texture2D", new[] { Pixel.TrimEnd('/') }))
        {
            string path = AssetDatabase.GUIDToAssetPath(guid);
            string name = System.IO.Path.GetFileNameWithoutExtension(path);
            var importer = (TextureImporter)AssetImporter.GetAtPath(path);

            importer.textureType = TextureImporterType.Sprite;
            importer.spriteImportMode = SpriteImportMode.Single;
            importer.spritePixelsPerUnit = 32f;
            importer.filterMode = FilterMode.Point;
            importer.textureCompression = TextureImporterCompression.Uncompressed;
            importer.mipmapEnabled = false;
            importer.wrapMode = tiled.Contains(name) ? TextureWrapMode.Repeat : TextureWrapMode.Clamp;

            var settings = new TextureImporterSettings();
            importer.ReadTextureSettings(settings);
            settings.spriteMeshType = tiled.Contains(name) ? SpriteMeshType.FullRect : SpriteMeshType.Tight;
            bool bottom = name.StartsWith("aketzali") || bottomPivot.Contains(name);
            settings.spriteAlignment = (int)(bottom ? SpriteAlignment.BottomCenter : SpriteAlignment.Center);
            importer.SetTextureSettings(settings);
            importer.SaveAndReimport();
        }

        foreach (string guid in AssetDatabase.FindAssets("t:Texture2D", new[] { "Assets/_Project/Art/Cinematica" }))
        {
            var importer = (TextureImporter)AssetImporter.GetAtPath(AssetDatabase.GUIDToAssetPath(guid));
            if (importer.textureType == TextureImporterType.Sprite && importer.spriteImportMode == SpriteImportMode.Single) continue;

            importer.textureType = TextureImporterType.Sprite;
            importer.spriteImportMode = SpriteImportMode.Single;
            importer.SaveAndReimport();
        }
    }

    // ---------------------------------------------------------------- 2) animaciones
    private static AnimationClip MakeClip(string name, float fps, bool loop, params string[] frames)
    {
        string path = Anim + name + ".anim";
        var clip = AssetDatabase.LoadAssetAtPath<AnimationClip>(path);
        if (clip == null)
        {
            clip = new AnimationClip();
            AssetDatabase.CreateAsset(clip, path);
        }

        clip.frameRate = fps;
        var keys = new ObjectReferenceKeyframe[frames.Length + 1];
        for (int i = 0; i < frames.Length; i++)
            keys[i] = new ObjectReferenceKeyframe { time = i / fps, value = S(frames[i]) };
        keys[frames.Length] = new ObjectReferenceKeyframe { time = frames.Length / fps, value = S(frames[loop ? 0 : frames.Length - 1]) };

        var binding = EditorCurveBinding.PPtrCurve("", typeof(SpriteRenderer), "m_Sprite");
        AnimationUtility.SetObjectReferenceCurve(clip, binding, keys);

        var clipSettings = AnimationUtility.GetAnimationClipSettings(clip);
        clipSettings.loopTime = loop;
        AnimationUtility.SetAnimationClipSettings(clip, clipSettings);
        EditorUtility.SetDirty(clip);
        return clip;
    }

    private static string[] Frames(string prefix, int count)
    {
        var names = new string[count];
        for (int i = 0; i < count; i++) names[i] = prefix + "_" + i;
        return names;
    }

    [MenuItem("Xoot/2. Crear animaciones")]
    public static void CreateAnimator()
    {
        Folder(Anim);
        var idle = MakeClip("Aketzali_Idle", 3f, true, Frames("aketzali_idle", 4));
        var walk = MakeClip("Aketzali_Walk", 10f, true, Frames("aketzali_walk", 6));
        var run = MakeClip("Aketzali_Run", 14f, true, Frames("aketzali_run", 6));
        var jump = MakeClip("Aketzali_Jump", 6f, false, "aketzali_jump_0");
        var fall = MakeClip("Aketzali_Fall", 6f, false, "aketzali_fall_0");
        var hurt = MakeClip("Aketzali_Hurt", 6f, false, "aketzali_hurt_0");

        string path = Anim + "Aketzali.controller";
        if (AssetDatabase.LoadAssetAtPath<AnimatorController>(path) != null) return;

        var controller = AnimatorController.CreateAnimatorControllerAtPath(path);
        controller.AddParameter("Speed", AnimatorControllerParameterType.Float);
        controller.AddParameter("Grounded", AnimatorControllerParameterType.Bool);
        controller.AddParameter("VerticalSpeed", AnimatorControllerParameterType.Float);
        controller.AddParameter("Hurt", AnimatorControllerParameterType.Bool);

        // DE: Blend Tree por velocidad (reposo, caminar, correr)
        BlendTree tree;
        var locomotion = controller.CreateBlendTreeInController("Locomotion", out tree);
        tree.blendType = BlendTreeType.Simple1D;
        tree.blendParameter = "Speed";
        tree.useAutomaticThresholds = false;
        tree.AddChild(idle, 0f);
        tree.AddChild(walk, 0.5f);
        tree.AddChild(run, 1f);

        var machine = controller.layers[0].stateMachine;
        machine.defaultState = locomotion;
        var jumpState = machine.AddState("Jump");
        jumpState.motion = jump;
        var fallState = machine.AddState("Fall");
        fallState.motion = fall;
        var hurtState = machine.AddState("Hurt");
        hurtState.motion = hurt;

        var toHurt = machine.AddAnyStateTransition(hurtState);
        toHurt.AddCondition(AnimatorConditionMode.If, 0, "Hurt");
        toHurt.canTransitionToSelf = false;
        Instant(toHurt);

        var recover = hurtState.AddTransition(locomotion);
        recover.AddCondition(AnimatorConditionMode.IfNot, 0, "Hurt");
        Instant(recover);

        var up = locomotion.AddTransition(jumpState);
        up.AddCondition(AnimatorConditionMode.IfNot, 0, "Grounded");
        up.AddCondition(AnimatorConditionMode.Greater, 0.5f, "VerticalSpeed");
        Instant(up);

        var drop = locomotion.AddTransition(fallState);
        drop.AddCondition(AnimatorConditionMode.IfNot, 0, "Grounded");
        drop.AddCondition(AnimatorConditionMode.Less, -2f, "VerticalSpeed");
        Instant(drop);

        var peak = jumpState.AddTransition(fallState);
        peak.AddCondition(AnimatorConditionMode.IfNot, 0, "Grounded");
        peak.AddCondition(AnimatorConditionMode.Less, -0.1f, "VerticalSpeed");
        Instant(peak);

        var landJump = jumpState.AddTransition(locomotion);
        landJump.AddCondition(AnimatorConditionMode.If, 0, "Grounded");
        landJump.AddCondition(AnimatorConditionMode.Less, 0.1f, "VerticalSpeed");
        Instant(landJump);

        var landFall = fallState.AddTransition(locomotion);
        landFall.AddCondition(AnimatorConditionMode.If, 0, "Grounded");
        Instant(landFall);

        var again = fallState.AddTransition(jumpState);
        again.AddCondition(AnimatorConditionMode.Greater, 0.5f, "VerticalSpeed");
        Instant(again);
    }

    private static void Instant(AnimatorStateTransition transition)
    {
        transition.hasExitTime = false;
        transition.duration = 0f;
    }

    // ---------------------------------------------------------------- 3) datos
    [MenuItem("Xoot/3. Crear datos (ScriptableObjects)")]
    public static void CreateAllData()
    {
        Folder(Data + "Eventos");
        Folder(Data + "Puzzles");

        string[] events =
        {
            "VidaPerdida", "Reaparecio", "LlaveObtenida", "CofreAbierto", "PuzzleResuelto", "PuzzleFallado",
            "PuntoSeguro", "PuertaAbierta", "NivelCompletado", "SinCorazones", "RecuerdoDesbloqueado",
            "JefeAparece", "FaseSuperada"
        };
        foreach (string name in events) CreateData<GameEvent>(Data + "Eventos/" + name + ".asset", null);

        CreateData<PlayerStats>(Data + "PlayerStats.asset", null);
        CreateData<ScoreRules>(Data + "ReglasPuntaje.asset", null);

        // Puzzles del Nivel 1 (Querétaro)
        CreateData<PuzzleData>(Data + "Puzzles/Q1_OrdenarFotos.asset", p =>
        {
            Set(p, "type", (int)PuzzleData.PuzzleType.Order);
            Set(p, "title", "La maleta olvidada");
            Set(p, "question", "El cierre de la maleta tiene tres fotos sueltas de un mismo viaje. Ordénalas: ¿qué pasó primero?");
            Set(p, "options", new[] { "Llegada a la estación", "Paseo bajo los arcos", "Despedida en el andén" });
            Set(p, "displayOrder", new[] { 2, 0, 1 });
        });
        CreateData<PuzzleData>(Data + "Puzzles/Q2_CajaConsigna.asset", p =>
        {
            Set(p, "type", (int)PuzzleData.PuzzleType.Code);
            Set(p, "title", "Caja de consigna");
            Set(p, "question", "Pide un código de 4 números. La fotografía de la maleta tiene una fecha marcada...");
            Set(p, "code", "1509");
        });
        CreateData<PuzzleData>(Data + "Puzzles/Q3_Registro.asset", p =>
        {
            Set(p, "type", (int)PuzzleData.PuzzleType.Choice);
            Set(p, "title", "El registro del andén");
            Set(p, "question", "A la hoja le falta una palabra: «Mi amigo me prometió volver a verme junto a ___».");
            Set(p, "options", new[] { "la fuente", "los arcos", "el reloj", "el puente" });
            Set(p, "correctChoice", 1);
        });

        CreateData<LevelData>(Data + "Nivel1_Queretaro.asset", level =>
        {
            Set(level, "levelName", "Nivel 1");
            Set(level, "regionName", "Querétaro");
            Set(level, "introText", "«Esta estación... no debería recordarla así.»\n\nSube hasta la puerta. En 3 cofres hay llaves y pistas sobre tu amigo.\n\nA/D o flechas: moverte · Shift: correr · Espacio: saltar · E: abrir");
            Set(level, "requiredKeys", 3);
            Set(level, "memoryFragmentText", "Un andén con papel picado... Alguien te sonríe, pero la niebla no te deja ver su rostro.");
            Set(level, "memoryText", "Tu amigo te saluda junto a los arcos y dice: «Nos vemos pronto...». Luego la niebla vuelve a borrar su rostro.\n\n«Se está borrando... tengo que seguir.»");
            Set(level, "completedText", "La niebla retrocede y Querétaro recupera sus colores. El viaje continúa en Guanajuato.");
            Set(level, "nextSceneName", "MainMenu");
        });

        AssetDatabase.SaveAssets();
    }

    // ---------------------------------------------------------------- 4) prefabs
    private static SpriteRenderer Renderer(GameObject parent, string name, Sprite sprite, int order, Vector3 local = default)
    {
        var child = new GameObject(name);
        child.transform.SetParent(parent.transform, false);
        child.transform.localPosition = local;
        var renderer = child.AddComponent<SpriteRenderer>();
        renderer.sprite = sprite;
        renderer.sortingOrder = order;
        return renderer;
    }

    private static BoxCollider2D Box(GameObject target, Vector2 size, Vector2 offset, bool trigger)
    {
        var box = target.AddComponent<BoxCollider2D>();
        box.size = size;
        box.offset = offset;
        box.isTrigger = trigger;
        return box;
    }

    private static void SavePrefab(GameObject go, string name)
    {
        PrefabUtility.SaveAsPrefabAsset(go, Prefabs + name + ".prefab");
        Object.DestroyImmediate(go);
    }

    [MenuItem("Xoot/4. Crear prefabs")]
    public static void CreatePrefabs()
    {
        // Rehacer los prefabs puede romper ajustes hechos en las escenas: se pide confirmación
        if (System.IO.File.Exists(Prefabs + "Jugador.prefab") && !Application.isBatchMode &&
            !EditorUtility.DisplayDialog("Xoot", "Los prefabs ya existen. ¿Rehacerlos?", "Rehacer", "Cancelar")) return;

        Folder(Prefabs.TrimEnd('/'));
        var stats = AssetDatabase.LoadAssetAtPath<PlayerStats>(Data + "PlayerStats.asset");
        var noFriction = AssetDatabase.LoadAssetAtPath<PhysicsMaterial2D>("Assets/_Project/Art/SinFriccion.physicsMaterial2D");

        // Jugador
        var player = new GameObject("Jugador") { tag = "Player" };
        var body = player.AddComponent<Rigidbody2D>();
        body.gravityScale = 3.2f;
        body.freezeRotation = true;
        body.interpolation = RigidbodyInterpolation2D.Interpolate;
        body.collisionDetectionMode = CollisionDetectionMode2D.Continuous;
        body.sleepMode = RigidbodySleepMode2D.NeverSleep;
        var capsule = player.AddComponent<CapsuleCollider2D>();
        capsule.size = new Vector2(0.56f, 1.24f);
        capsule.offset = new Vector2(0f, 0.62f);
        capsule.sharedMaterial = noFriction;
        var visual = Renderer(player, "Visual", S("aketzali_idle_0"), 10);
        var animator = visual.gameObject.AddComponent<Animator>();
        animator.runtimeAnimatorController = AssetDatabase.LoadAssetAtPath<AnimatorController>(Anim + "Aketzali.controller");
        var controller = player.AddComponent<PlayerController2D>();
        Set(controller, "stats", stats);
        Set(controller, "groundCheckDistance", 0.08f);
        var lives = player.AddComponent<PlayerLives>();
        Set(lives, "stats", stats);
        Set(lives, "spriteRenderer", visual);
        Set(lives, "lifeLostEvent", Ev("VidaPerdida"));
        Set(lives, "respawnedEvent", Ev("Reaparecio"));
        player.AddComponent<PlayerInteractor>();
        var animation = player.AddComponent<PlayerAnimation2D>();
        Set(animation, "controller", controller);
        Set(animation, "lives", lives);
        Set(animation, "animator", animator);
        Set(animation, "spriteRenderer", visual);
        SavePrefab(player, "Jugador");

        // Cofre con llave
        var chest = new GameObject("Cofre");
        Box(chest, new Vector2(1.5f, 1.4f), new Vector2(0f, 0.7f), true);
        var chestVisual = Renderer(chest, "Visual", S("chest_closed"), 2);
        var popup = Renderer(chest, "LlaveQueSale", S("key"), 12, new Vector3(0f, 1.3f, 0f));
        popup.gameObject.SetActive(false);
        var keyChest = chest.AddComponent<KeyChest>();
        Set(keyChest, "spriteRenderer", chestVisual);
        Set(keyChest, "openSprite", S("chest_open"));
        Set(keyChest, "keyPopup", popup);
        Set(keyChest, "chestOpenedEvent", Ev("CofreAbierto"));
        SavePrefab(chest, "Cofre");

        // Puerta de salida
        var door = new GameObject("PuertaSalida");
        Box(door, new Vector2(0.9f, 2f), new Vector2(0f, 1f), true);
        var doorVisual = Renderer(door, "Visual", S("door_closed"), 1);
        var exit = door.AddComponent<ExitDoor>();
        Set(exit, "spriteRenderer", doorVisual);
        Set(exit, "openSprite", S("door_open"));
        Set(exit, "doorOpenedEvent", Ev("PuertaAbierta"));
        SavePrefab(door, "PuertaSalida");

        // Punto seguro
        var checkpoint = new GameObject("PuntoSeguro");
        Box(checkpoint, new Vector2(0.8f, 2f), new Vector2(0f, 1f), true);
        var lantern = Renderer(checkpoint, "Visual", S("lantern_off"), 1);
        var respawn = new GameObject("Reaparicion");
        respawn.transform.SetParent(checkpoint.transform, false);
        respawn.transform.localPosition = new Vector3(0f, 0.05f, 0f);
        var check = checkpoint.AddComponent<Checkpoint>();
        Set(check, "spriteRenderer", lantern);
        Set(check, "onSprite", S("lantern_on"));
        Set(check, "respawnPoint", respawn.transform);
        Set(check, "activatedEvent", Ev("PuntoSeguro"));
        SavePrefab(checkpoint, "PuntoSeguro");

        // Suelo y muros: el tamaño se ajusta en cada nivel
        var ground = new GameObject("Bloque");
        Box(ground, new Vector2(4f, 2f), new Vector2(0f, -1f), false).sharedMaterial = noFriction;
        var top = Renderer(ground, "Orilla", S("tile_top"), 0, new Vector3(0f, -0.5f, 0f));
        top.drawMode = SpriteDrawMode.Tiled;
        top.size = new Vector2(4f, 1f);
        var fill = Renderer(ground, "Relleno", S("tile_fill"), 0, new Vector3(0f, -1.5f, 0f));
        fill.drawMode = SpriteDrawMode.Tiled;
        fill.size = new Vector2(4f, 1f);
        SavePrefab(ground, "Bloque");

        // Plataforma de un solo sentido (se puede saltar desde abajo)
        var platform = new GameObject("Plataforma");
        var platformBox = Box(platform, new Vector2(3f, 0.5f), new Vector2(0f, -0.25f), false);
        platformBox.sharedMaterial = noFriction;
        platformBox.usedByEffector = true;
        var effector = platform.AddComponent<PlatformEffector2D>();
        effector.surfaceArc = 160f;
        var platformVisual = Renderer(platform, "Visual", S("platform"), 0, new Vector3(0f, -0.25f, 0f));
        platformVisual.drawMode = SpriteDrawMode.Tiled;
        platformVisual.size = new Vector2(3f, 0.5f);
        SavePrefab(platform, "Plataforma");

        // Plataforma que se cae
        var falling = new GameObject("PlataformaQueCae");
        var fallingBody = falling.AddComponent<Rigidbody2D>();
        fallingBody.bodyType = RigidbodyType2D.Kinematic;
        Box(falling, new Vector2(2f, 0.5f), new Vector2(0f, -0.25f), false).sharedMaterial = noFriction;
        var shake = new GameObject("Temblor");
        shake.transform.SetParent(falling.transform, false);
        var cracked = Renderer(shake, "Visual", S("platform_cracked"), 0, new Vector3(0f, -0.25f, 0f));
        cracked.drawMode = SpriteDrawMode.Tiled;
        cracked.size = new Vector2(2f, 0.5f);
        var fallingScript = falling.AddComponent<FallingPlatform>();
        Set(fallingScript, "visual", shake.transform);
        SavePrefab(falling, "PlataformaQueCae");

        // Reja con tiempo
        var gate = new GameObject("RejaConTiempo");
        var gateBody = gate.AddComponent<Rigidbody2D>();
        gateBody.bodyType = RigidbodyType2D.Kinematic;
        Box(gate, new Vector2(0.4f, 3f), Vector2.zero, false).sharedMaterial = noFriction;
        Renderer(gate, "Visual", S("gate"), 3);
        var gateScript = gate.AddComponent<TimedGate>();
        Set(gateScript, "openHeight", 2.7f);
        SavePrefab(gate, "RejaConTiempo");

        // Pinchos y vacío
        var spikes = new GameObject("Pinchos");
        Box(spikes, new Vector2(1f, 0.3f), new Vector2(0f, 0.18f), true);
        var spikesVisual = Renderer(spikes, "Visual", S("spikes"), 1);
        spikesVisual.drawMode = SpriteDrawMode.Tiled;
        spikesVisual.size = new Vector2(1f, 0.5f);
        spikes.AddComponent<Hazard>();
        SavePrefab(spikes, "Pinchos");

        var kill = new GameObject("Vacio");
        Box(kill, new Vector2(40f, 2f), Vector2.zero, true);
        kill.AddComponent<Hazard>();
        SavePrefab(kill, "Vacio");

        // Murciélago (sale de un pool)
        var batObject = new GameObject("Murcielago");
        var batBody = batObject.AddComponent<Rigidbody2D>();
        batBody.bodyType = RigidbodyType2D.Kinematic;
        var circle = batObject.AddComponent<CircleCollider2D>();
        circle.radius = 0.3f;
        circle.isTrigger = true;
        var batVisual = Renderer(batObject, "Visual", S("bat_0"), 11);
        batObject.AddComponent<Hazard>();
        var bat = batObject.AddComponent<Bat>();
        Set(bat, "spriteRenderer", batVisual);
        Set(bat, "waveAmplitude", 0.25f);
        var flap = batObject.AddComponent<SpriteFlipbook>();
        Set(flap, "spriteRenderer", batVisual);
        Set(flap, "frames", new[] { S("bat_0"), S("bat_1") });
        Set(flap, "framesPerSecond", 7f);
        SavePrefab(batObject, "Murcielago");

        var spawnerObject = new GameObject("LanzaMurcielagos");
        var spawner = spawnerObject.AddComponent<BatSpawner>();
        Set(spawner, "batPrefab", AssetDatabase.LoadAssetAtPath<GameObject>(Prefabs + "Murcielago.prefab").GetComponent<Bat>());
        SavePrefab(spawnerObject, "LanzaMurcielagos");

        var zone = new GameObject("ZonaJefe");
        Box(zone, new Vector2(0.5f, 2.6f), new Vector2(0f, 1.3f), true);
        zone.AddComponent<BossZone>();
        SavePrefab(zone, "ZonaJefe");

        AssetDatabase.SaveAssets();
    }
}
