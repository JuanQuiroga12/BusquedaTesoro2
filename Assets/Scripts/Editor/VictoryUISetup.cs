#if UNITY_EDITOR
using UnityEngine;
using UnityEditor;
using UnityEngine.UI;
using TMPro;

namespace BusquedaTesoro.Editor
{
    public class VictoryUISetup
    {
        private static readonly Color COLOR_GOLD = HexColor("#D4A017");
        private static readonly Color COLOR_BG_DARK = HexColor("#1A2A2A");
        private static readonly Color COLOR_TEXT_WHITE = Color.white;
        private static readonly Color COLOR_TEXT_DARK = HexColor("#1A1A1A");
        private static readonly Color COLOR_PANEL_BG = HexColor("#2E3B2E", 200);
        private static readonly Color COLOR_MESSAGE_BG = HexColor("#1A2A2A", 220);

        [MenuItem("Tools/Búsqueda del Tesoro/Setup Victory UI")]
        public static void SetupVictoryUI()
        {
            if (GameObject.Find("Canvas_Victory") != null)
            {
                if (!EditorUtility.DisplayDialog(
                    "Canvas_Victory ya existe",
                    "¿Eliminar y recrear?",
                    "Sí", "Cancelar"))
                    return;
                Undo.DestroyObjectImmediate(GameObject.Find("Canvas_Victory"));
            }

            // ═══════════════════════════════════════
            // CANVAS
            // ═══════════════════════════════════════
            GameObject canvasGO = new GameObject("Canvas_Victory");
            Canvas canvas = canvasGO.AddComponent<Canvas>();
            canvas.renderMode = RenderMode.ScreenSpaceOverlay;
            canvas.sortingOrder = 0;

            CanvasScaler scaler = canvasGO.AddComponent<CanvasScaler>();
            scaler.uiScaleMode = CanvasScaler.ScaleMode.ScaleWithScreenSize;
            scaler.referenceResolution = new Vector2(1080, 1920);
            scaler.screenMatchMode = CanvasScaler.ScreenMatchMode.MatchWidthOrHeight;
            scaler.matchWidthOrHeight = 0.5f;

            canvasGO.AddComponent<GraphicRaycaster>();

            // ═══════════════════════════════════════
            // FONDO
            // ═══════════════════════════════════════
            GameObject bg = CreateImage("Image_Background", canvasGO.transform, COLOR_BG_DARK);
            SetAnchorsStretch(bg);

            // ═══════════════════════════════════════
            // MARCO DORADO
            // ═══════════════════════════════════════
            GameObject frame = CreateImage("Panel_BorderFrame", canvasGO.transform, new Color(0, 0, 0, 0));
            SetAnchorsStretch(frame);
            RectTransform frameRect = frame.GetComponent<RectTransform>();
            frameRect.offsetMin = new Vector2(20, 20);
            frameRect.offsetMax = new Vector2(-20, -20);
            frame.GetComponent<Image>().raycastTarget = false;

            // ═══════════════════════════════════════
            // CONTENEDOR PRINCIPAL (scrolleable si hace falta)
            // ═══════════════════════════════════════
            GameObject content = CreateEmpty("Content", canvasGO.transform);
            SetAnchorsStretch(content);
            RectTransform contentRect = content.GetComponent<RectTransform>();
            contentRect.offsetMin = new Vector2(60, 60);
            contentRect.offsetMax = new Vector2(-60, -60);

            // ═══════════════════════════════════════
            // TÍTULO "¡GANASTE!"
            // ═══════════════════════════════════════
            GameObject title = CreateTMP("TMP_Title", content.transform,
                "¡GANASTE!", 80, COLOR_GOLD,
                TextAlignmentOptions.Center, FontStyles.Bold);
            RectTransform titleRect = title.GetComponent<RectTransform>();
            titleRect.anchorMin = new Vector2(0, 1);
            titleRect.anchorMax = new Vector2(1, 1);
            titleRect.pivot = new Vector2(0.5f, 1);
            titleRect.offsetMin = new Vector2(0, 0);
            titleRect.offsetMax = new Vector2(0, 0);
            titleRect.sizeDelta = new Vector2(0, 120);
            titleRect.anchoredPosition = new Vector2(0, -20);

            // Línea debajo del título
            GameObject titleLine = CreateImage("Image_TitleLine", content.transform, COLOR_GOLD);
            RectTransform lineRect = titleLine.GetComponent<RectTransform>();
            lineRect.anchorMin = new Vector2(0.1f, 1);
            lineRect.anchorMax = new Vector2(0.9f, 1);
            lineRect.pivot = new Vector2(0.5f, 1);
            lineRect.sizeDelta = new Vector2(0, 4);
            lineRect.anchoredPosition = new Vector2(0, -140);

            // ═══════════════════════════════════════
            // DOTS COMPLETADOS
            // ═══════════════════════════════════════
            GameObject dotsContainer = CreateEmpty("Panel_CompletedDots", content.transform);
            RectTransform dotsRect = dotsContainer.GetComponent<RectTransform>();
            dotsRect.anchorMin = new Vector2(0.5f, 1);
            dotsRect.anchorMax = new Vector2(0.5f, 1);
            dotsRect.pivot = new Vector2(0.5f, 1);
            dotsRect.anchoredPosition = new Vector2(0, -160);
            dotsRect.sizeDelta = new Vector2(200, 30);

            HorizontalLayoutGroup dotsLayout = dotsContainer.AddComponent<HorizontalLayoutGroup>();
            dotsLayout.spacing = 20;
            dotsLayout.childAlignment = TextAnchor.MiddleCenter;
            dotsLayout.childControlWidth = true;
            dotsLayout.childControlHeight = true;
            dotsLayout.childForceExpandWidth = false;
            dotsLayout.childForceExpandHeight = false;

            for (int i = 0; i < 4; i++)
            {
                GameObject dot = CreateImage($"Image_Dot_{i + 1}", dotsContainer.transform, COLOR_GOLD);
                dot.GetComponent<Image>().sprite = AssetDatabase.GetBuiltinExtraResource<Sprite>("UI/Skin/Knob.psd");
                LayoutElement le = dot.AddComponent<LayoutElement>();
                le.preferredWidth = 24;
                le.preferredHeight = 24;
            }

            // ═══════════════════════════════════════
            // IMAGEN DEL COFRE (placeholder)
            // ═══════════════════════════════════════
            GameObject chest = CreateImage("Image_TreasureChest", content.transform, new Color(1, 1, 1, 1));
            RectTransform chestRect = chest.GetComponent<RectTransform>();
            chestRect.anchorMin = new Vector2(0.5f, 0.5f);
            chestRect.anchorMax = new Vector2(0.5f, 0.5f);
            chestRect.pivot = new Vector2(0.5f, 0.5f);
            chestRect.anchoredPosition = new Vector2(0, 150);
            chestRect.sizeDelta = new Vector2(500, 500);
            chest.GetComponent<Image>().preserveAspect = true;

            // ═══════════════════════════════════════
            // PANEL INFO EQUIPO (nombre + tiempo)
            // ═══════════════════════════════════════
            GameObject teamPanel = CreateImage("Panel_TeamInfo", content.transform, COLOR_PANEL_BG);
            RectTransform teamPanelRect = teamPanel.GetComponent<RectTransform>();
            teamPanelRect.anchorMin = new Vector2(0.1f, 0.5f);
            teamPanelRect.anchorMax = new Vector2(0.9f, 0.5f);
            teamPanelRect.pivot = new Vector2(0.5f, 0.5f);
            teamPanelRect.anchoredPosition = new Vector2(0, -170);
            teamPanelRect.sizeDelta = new Vector2(0, 160);

            // Texto del equipo
            GameObject teamText = CreateTMP("TMP_TeamName", teamPanel.transform,
                "EQUIPO ROJO", 30, COLOR_GOLD,
                TextAlignmentOptions.Center, FontStyles.Normal);
            RectTransform teamTextRect = teamText.GetComponent<RectTransform>();
            teamTextRect.anchorMin = new Vector2(0, 0.55f);
            teamTextRect.anchorMax = new Vector2(1, 1);
            teamTextRect.offsetMin = new Vector2(20, 0);
            teamTextRect.offsetMax = new Vector2(-20, -10);

            // Texto del tiempo
            GameObject timeText = CreateTMP("TMP_FinalTime", teamPanel.transform,
                "TIEMPO: 00:00.00", 40, COLOR_TEXT_WHITE,
                TextAlignmentOptions.Center, FontStyles.Bold);
            RectTransform timeTextRect = timeText.GetComponent<RectTransform>();
            timeTextRect.anchorMin = new Vector2(0, 0);
            timeTextRect.anchorMax = new Vector2(1, 0.55f);
            timeTextRect.offsetMin = new Vector2(20, 10);
            timeTextRect.offsetMax = new Vector2(-20, 0);

            // ═══════════════════════════════════════
            // PANEL MENSAJE FELICITACIONES
            // ═══════════════════════════════════════
            GameObject msgPanel = CreateImage("Panel_Message", content.transform, COLOR_MESSAGE_BG);
            RectTransform msgPanelRect = msgPanel.GetComponent<RectTransform>();
            msgPanelRect.anchorMin = new Vector2(0.05f, 0);
            msgPanelRect.anchorMax = new Vector2(0.95f, 0);
            msgPanelRect.pivot = new Vector2(0.5f, 0);
            msgPanelRect.anchoredPosition = new Vector2(0, 160);
            msgPanelRect.sizeDelta = new Vector2(0, 200);

            // Título felicitaciones
            GameObject congratsTitle = CreateTMP("TMP_CongratsTitle", msgPanel.transform,
                "¡FELICITACIONES!", 32, COLOR_GOLD,
                TextAlignmentOptions.Center, FontStyles.Bold);
            RectTransform congratsTitleRect = congratsTitle.GetComponent<RectTransform>();
            congratsTitleRect.anchorMin = new Vector2(0, 0.7f);
            congratsTitleRect.anchorMax = new Vector2(1, 1);
            congratsTitleRect.offsetMin = new Vector2(10, 0);
            congratsTitleRect.offsetMax = new Vector2(-10, -10);

            // Texto del mensaje
            GameObject msgText = CreateTMP("TMP_MessageText", msgPanel.transform,
                "LA AVENTURA HA TERMINADO.\nNO HAY NINGÚN PREMIO FÍSICO. EL PREMIO\nSIEMPRE FUERON LOS AMIGOS CON LOS QUE\nLLEGASTE AL FINAL.",
                20, COLOR_TEXT_WHITE,
                TextAlignmentOptions.Center, FontStyles.Normal);
            RectTransform msgTextRect = msgText.GetComponent<RectTransform>();
            msgTextRect.anchorMin = new Vector2(0, 0);
            msgTextRect.anchorMax = new Vector2(1, 0.7f);
            msgTextRect.offsetMin = new Vector2(15, 10);
            msgTextRect.offsetMax = new Vector2(-15, 0);
            msgText.GetComponent<TextMeshProUGUI>().textWrappingMode = TextWrappingModes.Normal;

            // ═══════════════════════════════════════
            // BOTÓN "JUGAR DE NUEVO"
            // ═══════════════════════════════════════
            GameObject playAgainBtn = CreateButton("Button_PlayAgain", content.transform,
                "JUGAR DE NUEVO", 36, COLOR_GOLD, COLOR_TEXT_DARK);
            RectTransform btnRect = playAgainBtn.GetComponent<RectTransform>();
            btnRect.anchorMin = new Vector2(0.2f, 0);
            btnRect.anchorMax = new Vector2(0.8f, 0);
            btnRect.pivot = new Vector2(0.5f, 0);
            btnRect.anchoredPosition = new Vector2(0, 40);
            btnRect.sizeDelta = new Vector2(0, 80);

            // ═══════════════════════════════════════
            // ESQUINAS DECORATIVAS
            // ═══════════════════════════════════════
            CreateCornerDot(canvasGO.transform, "TopLeft", new Vector2(40, -40), new Vector2(0, 1));
            CreateCornerDot(canvasGO.transform, "TopRight", new Vector2(-40, -40), new Vector2(1, 1));
            CreateCornerDot(canvasGO.transform, "BottomLeft", new Vector2(40, 40), new Vector2(0, 0));
            CreateCornerDot(canvasGO.transform, "BottomRight", new Vector2(-40, 40), new Vector2(1, 0));

            // ═══════════════════════════════════════
            Undo.RegisterCreatedObjectUndo(canvasGO, "Setup Victory UI");
            Selection.activeGameObject = canvasGO;

            Debug.Log("[VictoryUISetup] ✅ UI de Victoria creada exitosamente.");
        }

        // ═══════════════════════════════════════════
        // HELPERS
        // ═══════════════════════════════════════════

        private static GameObject CreateImage(string name, Transform parent, Color color)
        {
            GameObject go = new GameObject(name);
            go.transform.SetParent(parent, false);
            go.AddComponent<RectTransform>();
            Image img = go.AddComponent<Image>();
            img.color = color;
            return go;
        }

        private static GameObject CreateEmpty(string name, Transform parent)
        {
            GameObject go = new GameObject(name);
            go.transform.SetParent(parent, false);
            go.AddComponent<RectTransform>();
            return go;
        }

        private static GameObject CreateTMP(string name, Transform parent,
            string text, float fontSize, Color color,
            TextAlignmentOptions alignment, FontStyles style)
        {
            GameObject go = new GameObject(name);
            go.transform.SetParent(parent, false);
            go.AddComponent<RectTransform>();
            TextMeshProUGUI tmp = go.AddComponent<TextMeshProUGUI>();
            tmp.text = text;
            tmp.fontSize = fontSize;
            tmp.color = color;
            tmp.alignment = alignment;
            tmp.fontStyle = style;
            tmp.textWrappingMode = TextWrappingModes.Normal;
            tmp.richText = true;
            tmp.raycastTarget = false;
            return go;
        }

        private static GameObject CreateButton(string name, Transform parent,
            string text, float fontSize, Color bgColor, Color textColor)
        {
            GameObject go = new GameObject(name);
            go.transform.SetParent(parent, false);
            go.AddComponent<RectTransform>();
            Image img = go.AddComponent<Image>();
            img.color = bgColor;
            Button btn = go.AddComponent<Button>();
            btn.targetGraphic = img;

            GameObject textGO = new GameObject("TMP_ButtonText");
            textGO.transform.SetParent(go.transform, false);
            RectTransform textRect = textGO.AddComponent<RectTransform>();
            textRect.anchorMin = Vector2.zero;
            textRect.anchorMax = Vector2.one;
            textRect.offsetMin = Vector2.zero;
            textRect.offsetMax = Vector2.zero;

            TextMeshProUGUI tmp = textGO.AddComponent<TextMeshProUGUI>();
            tmp.text = text;
            tmp.fontSize = fontSize;
            tmp.color = textColor;
            tmp.alignment = TextAlignmentOptions.Center;
            tmp.fontStyle = FontStyles.Bold;
            tmp.raycastTarget = false;

            return go;
        }

        private static void CreateCornerDot(Transform parent, string name, Vector2 pos, Vector2 anchor)
        {
            GameObject go = CreateImage($"Image_Corner_{name}", parent, COLOR_GOLD);
            RectTransform rect = go.GetComponent<RectTransform>();
            rect.anchorMin = anchor;
            rect.anchorMax = anchor;
            rect.pivot = new Vector2(0.5f, 0.5f);
            rect.anchoredPosition = pos;
            rect.sizeDelta = new Vector2(16, 16);
        }

        private static void SetAnchorsStretch(GameObject go)
        {
            RectTransform rect = go.GetComponent<RectTransform>();
            rect.anchorMin = Vector2.zero;
            rect.anchorMax = Vector2.one;
            rect.offsetMin = Vector2.zero;
            rect.offsetMax = Vector2.zero;
        }

        private static Color HexColor(string hex, int alpha = 255)
        {
            ColorUtility.TryParseHtmlString(hex, out Color color);
            color.a = alpha / 255f;
            return color;
        }
    }
}
#endif