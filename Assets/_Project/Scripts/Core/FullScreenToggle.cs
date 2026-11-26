using UnityEngine;
using UnityEngine.InputSystem;

// F11 cambia entre ventana y pantalla completa. Se crea solo al iniciar el juego y vive en todas las escenas.
public class FullScreenToggle : MonoBehaviour
{
    private const int WindowWidth = 1280;
    private const int WindowHeight = 720;

    [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.AfterSceneLoad)]
    private static void Create()
    {
        var go = new GameObject("PantallaCompleta");
        DontDestroyOnLoad(go);
        go.AddComponent<FullScreenToggle>();
    }

    private void Update()
    {
        Keyboard keyboard = Keyboard.current;
        if (keyboard == null || !keyboard.f11Key.wasPressedThisFrame) return;

        if (Screen.fullScreen)
            Screen.SetResolution(WindowWidth, WindowHeight, FullScreenMode.Windowed);
        else
            Screen.SetResolution(Display.main.systemWidth, Display.main.systemHeight, FullScreenMode.FullScreenWindow);
    }
}
