using UnityEngine;
using TMPro;

public class TextoPulso : MonoBehaviour
{
    public float tamañoMinimo = 25f;
    public float tamañoMaximo = 35f;
    public float velocidad = 2f;
    private TextMeshProUGUI texto;

    void Start()
    {
        texto = GetComponent<TextMeshProUGUI>();
    }

    void Update()
    {
        float tamaño = Mathf.Lerp(tamañoMinimo, tamañoMaximo,
                       (Mathf.Sin(Time.time * velocidad) + 1) / 2);
        texto.fontSize = tamaño;
    }
}