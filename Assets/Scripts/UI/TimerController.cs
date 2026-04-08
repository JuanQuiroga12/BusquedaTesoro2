using UnityEngine;
using TMPro;
using BusquedaTesoro.Core;

namespace BusquedaTesoro.UI
{
    /// <summary>
    /// Timer que inicia al escanear el primer QR y se detiene
    /// al completar la última pista. Muestra el tiempo transcurrido en pantalla.
    /// </summary>
    public class TimerController : MonoBehaviour
    {
        [Header("Referencias")]
        [SerializeField] private GameStateManager gameStateManager;
        [SerializeField] private TextMeshProUGUI tmpTimerText;

        [Header("Configuración")]
        [Tooltip("Formato del timer: true = MM:SS, false = MM:SS.ms")]
        [SerializeField] private bool showMilliseconds = true;

        private float elapsedTime = 0f;
        private bool isRunning = false;

        /// <summary>Tiempo final registrado al completar el juego.</summary>
        public float FinalTime { get; private set; } = 0f;

        private void Start()
        {
            // Estado inicial: timer oculto o en cero
            UpdateTimerDisplay(0f);

            // Suscribirse a eventos
            gameStateManager.OnGameStarted += HandleGameStarted;
            gameStateManager.OnGameWon += HandleGameWon;
        }

        private void Update()
        {
            if (!isRunning) return;

            elapsedTime += Time.deltaTime;
            UpdateTimerDisplay(elapsedTime);
        }

        private void HandleGameStarted()
        {
            elapsedTime = 0f;
            isRunning = true;
            Debug.Log("[TimerController] Timer iniciado.");
        }

        private void HandleGameWon()
        {
            isRunning = false;
            FinalTime = elapsedTime;
            Debug.Log($"[TimerController] Timer detenido. Tiempo final: {FormatTime(FinalTime)}");
        }

        private void UpdateTimerDisplay(float time)
        {
            if (tmpTimerText != null)
            {
                tmpTimerText.text = FormatTime(time);
            }
        }

        private string FormatTime(float time)
        {
            int minutes = Mathf.FloorToInt(time / 60f);
            int seconds = Mathf.FloorToInt(time % 60f);

            if (showMilliseconds)
            {
                int milliseconds = Mathf.FloorToInt((time % 1f) * 100f);
                return $"{minutes:00}:{seconds:00}.{milliseconds:00}";
            }

            return $"{minutes:00}:{seconds:00}";
        }

        /// <summary>
        /// Reinicia el timer para una nueva ronda.
        /// </summary>
        public void ResetTimer()
        {
            elapsedTime = 0f;
            FinalTime = 0f;
            isRunning = false;
            UpdateTimerDisplay(0f);
        }

        private void OnDestroy()
        {
            if (gameStateManager != null)
            {
                gameStateManager.OnGameStarted -= HandleGameStarted;
                gameStateManager.OnGameWon -= HandleGameWon;
            }
        }
    }
}