using UnityEngine;
using UnityEngine.SceneManagement;

public class BotonVolver : MonoBehaviour
{
    public void VolverAlMenu()
    {
        SceneManager.LoadScene("Pantalla Inicio");
    }
}
