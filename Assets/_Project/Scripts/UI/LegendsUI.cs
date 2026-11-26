using System;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

// Sección "Leyendas" del menú: se elige una y se lee su información.
public class LegendsUI : MonoBehaviour
{
    [Serializable]
    public class Entry
    {
        public LegendData legend;
        public Button button;
    }

    [SerializeField] private Entry[] entries;
    [SerializeField] private TMP_Text titleText;
    [SerializeField] private TMP_Text bodyText;
    [SerializeField] private Image picture;

    private void Awake()
    {
        for (int i = 0; i < entries.Length; i++)
        {
            int index = i;
            entries[i].button.onClick.AddListener(() => Show(index));
        }
    }

    private void OnEnable()
    {
        Show(0);
    }

    public void Show(int index)
    {
        LegendData legend = entries[index].legend;
        titleText.text = legend.LegendName + " · " + legend.RegionName;
        bodyText.text = legend.MenuText;
        picture.sprite = legend.Picture;
        picture.enabled = legend.Picture != null;

        // El botón de la leyenda abierta queda marcado
        for (int i = 0; i < entries.Length; i++)
            entries[i].button.interactable = i != index;
    }
}
