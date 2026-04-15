using UnityEngine;

public class MenuEmergente : MonoBehaviour
{
    public GameObject panelMenu;

    public void AbrirMenu()
    {
        panelMenu.SetActive(true);
    }

    public void CerrarMenu()
    {
        panelMenu.SetActive(false);
    }
}