using UnityEngine;

namespace BusquedaTesoro.Data
{
    /// <summary>
    /// Contiene los datos de todas las pistas del juego.
    /// Single Source of Truth para el contenido de las pistas.
    /// Se crea como asset desde: Create → Búsqueda del Tesoro → Clue Database.
    /// </summary>
    [CreateAssetMenu(fileName = "ClueDatabase", menuName = "Búsqueda del Tesoro/Clue Database")]
    public class ClueDatabase : ScriptableObject
    {
        [Tooltip("Valor del QR único que se usa en todas las estaciones")]
        public string universalQR = "AVANZAR";

        [Tooltip("Todas las pistas disponibles (pool completo)")]
        public ClueEntry[] allClues;

        [Tooltip("Índice dentro de allClues de la pista final obligatoria (P-04)")]
        public int finalClueIndex = 3;

        [Tooltip("Cantidad de pistas aleatorias antes de la pista final")]
        public int randomCluesCount = 3;

        public ClueEntry FinalClue => allClues[finalClueIndex];

        public int CluesPerTeam => randomCluesCount + 1;
    }
}