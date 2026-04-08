using UnityEngine;
using UnityEngine.UI;
using DG.Tweening;
using BusquedaTesoro.Core;
using BusquedaTesoro.Data;

namespace BusquedaTesoro.UI
{
    /// <summary>
    /// Controla los dots de progreso, la barra de progreso inferior,
    /// y los mini stickers. Escucha eventos del GameStateManager.
    /// </summary>
    public class ProgressUIController : MonoBehaviour
    {
        [Header("Core")]
        [SerializeField] private GameStateManager gameStateManager;
        [SerializeField] private TeamThemeManager teamThemeManager;

        [Header("Dots de Progreso (Header)")]
        [SerializeField] private Image[] progressDots;

        [Header("Barra de Progreso")]
        [SerializeField] private Image progressBarFill;
        [SerializeField] private float barFillDuration = 0.6f;

        [Header("Mini Stickers")]
        [SerializeField] private Image[] miniStickerImages;

        [Header("Configuración de Animación")]
        [SerializeField] private float miniStickerStampDuration = 0.4f;
        [SerializeField] private float dotActivateDuration = 0.3f;

        // Para cada mini sticker, guardamos referencia a la sombra y al filled
        // Se crean dinámicamente como hijos de cada mini sticker
        private GameObject[] miniStickerShadows;
        private GameObject[] miniStickerFilled;

        private void Start()
        {
            // Inicializar mini stickers con sombras
            InitializeMiniStickers();

            // Suscribirse a eventos
            gameStateManager.OnGameStarted += HandleGameStarted;
            gameStateManager.OnClueRevealed += HandleClueRevealed;
            gameStateManager.OnClueCompleted += HandleClueCompleted;
        }

        /// <summary>
        /// Crea las capas de sombra y filled para cada mini sticker.
        /// Inicialmente todos muestran la sombra genérica.
        /// </summary>
        private void InitializeMiniStickers()
        {
            int count = miniStickerImages.Length;
            miniStickerShadows = new GameObject[count];
            miniStickerFilled = new GameObject[count];

            for (int i = 0; i < count; i++)
            {
                // El Image original del mini sticker se convierte en el contenedor
                // Lo dejamos con color transparente
                miniStickerImages[i].color = new Color(0, 0, 0, 0);

                RectTransform parentRect = miniStickerImages[i].GetComponent<RectTransform>();

                // Crear hijo: sombra del mini sticker
                GameObject shadow = new GameObject($"MiniStickerShadow_{i + 1}");
                shadow.transform.SetParent(parentRect, false);
                RectTransform shadowRect = shadow.AddComponent<RectTransform>();
                shadowRect.anchorMin = Vector2.zero;
                shadowRect.anchorMax = Vector2.one;
                shadowRect.offsetMin = Vector2.zero;
                shadowRect.offsetMax = Vector2.zero;
                Image shadowImg = shadow.AddComponent<Image>();
                shadowImg.color = new Color(0.1f, 0.1f, 0.06f, 0.6f); // Sombra oscura por defecto
                shadowImg.sprite = miniStickerImages[i].sprite; // Misma forma
                shadow.SetActive(false); // Se activa cuando se revela la pista
                miniStickerShadows[i] = shadow;

                // Crear hijo: sticker coloreado (completado)
                GameObject filled = new GameObject($"MiniStickerFilled_{i + 1}");
                filled.transform.SetParent(parentRect, false);
                RectTransform filledRect = filled.AddComponent<RectTransform>();
                filledRect.anchorMin = Vector2.zero;
                filledRect.anchorMax = Vector2.one;
                filledRect.offsetMin = Vector2.zero;
                filledRect.offsetMax = Vector2.zero;
                Image filledImg = filled.AddComponent<Image>();
                filledImg.color = Color.white;
                filledImg.sprite = miniStickerImages[i].sprite;
                filled.SetActive(false); // Se activa al completar la pista
                miniStickerFilled[i] = filled;
            }
        }

        // ═══════════════════════════════════════════
        // HANDLERS DE EVENTOS
        // ═══════════════════════════════════════════

        private void HandleGameStarted()
        {
            // Resetear todo al iniciar
            ResetProgress();

            // Mostrar las sombras de los mini stickers para las 4 pistas
            int totalClues = gameStateManager.TotalCluesForTeam;
            for (int i = 0; i < miniStickerShadows.Length && i < totalClues; i++)
            {
                miniStickerShadows[i].SetActive(true);
            }
        }

        private void HandleClueRevealed(int sequenceIndex)
        {
            // Actualizar el sprite de la sombra del mini sticker actual
            // con el sprite específico de la pista si existe
            ClueEntry clue = gameStateManager.AssignedClues[sequenceIndex];

            if (sequenceIndex < miniStickerShadows.Length)
            {
                if (clue.stickerShadowSprite != null)
                {
                    Image shadowImg = miniStickerShadows[sequenceIndex].GetComponent<Image>();
                    shadowImg.sprite = clue.stickerShadowSprite;
                }

                // También actualizar el sprite del filled
                if (clue.stickerSprite != null)
                {
                    Image filledImg = miniStickerFilled[sequenceIndex].GetComponent<Image>();
                    filledImg.sprite = clue.stickerSprite;
                }
            }
        }

        private void HandleClueCompleted(int sequenceIndex)
        {
            // 1. Activar dot de progreso con animación
            AnimateDotActivation(sequenceIndex);

            // 2. Activar mini sticker con animación
            AnimateMiniStickerStamp(sequenceIndex);

            // 3. Llenar barra de progreso
            AnimateProgressBar(sequenceIndex);
        }

        // ═══════════════════════════════════════════
        // ANIMACIONES
        // ═══════════════════════════════════════════

        private void AnimateDotActivation(int index)
        {
            if (index >= progressDots.Length) return;

            Image dot = progressDots[index];
            Color targetColor = TeamData.TeamColor;

            // Pulso de escala + cambio de color
            RectTransform dotRect = dot.GetComponent<RectTransform>();

            Sequence dotSequence = DOTween.Sequence();
            dotSequence.Append(
                dot.DOColor(targetColor, dotActivateDuration)
            );
            dotSequence.Join(
                dotRect.DOPunchScale(new Vector3(0.5f, 0.5f, 0), dotActivateDuration, 1)
            );

            // También notificar al TeamThemeManager
            if (teamThemeManager != null)
            {
                teamThemeManager.ActivateDot(index);
            }
        }

        private void AnimateMiniStickerStamp(int index)
        {
            if (index >= miniStickerFilled.Length) return;

            GameObject filled = miniStickerFilled[index];
            filled.SetActive(true);

            // Ocultar la sombra
            if (index < miniStickerShadows.Length)
            {
                miniStickerShadows[index].SetActive(false);
            }

            RectTransform filledRect = filled.GetComponent<RectTransform>();
            Image filledImg = filled.GetComponent<Image>();

            // Empezar grande y transparente, mantener color blanco para mostrar sprite original
            filledRect.localScale = new Vector3(2f, 2f, 1f);
            filledImg.color = new Color(1f, 1f, 1f, 0f);

            Sequence stampSequence = DOTween.Sequence();
            stampSequence.Append(
                filledImg.DOFade(1f, miniStickerStampDuration * 0.3f)
            );
            stampSequence.Join(
                filledRect.DOScale(Vector3.one, miniStickerStampDuration)
                    .SetEase(Ease.OutBounce)
            );
        }

        private void AnimateProgressBar(int sequenceIndex)
        {
            if (progressBarFill == null) return;

            int totalClues = gameStateManager.TotalCluesForTeam;
            float targetFill = (float)(sequenceIndex + 1) / totalClues;

            progressBarFill.DOFillAmount(targetFill, barFillDuration)
                .SetEase(Ease.OutQuad);
        }

        // ═══════════════════════════════════════════
        // RESET
        // ═══════════════════════════════════════════

        private void ResetProgress()
        {
            // Resetear dots
            Color inactiveColor = new Color(0.16f, 0.16f, 0.16f, 1f); // #2A2A2A
            foreach (Image dot in progressDots)
            {
                dot.color = inactiveColor;
            }

            // Resetear barra de progreso
            if (progressBarFill != null)
            {
                progressBarFill.fillAmount = 0f;
            }

            // Resetear mini stickers
            for (int i = 0; i < miniStickerShadows.Length; i++)
            {
                miniStickerShadows[i].SetActive(false);
                miniStickerFilled[i].SetActive(false);
            }
        }

        private void OnDestroy()
        {
            if (gameStateManager != null)
            {
                gameStateManager.OnGameStarted -= HandleGameStarted;
                gameStateManager.OnClueRevealed -= HandleClueRevealed;
                gameStateManager.OnClueCompleted -= HandleClueCompleted;
            }
        }
    }
}