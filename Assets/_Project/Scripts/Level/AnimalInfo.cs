using TMPro;
using UnityEngine;

// Dato informativo antes de pisarlo: cuando Aketzali se acerca aparece el nombre de la especie
// sobre el animal y, la primera vez, su ficha completa.
public class AnimalInfo : MonoBehaviour
{
    [SerializeField] private AnimalData animal;
    [SerializeField] private AnimalGuide guide;
    [SerializeField] private Transform player;
    [SerializeField] private TMP_Text speciesLabel;

    [Header("Distancia a la que se muestra")]
    [SerializeField] private float cardDistance = 3.6f;
    [SerializeField] private float labelDistance = 5f;
    [SerializeField] private float heightRange = 1.6f;

    private void Start()
    {
        if (speciesLabel != null)
        {
            // Arriba el nombre más conocido y abajo el nombre científico
            speciesLabel.fontStyle = FontStyles.Bold;
            speciesLabel.text = animal.CommonName + "\n<size=80%><i>" + animal.Species + "</i></size>";
            speciesLabel.enabled = false;
        }
    }

    private void Update()
    {
        if (player == null) return;

        float dx = Mathf.Abs(player.position.x - transform.position.x);
        float dy = Mathf.Abs(player.position.y - transform.position.y);
        bool sameHeight = dy <= heightRange;

        if (speciesLabel != null) speciesLabel.enabled = sameHeight && dx <= labelDistance;

        if (sameHeight && dx <= cardDistance && guide != null) guide.Present(animal);
    }
}
