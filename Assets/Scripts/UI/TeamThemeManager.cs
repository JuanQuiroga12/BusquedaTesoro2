using UnityEngine;
using UnityEngine.UI;
using TMPro;
using BusquedaTesoro.Core;

namespace BusquedaTesoro.UI
{
    /// <summary>
    /// Lee el equipo seleccionado desde TeamData y aplica los colores
    /// correspondientes a todos los elementos de UI en la escena Gameplay.
    /// </summary>
    public class TeamThemeManager : MonoBehaviour
    {
        [Header("Header")]
        [SerializeField] private Image panelHeader;
        [SerializeField] private Image imageTeamColorBar;
        [SerializeField] private Image imageProgressLine;
        [SerializeField] private TextMeshProUGUI tmpTeamLabel;

        [Header("Dots de Progreso")]
        [SerializeField] private Image[] progressDots;

        [Header("Barra de Progreso")]
        [SerializeField] private Image imageProgressBarFill;

        [Header("Mini Stickers")]
        [SerializeField] private Image[] miniStickers;

        private void Start()
        {
            ApplyTheme();
        }

        public void ApplyTheme()
        {
            Color mainColor = TeamData.TeamColor;
            Color darkColor = TeamData.TeamDarkColor;
            string displayName = TeamData.TeamDisplayName;
            int teamNumber = TeamData.TeamNumber;

            // Header
            if (panelHeader != null)
            {
                Color headerColor = darkColor;
                headerColor.a = panelHeader.color.a; // Mantener alpha original
                panelHeader.color = headerColor;
            }

            if (imageTeamColorBar != null)
                imageTeamColorBar.color = mainColor;

            if (imageProgressLine != null)
                imageProgressLine.color = mainColor;

            if (tmpTeamLabel != null)
                tmpTeamLabel.text = $"EQUIPO\n{displayName.Replace("LOS ", "")}";

            // Barra de progreso
            if (imageProgressBarFill != null)
                imageProgressBarFill.color = mainColor;

            Debug.Log($"[TeamThemeManager] Tema aplicado: {displayName} ({mainColor})");
        }

        /// <summary>
        /// Ilumina un dot de progreso al completar una pista.
        /// Llamado desde ProgressUIController.
        /// </summary>
        public void ActivateDot(int index)
        {
            if (index >= 0 && index < progressDots.Length)
            {
                progressDots[index].color = TeamData.TeamColor;
            }
        }

        /// <summary>
        /// Activa un mini sticker con el color del equipo.
        /// Llamado desde ProgressUIController.
        /// </summary>
        public void ActivateMiniSticker(int index)
        {
            if (index >= 0 && index < miniStickers.Length)
            {
                miniStickers[index].color = TeamData.TeamColor;
            }
        }
    }
}