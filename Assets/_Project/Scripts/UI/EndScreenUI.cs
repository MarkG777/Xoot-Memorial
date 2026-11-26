using TMPro;
using UnityEngine;

// Pantalla de cierre: muestra el puntaje total de la partida.
public class EndScreenUI : MonoBehaviour
{
    [SerializeField] private TMP_Text scoreText;

    private void Start()
    {
        scoreText.text = "Puntaje de la partida: " + GameSession.TotalScore;
    }
}
