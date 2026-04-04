using UnityEngine;
using UnityEngine.SceneManagement;

public class BotonJugar : MonoBehaviour
{
    public void IrAJuego()
    {
        SceneManager.LoadScene("Seleccion de grupos");
    }
}