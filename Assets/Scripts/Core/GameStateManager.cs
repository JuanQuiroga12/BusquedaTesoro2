using System;
using System.Collections.Generic;
using UnityEngine;
using BusquedaTesoro.Data;

namespace BusquedaTesoro.Core
{
    /// <summary>
    /// Máquina de estados central del juego.
    /// Single Source of Truth del progreso y estado actual.
    /// Gestiona la selección aleatoria de pistas y el flujo completo.
    /// </summary>
    public class GameStateManager : MonoBehaviour
    {
        // ═══════════════════════════════════════════
        // ESTADOS DEL JUEGO
        // ═══════════════════════════════════════════
        public enum GameState
        {
            WaitingForStart,    // Esperando escanear QR de inicio
            ShowingClue,        // Mostrando una pista, esperando que escaneen el QR de la estación
            ClueCompleted,      // Pista completada, mostrando animación y botón "Siguiente Pista"
            GameWon             // Todas las pistas completadas, transición a pantalla final
        }

        // ═══════════════════════════════════════════
        // EVENTOS — otros scripts se suscriben a estos
        // ═══════════════════════════════════════════

        /// <summary>Se dispara cuando el juego inicia (se escanea el QR de inicio).</summary>
        public event Action OnGameStarted;

        /// <summary>Se dispara cuando se muestra una nueva pista. Parámetro: número secuencial de pista (0-3).</summary>
        public event Action<int> OnClueRevealed;

        /// <summary>Se dispara cuando se completa una pista. Parámetro: número secuencial de pista (0-3).</summary>
        public event Action<int> OnClueCompleted;

        /// <summary>Se dispara cuando se gana el juego.</summary>
        public event Action OnGameWon;

        /// <summary>Se dispara cuando se escanea un QR inválido o en momento incorrecto.</summary>
        public event Action<string> OnInvalidQR;

        // ═══════════════════════════════════════════
        // REFERENCIAS
        // ═══════════════════════════════════════════
        [Header("Base de Datos de Pistas")]
        [SerializeField] private ClueDatabase clueDatabase;

        // ═══════════════════════════════════════════
        // ESTADO INTERNO
        // ═══════════════════════════════════════════
        private GameState currentState = GameState.WaitingForStart;

        /// <summary>
        /// Pistas asignadas a este equipo en orden (3 aleatorias + 1 final).
        /// Se genera al iniciar el juego.
        /// </summary>
        private List<ClueEntry> assignedClues = new List<ClueEntry>();

        /// <summary>Índice dentro de assignedClues (0-3). -1 = no ha empezado.</summary>
        private int currentSequenceIndex = -1;

        // Propiedades públicas de solo lectura
        public GameState CurrentState => currentState;
        public int CurrentSequenceIndex => currentSequenceIndex;
        public int TotalCluesForTeam => clueDatabase.CluesPerTeam;
        public ClueDatabase Database => clueDatabase;
        public IReadOnlyList<ClueEntry> AssignedClues => assignedClues.AsReadOnly();

        /// <summary>
        /// Retorna los datos de la pista actual en la secuencia. Null si no ha empezado.
        /// </summary>
        public ClueEntry CurrentClue
        {
            get
            {
                if (currentSequenceIndex >= 0 && currentSequenceIndex < assignedClues.Count)
                    return assignedClues[currentSequenceIndex];
                return null;
            }
        }

        // ═══════════════════════════════════════════
        // GENERACIÓN ALEATORIA DE RUTA
        // ═══════════════════════════════════════════

        /// <summary>
        /// Genera la secuencia de pistas para este equipo:
        /// - N pistas aleatorias del pool (excluyendo la final)
        /// - La pista final siempre al último
        /// </summary>
        private void GenerateRandomRoute()
        {
            assignedClues.Clear();

            // Construir pool de pistas aleatorias (todas menos la final)
            List<ClueEntry> pool = new List<ClueEntry>();
            for (int i = 0; i < clueDatabase.allClues.Length; i++)
            {
                if (i != clueDatabase.finalClueIndex)
                {
                    pool.Add(clueDatabase.allClues[i]);
                }
            }

            // Mezclar el pool (Fisher-Yates shuffle)
            for (int i = pool.Count - 1; i > 0; i--)
            {
                int j = UnityEngine.Random.Range(0, i + 1);
                (pool[i], pool[j]) = (pool[j], pool[i]);
            }

            // Tomar las primeras N del pool mezclado
            int count = Mathf.Min(clueDatabase.randomCluesCount, pool.Count);
            for (int i = 0; i < count; i++)
            {
                assignedClues.Add(pool[i]);
            }

            // Agregar la pista final al último
            assignedClues.Add(clueDatabase.FinalClue);

            // Log de la ruta generada
            Debug.Log("[GameStateManager] Ruta generada:");
            for (int i = 0; i < assignedClues.Count; i++)
            {
                Debug.Log($"  Pista {i + 1}: {assignedClues[i].clueID} → {assignedClues[i].locationName}");
            }
        }

        // ═══════════════════════════════════════════
        // MÉTODO PRINCIPAL: Procesar QR escaneado
        // ═══════════════════════════════════════════

        /// <summary>
        /// Recibe el string decodificado de un QR y procesa la lógica de transición.
        /// Como todos los QRs tienen el mismo valor, la validación es contra el QR universal.
        /// </summary>
        public void ProcessScannedQR(string qrValue)
        {
            if (string.IsNullOrEmpty(qrValue))
            {
                Debug.LogWarning("[GameStateManager] QR vacío recibido.");
                return;
            }

            string trimmedValue = qrValue.Trim().ToUpper();
            string expectedQR = clueDatabase.universalQR.Trim().ToUpper();

            Debug.Log($"[GameStateManager] QR recibido: '{trimmedValue}' | Estado: {currentState}");

            // Validar que sea el QR correcto
            if (trimmedValue != expectedQR)
            {
                Debug.Log($"[GameStateManager] QR no reconocido. Esperaba: '{expectedQR}'");
                OnInvalidQR?.Invoke("QR no válido para este juego");
                return;
            }

            // Procesar según el estado actual
            switch (currentState)
            {
                case GameState.WaitingForStart:
                    StartGame();
                    break;

                case GameState.ShowingClue:
                    CompleteCurrentClue();
                    break;

                case GameState.ClueCompleted:
                    Debug.Log("[GameStateManager] Esperando que presionen 'Siguiente Pista'.");
                    OnInvalidQR?.Invoke("Primero presiona 'Siguiente Pista'");
                    break;

                case GameState.GameWon:
                    Debug.Log("[GameStateManager] El juego ya terminó.");
                    break;
            }
        }

        /// <summary>
        /// Llamado cuando el jugador presiona el botón "Siguiente Pista".
        /// Solo funciona en estado ClueCompleted.
        /// </summary>
        public void OnNextCluePressed()
        {
            if (currentState != GameState.ClueCompleted)
            {
                Debug.LogWarning("[GameStateManager] OnNextCluePressed en estado incorrecto: " + currentState);
                return;
            }

            int nextIndex = currentSequenceIndex + 1;

            if (nextIndex < assignedClues.Count)
            {
                currentSequenceIndex = nextIndex;
                currentState = GameState.ShowingClue;

                Debug.Log($"[GameStateManager] → Mostrando Pista {currentSequenceIndex + 1}: {CurrentClue.clueID}");
                OnClueRevealed?.Invoke(currentSequenceIndex);
            }
            else
            {
                Debug.LogWarning("[GameStateManager] No hay más pistas.");
            }
        }

        // ═══════════════════════════════════════════
        // HANDLERS INTERNOS
        // ═══════════════════════════════════════════

        private void StartGame()
        {
            // Generar ruta aleatoria para este equipo
            GenerateRandomRoute();

            // Mostrar la primera pista
            currentSequenceIndex = 0;
            currentState = GameState.ShowingClue;

            Debug.Log($"[GameStateManager] ★ Juego iniciado → Pista 1: {CurrentClue.clueID}");
            OnGameStarted?.Invoke();
            OnClueRevealed?.Invoke(currentSequenceIndex);
        }

        private void CompleteCurrentClue()
        {
            currentState = GameState.ClueCompleted;
            Debug.Log($"[GameStateManager] ✓ Pista {currentSequenceIndex + 1} completada ({CurrentClue.clueID})");
            OnClueCompleted?.Invoke(currentSequenceIndex);

            // Si era la última pista (la final), el juego se gana
            bool isLastClue = (currentSequenceIndex >= assignedClues.Count - 1);
            if (isLastClue)
            {
                currentState = GameState.GameWon;
                Debug.Log("[GameStateManager] ★★★ ¡JUEGO GANADO! ★★★");
                OnGameWon?.Invoke();
            }
        }

        // ═══════════════════════════════════════════
        // RESET — Para múltiples rondas
        // ═══════════════════════════════════════════

        /// <summary>
        /// Reinicia el juego completamente para una nueva ronda.
        /// Genera una nueva ruta aleatoria al volver a escanear el QR de inicio.
        /// </summary>
        public void ResetGame()
        {
            currentState = GameState.WaitingForStart;
            currentSequenceIndex = -1;
            assignedClues.Clear();
            Debug.Log("[GameStateManager] Juego reiniciado. Listo para nueva ronda.");
        }

        // ═══════════════════════════════════════════
        // DEBUG: Para testing en el Editor
        // ═══════════════════════════════════════════

#if UNITY_EDITOR
        [Header("Debug — Solo en Editor")]
        [SerializeField] private string debugQRValue = "AVANZAR";

        [ContextMenu("Debug: Simular Escaneo QR")]
        private void DebugSimulateScan()
        {
            if (!Application.isPlaying)
            {
                Debug.LogWarning("[GameStateManager] Solo funciona en Play Mode.");
                return;
            }
            ProcessScannedQR(debugQRValue);
        }

        [ContextMenu("Debug: Simular Siguiente Pista")]
        private void DebugNextClue()
        {
            if (!Application.isPlaying)
            {
                Debug.LogWarning("[GameStateManager] Solo funciona en Play Mode.");
                return;
            }
            OnNextCluePressed();
        }

        [ContextMenu("Debug: Reiniciar Juego")]
        private void DebugReset()
        {
            if (!Application.isPlaying)
            {
                Debug.LogWarning("[GameStateManager] Solo funciona en Play Mode.");
                return;
            }
            ResetGame();
        }

        [ContextMenu("Debug: Log Estado Actual")]
        private void DebugLogState()
        {
            string clueInfo = CurrentClue != null ? $"{CurrentClue.clueID} ({CurrentClue.locationName})" : "ninguna";
            Debug.Log($"[GameStateManager] Estado: {currentState} | Pista: {clueInfo} | Secuencia: {currentSequenceIndex + 1}/{assignedClues.Count} | Equipo: {TeamData.SelectedTeam}");
        }
#endif
    }
}