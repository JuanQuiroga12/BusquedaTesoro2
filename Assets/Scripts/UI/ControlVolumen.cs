using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class ControlVolumen : MonoBehaviour
{
    public Slider sliderMusica;
    public Slider sliderEfectos;
    public AudioSource musicaFondo;
    public float volumenMaxMusica = 0.1f;

    void Start()
    {
        DontDestroyOnLoad(gameObject);
        sliderMusica.value = 0.5f;
        sliderEfectos.value = 0.5f;
    }

    public void CambiarMusica(float valor)
    {
        if (musicaFondo != null)
            musicaFondo.volume = valor * volumenMaxMusica;
    }

    public void CambiarEfectos(float valor)
    {
        AudioListener.volume = valor;
    }
}