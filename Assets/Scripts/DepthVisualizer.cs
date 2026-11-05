using UnityEngine;

namespace ZBufferDemo
{
    /// <summary>
    /// Controls camera depth texture generation and real-time fullscreen depth buffer visualization.
    /// Uses Unity's Built-in Render Pipeline DepthTextureMode and OnRenderImage post-processing blit.
    /// Toggle with keyboard shortcut 'Z'.
    /// </summary>
    [RequireComponent(typeof(Camera))]
    [ExecuteInEditMode]
    public class DepthVisualizer : MonoBehaviour
    {
        public enum DepthMode
        {
            [Tooltip("Eye-space linear distance remapped to [NearDisplayDistance, FarDisplayDistance]. Best contrast.")]
            LinearEyeRange = 0,

            [Tooltip("Normalized [0, 1] depth across the camera's entire near/far clip plane.")]
            Linear01 = 1,

            [Tooltip("Raw non-linear hyperbolic hardware Z-buffer values.")]
            RawHardwareZ = 2
        }

        [Header("Visualization Toggle")]
        [Tooltip("Toggle real-time depth buffer visualization. Hotkey: 'Z'")]
        [SerializeField]
        private bool isDepthActive = false;

        [Header("Contrast & Tuning")]
        [Tooltip("Eye-space distance where objects start having minimum display intensity.")]
        public float nearDisplayDistance = 0.5f;

        [Tooltip("Eye-space distance representing the far threshold (e.g. background orbital station).")]
        public float farDisplayDistance = 65.0f;

        [Tooltip("If true, near objects are bright/white and far objects are dark/black.")]
        public bool invertDepth = true;

        [Tooltip("Algorithm for mapping depth values to screen intensity.")]
        public DepthMode depthMode = DepthMode.LinearEyeRange;

        [Range(0.2f, 3.0f)]
        [Tooltip("Contrast curve exponent (1.0 = linear).")]
        public float contrast = 1.0f;

        [Header("Shader Reference")]
        [Tooltip("Shader used for the fullscreen depth blit.")]
        [SerializeField]
        private Shader depthShader;

        private Material depthMaterial;
        private Camera targetCamera;
        private DepthTextureMode originalDepthMode;

        public bool IsDepthActive
        {
            get => isDepthActive;
            set => SetDepthActive(value);
        }

        private void Awake()
        {
            targetCamera = GetComponent<Camera>();
        }

        private void OnEnable()
        {
            if (targetCamera == null)
            {
                targetCamera = GetComponent<Camera>();
            }

            if (targetCamera != null)
            {
                originalDepthMode = targetCamera.depthTextureMode;
                // Enable depth texture generation in Built-in Render Pipeline
                targetCamera.depthTextureMode |= DepthTextureMode.Depth;
            }

            InitializeMaterial();
        }

        private void OnDisable()
        {
            if (targetCamera != null)
            {
                // Restore camera depth texture mode
                targetCamera.depthTextureMode = originalDepthMode;
            }

            CleanupMaterial();
        }

        private void OnDestroy()
        {
            CleanupMaterial();
        }

        private void InitializeMaterial()
        {
            if (depthShader == null)
            {
                depthShader = Shader.Find("ZBuffer/DepthVisualization");
            }

            if (depthShader == null)
            {
                Debug.LogError("[Z-Buffer] DepthVisualization shader ('ZBuffer/DepthVisualization') not found! Please ensure DepthVisualization.shader is present in Assets/Shaders/.");
                return;
            }

            if (!depthShader.isSupported)
            {
                Debug.LogError("[Z-Buffer] DepthVisualization shader is not supported on this graphics device/platform!");
                return;
            }

            if (depthMaterial == null)
            {
                depthMaterial = new Material(depthShader)
                {
                    name = "Mat_DepthVisualizer_RuntimeInstance",
                    hideFlags = HideFlags.DontSave
                };
            }
        }

        private void CleanupMaterial()
        {
            if (depthMaterial != null)
            {
                if (Application.isPlaying)
                {
                    Destroy(depthMaterial);
                }
                else
                {
                    DestroyImmediate(depthMaterial);
                }
                depthMaterial = null;
            }
        }

        private void Update()
        {
            // Only process input if application is focused (or in editor)
            if (!Application.isFocused && Application.isPlaying)
            {
                return;
            }

            // Keyboard shortcut 'Z' to toggle Depth Visualization
            if (Input.GetKeyDown(KeyCode.Z))
            {
                ToggleDepth();
            }

            // Keyboard shortcut 'X' to toggle near-white / near-black inversion
            if (Input.GetKeyDown(KeyCode.X))
            {
                invertDepth = !invertDepth;
                Debug.Log($"<color=#00f0ff>[Z-Buffer]</color> Depth Inversion: {(invertDepth ? "Near=Bright / Far=Dark (White=Close)" : "Near=Dark / Far=Bright (Black=Close)")}");
            }

            // Keyboard shortcut 'C' to cycle depth calculation mode
            if (Input.GetKeyDown(KeyCode.C))
            {
                depthMode = (DepthMode)(((int)depthMode + 1) % 3);
                Debug.Log($"<color=#00f0ff>[Z-Buffer]</color> Depth Mode switched to: <b>{depthMode}</b>");
            }
        }

        /// <summary>
        /// Toggles between normal scene rendering and depth visualization mode.
        /// </summary>
        public void ToggleDepth()
        {
            SetDepthActive(!isDepthActive);
        }

        /// <summary>
        /// Explicitly sets the depth visualization state.
        /// </summary>
        public void SetDepthActive(bool active)
        {
            isDepthActive = active;

            if (isDepthActive)
            {
                if (targetCamera != null)
                {
                    targetCamera.depthTextureMode |= DepthTextureMode.Depth;
                }
                InitializeMaterial();
                Debug.Log("<color=#00f0ff><b>[Z-BUFFER DEPTH MODE: ON]</b></color> Real-time camera depth visualization activated.\n" +
                          $"Mode: {depthMode} | Range: [{nearDisplayDistance:F1}m - {farDisplayDistance:F1}m] | Invert: {invertDepth}\n" +
                          "Hotkeys: [Z] Toggle Normal/Depth | [X] Invert Intensity | [C] Cycle Depth Mode (Eye/0-1/Hardware Z)");
            }
            else
            {
                Debug.Log("<color=#ffff00><b>[Z-BUFFER DEPTH MODE: OFF]</b></color> Restored normal standard scene rendering.");
            }
        }

        /// <summary>
        /// Built-in Render Pipeline post-processing callback. Blits depth texture to destination when active.
        /// </summary>
        private void OnRenderImage(RenderTexture source, RenderTexture destination)
        {
            if (isDepthActive && depthMaterial != null)
            {
                // Update shader uniforms
                depthMaterial.SetFloat("_DepthRangeNear", nearDisplayDistance);
                depthMaterial.SetFloat("_DepthRangeFar", farDisplayDistance);
                depthMaterial.SetFloat("_InvertDepth", invertDepth ? 1.0f : 0.0f);
                depthMaterial.SetFloat("_DepthMode", (float)depthMode);
                depthMaterial.SetFloat("_Contrast", Mathf.Max(contrast, 0.01f));

                Graphics.Blit(source, destination, depthMaterial);
            }
            else
            {
                // Normal standard rendering pass-through
                Graphics.Blit(source, destination);
            }
        }
    }
}
