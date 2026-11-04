# Xoot Memorial

Videojuego 2D de exploración y misterio con plataformas para PC (Windows).
Proyecto Integrador de Creación de Videojuegos, UTEQ (IDGS 16, septiembre–diciembre 2026).

**Equipo:** Marco Antonio Gómez Olvera · Sandra Zoé Cabrera Velázquez · Monserrat Olguin Saucedo

## Requisitos para abrir el proyecto
- **Unity 6.5, versión exacta `6000.5.11f1`** (instalar desde Unity Hub; licencia Personal).
- Git instalado (el paquete `MCP for Unity` se descarga desde GitHub).
- Visual Studio Code con las extensiones C#, C# Dev Kit y Unity (opcional, recomendado).

Al abrir el proyecto por primera vez, Unity regenera la carpeta `Library/` y puede tardar varios minutos.

## Render y paquetes
URP con Renderer 2D, Input System, Cinemachine, paquetes 2D y Unity UI (incluye TextMeshPro).
Las versiones están fijadas en `Packages/manifest.json`.

## Estructura
```
Assets/
  Settings/        Configuración de URP 2D y Input System
  _Project/
    Art/ Audio/ Prefabs/ Scenes/ ScriptableObjects/
    Scripts/  Core · Player · Interaction · Clues · Puzzles · Memory · UI
```

## Trabajo en equipo (Git)
1. Antes de empezar: `git pull`.
2. Avisar al equipo antes de editar una escena; **una persona por escena a la vez**.
3. Commits pequeños y frecuentes con mensajes claros.
4. Al terminar: `git push`.
5. No se sube `Library/`, `Temp/`, `Logs/` ni `UserSettings/` (ya están en `.gitignore`).
