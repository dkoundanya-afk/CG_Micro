using UnityEngine;
using UnityEngine.UI;

namespace ZBufferDemo
{
    /// <summary>
    /// Runtime controller for the Futuristic Sci-Fi HUD.
    /// Dynamically reflects DepthVisualizer state, FPS, camera orbit status, and reticle animations.
    /// Operates in Screen Space - Overlay so it remains vibrant and unaffected by the depth grayscale shader.
    /// </summary>
    public class FuturisticHUDController : MonoBehaviour
    {
        [Header("System References")]
        public DepthVisualizer depthVisualizer;
        public OrbitCamera orbitCamera;

        [Header("Occlusion Demonstration")]
        public OcclusionDemoController occlusionController;
        public Text occlusionStatusText;

        [Header("Depth Precision / Z-Fighting Lab")]
        public DepthPrecisionDemoController precisionController;
        public Text precisionStatusText;

        [Header("Top-Left Header Elements")]
        public Text titleText;
        public Text subtitleText;
        public Text rendererStatusText;
        public Image rendererStatusDot;

        [Header("Top-Right Mode Elements")]
        public Text modeBadgeText;
        public Image modeBadgeBackground;
        public Text algorithmText;
        public Text gradientStatusText;

        [Header("Bottom-Left Performance Elements")]
        public Text fpsText;
        public Text sceneStatusText;

        [Header("Center Reticle Elements")]
        public RectTransform rotatingReticleRing;
        public Text reticleStatusText;
        public float reticleRotationSpeed = 15f;

        [Header("Pulsing Indicators")]
        public Image[] pulsingDots;

        [Header("Settings")]
        public float fpsUpdateInterval = 0.25f;

        // Performance tracking
        private float fpsTimer = 0f;
        private int frameCount = 0;
        private float currentFps = 60f;
        private float currentFrameTimeMs = 16.6f;

        // Cached state tracking
        private bool lastDepthState = false;
        private DepthVisualizer.DepthMode lastDepthMode = (DepthVisualizer.DepthMode)(-1);
        private bool lastInvertState = false;
        private bool lastOrbitState = false;
        private bool lastOcclusionState = false;
        private bool lastPrecisionActive = true;
        private bool lastCoincidentState = true;
        private int cachedObjectCount = 0;

        private void Start()
        {
            EnsureHUDAppearance();
            EnsureTextRenderingQuality();
            FindReferencesIfNull();
            CountSceneObjects();
            UpdateHUDState(true);
        }

        private void OnEnable()
        {
            EnsureHUDAppearance();
            EnsureTextRenderingQuality();
            FindReferencesIfNull();
            UpdateHUDState(true);
        }

        private void EnsureHUDAppearance()
        {
            Color darkBg = new Color(0.015f, 0.03f, 0.07f, 0.92f);
            Transform[] panels = new Transform[]
            {
                transform.Find("Panel_TopLeft"),
                transform.Find("Panel_TopRight"),
                transform.Find("Panel_BottomLeft"),
                transform.Find("Panel_BottomRight")
            };

            foreach (Transform p in panels)
            {
                if (p != null)
                {
                    Image img = p.GetComponent<Image>();
                    if (img != null)
                    {
                        // If sprite is missing or white default, guarantee dark translucent background
                        if (img.sprite == null || img.sprite.name == "Background" || img.sprite.name == "UISprite")
                        {
                            img.color = darkBg;
                        }
                    }
                }
            }
        }

        private void EnsureTextRenderingQuality()
        {
            Text[] allTexts = GetComponentsInChildren<Text>(true);
            foreach (Text t in allTexts)
            {
                if (t != null)
                {
                    t.horizontalOverflow = HorizontalWrapMode.Overflow;
                    t.verticalOverflow = VerticalWrapMode.Overflow;

                    // Ensure high-contrast drop shadow
                    if (t.GetComponent<Shadow>() == null)
                    {
                        Shadow shadow = t.gameObject.AddComponent<Shadow>();
                        shadow.effectColor = new Color(0.01f, 0.02f, 0.05f, 0.95f);
                        shadow.effectDistance = new Vector2(1.5f, -1.5f);
                        shadow.useGraphicAlpha = true;
                    }
                }
            }
        }

        public void FindReferencesIfNull()
        {
            if (depthVisualizer == null)
            {
                depthVisualizer = FindFirstObjectByType<DepthVisualizer>();
                if (depthVisualizer == null && Camera.main != null)
                {
                    depthVisualizer = Camera.main.GetComponent<DepthVisualizer>();
                }
            }

            if (orbitCamera == null)
            {
                orbitCamera = FindFirstObjectByType<OrbitCamera>();
                if (orbitCamera == null && Camera.main != null)
                {
                    orbitCamera = Camera.main.GetComponent<OrbitCamera>();
                }
            }

            if (occlusionController == null)
            {
                occlusionController = FindFirstObjectByType<OcclusionDemoController>();
            }

            if (precisionController == null)
            {
                precisionController = FindFirstObjectByType<DepthPrecisionDemoController>();
            }
        }

        private void CountSceneObjects()
        {
            GameObject sceneRoot = GameObject.Find(ZBufferSceneBuilder.ROOT_NAME);
            if (sceneRoot != null)
            {
                Renderer[] rends = sceneRoot.GetComponentsInChildren<Renderer>(true);
                cachedObjectCount = rends.Length;
            }
            else
            {
                Renderer[] allRends = FindObjectsByType<Renderer>(FindObjectsSortMode.None);
                cachedObjectCount = allRends.Length;
            }
        }

        private void Update()
        {
            UpdateFPS();
            UpdateReticleAnimation();
            UpdatePulseAnimation();
            CheckStateChanges();
        }

        private void UpdateFPS()
        {
            frameCount++;
            fpsTimer += Time.unscaledDeltaTime;

            if (fpsTimer >= fpsUpdateInterval)
            {
                currentFps = frameCount / fpsTimer;
                currentFrameTimeMs = (fpsTimer / frameCount) * 1000f;

                if (fpsText != null)
                {
                    fpsText.text = $"PERF // {currentFps:F1} FPS ({currentFrameTimeMs:F1} ms)";
                }

                frameCount = 0;
                fpsTimer = 0f;
            }
        }

        private void UpdateReticleAnimation()
        {
            if (rotatingReticleRing != null)
            {
                rotatingReticleRing.Rotate(0f, 0f, -reticleRotationSpeed * Time.deltaTime);
            }
        }

        private void UpdatePulseAnimation()
        {
            float pulse = 0.6f + 0.4f * Mathf.Sin(Time.time * 4f);

            if (rendererStatusDot != null)
            {
                Color c = rendererStatusDot.color;
                c.a = pulse;
                rendererStatusDot.color = c;
            }

            if (pulsingDots != null)
            {
                for (int i = 0; i < pulsingDots.Length; i++)
                {
                    if (pulsingDots[i] != null)
                    {
                        Color dc = pulsingDots[i].color;
                        dc.a = pulse;
                        pulsingDots[i].color = dc;
                    }
                }
            }
        }

        private void CheckStateChanges()
        {
            if (depthVisualizer == null) return;

            bool depthActive = depthVisualizer.IsDepthActive;
            DepthVisualizer.DepthMode dMode = depthVisualizer.depthMode;
            bool invert = depthVisualizer.invertDepth;
            bool orbitActive = orbitCamera != null && orbitCamera.autoRotate;
            bool occActive = occlusionController != null && occlusionController.IsDemoActive;
            bool precActive = precisionController != null && precisionController.IsDemoActive;
            bool isCoincident = precisionController != null && precisionController.IsCoincident;

            if (depthActive != lastDepthState || dMode != lastDepthMode || invert != lastInvertState || orbitActive != lastOrbitState || occActive != lastOcclusionState || precActive != lastPrecisionActive || isCoincident != lastCoincidentState)
            {
                lastDepthState = depthActive;
                lastDepthMode = dMode;
                lastInvertState = invert;
                lastOrbitState = orbitActive;
                lastOcclusionState = occActive;
                lastPrecisionActive = precActive;
                lastCoincidentState = isCoincident;
                UpdateHUDState(false);
            }
        }

        public void UpdateHUDState(bool force)
        {
            if (depthVisualizer == null || occlusionController == null || precisionController == null)
            {
                FindReferencesIfNull();
            }

            bool depthActive = depthVisualizer != null && depthVisualizer.IsDepthActive;
            bool occActive = occlusionController != null && occlusionController.IsDemoActive;
            bool precActive = precisionController != null && precisionController.IsDemoActive;
            bool isCoincident = precisionController != null && precisionController.IsCoincident;

            // 1. Top-Right Mode Badge
            if (modeBadgeText != null)
            {
                if (depthActive)
                {
                    modeBadgeText.text = "MODE: [DEPTH ANALYSIS]";
                    modeBadgeText.color = new Color(0.0f, 1.0f, 0.90f); // Neon Cyan-Green
                }
                else
                {
                    modeBadgeText.text = "MODE: [NORMAL SHADING]";
                    modeBadgeText.color = new Color(0.98f, 0.99f, 1.0f); // Bright Crisp White
                }
            }

            if (modeBadgeBackground != null)
            {
                modeBadgeBackground.color = depthActive
                    ? new Color(0.0f, 0.40f, 0.50f, 0.90f)  // Cyan glow background
                    : new Color(0.08f, 0.12f, 0.20f, 0.90f); // Dark slate background
            }

            // 2. Algorithm & Gradient Information
            if (algorithmText != null && depthVisualizer != null)
            {
                string algoDesc = "";
                switch (depthVisualizer.depthMode)
                {
                    case DepthVisualizer.DepthMode.LinearEyeRange:
                        algoDesc = $"ALGO: LINEAR EYE [{depthVisualizer.nearDisplayDistance:F1}m - {depthVisualizer.farDisplayDistance:F1}m]";
                        break;
                    case DepthVisualizer.DepthMode.Linear01:
                        algoDesc = "ALGO: LINEAR 0-1 (FULL FRUSTUM)";
                        break;
                    case DepthVisualizer.DepthMode.RawHardwareZ:
                        algoDesc = "ALGO: RAW HARDWARE Z (NON-LINEAR)";
                        break;
                }
                algorithmText.text = algoDesc;
            }

            if (gradientStatusText != null && depthVisualizer != null)
            {
                gradientStatusText.text = depthVisualizer.invertDepth
                    ? "GRADIENT: NEAR=WHITE / FAR=BLACK"
                    : "GRADIENT: NEAR=BLACK / FAR=WHITE";
            }

            // 3. Occlusion Demonstration Status
            if (occlusionStatusText != null)
            {
                occlusionStatusText.text = occActive
                    ? "OCCLUSION TEST: [ACTIVE // OVERLAP RIG]"
                    : "OCCLUSION TEST: [STANDBY] (PRESS 'O')";
                occlusionStatusText.color = occActive
                    ? new Color(0.0f, 1.0f, 0.90f, 1.0f)
                    : new Color(0.88f, 0.92f, 0.98f, 1.0f);
            }

            // 4. Depth Precision / Z-Fighting Lab Status
            if (precisionStatusText != null)
            {
                if (!precActive)
                {
                    precisionStatusText.text = "Z-FIGHT LAB: [STANDBY] (PRESS 'F')";
                    precisionStatusText.color = new Color(0.88f, 0.92f, 0.98f, 1.0f);
                }
                else if (isCoincident)
                {
                    precisionStatusText.text = "Z-FIGHT LAB: [ACTIVE] // COINCIDENT (FLICKER)";
                    precisionStatusText.color = new Color(1.0f, 0.20f, 0.55f, 1.0f); // Neon warning red/magenta
                }
                else
                {
                    precisionStatusText.text = "Z-FIGHT LAB: [ACTIVE] // CORRECTED (0.08m OFFSET)";
                    precisionStatusText.color = new Color(0.2f, 1.0f, 0.55f, 1.0f); // Neon green
                }
            }

            // 5. Scene Status & Orbit Info
            if (sceneStatusText != null)
            {
                string orbitStr = (orbitCamera != null && orbitCamera.autoRotate) ? "ORBIT: ON" : "ORBIT: PAUSED";
                sceneStatusText.text = $"ENTITIES: {cachedObjectCount} // Z-BUFFER: 24-BIT // {orbitStr}";
            }

            // 6. Center Reticle Status
            if (reticleStatusText != null)
            {
                if (depthActive)
                {
                    reticleStatusText.text = "DEPTH BUFFER ANALYSIS // SCANNING";
                }
                else if (precActive)
                {
                    reticleStatusText.text = isCoincident ? "TARGET: Z-FIGHTING INTERFERENCE ZONE" : "TARGET: PRECISION SEPARATION VERIFIED";
                }
                else if (occActive)
                {
                    reticleStatusText.text = "TARGET: OCCLUSION CALIBRATION RIG";
                }
                else
                {
                    reticleStatusText.text = "TARGET LOCK // VALKYRIE-Z1";
                }

                reticleStatusText.color = depthActive
                    ? new Color(0.0f, 1.0f, 1.0f, 1.0f)
                    : new Color(0.88f, 0.95f, 1.0f, 1.0f);
            }
        }
    }
}
