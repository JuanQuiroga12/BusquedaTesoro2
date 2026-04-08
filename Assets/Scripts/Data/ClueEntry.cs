using UnityEngine;

namespace BusquedaTesoro.Data
{
    [System.Serializable]
    public class ClueEntry
    {
        [Tooltip("Identificador de la pista (P-01, P-02, etc.)")]
        public string clueID;

        [Tooltip("Nombre de la ubicación a donde dirige la pista")]
        public string locationName;

        [Tooltip("Texto principal de la pista")]
        [TextArea(3, 6)]
        public string clueText;

        [Tooltip("Subtítulo debajo del separador")]
        public string subtitleText;

        [Tooltip("Sprite del sticker coloreado (dejar vacío para usar placeholder)")]
        public Sprite stickerSprite;

        [Tooltip("Sprite de la silueta/sombra del sticker (dejar vacío para usar placeholder)")]
        public Sprite stickerShadowSprite;
    }
}