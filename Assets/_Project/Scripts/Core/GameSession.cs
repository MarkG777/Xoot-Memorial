// Conserva solo los datos que deben sobrevivir al cambiar de escena (como en la P5).
public static class GameSession
{
    // Puntaje acumulado de los niveles ya terminados
    public static int TotalScore { get; set; }

    // Se llama al empezar una partida nueva desde el menú
    public static void Clear()
    {
        TotalScore = 0;
    }
}
