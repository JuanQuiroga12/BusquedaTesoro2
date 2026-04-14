using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;
using TMPro;
using DG.Tweening;
using BusquedaTesoro.Core;

namespace BusquedaTesoro.UI
{
    /// <summary>
    /// Controla la pantalla de victoria: muestra datos del equipo,
    /// tiempo final, y permite volver a jugar.
    /// </summary>
    public class VictoryController : MonoBehaviour
    {
        [Header("Textos")]
        [SerializeField] private TextMeshProUGUI tmpTitle;
        [SerializeField] private TextMeshProUGUI tmpTeamName;
        [SerializeField] private TextMeshProUGUI tmpFinalTime;

        [Header("Imagen")]
        [SerializeField] private Image imageTreasureChest;

        [Header("Dots")]
        [SerializeField] private Image[] completedDots;

        [Header("Botón")]
        [SerializeField] private Button buttonPlayAgain;

        [Header("Escenas")]
        [SerializeField] private string teamSelectScene = "Seleccion de grupos";

        [Header("Animación")]
        [SerializeField] private float entranceDelay = 0.3f;

        private void Start()
        {
            // Asignar datos del equipo
            if (tmpTeamName != null)
            {
                tmpTeamName.text = TeamData.TeamDisplayName;
                tmpTeamName.color = TeamData.TeamColor;
            }

            if (tmpFinalTime != null)
            {
                tmpFinalTime.text = $"TIEMPO: {TeamData.FinalTimeFormatted}";
            }

            // Colorear dots con el color del equipo
            foreach (Image dot in completedDots)
            {
                if (dot != null)
                    dot.color = TeamData.TeamColor;
            }

            // Conectar botón
            if (buttonPlayAgain != null)
            {
                buttonPlayAgain.onClick.AddListener(OnPlayAgain);
            }

            // Animaciones de entrada
            PlayEntranceAnimations();
        }

        private void PlayEntranceAnimations()
        {
            // Título: baja desde arriba
            if (tmpTitle != null)
            {
                RectTransform titleRect = tmpTitle.GetComponent<RectTransform>();
                Vector2 originalPos = titleRect.anchoredPosition;
                titleRect.anchoredPosition = originalPos + new Vector2(0, 200);
                tmpTitle.color = new Color(tmpTitle.color.r, tmpTitle.color.g, tmpTitle.color.b, 0);

                Sequence titleSeq = DOTween.Sequence();
                titleSeq.AppendInterval(entranceDelay);
                titleSeq.Append(titleRect.DOAnchorPos(originalPos, 0.6f).SetEase(Ease.OutBack));
                titleSeq.Join(tmpTitle.DOFade(1f, 0.4f));
            }

            // Cofre: escala desde 0
            if (imageTreasureChest != null)
            {
                RectTransform chestRect = imageTreasureChest.GetComponent<RectTransform>();
                chestRect.localScale = Vector3.zero;

                DOTween.Sequence()
                    .AppendInterval(entranceDelay + 0.3f)
                    .Append(chestRect.DOScale(Vector3.one, 0.7f).SetEase(Ease.OutBounce));
            }

            // Botón: fade in
            if (buttonPlayAgain != null)
            {
                CanvasGroup btnGroup = buttonPlayAgain.gameObject.AddComponent<CanvasGroup>();
                btnGroup.alpha = 0f;

                DOTween.Sequence()
                    .AppendInterval(entranceDelay + 1f)
                    .Append(DOTween.To(() => btnGroup.alpha, x => btnGroup.alpha = x, 1f, 0.5f));
            }
        }

        private void OnPlayAgain()
        {
            // Resetear datos estáticos
            TeamData.FinalTime = 0f;
            SceneManager.LoadScene(teamSelectScene);
        }

        private void OnDestroy()
        {
            if (buttonPlayAgain != null)
            {
                buttonPlayAgain.onClick.RemoveListener(OnPlayAgain);
            }
        }
    }
}