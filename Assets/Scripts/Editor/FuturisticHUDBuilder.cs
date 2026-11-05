using UnityEngine;
using UnityEngine.UI;
using UnityEditor;
using UnityEditor.SceneManagement;

namespace ZBufferDemo.Editor
{
    /// <summary>
    /// Procedural builder for the Sci-Fi HUD Canvas and elements.
    /// Generates high-tech cockpit panels, corner brackets, and reticles with crisp, high-contrast, scalable text.
    /// Operates in Screen Space - Overlay so it remains vibrant and unaffected by the depth grayscale shader.
    /// </summary>
    public static class FuturisticHUDBuilder
    {
        public const string HUD_ROOT_NAME = "ZBuffer_HUD_Root";

        // Curated High-Contrast Sci-Fi Color Palette
        public static readonly Color ColorBgDark = new Color(0.015f, 0.03f, 0.07f, 0.92f);
        public static readonly Color ColorBorderCyan = new Color(0.0f, 0.95f, 1.0f, 0.75f);
        public static readonly Color ColorNeonCyan = new Color(0.0f, 1.0f, 1.0f, 1.0f);
        public static readonly Color ColorNeonViolet = new Color(0.85f, 0.70f, 1.0f, 1.0f);
        public static readonly Color ColorNeonMagenta = new Color(1.0f, 0.20f, 0.65f, 1.0f);
        public static readonly Color ColorTextWhite = new Color(0.98f, 0.99f, 1.0f, 1.0f);
        public static readonly Color ColorTextSilver = new Color(0.88f, 0.92f, 0.98f, 1.0f);
        public static readonly Color ColorTextMuted = new Color(0.78f, 0.86f, 0.95f, 0.95f);
        public static readonly Color ColorLedGreen = new Color(0.2f, 1.0f, 0.55f, 1.0f);

        private static Font cachedFont;

        public static Font GetDefaultFont()
        {
            if (cachedFont != null) return cachedFont;

            // 1. Try dedicated high-contrast TrueType font assets in Assets/Fonts/
            cachedFont = AssetDatabase.LoadAssetAtPath<Font>("Assets/Fonts/SegoeUI-Bold.ttf");
            if (cachedFont == null)
            {
                cachedFont = AssetDatabase.LoadAssetAtPath<Font>("Assets/Fonts/SegoeUI.ttf");
            }
            if (cachedFont == null)
            {
                cachedFont = AssetDatabase.LoadAssetAtPath<Font>("Assets/Fonts/Consolas.ttf");
            }

            // 2. Try OS dynamic font with crisp high-resolution base size
            if (cachedFont == null)
            {
                cachedFont = Font.CreateDynamicFontFromOSFont(new string[] { "Segoe UI Bold", "Segoe UI Semibold", "Segoe UI", "Consolas", "Arial" }, 32);
            }

            // 3. Fallbacks to Unity built-in fonts
            if (cachedFont == null)
            {
                cachedFont = Resources.GetBuiltinResource<Font>("Arial.ttf");
            }
            if (cachedFont == null)
            {
                cachedFont = Resources.GetBuiltinResource<Font>("LegacyRuntime.ttf");
            }

            return cachedFont;
        }

        private static Sprite GetSpriteAsset(string path)
        {
            return AssetDatabase.LoadAssetAtPath<Sprite>(path);
        }

        [UnityEditor.Callbacks.DidReloadScripts]
        [InitializeOnLoadMethod]
        private static void OnScriptsReloaded()
        {
            EditorApplication.delayCall += () =>
            {
                if (!EditorApplication.isPlayingOrWillChangePlaymode)
                {
                    GameObject existing = GameObject.Find(HUD_ROOT_NAME);
                    if (existing == null) existing = GameObject.Find("[" + HUD_ROOT_NAME + "]");
                    if (existing != null)
                    {
                        BuildOrRefreshHUD(false);
                        var activeScene = EditorSceneManager.GetActiveScene();
                        EditorSceneManager.SaveScene(activeScene);
                    }
                }
            };
        }

        public static GameObject BuildOrRefreshHUD(bool selectAfterBuild = true)
        {
            // Remove existing HUD root if present safely
            GameObject existing = GameObject.Find(HUD_ROOT_NAME);
            if (existing == null)
            {
                existing = GameObject.Find("[" + HUD_ROOT_NAME + "]");
            }
            if (existing != null)
            {
                if (Selection.activeGameObject != null &&
                    (Selection.activeGameObject == existing || Selection.activeTransform.IsChildOf(existing.transform)))
                {
                    Selection.activeObject = null;
                }
                Undo.DestroyObjectImmediate(existing);
            }

            // 1. Create Canvas Root in Screen Space - Overlay
            GameObject canvasGo = new GameObject(HUD_ROOT_NAME);
            canvasGo.layer = LayerMask.NameToLayer("UI");
            Undo.RegisterCreatedObjectUndo(canvasGo, "Create Z-Buffer HUD");

            Canvas canvas = canvasGo.AddComponent<Canvas>();
            canvas.renderMode = RenderMode.ScreenSpaceOverlay;
            canvas.sortingOrder = 100;
            canvas.pixelPerfect = true; // Crisp pixel alignment for UI text & borders

            CanvasScaler scaler = canvasGo.AddComponent<CanvasScaler>();
            scaler.uiScaleMode = CanvasScaler.ScaleMode.ScaleWithScreenSize;
            scaler.referenceResolution = new Vector2(1920, 1080);
            scaler.matchWidthOrHeight = 0.5f;
            scaler.dynamicPixelsPerUnit = 1.0f; // Direct 1:1 pixel rasterization at reference resolution

            canvasGo.AddComponent<GraphicRaycaster>();

            FuturisticHUDController hudController = canvasGo.AddComponent<FuturisticHUDController>();

            // Load permanent texture assets from Assets/Textures/ with procedural fallbacks
            Sprite panelSpriteCyan = GetSpriteAsset("Assets/Textures/Panel_CyanBorder.png");
            if (panelSpriteCyan == null)
            {
                panelSpriteCyan = CreatePanelSprite(64, 64, ColorBgDark, ColorBorderCyan, 2);
            }

            Sprite panelSpriteMagenta = GetSpriteAsset("Assets/Textures/Panel_MagentaBorder.png");
            if (panelSpriteMagenta == null)
            {
                panelSpriteMagenta = CreatePanelSprite(64, 64, ColorBgDark, ColorNeonMagenta, 2);
            }

            Sprite badgeSprite = GetSpriteAsset("Assets/Textures/Badge_CyanBorder.png");
            if (badgeSprite == null)
            {
                badgeSprite = CreatePanelSprite(32, 32, new Color(0.06f, 0.09f, 0.15f, 0.90f), ColorBorderCyan, 2);
            }

            Sprite bracketSpriteCyan = GetSpriteAsset("Assets/Textures/CornerBracket_Cyan.png");
            if (bracketSpriteCyan == null)
            {
                bracketSpriteCyan = CreateCornerBracketSprite(32, 2, ColorNeonCyan);
            }

            Sprite bracketSpriteMagenta = GetSpriteAsset("Assets/Textures/CornerBracket_Magenta.png");
            if (bracketSpriteMagenta == null)
            {
                bracketSpriteMagenta = CreateCornerBracketSprite(32, 2, ColorNeonMagenta);
            }

            Sprite circleSprite = CreateCircleSprite(32, ColorLedGreen);
            Sprite reticleSprite = CreateReticleSprite(180, 2);

            // 2. Build Top-Left Title Panel (Cyan Theme)
            BuildTopLeftPanel(canvasGo, hudController, panelSpriteCyan, circleSprite, bracketSpriteCyan);

            // 3. Build Top-Right Mode & Algorithm Panel (Cyan Theme)
            BuildTopRightPanel(canvasGo, hudController, panelSpriteCyan, badgeSprite, bracketSpriteCyan);

            // 4. Build Bottom-Left Performance Panel (Cyan Theme)
            BuildBottomLeftPanel(canvasGo, hudController, panelSpriteCyan, bracketSpriteCyan);

            // 5. Build Bottom-Right Controls Legend Panel (Magenta Accents Theme)
            BuildBottomRightPanel(canvasGo, hudController, panelSpriteMagenta, bracketSpriteMagenta);

            // 6. Build Center Targeting Reticle
            BuildCenterReticle(canvasGo, hudController, reticleSprite);

            // Connect runtime references and push immediate state
            hudController.FindReferencesIfNull();
            hudController.UpdateHUDState(true);

            var activeScene = EditorSceneManager.GetActiveScene();
            EditorSceneManager.MarkSceneDirty(activeScene);

            if (selectAfterBuild)
            {
                EditorApplication.delayCall += () =>
                {
                    if (canvasGo != null)
                    {
                        Selection.activeGameObject = canvasGo;
                        EditorGUIUtility.PingObject(canvasGo);
                    }
                };
            }

            Debug.Log("<color=#00f0ff><b>[Z-Buffer Demo]</b> Futuristic Sci-Fi HUD successfully refreshed with dark translucent panels and neon accents!</color>");
            return canvasGo;
        }

        private static void BuildTopLeftPanel(GameObject root, FuturisticHUDController ctrl, Sprite bg, Sprite led, Sprite bracket)
        {
            GameObject panel = CreateUIObject("Panel_TopLeft", root);
            RectTransform rt = panel.GetComponent<RectTransform>();
            SetAnchors(rt, new Vector2(0, 1), new Vector2(0, 1), new Vector2(0, 1), new Vector2(30, -25), new Vector2(540, 148));

            Image img = panel.AddComponent<Image>();
            img.sprite = bg;
            img.type = Image.Type.Sliced;
            img.color = bg != null ? Color.white : ColorBgDark;

            AddCornerBrackets(panel, bracket, ColorNeonCyan);

            // Title: Z-BUFFER // DEPTH MANAGEMENT SYSTEM
            Text tTitle = AddCrispText(panel, "Text_Title", "Z-BUFFER // DEPTH MANAGEMENT SYSTEM", 28, FontStyle.Bold, ColorNeonCyan, TextAnchor.MiddleLeft);
            RectTransform titleRt = tTitle.GetComponent<RectTransform>();
            SetAnchors(titleRt, new Vector2(0, 1), new Vector2(1, 1), new Vector2(0, 1), new Vector2(22, -16), new Vector2(-44, 38));
            ctrl.titleText = tTitle;

            // Subtitle
            Text tSub = AddCrispText(panel, "Text_Subtitle", "REAL-TIME OCCLUSION & DEPTH BUFFER VIEWER", 17, FontStyle.Bold, ColorTextSilver, TextAnchor.MiddleLeft);
            RectTransform subRt = tSub.GetComponent<RectTransform>();
            SetAnchors(subRt, new Vector2(0, 1), new Vector2(1, 1), new Vector2(0, 1), new Vector2(22, -60), new Vector2(-44, 28));
            ctrl.subtitleText = tSub;

            // Status Indicator Dot + Text
            GameObject dotGo = CreateUIObject("Image_StatusDot", panel);
            RectTransform dotRt = dotGo.GetComponent<RectTransform>();
            SetAnchors(dotRt, new Vector2(0, 1), new Vector2(0, 1), new Vector2(0.5f, 0.5f), new Vector2(30, -106), new Vector2(14, 14));
            Image imgDot = dotGo.AddComponent<Image>();
            imgDot.sprite = led;
            imgDot.color = ColorLedGreen;
            ctrl.rendererStatusDot = imgDot;

            Text tStat = AddCrispText(panel, "Text_Status", "RENDER PIPELINE: BUILT-IN (FORWARD) // ONLINE", 16, FontStyle.Bold, ColorLedGreen, TextAnchor.MiddleLeft);
            RectTransform statRt = tStat.GetComponent<RectTransform>();
            SetAnchors(statRt, new Vector2(0, 1), new Vector2(1, 1), new Vector2(0, 1), new Vector2(52, -99), new Vector2(-68, 28));
            ctrl.rendererStatusText = tStat;
        }

        private static void BuildTopRightPanel(GameObject root, FuturisticHUDController ctrl, Sprite bg, Sprite badgeBg, Sprite bracket)
        {
            GameObject panel = CreateUIObject("Panel_TopRight", root);
            RectTransform rt = panel.GetComponent<RectTransform>();
            SetAnchors(rt, new Vector2(1, 1), new Vector2(1, 1), new Vector2(1, 1), new Vector2(-30, -25), new Vector2(540, 155));

            Image img = panel.AddComponent<Image>();
            img.sprite = bg;
            img.type = Image.Type.Sliced;
            img.color = bg != null ? Color.white : ColorBgDark;

            AddCornerBrackets(panel, bracket, ColorNeonCyan);

            // Mode Badge
            GameObject badgeGo = CreateUIObject("Badge_Mode", panel);
            RectTransform badgeRt = badgeGo.GetComponent<RectTransform>();
            SetAnchors(badgeRt, new Vector2(0, 1), new Vector2(1, 1), new Vector2(0.5f, 1), new Vector2(0, -14), new Vector2(-36, 44));
            Image imgBadge = badgeGo.AddComponent<Image>();
            imgBadge.sprite = badgeBg;
            imgBadge.type = Image.Type.Sliced;
            imgBadge.color = badgeBg != null ? Color.white : new Color(0.06f, 0.09f, 0.15f, 0.90f);
            ctrl.modeBadgeBackground = imgBadge;

            Text tBadge = AddCrispText(badgeGo, "Text_Mode", "MODE: [NORMAL SHADING]", 20, FontStyle.Bold, ColorTextWhite, TextAnchor.MiddleCenter);
            RectTransform bTxtRt = tBadge.GetComponent<RectTransform>();
            SetAnchors(bTxtRt, Vector2.zero, Vector2.one, new Vector2(0.5f, 0.5f), Vector2.zero, Vector2.zero);
            ctrl.modeBadgeText = tBadge;

            // Algorithm
            Text tAlgo = AddCrispText(panel, "Text_Algorithm", "ALGORITHM: LINEAR EYE [0.5m - 65.0m]", 17, FontStyle.Bold, ColorNeonViolet, TextAnchor.MiddleLeft);
            RectTransform algoRt = tAlgo.GetComponent<RectTransform>();
            SetAnchors(algoRt, new Vector2(0, 1), new Vector2(1, 1), new Vector2(0, 1), new Vector2(22, -70), new Vector2(-44, 28));
            ctrl.algorithmText = tAlgo;

            // Gradient Status
            Text tGrad = AddCrispText(panel, "Text_Gradient", "GRADIENT: NEAR=WHITE / FAR=BLACK", 16, FontStyle.Bold, ColorTextSilver, TextAnchor.MiddleLeft);
            RectTransform gradRt = tGrad.GetComponent<RectTransform>();
            SetAnchors(gradRt, new Vector2(0, 1), new Vector2(1, 1), new Vector2(0, 1), new Vector2(22, -108), new Vector2(-44, 28));
            ctrl.gradientStatusText = tGrad;
        }

        private static void BuildBottomLeftPanel(GameObject root, FuturisticHUDController ctrl, Sprite bg, Sprite bracket)
        {
            GameObject panel = CreateUIObject("Panel_BottomLeft", root);
            RectTransform rt = panel.GetComponent<RectTransform>();
            SetAnchors(rt, new Vector2(0, 0), new Vector2(0, 0), new Vector2(0, 0), new Vector2(30, 25), new Vector2(550, 225));

            Image img = panel.AddComponent<Image>();
            img.sprite = bg;
            img.type = Image.Type.Sliced;
            img.color = bg != null ? Color.white : ColorBgDark;

            AddCornerBrackets(panel, bracket, ColorNeonCyan);

            // FPS / Perf
            Text tFps = AddCrispText(panel, "Text_FPS", "PERF // 60.0 FPS (16.6 ms)", 20, FontStyle.Bold, ColorNeonCyan, TextAnchor.MiddleLeft);
            RectTransform fpsRt = tFps.GetComponent<RectTransform>();
            SetAnchors(fpsRt, new Vector2(0, 1), new Vector2(1, 1), new Vector2(0, 1), new Vector2(22, -16), new Vector2(-44, 32));
            ctrl.fpsText = tFps;

            // Scene Entities
            Text tScene = AddCrispText(panel, "Text_SceneStatus", "ENTITIES: 54 // Z-BUFFER: 24-BIT // ORBIT: ON", 17, FontStyle.Bold, ColorTextSilver, TextAnchor.MiddleLeft);
            RectTransform scRt = tScene.GetComponent<RectTransform>();
            SetAnchors(scRt, new Vector2(0, 1), new Vector2(1, 1), new Vector2(0, 1), new Vector2(22, -56), new Vector2(-44, 28));
            ctrl.sceneStatusText = tScene;

            // Occlusion Demonstration Status
            Text tOcc = AddCrispText(panel, "Text_OcclusionStatus", "OCCLUSION TEST: [STANDBY] (PRESS 'O')", 17, FontStyle.Bold, ColorTextSilver, TextAnchor.MiddleLeft);
            RectTransform occRt = tOcc.GetComponent<RectTransform>();
            SetAnchors(occRt, new Vector2(0, 1), new Vector2(1, 1), new Vector2(0, 1), new Vector2(22, -92), new Vector2(-44, 28));
            ctrl.occlusionStatusText = tOcc;

            // Z-Fighting / Precision Lab Status
            Text tPrec = AddCrispText(panel, "Text_PrecisionStatus", "Z-FIGHT LAB: [STANDBY] (PRESS 'F')", 17, FontStyle.Bold, ColorTextSilver, TextAnchor.MiddleLeft);
            RectTransform precRt = tPrec.GetComponent<RectTransform>();
            SetAnchors(precRt, new Vector2(0, 1), new Vector2(1, 1), new Vector2(0, 1), new Vector2(22, -128), new Vector2(-44, 28));
            ctrl.precisionStatusText = tPrec;

            // Precision Math Note
            Text tNote = AddCrispText(panel, "Text_PrecisionNote", "PRECISION: NON-LINEAR HW-Z // Z_hw = f(z-n)/z(f-n)", 15, FontStyle.Bold, ColorTextMuted, TextAnchor.MiddleLeft);
            RectTransform noteRt = tNote.GetComponent<RectTransform>();
            SetAnchors(noteRt, new Vector2(0, 1), new Vector2(1, 1), new Vector2(0, 1), new Vector2(22, -164), new Vector2(-44, 26));
        }

        private static void BuildBottomRightPanel(GameObject root, FuturisticHUDController ctrl, Sprite bg, Sprite bracket)
        {
            GameObject panel = CreateUIObject("Panel_BottomRight", root);
            RectTransform rt = panel.GetComponent<RectTransform>();
            SetAnchors(rt, new Vector2(1, 0), new Vector2(1, 0), new Vector2(1, 0), new Vector2(-30, 25), new Vector2(550, 340));

            Image img = panel.AddComponent<Image>();
            img.sprite = bg;
            img.type = Image.Type.Sliced;
            img.color = bg != null ? Color.white : ColorBgDark;

            AddCornerBrackets(panel, bracket, ColorNeonMagenta);

            // Title
            Text tTitle = AddCrispText(panel, "Text_LegendTitle", "// SYSTEM CONTROLS", 20, FontStyle.Bold, ColorNeonMagenta, TextAnchor.MiddleLeft);
            RectTransform tRt = tTitle.GetComponent<RectTransform>();
            SetAnchors(tRt, new Vector2(0, 1), new Vector2(1, 1), new Vector2(0, 1), new Vector2(22, -16), new Vector2(-44, 30));

            string[] keys = new string[]
            {
                "[Z]  TOGGLE DEPTH VISUALIZATION",
                "[X]  INVERT DEPTH GRADIENT",
                "[C]  CYCLE DEPTH ALGORITHM",
                "[P]  TOGGLE CAMERA ORBIT",
                "[R]  RESET CAMERA PERSPECTIVE",
                "[O]  TOGGLE OCCLUSION RIG",
                "[F]  TOGGLE Z-FIGHT RIG",
                "[G]  TOGGLE COINCIDENT / CORRECTED"
            };

            for (int i = 0; i < keys.Length; i++)
            {
                Color keyColor;
                if (i == 0 || i == 6)
                {
                    keyColor = ColorNeonCyan;
                }
                else if (i == 7)
                {
                    keyColor = ColorNeonMagenta;
                }
                else
                {
                    keyColor = ColorTextWhite;
                }

                Text tKey = AddCrispText(panel, $"Key_{i}", keys[i], 17, FontStyle.Bold, keyColor, TextAnchor.MiddleLeft);
                RectTransform kRt = tKey.GetComponent<RectTransform>();
                SetAnchors(kRt, new Vector2(0, 1), new Vector2(1, 1), new Vector2(0, 1), new Vector2(22, -54 - (i * 32)), new Vector2(-44, 28));
            }
        }

        private static void BuildCenterReticle(GameObject root, FuturisticHUDController ctrl, Sprite reticleRing)
        {
            GameObject centerGo = CreateUIObject("Center_Reticle_Group", root);
            RectTransform rt = centerGo.GetComponent<RectTransform>();
            SetAnchors(rt, new Vector2(0.5f, 0.5f), new Vector2(0.5f, 0.5f), new Vector2(0.5f, 0.5f), Vector2.zero, new Vector2(180, 180));

            // Rotating Reticle Ring
            GameObject ringGo = CreateUIObject("Rotating_Ring", centerGo);
            RectTransform ringRt = ringGo.GetComponent<RectTransform>();
            SetAnchors(ringRt, Vector2.zero, Vector2.one, new Vector2(0.5f, 0.5f), Vector2.zero, Vector2.zero);
            Image imgRing = ringGo.AddComponent<Image>();
            imgRing.sprite = reticleRing;
            imgRing.color = new Color(0.0f, 0.95f, 1.0f, 0.65f);
            ctrl.rotatingReticleRing = ringRt;

            // Reticle Target Text below ring
            Text tRet = AddCrispText(centerGo, "Text_ReticleStatus", "TARGET LOCK // VALKYRIE-Z1", 16, FontStyle.Bold, new Color(0.0f, 1.0f, 1.0f, 0.95f), TextAnchor.MiddleCenter);
            RectTransform txtRt = tRet.GetComponent<RectTransform>();
            SetAnchors(txtRt, new Vector2(0.5f, 0), new Vector2(0.5f, 0), new Vector2(0.5f, 1), new Vector2(0, -26), new Vector2(380, 32));
            ctrl.reticleStatusText = tRet;
        }

        private static Text AddCrispText(GameObject parent, string name, string content, int fontSize, FontStyle fontStyle, Color color, TextAnchor alignment)
        {
            GameObject go = CreateUIObject(name, parent);
            Text t = go.AddComponent<Text>();
            t.font = GetDefaultFont();
            t.text = content;
            t.fontSize = fontSize;
            t.fontStyle = fontStyle;
            t.color = color;
            t.alignment = alignment;
            t.horizontalOverflow = HorizontalWrapMode.Overflow;
            t.verticalOverflow = VerticalWrapMode.Overflow;
            t.supportRichText = true;
            t.raycastTarget = false;
            t.resizeTextForBestFit = false;

            Shadow shadow = go.AddComponent<Shadow>();
            shadow.effectColor = new Color(0.01f, 0.02f, 0.05f, 0.95f);
            shadow.effectDistance = new Vector2(1.5f, -1.5f);
            shadow.useGraphicAlpha = true;

            return t;
        }

        private static void AddCornerBrackets(GameObject parent, Sprite bracketSprite, Color bracketColor)
        {
            float bSize = 16f;
            Vector2[] corners = new Vector2[]
            {
                new Vector2(0, 1), // Top-Left
                new Vector2(1, 1), // Top-Right
                new Vector2(0, 0), // Bottom-Left
                new Vector2(1, 0)  // Bottom-Right
            };

            Vector2[] pivots = new Vector2[]
            {
                new Vector2(0, 1),
                new Vector2(1, 1),
                new Vector2(0, 0),
                new Vector2(1, 0)
            };

            float[] rotZ = new float[] { 0f, -90f, 90f, 180f };

            for (int i = 0; i < 4; i++)
            {
                GameObject bGo = CreateUIObject($"CornerBracket_{i}", parent);
                RectTransform bRt = bGo.GetComponent<RectTransform>();
                bRt.anchorMin = corners[i];
                bRt.anchorMax = corners[i];
                bRt.pivot = pivots[i];
                bRt.sizeDelta = new Vector2(bSize, bSize);
                bRt.anchoredPosition = Vector2.zero;
                bRt.localRotation = Quaternion.Euler(0, 0, rotZ[i]);

                Image bImg = bGo.AddComponent<Image>();
                bImg.sprite = bracketSprite;
                bImg.color = bracketColor;
                bImg.raycastTarget = false;
            }
        }

        private static GameObject CreateUIObject(string name, GameObject parent)
        {
            GameObject go = new GameObject(name);
            go.layer = LayerMask.NameToLayer("UI");
            go.transform.SetParent(parent.transform, false);
            go.AddComponent<RectTransform>();
            return go;
        }

        private static void SetAnchors(RectTransform rt, Vector2 min, Vector2 max, Vector2 pivot, Vector2 pos, Vector2 size)
        {
            rt.anchorMin = min;
            rt.anchorMax = max;
            rt.pivot = pivot;
            rt.anchoredPosition = pos;
            rt.sizeDelta = size;
        }

        // --- Procedural Fallback Sprite Generators ---

        public static Sprite CreatePanelSprite(int width, int height, Color bgColor, Color borderColor, int borderWidth)
        {
            Texture2D tex = new Texture2D(width, height, TextureFormat.RGBA32, false);
            tex.filterMode = FilterMode.Bilinear;
            tex.wrapMode = TextureWrapMode.Clamp;

            Color[] pixels = new Color[width * height];
            for (int y = 0; y < height; y++)
            {
                for (int x = 0; x < width; x++)
                {
                    bool isBorder = (x < borderWidth || x >= width - borderWidth || y < borderWidth || y >= height - borderWidth);
                    pixels[y * width + x] = isBorder ? borderColor : bgColor;
                }
            }
            tex.SetPixels(pixels);
            tex.Apply();

            return Sprite.Create(tex, new Rect(0, 0, width, height), new Vector2(0.5f, 0.5f), 100f, 0, SpriteMeshType.FullRect, new Vector4(borderWidth + 4, borderWidth + 4, borderWidth + 4, borderWidth + 4));
        }

        public static Sprite CreateCircleSprite(int size, Color color)
        {
            Texture2D tex = new Texture2D(size, size, TextureFormat.RGBA32, false);
            tex.filterMode = FilterMode.Bilinear;
            tex.wrapMode = TextureWrapMode.Clamp;

            float radius = (size - 2) * 0.5f;
            Vector2 center = new Vector2(size * 0.5f, size * 0.5f);
            Color[] pixels = new Color[size * size];

            for (int y = 0; y < size; y++)
            {
                for (int x = 0; x < size; x++)
                {
                    float dist = Vector2.Distance(new Vector2(x, y), center);
                    float alpha = Mathf.Clamp01((radius - dist) + 0.5f);
                    pixels[y * size + x] = new Color(color.r, color.g, color.b, color.a * alpha);
                }
            }
            tex.SetPixels(pixels);
            tex.Apply();

            return Sprite.Create(tex, new Rect(0, 0, size, size), new Vector2(0.5f, 0.5f));
        }

        public static Sprite CreateReticleSprite(int size, int thickness)
        {
            Texture2D tex = new Texture2D(size, size, TextureFormat.RGBA32, false);
            tex.filterMode = FilterMode.Bilinear;
            tex.wrapMode = TextureWrapMode.Clamp;

            float outerRadius = (size - 2) * 0.5f;
            float innerRadius = outerRadius - thickness;
            Vector2 center = new Vector2(size * 0.5f, size * 0.5f);
            Color[] pixels = new Color[size * size];

            for (int y = 0; y < size; y++)
            {
                for (int x = 0; x < size; x++)
                {
                    Vector2 pos = new Vector2(x, y);
                    float dist = Vector2.Distance(pos, center);

                    float ringAlpha = (dist >= innerRadius && dist <= outerRadius) ? 1f : 0f;

                    float angleDeg = (Mathf.Atan2(pos.y - center.y, pos.x - center.x) * Mathf.Rad2Deg + 360f) % 360f;
                    bool inGap = (angleDeg > 40 && angleDeg < 50) ||
                                 (angleDeg > 130 && angleDeg < 140) ||
                                 (angleDeg > 220 && angleDeg < 230) ||
                                 (angleDeg > 310 && angleDeg < 320);

                    if (inGap) ringAlpha = 0f;

                    bool isTick = (dist >= innerRadius - 4 && dist <= outerRadius + 4) &&
                                  ((Mathf.Abs(pos.x - center.x) < 1.5f && Mathf.Abs(pos.y - center.y) > innerRadius) ||
                                   (Mathf.Abs(pos.y - center.y) < 1.5f && Mathf.Abs(pos.x - center.x) > innerRadius));

                    float finalAlpha = Mathf.Max(ringAlpha, isTick ? 1f : 0f);
                    pixels[y * size + x] = new Color(1f, 1f, 1f, finalAlpha);
                }
            }
            tex.SetPixels(pixels);
            tex.Apply();

            return Sprite.Create(tex, new Rect(0, 0, size, size), new Vector2(0.5f, 0.5f));
        }

        public static Sprite CreateCornerBracketSprite(int size, int thickness, Color color)
        {
            Texture2D tex = new Texture2D(size, size, TextureFormat.RGBA32, false);
            tex.filterMode = FilterMode.Point;
            tex.wrapMode = TextureWrapMode.Clamp;

            Color[] pixels = new Color[size * size];
            for (int y = 0; y < size; y++)
            {
                for (int x = 0; x < size; x++)
                {
                    bool isL = (y >= size - thickness) || (x < thickness);
                    pixels[y * size + x] = isL ? color : Color.clear;
                }
            }
            tex.SetPixels(pixels);
            tex.Apply();

            return Sprite.Create(tex, new Rect(0, 0, size, size), new Vector2(0, 1));
        }
    }
}
