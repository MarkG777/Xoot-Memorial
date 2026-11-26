using TMPro;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;

// Herramienta de editor: pantalla de cierre de la beta.
public static partial class XootBuilder
{
    [MenuItem("Xoot/25. Construir pantalla de cierre")]
    public static void BuildEndScreen()
    {
        if (!CanBuild("Cierre")) return;

        EditorSceneManager.NewScene(NewSceneSetup.EmptyScene, NewSceneMode.Single);
        var cameraObject = new GameObject("Main Camera") { tag = "MainCamera" };
        var camera = cameraObject.AddComponent<Camera>();
        camera.orthographic = true;
        camera.clearFlags = CameraClearFlags.SolidColor;
        camera.backgroundColor = new Color(0.12f, 0.13f, 0.27f);
        cameraObject.AddComponent<AudioListener>();
        var music = cameraObject.AddComponent<AudioSource>();
        music.clip = AssetDatabase.LoadAssetAtPath<AudioClip>(Audio + "Intro/musica_intro.wav");
        music.loop = true;
        music.volume = 0.45f;

        var flow = new GameObject("SceneFlow").AddComponent<SceneFlow>();
        Set(flow, "firstScene", "Intro");

        var canvas = UiCanvas("Cierre");
        CreateEventSystem();
        var sky = UiImage(canvas.transform, "Cielo", S("bg_sky"), Color.white);
        Stretch(sky.gameObject);
        var volcanoes = UiImage(canvas.transform, "Volcanes", S("bg_volcanes"), new Color(1f, 1f, 1f, 0.75f));
        volcanoes.preserveAspect = true;
        Place(volcanoes.gameObject, new Vector2(0.5f, 0f), new Vector2(0f, -250f), new Vector2(900f, 825f));

        var shade = UiImage(canvas.transform, "Sombra", UiSprite, new Color(0.07f, 0.05f, 0.12f, 0.82f), true);
        Place(shade.gameObject, new Vector2(0.5f, 0.5f), new Vector2(0f, 20f), new Vector2(940f, 520f));

        var title = UiText(canvas.transform, "Titulo", "Continuará...", 72f, Gold, TextAlignmentOptions.Center);
        title.fontStyle = FontStyles.Bold;
        Place(title.gameObject, new Vector2(0.5f, 0.5f), new Vector2(0f, 220f), new Vector2(900f, 96f));
        var story = UiText(canvas.transform, "Historia",
            "Querétaro, Guanajuato y Puebla vuelven a recordar. Aketzali ya tiene tres recuerdos de su amigo.\nEl viaje sigue hacia Oaxaca y Yucatán, donde espera el Ente del Olvido.",
            27f, Cream, TextAlignmentOptions.Center);
        Place(story.gameObject, new Vector2(0.5f, 0.5f), new Vector2(0f, 110f), new Vector2(860f, 120f));
        var score = UiText(canvas.transform, "Puntaje", "Puntaje de la partida: 0", 36f, Gold, TextAlignmentOptions.Center);
        Place(score.gameObject, new Vector2(0.5f, 0.5f), new Vector2(0f, 20f), new Vector2(860f, 50f));
        var credits = UiText(canvas.transform, "Creditos",
            "Equipo Xoot Memorial\nStoryboard y secuencia inicial: Zoé\nArte, música y efectos del juego: equipo Xoot Memorial, con apoyo de Claude Code\nLlanto: «La Llorona» de RaspberryTickle2 (freesound.org), licencia CC BY 3.0, con volumen ajustado",
            20f, Cream, TextAlignmentOptions.Center);
        Place(credits.gameObject, new Vector2(0.5f, 0.5f), new Vector2(0f, -90f), new Vector2(880f, 130f));

        var menu = UiButton(canvas.transform, "Menu", "Menú principal", new Vector2(0f, -195f), new Vector2(340f, 66f), 30f);
        OnClick(menu, flow.ReturnToMenu);
        Set(canvas.AddComponent<EndScreenUI>(), "scoreText", score);

        SaveScene("Cierre");

        var data = AssetDatabase.LoadAssetAtPath<LevelData>(Data + "Nivel3_Puebla.asset");
        Set(data, "nextSceneName", "Cierre");
        EditorUtility.SetDirty(data);
        AssetDatabase.SaveAssets();
    }
}
