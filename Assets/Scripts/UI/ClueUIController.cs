using UnityEngine;
using UnityEngine.UI;
using TMPro;
using DG.Tweening;
using BusquedaTesoro.Core;
using BusquedaTesoro.Data;
using UnityEngine.SceneManagement;

namespace BusquedaTesoro.UI
{
    /// <summary>
    /// Controla el panel de pistas: mostrar/ocultar, colapsar/expandir,
    /// actualizar textos, y gestionar los stickers.
    /// Escucha los eventos del GameStateManager para reaccionar.
    /// </summary>
    public class ClueUIController : MonoBehaviour
    {
        // ═══════════════════════════════════════════
        // REFERENCIAS
        // ═══════════════════════════════════════════

        [Header("Core")]
        [SerializeField] private GameStateManager gameStateManager;

        [Header("Panel de Pista")]
        [SerializeField] private GameObject panelClueInfo;
        [SerializeField] private Button buttonClueHeader;
        [SerializeField] private TextMeshProUGUI tmpClueHeaderText;
        [SerializeField] private GameObject panelClueBody;
        [SerializeField] private TextMeshProUGUI tmpClueText;
        [SerializeField] private TextMeshProUGUI tmpClueSubtext;

        [Header("Sticker (dentro del panel)")]
        [SerializeField] private Image imageStickerShadow;
        [SerializeField] private Image imageStickerFilled;

        [Header("Header")]
        [SerializeField] private TextMeshProUGUI tmpClueCounter;

        [Header("Botones de Acción")]
        [SerializeField] private GameObject buttonScan;
        [SerializeField] private GameObject buttonNextClue;

        [Header("Configuración de Animación")]
        [SerializeField] private float panelRevealDuration = 0.5f;
        [SerializeField] private float stickerStampDuration = 0.4f;
        [SerializeField] private float buttonSwapDuration = 0.3f;
        [SerializeField] private float collapseExpandDuration = 0.3f;

        // ═══════════════════════════════════════════
        // ESTADO INTERNO
        // ═══════════════════════════════════════════
        private bool isBodyVisible = true;
        private bool isAnimating = false;
        private RectTransform panelClueInfoRect;
        private RectTransform panelClueBodyRect;
        private Vector2 bodyExpandedSize;
        private float panelExpandedHeight;

        // ═══════════════════════════════════════════
        // INICIALIZACIÓN
        // ═══════════════════════════════════════════

        private void Awake()
        {
            panelClueInfoRect = panelClueInfo.GetComponent<RectTransform>();
            panelClueBodyRect = panelClueBody.GetComponent<RectTransform>();
        }

        private void Start()
        {
            // Guardar tamaños originales para collapse/expand
            bodyExpandedSize = panelClueBodyRect.sizeDelta;
            panelExpandedHeight = panelClueInfoRect.sizeDelta.y;

            // Estado inicial: todo oculto
            panelClueInfo.SetActive(false);
            buttonNextClue.SetActive(false);
            buttonScan.SetActive(true);

            // Conectar botones
            buttonClueHeader.onClick.AddListener(ToggleClueBody);

            Button nextBtn = buttonNextClue.GetComponent<Button>();
            if (nextBtn != null)
            {
                nextBtn.onClick.AddListener(OnNextCluePressed);
            }

            // Suscribirse a eventos del GameStateManager
            gameStateManager.OnGameStarted += HandleGameStarted;
            gameStateManager.OnClueRevealed += HandleClueRevealed;
            gameStateManager.OnClueCompleted += HandleClueCompleted;
            gameStateManager.OnGameWon += HandleGameWon;
        }

        // ═══════════════════════════════════════════
        // HANDLERS DE EVENTOS
        // ═══════════════════════════════════════════

        private void HandleGameStarted()
        {
            // El juego inició — OnClueRevealed se dispara inmediatamente después,
            // así que no necesitamos hacer nada extra aquí.
        }

        private void HandleClueRevealed(int sequenceIndex)
        {
            ClueEntry clue = gameStateManager.AssignedClues[sequenceIndex];
            int displayNumber = sequenceIndex + 1;
            int totalClues = gameStateManager.TotalCluesForTeam;

            // Actualizar textos
            tmpClueHeaderText.text = $"PISTA {displayNumber}";
            tmpClueText.text = clue.clueText;
            tmpClueSubtext.text = clue.subtitleText;
            tmpClueCounter.text = $"PISTA\n<size=48>{displayNumber}<size=32>|<size=48>{totalClues}";

            // Actualizar sticker shadow si tiene sprite personalizado
            if (clue.stickerShadowSprite != null)
            {
                imageStickerShadow.sprite = clue.stickerShadowSprite;
            }

            // Resetear sticker filled
            imageStickerFilled.gameObject.SetActive(false);
            if (clue.stickerSprite != null)
            {
                imageStickerFilled.sprite = clue.stickerSprite;
            }

            // Mostrar panel con animación
            ShowCluePanel(displayNumber);

            // Asegurar que se muestra botón de escaneo, no el de siguiente
            buttonNextClue.SetActive(false);
            buttonScan.SetActive(true);

            // Asegurar que el body esté expandido
            isBodyVisible = true;
            panelClueBody.SetActive(true);
        }

        private void HandleClueCompleted(int sequenceIndex)
        {
            // Animar el sticker
            AnimateStickerStamp();

            // Intercambiar botones: Escanear → Siguiente Pista
            SwapToNextButton();
        }

        private void HandleGameWon()
        {
            buttonScan.SetActive(false);
            buttonNextClue.SetActive(false);

            // Animar el último sticker
            AnimateStickerStamp();

            // Esperar a que termine la animación y transicionar
            DOVirtual.DelayedCall(2.5f, () =>
            {
                UnityEngine.SceneManagement.SceneManager.LoadScene("Victory");
            });
        }

        // ═══════════════════════════════════════════
        // ANIMACIÓN: Mostrar panel de pista
        // ═══════════════════════════════════════════

        private void ShowCluePanel(int displayNumber)
        {
            // Activar el panel
            panelClueInfo.SetActive(true);

            // Restaurar tamaño completo
            panelClueInfoRect.sizeDelta = new Vector2(panelClueInfoRect.sizeDelta.x, panelExpandedHeight);

            // Animación de entrada: escala desde 0
            panelClueInfoRect.localScale = Vector3.zero;
            panelClueInfoRect.DOScale(Vector3.one, panelRevealDuration)
                .SetEase(Ease.OutBack);
        }

        // ═══════════════════════════════════════════
        // ANIMACIÓN: Sticker stamp
        // ═══════════════════════════════════════════

        private void AnimateStickerStamp()
        {
            imageStickerFilled.gameObject.SetActive(true);
            RectTransform stickerRect = imageStickerFilled.GetComponent<RectTransform>();

            // Empezar grande e invisible, luego "sellar"
            stickerRect.localScale = new Vector3(2f, 2f, 1f);
            imageStickerFilled.color = new Color(
                imageStickerFilled.color.r,
                imageStickerFilled.color.g,
                imageStickerFilled.color.b,
                0f
            );

            Sequence stampSequence = DOTween.Sequence();
            stampSequence.Append(
                imageStickerFilled.DOFade(1f, stickerStampDuration * 0.3f)
            );
            stampSequence.Join(
                stickerRect.DOScale(Vector3.one, stickerStampDuration)
                    .SetEase(Ease.OutBounce)
            );
        }

        // ═══════════════════════════════════════════
        // ANIMACIÓN: Intercambio de botones
        // ═══════════════════════════════════════════

        private void SwapToNextButton()
        {
            // Fade out del botón de escaneo
            CanvasGroup scanGroup = GetOrAddCanvasGroup(buttonScan);
            CanvasGroup nextGroup = GetOrAddCanvasGroup(buttonNextClue);

            // Preparar botón siguiente: visible pero transparente
            buttonNextClue.SetActive(true);
            nextGroup.alpha = 0f;

            Sequence swapSequence = DOTween.Sequence();

            // Fade out escanear
            swapSequence.Append(
                scanGroup.DOFade(0f, buttonSwapDuration)
            );
            swapSequence.AppendCallback(() =>
            {
                buttonScan.SetActive(false);
                scanGroup.alpha = 1f; // Resetear para la próxima vez
            });

            // Fade in siguiente pista
            swapSequence.Append(
                nextGroup.DOFade(1f, buttonSwapDuration)
            );
        }

        private void SwapToScanButton()
        {
            CanvasGroup scanGroup = GetOrAddCanvasGroup(buttonScan);
            CanvasGroup nextGroup = GetOrAddCanvasGroup(buttonNextClue);

            buttonScan.SetActive(true);
            scanGroup.alpha = 0f;

            Sequence swapSequence = DOTween.Sequence();

            swapSequence.Append(
                nextGroup.DOFade(0f, buttonSwapDuration)
            );
            swapSequence.AppendCallback(() =>
            {
                buttonNextClue.SetActive(false);
                nextGroup.alpha = 1f;
            });

            swapSequence.Append(
                scanGroup.DOFade(1f, buttonSwapDuration)
            );
        }

        // ═══════════════════════════════════════════
        // COLAPSAR / EXPANDIR PANEL
        // ═══════════════════════════════════════════

        private void ToggleClueBody()
        {
            if (isAnimating) return;

            isAnimating = true;

            // Obtener o crear CanvasGroup en el body para controlar opacidad
            CanvasGroup bodyGroup = GetOrAddCanvasGroup(panelClueBody);

            if (isBodyVisible)
            {
                // Colapsar: fade out del contenido + reducir panel
                float headerHeight = buttonClueHeader.GetComponent<RectTransform>().sizeDelta.y + 20f;

                Sequence collapseSequence = DOTween.Sequence();

                // Fade out del contenido
                collapseSequence.Append(
                    bodyGroup.DOFade(0f, collapseExpandDuration * 0.6f)
                );

                // Reducir el panel
                collapseSequence.Join(
                    panelClueInfoRect.DOSizeDelta(
                        new Vector2(panelClueInfoRect.sizeDelta.x, headerHeight),
                        collapseExpandDuration
                    ).SetEase(Ease.InOutQuad)
                );

                collapseSequence.OnComplete(() =>
                {
                    panelClueBody.SetActive(false);
                    isBodyVisible = false;
                    isAnimating = false;
                });
            }
            else
            {
                // Expandir: restaurar tamaño + fade in del contenido
                panelClueBody.SetActive(true);
                bodyGroup.alpha = 0f;

                Sequence expandSequence = DOTween.Sequence();

                // Expandir el panel primero
                expandSequence.Append(
                    panelClueInfoRect.DOSizeDelta(
                        new Vector2(panelClueInfoRect.sizeDelta.x, panelExpandedHeight),
                        collapseExpandDuration
                    ).SetEase(Ease.InOutQuad)
                );

                // Fade in del contenido con un pequeño delay
                expandSequence.Insert(collapseExpandDuration * 0.4f,
                    bodyGroup.DOFade(1f, collapseExpandDuration * 0.6f)
                );

                expandSequence.OnComplete(() =>
                {
                    isBodyVisible = true;
                    isAnimating = false;
                });
            }
        }

        // ═══════════════════════════════════════════
        // BOTÓN "SIGUIENTE PISTA"
        // ═══════════════════════════════════════════

        private void OnNextCluePressed()
        {
            // Ocultar panel actual con animación antes de revelar la siguiente pista
            panelClueInfoRect.DOScale(Vector3.zero, panelRevealDuration * 0.5f)
                .SetEase(Ease.InBack)
                .OnComplete(() =>
                {
                    panelClueInfo.SetActive(false);
                    panelClueInfoRect.localScale = Vector3.one; // Resetear escala

                    // Intercambiar botones de vuelta
                    SwapToScanButton();

                    // Notificar al GameStateManager
                    gameStateManager.OnNextCluePressed();
                });
        }

        // ═══════════════════════════════════════════
        // UTILIDADES
        // ═══════════════════════════════════════════

        private CanvasGroup GetOrAddCanvasGroup(GameObject go)
        {
            CanvasGroup group = go.GetComponent<CanvasGroup>();
            if (group == null)
            {
                group = go.AddComponent<CanvasGroup>();
            }
            return group;
        }

        private void OnDestroy()
        {
            buttonClueHeader.onClick.RemoveListener(ToggleClueBody);

            Button nextBtn = buttonNextClue.GetComponent<Button>();
            if (nextBtn != null)
            {
                nextBtn.onClick.RemoveListener(OnNextCluePressed);
            }

            if (gameStateManager != null)
            {
                gameStateManager.OnGameStarted -= HandleGameStarted;
                gameStateManager.OnClueRevealed -= HandleClueRevealed;
                gameStateManager.OnClueCompleted -= HandleClueCompleted;
                gameStateManager.OnGameWon -= HandleGameWon;
            }
        }
    }
}