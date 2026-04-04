using UnityEngine;
using TMPro;

public class TextBlink : MonoBehaviour
{
    public float intervalo = 0.5f;
    private TextMeshProUGUI texto;
    private float timer;

    void Start()
    {
        texto = GetComponent<TextMeshProUGUI>();
    }

    void Update()
    {
        timer += Time.deltaTime;
        if (timer >= intervalo)
        {
            texto.enabled = !texto.enabled;
            timer = 0f;
        }
    }
}