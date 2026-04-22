using UnityEngine;
using UnityEngine.UI;

public class MenuEmergente : MonoBehaviour
{
    public GameObject panelMenu;

    [Header("Sliders de Ajustes")]
    [SerializeField] private Slider sliderMusica;
    [SerializeField] private Slider sliderSonido;

    public void AbrirMenu()
    {
        panelMenu.SetActive(true);

        // Reconectar sliders al ControlVolumen cada vez que se abre
        if (ControlVolumen.Instance != null)
        {
            ControlVolumen.Instance.ConectarSliders(sliderMusica, sliderSonido);
        }
    }

    public void CerrarMenu()
    {
        panelMenu.SetActive(false);
    }
}