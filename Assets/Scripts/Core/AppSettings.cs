using UnityEngine;

namespace BusquedaTesoro.Core
{
    /// <summary>
    /// Configuración global de la aplicación. Colocar en la primera escena.
    /// </summary>
    public class AppSettings : MonoBehaviour
    {
        [SerializeField] private int targetFrameRate = 60;

        private void Awake()
        {
            Application.targetFrameRate = targetFrameRate;
            QualitySettings.vSyncCount = 0;
        }
    }
}