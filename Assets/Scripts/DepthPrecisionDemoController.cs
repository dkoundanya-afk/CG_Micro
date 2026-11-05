using UnityEngine;

namespace ZBufferDemo
{
    /// <summary>
    /// Interactive controller for the Phase 5 Z-Fighting and Depth Precision Laboratory.
    /// Demonstrates hardware Z-buffer precision limits and geometric surface interference.
    /// 
    /// SCIENTIFIC BACKGROUND:
    /// 1. Z-FIGHTING (DEPTH INTERFERENCE):
    ///    When two opaque polygon surfaces occupy nearly identical depths in camera space, the finite
    ///    precision of the hardware depth buffer (typically 24-bit fixed/float) cannot reliably determine
    ///    which surface is in front. Floating-point rounding errors during perspective projection matrix
    ///    multiplication and sub-pixel rasterization cause adjacent pixels to alternate winning the depth
    ///    test ('LEqual'). This manifests as visual flickering, moire banding, and stitching.
    ///    NOTE: The exact flickering pattern is hardware-dependent (varies with GPU vendor, driver,
    ///    depth buffer format, and whether reversed-Z floating point is employed by the graphics API).
    /// 
    /// 2. NON-LINEAR HARDWARE Z VS LINEAR EYE DEPTH:
    ///    Standard perspective projection maps depth non-linearly: Z_hw = (f * (z - n)) / (z * (f - n)).
    ///    Consequently, the vast majority of numerical precision is concentrated near the camera near plane,
    ///    leaving relatively few depth values for distant objects. In contrast, Linear Eye Depth
    ///    represents true physical distance in world units (meters).
    /// </summary>
    public class DepthPrecisionDemoController : MonoBehaviour
    {
        [Header("Demonstration State")]
        [Tooltip("Toggle Z-fighting laboratory rig visibility. Hotkey: 'F'")]
        [SerializeField]
        private bool isDemoActive = true;

        [Tooltip("If true, surfaces are virtually coincident (Z-fighting active). If false, surfaces have safe separation (corrected). Hotkey: 'G'")]
        [SerializeField]
        private bool isCoincident = true;

        [Header("Precision Parameters")]
        [Tooltip("Separation distance in meters when in coincident/fighting mode.")]
        public float coincidentOffset = 0.0002f;

        [Tooltip("Separation distance in meters when in corrected mode (eliminates Z-fighting).")]
        public float correctedOffset = 0.08f;

        [Header("Rig Transforms")]
        public GameObject rigContainer;
        public Transform surfaceA;
        public Transform surfaceB;

        private Vector3 surfaceBBaseLocalPos;
        private OrbitCamera orbitCamera;

        public bool IsDemoActive
        {
            get => isDemoActive;
            set => SetDemoActive(value);
        }

        public bool IsCoincident
        {
            get => isCoincident;
            set => SetCoincident(value);
        }

        public float CurrentSeparation => isCoincident ? coincidentOffset : correctedOffset;

        private void Awake()
        {
            if (surfaceB != null)
            {
                surfaceBBaseLocalPos = surfaceB.localPosition;
            }
        }

        private void Start()
        {
            if (surfaceB != null && surfaceBBaseLocalPos == Vector3.zero)
            {
                surfaceBBaseLocalPos = surfaceB.localPosition;
            }

            orbitCamera = FindFirstObjectByType<OrbitCamera>();
            ApplyState();
        }

        private void Update()
        {
            // Only process input if application is focused (or in editor)
            if (!Application.isPlaying || Application.isFocused)
            {
                // Hotkey 'F' to toggle Phase 5 Rig Active/Standby
                if (Input.GetKeyDown(KeyCode.F))
                {
                    ToggleDemoActive();
                }

                // Hotkey 'G' to toggle between Coincident (Flicker) and Corrected (Separated)
                if (Input.GetKeyDown(KeyCode.G))
                {
                    ToggleCorrectionMode();
                }
            }
        }

        public void ToggleDemoActive()
        {
            SetDemoActive(!isDemoActive);
        }

        public void ToggleCorrectionMode()
        {
            SetCoincident(!isCoincident);
        }

        public void SetDemoActive(bool active)
        {
            isDemoActive = active;
            ApplyState();

            if (isDemoActive)
            {
                Debug.Log("<color=#00f0ff><b>[Z-BUFFER PRECISION LAB: ACTIVE]</b></color>\n" +
                          $"• State: {(isCoincident ? "<color=#ff0055>COINCIDENT (Z-FIGHTING ACTIVE)</color>" : "<color=#00ff88>CORRECTED (SEPARATED)</color>")}\n" +
                          $"• Separation: {CurrentSeparation:F5}m | Press [G] to toggle Coincident/Corrected | [F] to toggle Rig.");
            }
            else
            {
                Debug.Log("<color=#ffff00><b>[Z-BUFFER PRECISION LAB: STANDBY]</b></color> Precision test rig hidden.");
            }
        }

        public void SetCoincident(bool coincident)
        {
            isCoincident = coincident;
            ApplyState();

            string stateName = isCoincident ? "<color=#ff0055>COINCIDENT // FLICKERING ACTIVE</color>" : "<color=#00ff88>CORRECTED // SAFE SEPARATION (0.08m)</color>";
            Debug.Log($"<color=#00f0ff>[Z-Buffer Lab]</color> Surface Precision State: {stateName} (Offset: {CurrentSeparation:F5}m)");
        }

        private void ApplyState()
        {
            if (rigContainer != null)
            {
                rigContainer.SetActive(isDemoActive);
            }

            if (surfaceB != null)
            {
                // Offset surface B along its local Z normal
                float offset = isCoincident ? coincidentOffset : correctedOffset;
                surfaceB.localPosition = surfaceBBaseLocalPos + new Vector3(0f, 0f, -offset);
            }
        }
    }
}
