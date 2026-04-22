using UnityEngine;
using UnityEngine.UI;

public class ControlVolumen : MonoBehaviour
{
    public static ControlVolumen Instance { get; private set; }

    public AudioSource musicaFondo;
    public float volumenMaxMusica = 0.3f;

    private float valorMusica = 0.5f;
    private float valorEfectos = 0.5f;

    void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }

        Instance = this;
        DontDestroyOnLoad(gameObject);
    }

    void Start()
    {
        CambiarMusica(valorMusica);
        CambiarEfectos(valorEfectos);
    }

    /// <summary>
    /// Llamado por PanelAjustesConector cuando el panel se activa.
    /// </summary>
    public void ConectarSliders(Slider sliderMusica, Slider sliderSonido)
    {
        if (sliderMusica != null)
        {
            sliderMusica.value = valorMusica;
            sliderMusica.onValueChanged.RemoveAllListeners();
            sliderMusica.onValueChanged.AddListener(CambiarMusica);
        }

        if (sliderSonido != null)
        {
            sliderSonido.value = valorEfectos;
            sliderSonido.onValueChanged.RemoveAllListeners();
            sliderSonido.onValueChanged.AddListener(CambiarEfectos);
        }
    }

    public void CambiarMusica(float valor)
    {
        valorMusica = valor;
        if (musicaFondo != null)
            musicaFondo.volume = valor * volumenMaxMusica;
    }

    public void CambiarEfectos(float valor)
    {
        valorEfectos = valor;
        AudioListener.volume = valor;
    }
}