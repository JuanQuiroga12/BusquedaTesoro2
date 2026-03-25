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
        [Tooltip("QR que inicia el juego")]
        public string startQR = "INICIO_JUEGO";

        [Tooltip("QR que activa la pantalla final de victoria")]
        public string finalQR = "TESORO_FINAL";

        public ClueEntry[] clues;
    }

    /// <summary>
    /// Datos individuales de una pista.
    /// </summary>
    [System.Serializable]
    public class ClueEntry
    {
        [Tooltip("Número de la pista (1-4)")]
        public int clueNumber;

        [Tooltip("Texto principal de la pista")]
        [TextArea(3, 6)]
        public string clueText;

        [Tooltip("Subtítulo debajo del separador")]
        public string subtitleText;

        [Tooltip("Valor del QR que completa esta pista")]
        public string expectedQR;

        [Tooltip("Sprite del sticker coloreado (dejar vacío para usar placeholder)")]
        public Sprite stickerSprite;

        [Tooltip("Sprite de la silueta/sombra del sticker (dejar vacío para usar placeholder)")]
        public Sprite stickerShadowSprite;
    }
}