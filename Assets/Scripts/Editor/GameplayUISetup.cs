#if UNITY_EDITOR
using UnityEngine;
using UnityEditor;
using UnityEngine.UI;
using TMPro;

namespace BusquedaTesoro.Editor
{
    /// <summary>
    /// Editor script que genera toda la jerarquía de UI de la escena Gameplay.
    /// Ejecutar desde Tools → Búsqueda del Tesoro → Setup Gameplay UI.
    /// IMPORTANTE: Ejecutar una sola vez. Si necesitás regenerar, eliminá Canvas_Gameplay primero.
    /// </summary>
    public class GameplayUISetup
    {
        // ═══════════════════════════════════════════
        // PALETA DE COLORES (placeholders equipo rojo)
        // ═══════════════════════════════════════════
        private static readonly Color COLOR_TEAM_BAR     = HexColor("#CC2222");
        private static readonly Color COLOR_HEADER_BG    = HexColor("#3D0A0A", 200);
        private static readonly Color COLOR_GOLD         = HexColor("#D4A017");
        private static readonly Color COLOR_PANEL_BG     = HexColor("#2E2E1A", 230);
        private static readonly Color COLOR_TEXT_WHITE    = Color.white;
        private static readonly Color COLOR_TEXT_DARK     = HexColor("#1A1A1A");
        private static readonly Color COLOR_SUBTITLE     = HexColor("#8A8A6A");
        private static readonly Color COLOR_DOT_INACTIVE = HexColor("#2A2A2A");
        private static readonly Color COLOR_STICKER_SHADOW = HexColor("#1A1A10", 150);
        private static readonly Color COLOR_STICKER_FILLED = HexColor("#E85050");
        private static readonly Color COLOR_MINI_INACTIVE  = HexColor("#4A4A4A");
        private static readonly Color COLOR_BAR_BG       = HexColor("#2A2A2A");
        private static readonly Color COLOR_BAR_FILL     = HexColor("#CC2222");
        private static readonly Color COLOR_SEPARATOR    = HexColor("#8A8A6A");
        private static readonly Color COLOR_TRANSPARENT  = new Color(0, 0, 0, 0);

        // ═══════════════════════════════════════════
        // MENU ENTRY
        // ═══════════════════════════════════════════
        [MenuItem("Tools/Búsqueda del Tesoro/Setup Gameplay UI")]
        public static void SetupGameplayUI()
        {
            // Verificar que no exista ya
            if (GameObject.Find("Canvas_Gameplay") != null)
            {
                if (!EditorUtility.DisplayDialog(
                    "Canvas_Gameplay ya existe",
                    "Ya existe un Canvas_Gameplay en la escena. ¿Querés eliminarlo y recrearlo?",
                    "Sí, recrear", "Cancelar"))
                {
                    return;
                }
                Undo.DestroyObjectImmediate(GameObject.Find("Canvas_Gameplay"));
            }

            // ═══════════════════════════════════════
            // CANVAS PRINCIPAL
            // ═══════════════════════════════════════
            GameObject canvasGO = CreateCanvas("Canvas_Gameplay", 1);
            Canvas canvas = canvasGO.GetComponent<Canvas>();
            RectTransform canvasRect = canvasGO.GetComponent<RectTransform>();

            // ═══════════════════════════════════════
            // PANEL HEADER
            // ═══════════════════════════════════════
            GameObject header = CreatePanel("Panel_Header", canvasGO.transform, COLOR_HEADER_BG);
            SetAnchors(header, AnchorPreset.TopStretch);
            SetRect(header, 0, 0, 0, 0);
            header.GetComponent<RectTransform>().sizeDelta = new Vector2(0, 120);
            header.GetComponent<RectTransform>().pivot = new Vector2(0.5f, 1f);
            SetAnchoredPosition(header, 0, 0);

            // -- Team Color Bar --
            GameObject teamBar = CreateImage("Image_TeamColorBar", header.transform, COLOR_TEAM_BAR);
            SetAnchors(teamBar, AnchorPreset.LeftStretch);
            RectTransform teamBarRect = teamBar.GetComponent<RectTransform>();
            teamBarRect.sizeDelta = new Vector2(8, 0);
            teamBarRect.anchoredPosition = new Vector2(4, 0);
            teamBarRect.offsetMin = new Vector2(0, 0);
            teamBarRect.offsetMax = new Vector2(8, 0);

            // -- Team Label --
            GameObject teamLabel = CreateTMP("TMP_TeamLabel", header.transform,
                "EQUIPO\nROJO", 28, COLOR_TEXT_WHITE,
                TextAlignmentOptions.TopLeft, FontStyles.Bold);
            RectTransform teamLabelRect = teamLabel.GetComponent<RectTransform>();
            SetAnchors(teamLabel, AnchorPreset.TopLeft);
            teamLabelRect.anchoredPosition = new Vector2(70, -20);
            teamLabelRect.sizeDelta = new Vector2(200, 80);

            // -- Progress Dots Container --
            GameObject dotsContainer = CreateEmpty("Panel_ProgressDots", header.transform);
            SetAnchors(dotsContainer, AnchorPreset.TopCenter);
            RectTransform dotsRect = dotsContainer.GetComponent<RectTransform>();
            dotsRect.anchoredPosition = new Vector2(0, -55);
            dotsRect.sizeDelta = new Vector2(200, 30);

            HorizontalLayoutGroup dotsLayout = dotsContainer.AddComponent<HorizontalLayoutGroup>();
            dotsLayout.spacing = 20;
            dotsLayout.childAlignment = TextAnchor.MiddleCenter;
            dotsLayout.childControlWidth = true;
            dotsLayout.childControlHeight = true;
            dotsLayout.childForceExpandWidth = false;
            dotsLayout.childForceExpandHeight = false;

            for (int i = 1; i <= 4; i++)
            {
                GameObject dot = CreateImage($"Image_Dot_{i}", dotsContainer.transform, COLOR_DOT_INACTIVE);
                LayoutElement le = dot.AddComponent<LayoutElement>();
                le.preferredWidth = 24;
                le.preferredHeight = 24;
                // Hacerlo circular: usar sprite default Knob
                dot.GetComponent<Image>().sprite = AssetDatabase.GetBuiltinExtraResource<Sprite>("UI/Skin/Knob.psd");
            }

            // -- Clue Counter --
            GameObject clueCounter = CreateTMP("TMP_ClueCounter", header.transform,
                "PISTA\n<size=48>1<size=32>|<size=48>4", 32, COLOR_TEXT_WHITE,
                TextAlignmentOptions.TopRight, FontStyles.Normal);
            clueCounter.GetComponent<TextMeshProUGUI>().richText = true;
            SetAnchors(clueCounter, AnchorPreset.TopRight);
            RectTransform counterRect = clueCounter.GetComponent<RectTransform>();
            counterRect.anchoredPosition = new Vector2(-60, -20);
            counterRect.sizeDelta = new Vector2(180, 80);

            // -- Progress Line --
            GameObject progressLine = CreateImage("Image_ProgressLine", header.transform, COLOR_TEAM_BAR);
            SetAnchors(progressLine, AnchorPreset.BottomStretch);
            RectTransform lineRect = progressLine.GetComponent<RectTransform>();
            lineRect.offsetMin = new Vector2(0, 0);
            lineRect.offsetMax = new Vector2(0, 4);

            // ═══════════════════════════════════════
            // PANEL BORDER FRAME (decorativo)
            // ═══════════════════════════════════════
            GameObject borderFrame = CreateImage("Panel_BorderFrame", canvasGO.transform, COLOR_TRANSPARENT);
            SetAnchors(borderFrame, AnchorPreset.Stretch);
            RectTransform borderRect = borderFrame.GetComponent<RectTransform>();
            borderRect.offsetMin = new Vector2(20, 20);
            borderRect.offsetMax = new Vector2(-20, -20);
            borderFrame.GetComponent<Image>().raycastTarget = false;

            // ═══════════════════════════════════════
            // PANEL CLUE INFO (colapsable)
            // ═══════════════════════════════════════
            GameObject clueInfo = CreatePanel("Panel_ClueInfo", canvasGO.transform, COLOR_PANEL_BG);
            SetAnchors(clueInfo, AnchorPreset.MiddleCenter);
            RectTransform clueInfoRect = clueInfo.GetComponent<RectTransform>();
            clueInfoRect.anchoredPosition = new Vector2(0, 50);
            clueInfoRect.sizeDelta = new Vector2(900, 750);
            clueInfo.SetActive(false); // Empieza desactivado

            // -- Clue Header Button --
            GameObject clueHeaderBtn = CreateButton("Button_ClueHeader", clueInfo.transform,
                "PISTA 1", 32, COLOR_GOLD, COLOR_TEXT_DARK);
            SetAnchors(clueHeaderBtn, AnchorPreset.TopCenter);
            RectTransform headerBtnRect = clueHeaderBtn.GetComponent<RectTransform>();
            headerBtnRect.anchoredPosition = new Vector2(0, 50);
            headerBtnRect.sizeDelta = new Vector2(220, 65);
            clueHeaderBtn.GetComponent<Button>().transition = Selectable.Transition.None;

            // -- Clue Body --
            GameObject clueBody = CreateEmpty("Panel_ClueBody", clueInfo.transform);
            SetAnchors(clueBody, AnchorPreset.Stretch);
            RectTransform bodyRect = clueBody.GetComponent<RectTransform>();
            bodyRect.offsetMin = new Vector2(40, 40);
            bodyRect.offsetMax = new Vector2(-40, -80);

            // -- Clue Text --
            GameObject clueText = CreateTMP("TMP_ClueText", clueBody.transform,
                "Si tu cuerpo fuera un PC, aquí es donde le haces el upgrade.",
                40, COLOR_TEXT_WHITE, TextAlignmentOptions.Top, FontStyles.Normal);
            SetAnchors(clueText, AnchorPreset.TopStretch);
            RectTransform clueTextRect = clueText.GetComponent<RectTransform>();
            clueTextRect.offsetMin = new Vector2(20, 0);
            clueTextRect.offsetMax = new Vector2(-20, 0);
            clueTextRect.sizeDelta = new Vector2(clueTextRect.sizeDelta.x, 250);
            clueTextRect.anchoredPosition = new Vector2(0, 0);
            clueText.GetComponent<TextMeshProUGUI>().textWrappingMode = TextWrappingModes.Normal;

            // -- Separator --
            GameObject separator = CreateImage("Image_Separator", clueBody.transform, COLOR_SEPARATOR);
            SetAnchors(separator, AnchorPreset.MiddleStretch);
            RectTransform sepRect = separator.GetComponent<RectTransform>();
            sepRect.offsetMin = new Vector2(20, 0);
            sepRect.offsetMax = new Vector2(-20, 3);
            sepRect.anchoredPosition = new Vector2(0, -20);

            // -- Clue Subtitle --
            GameObject subtitleText = CreateTMP("TMP_ClueSubtext", clueBody.transform,
                "ENCUENTRA\nEL LUGAR", 24, COLOR_SUBTITLE,
                TextAlignmentOptions.Center, FontStyles.Italic);
            SetAnchors(subtitleText, AnchorPreset.MiddleCenter);
            RectTransform subRect = subtitleText.GetComponent<RectTransform>();
            subRect.anchoredPosition = new Vector2(0, -60);
            subRect.sizeDelta = new Vector2(400, 60);

            // -- Sticker Slot --
            GameObject stickerSlot = CreateImage("Panel_StickerSlot", clueBody.transform, COLOR_TRANSPARENT);
            SetAnchors(stickerSlot, AnchorPreset.BottomCenter);
            RectTransform slotRect = stickerSlot.GetComponent<RectTransform>();
            slotRect.anchoredPosition = new Vector2(0, 40);
            slotRect.sizeDelta = new Vector2(150, 150);

            // Sticker Shadow
            GameObject stickerShadow = CreateImage("Image_StickerShadow", stickerSlot.transform, COLOR_STICKER_SHADOW);
            SetAnchors(stickerShadow, AnchorPreset.Stretch);
            stickerShadow.GetComponent<RectTransform>().offsetMin = Vector2.zero;
            stickerShadow.GetComponent<RectTransform>().offsetMax = Vector2.zero;
            stickerShadow.GetComponent<Image>().sprite = AssetDatabase.GetBuiltinExtraResource<Sprite>("UI/Skin/Knob.psd");

            // Sticker Filled (desactivado)
            GameObject stickerFilled = CreateImage("Image_StickerFilled", stickerSlot.transform, COLOR_STICKER_FILLED);
            SetAnchors(stickerFilled, AnchorPreset.Stretch);
            stickerFilled.GetComponent<RectTransform>().offsetMin = Vector2.zero;
            stickerFilled.GetComponent<RectTransform>().offsetMax = Vector2.zero;
            stickerFilled.GetComponent<Image>().sprite = AssetDatabase.GetBuiltinExtraResource<Sprite>("UI/Skin/Knob.psd");
            stickerFilled.SetActive(false);

            // -- Panel Corners (tornillos decorativos) --
            CreateCorner("Image_Corner_TL", clueInfo.transform, AnchorPreset.TopLeft, new Vector2(15, -15));
            CreateCorner("Image_Corner_TR", clueInfo.transform, AnchorPreset.TopRight, new Vector2(-15, -15));
            CreateCorner("Image_Corner_BL", clueInfo.transform, AnchorPreset.BottomLeft, new Vector2(15, 15));
            CreateCorner("Image_Corner_BR", clueInfo.transform, AnchorPreset.BottomRight, new Vector2(-15, 15));

            // ═══════════════════════════════════════
            // PANEL BOTTOM ACTION
            // ═══════════════════════════════════════
            GameObject bottomAction = CreateEmpty("Panel_BottomAction", canvasGO.transform);
            SetAnchors(bottomAction, AnchorPreset.BottomStretch);
            RectTransform bottomRect = bottomAction.GetComponent<RectTransform>();
            bottomRect.offsetMin = new Vector2(80, 200);
            bottomRect.offsetMax = new Vector2(-80, 310);

            // -- Scan Button --
            GameObject scanBtn = CreateButton("Button_Scan", bottomAction.transform,
                "Escanear", 42, COLOR_GOLD, COLOR_TEXT_DARK);
            SetAnchors(scanBtn, AnchorPreset.Stretch);
            scanBtn.GetComponent<RectTransform>().offsetMin = Vector2.zero;
            scanBtn.GetComponent<RectTransform>().offsetMax = Vector2.zero;

            // QR Icon dentro del botón scan
            GameObject qrIcon = CreateImage("Image_QRIcon", scanBtn.transform, COLOR_TEXT_DARK);
            SetAnchors(qrIcon, AnchorPreset.BottomCenter);
            RectTransform qrRect = qrIcon.GetComponent<RectTransform>();
            qrRect.anchoredPosition = new Vector2(0, 15);
            qrRect.sizeDelta = new Vector2(32, 32);

            // -- Next Clue Button (desactivado) --
            GameObject nextBtn = CreateButton("Button_NextClue", bottomAction.transform,
                "Siguiente Pista", 42, COLOR_GOLD, COLOR_TEXT_DARK);
            SetAnchors(nextBtn, AnchorPreset.Stretch);
            nextBtn.GetComponent<RectTransform>().offsetMin = Vector2.zero;
            nextBtn.GetComponent<RectTransform>().offsetMax = Vector2.zero;
            nextBtn.SetActive(false);

            // Next Icon
            GameObject nextIcon = CreateImage("Image_NextIcon", nextBtn.transform, COLOR_TEXT_DARK);
            SetAnchors(nextIcon, AnchorPreset.BottomCenter);
            RectTransform nextIconRect = nextIcon.GetComponent<RectTransform>();
            nextIconRect.anchoredPosition = new Vector2(0, 15);
            nextIconRect.sizeDelta = new Vector2(64, 32);

            // ═══════════════════════════════════════
            // PANEL PROGRESS BAR
            // ═══════════════════════════════════════
            GameObject progressBar = CreateEmpty("Panel_ProgressBar", canvasGO.transform);
            SetAnchors(progressBar, AnchorPreset.BottomStretch);
            RectTransform progRect = progressBar.GetComponent<RectTransform>();
            progRect.offsetMin = new Vector2(60, 80);
            progRect.offsetMax = new Vector2(-60, 160);

            // -- Bar BG --
            GameObject barBG = CreateImage("Image_ProgressBarBG", progressBar.transform, COLOR_BAR_BG);
            SetAnchors(barBG, AnchorPreset.BottomStretch);
            RectTransform barBGRect = barBG.GetComponent<RectTransform>();
            barBGRect.offsetMin = new Vector2(0, 0);
            barBGRect.offsetMax = new Vector2(0, 12);

            // -- Bar Fill --
            GameObject barFill = CreateImage("Image_ProgressBarFill", progressBar.transform, COLOR_BAR_FILL);
            SetAnchors(barFill, AnchorPreset.BottomStretch);
            RectTransform barFillRect = barFill.GetComponent<RectTransform>();
            barFillRect.offsetMin = new Vector2(0, 0);
            barFillRect.offsetMax = new Vector2(0, 12);
            Image barFillImg = barFill.GetComponent<Image>();
            barFillImg.type = Image.Type.Filled;
            barFillImg.fillMethod = Image.FillMethod.Horizontal;
            barFillImg.fillOrigin = (int)Image.OriginHorizontal.Left;
            barFillImg.fillAmount = 0f;

            // -- Mini Stickers Container --
            GameObject miniContainer = CreateEmpty("MiniStickersContainer", progressBar.transform);
            SetAnchors(miniContainer, AnchorPreset.Stretch);
            RectTransform miniContRect = miniContainer.GetComponent<RectTransform>();
            miniContRect.offsetMin = new Vector2(0, 12);
            miniContRect.offsetMax = Vector2.zero;

            HorizontalLayoutGroup miniLayout = miniContainer.AddComponent<HorizontalLayoutGroup>();
            miniLayout.spacing = 80;
            miniLayout.childAlignment = TextAnchor.MiddleCenter;
            miniLayout.childControlWidth = true;
            miniLayout.childControlHeight = true;
            miniLayout.childForceExpandWidth = false;
            miniLayout.childForceExpandHeight = false;
            miniLayout.padding = new RectOffset(30, 30, 0, 0);

            for (int i = 1; i <= 4; i++)
            {
                GameObject mini = CreateImage($"Image_MiniSticker_{i}", miniContainer.transform, COLOR_MINI_INACTIVE);
                mini.GetComponent<Image>().sprite = AssetDatabase.GetBuiltinExtraResource<Sprite>("UI/Skin/Knob.psd");
                LayoutElement le = mini.AddComponent<LayoutElement>();
                le.preferredWidth = 48;
                le.preferredHeight = 48;
            }

            // -- Corner Dots --
            GameObject dotLeft = CreateImage("Image_CornerDot_Left", progressBar.transform, COLOR_GOLD);
            SetAnchors(dotLeft, AnchorPreset.BottomLeft);
            dotLeft.GetComponent<RectTransform>().anchoredPosition = new Vector2(10, 10);
            dotLeft.GetComponent<RectTransform>().sizeDelta = new Vector2(12, 12);

            GameObject dotRight = CreateImage("Image_CornerDot_Right", progressBar.transform, COLOR_GOLD);
            SetAnchors(dotRight, AnchorPreset.BottomRight);
            dotRight.GetComponent<RectTransform>().anchoredPosition = new Vector2(-10, 10);
            dotRight.GetComponent<RectTransform>().sizeDelta = new Vector2(12, 12);

            // ═══════════════════════════════════════
            // REGISTRO UNDO Y SELECCIÓN
            // ═══════════════════════════════════════
            Undo.RegisterCreatedObjectUndo(canvasGO, "Setup Gameplay UI");
            Selection.activeGameObject = canvasGO;

            Debug.Log("[GameplayUISetup] ✅ UI de Gameplay creada exitosamente. " +
                      "Panel_ClueInfo y Button_NextClue están desactivados por defecto (se activan por código).");
        }

        // ═══════════════════════════════════════════════════
        // HELPER METHODS
        // ═══════════════════════════════════════════════════

        private static GameObject CreateCanvas(string name, int sortOrder)
        {
            GameObject go = new GameObject(name);
            Canvas canvas = go.AddComponent<Canvas>();
            canvas.renderMode = RenderMode.ScreenSpaceOverlay;
            canvas.sortingOrder = sortOrder;

            CanvasScaler scaler = go.AddComponent<CanvasScaler>();
            scaler.uiScaleMode = CanvasScaler.ScaleMode.ScaleWithScreenSize;
            scaler.referenceResolution = new Vector2(1080, 1920);
            scaler.screenMatchMode = CanvasScaler.ScreenMatchMode.MatchWidthOrHeight;
            scaler.matchWidthOrHeight = 0.5f;

            go.AddComponent<GraphicRaycaster>();
            return go;
        }

        private static GameObject CreatePanel(string name, Transform parent, Color color)
        {
            GameObject go = new GameObject(name);
            go.transform.SetParent(parent, false);
            go.AddComponent<RectTransform>();
            Image img = go.AddComponent<Image>();
            img.color = color;
            return go;
        }

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
            // Crear el GO del botón con Image
            GameObject go = new GameObject(name);
            go.transform.SetParent(parent, false);
            go.AddComponent<RectTransform>();
            Image img = go.AddComponent<Image>();
            img.color = bgColor;
            Button btn = go.AddComponent<Button>();
            btn.targetGraphic = img;

            // Crear el texto hijo
            GameObject textGO = new GameObject($"TMP_{name.Replace("Button_", "")}Text");
            textGO.transform.SetParent(go.transform, false);
            RectTransform textRect = textGO.AddComponent<RectTransform>();
            SetAnchors(textGO, AnchorPreset.Stretch);
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

        private static void CreateCorner(string name, Transform parent, AnchorPreset anchor, Vector2 position)
        {
            GameObject go = CreateImage(name, parent, COLOR_GOLD);
            SetAnchors(go, anchor);
            RectTransform rect = go.GetComponent<RectTransform>();
            rect.anchoredPosition = position;
            rect.sizeDelta = new Vector2(24, 24);
        }

        // ═══════════════════════════════════════════════════
        // ANCHOR PRESETS
        // ═══════════════════════════════════════════════════

        private enum AnchorPreset
        {
            TopLeft, TopCenter, TopRight, TopStretch,
            MiddleLeft, MiddleCenter, MiddleRight, MiddleStretch,
            BottomLeft, BottomCenter, BottomRight, BottomStretch,
            LeftStretch, RightStretch, Stretch
        }

        private static void SetAnchors(GameObject go, AnchorPreset preset)
        {
            RectTransform rect = go.GetComponent<RectTransform>();
            switch (preset)
            {
                case AnchorPreset.TopLeft:
                    rect.anchorMin = new Vector2(0, 1); rect.anchorMax = new Vector2(0, 1);
                    rect.pivot = new Vector2(0, 1); break;
                case AnchorPreset.TopCenter:
                    rect.anchorMin = new Vector2(0.5f, 1); rect.anchorMax = new Vector2(0.5f, 1);
                    rect.pivot = new Vector2(0.5f, 1); break;
                case AnchorPreset.TopRight:
                    rect.anchorMin = new Vector2(1, 1); rect.anchorMax = new Vector2(1, 1);
                    rect.pivot = new Vector2(1, 1); break;
                case AnchorPreset.TopStretch:
                    rect.anchorMin = new Vector2(0, 1); rect.anchorMax = new Vector2(1, 1);
                    rect.pivot = new Vector2(0.5f, 1); break;
                case AnchorPreset.MiddleLeft:
                    rect.anchorMin = new Vector2(0, 0.5f); rect.anchorMax = new Vector2(0, 0.5f);
                    rect.pivot = new Vector2(0, 0.5f); break;
                case AnchorPreset.MiddleCenter:
                    rect.anchorMin = new Vector2(0.5f, 0.5f); rect.anchorMax = new Vector2(0.5f, 0.5f);
                    rect.pivot = new Vector2(0.5f, 0.5f); break;
                case AnchorPreset.MiddleRight:
                    rect.anchorMin = new Vector2(1, 0.5f); rect.anchorMax = new Vector2(1, 0.5f);
                    rect.pivot = new Vector2(1, 0.5f); break;
                case AnchorPreset.MiddleStretch:
                    rect.anchorMin = new Vector2(0, 0.5f); rect.anchorMax = new Vector2(1, 0.5f);
                    rect.pivot = new Vector2(0.5f, 0.5f); break;
                case AnchorPreset.BottomLeft:
                    rect.anchorMin = new Vector2(0, 0); rect.anchorMax = new Vector2(0, 0);
                    rect.pivot = new Vector2(0, 0); break;
                case AnchorPreset.BottomCenter:
                    rect.anchorMin = new Vector2(0.5f, 0); rect.anchorMax = new Vector2(0.5f, 0);
                    rect.pivot = new Vector2(0.5f, 0); break;
                case AnchorPreset.BottomRight:
                    rect.anchorMin = new Vector2(1, 0); rect.anchorMax = new Vector2(1, 0);
                    rect.pivot = new Vector2(1, 0); break;
                case AnchorPreset.BottomStretch:
                    rect.anchorMin = new Vector2(0, 0); rect.anchorMax = new Vector2(1, 0);
                    rect.pivot = new Vector2(0.5f, 0); break;
                case AnchorPreset.LeftStretch:
                    rect.anchorMin = new Vector2(0, 0); rect.anchorMax = new Vector2(0, 1);
                    rect.pivot = new Vector2(0, 0.5f); break;
                case AnchorPreset.RightStretch:
                    rect.anchorMin = new Vector2(1, 0); rect.anchorMax = new Vector2(1, 1);
                    rect.pivot = new Vector2(1, 0.5f); break;
                case AnchorPreset.Stretch:
                    rect.anchorMin = Vector2.zero; rect.anchorMax = Vector2.one;
                    rect.pivot = new Vector2(0.5f, 0.5f); break;
            }
        }

        private static void SetRect(GameObject go, float left, float right, float top, float bottom)
        {
            RectTransform rect = go.GetComponent<RectTransform>();
            rect.offsetMin = new Vector2(left, bottom);
            rect.offsetMax = new Vector2(-right, -top);
        }

        private static void SetAnchoredPosition(GameObject go, float x, float y)
        {
            go.GetComponent<RectTransform>().anchoredPosition = new Vector2(x, y);
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