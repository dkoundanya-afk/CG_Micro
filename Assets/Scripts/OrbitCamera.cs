using UnityEngine;

namespace ZBufferDemo
{
    /// <summary>
    /// Smooth orbit camera with auto-pan and optional mouse controls.
    /// Perfectly demonstrates changing occlusion and perspective depth relationships.
    /// </summary>
    public class OrbitCamera : MonoBehaviour
    {
        [Header("Target & Positioning")]
        [Tooltip("Point in world space to orbit around.")]
        public Vector3 focalPoint = new Vector3(0f, 0.5f, 0f);

        [Tooltip("Distance from the focal point.")]
        public float distance = 16f;

        [Tooltip("Minimum and maximum zoom distance.")]
        public Vector2 distanceLimits = new Vector2(6f, 35f);

        [Header("Angles (Degrees)")]
        [Tooltip("Current horizontal angle (yaw).")]
        public float currentYaw = -35f;

        [Tooltip("Current vertical angle (pitch).")]
        public float currentPitch = 15f;

        [Tooltip("Vertical angle clamp range.")]
        public Vector2 pitchLimits = new Vector2(-10f, 60f);

        [Header("Auto Cinematic Rotation")]
        [Tooltip("Automatically orbit around the focal point.")]
        public bool autoRotate = true;

        [Tooltip("Speed of auto rotation in degrees per second.")]
        public float autoRotateSpeed = 6f;

        [Header("Interactive Controls")]
        [Tooltip("Mouse orbit sensitivity.")]
        public float mouseSensitivity = 3f;

        [Tooltip("Mouse zoom scroll sensitivity.")]
        public float zoomSensitivity = 4f;

        [Tooltip("Smoothing factor for camera movement (higher = snappier).")]
        public float smoothing = 8f;

        private float targetYaw;
        private float targetPitch;
        private float targetDistance;

        private void Start()
        {
            targetYaw = currentYaw;
            targetPitch = currentPitch;
            targetDistance = distance;
            UpdateCameraTransform(true);
        }

        private void Update()
        {
            // Interactive mouse input (Right or Left mouse button drag)
            if (Input.GetMouseButton(0) || Input.GetMouseButton(1))
            {
                float mouseX = Input.GetAxis("Mouse X");
                float mouseY = Input.GetAxis("Mouse Y");

                if (Mathf.Abs(mouseX) > 0.001f || Mathf.Abs(mouseY) > 0.001f)
                {
                    targetYaw += mouseX * mouseSensitivity;
                    targetPitch -= mouseY * mouseSensitivity;
                    targetPitch = Mathf.Clamp(targetPitch, pitchLimits.x, pitchLimits.y);
                }
            }
            else if (autoRotate)
            {
                // Auto-cinematic pan when not dragging
                targetYaw += autoRotateSpeed * Time.deltaTime;
            }

            // Scroll zoom
            float scroll = Input.GetAxis("Mouse ScrollWheel");
            if (Mathf.Abs(scroll) > 0.001f)
            {
                targetDistance -= scroll * zoomSensitivity;
                targetDistance = Mathf.Clamp(targetDistance, distanceLimits.x, distanceLimits.y);
            }

            // Toggle auto-rotation with 'P' key, reset with 'R' key
            if (Input.GetKeyDown(KeyCode.P))
            {
                ToggleOrbit();
            }
            if (Input.GetKeyDown(KeyCode.R))
            {
                ResetView();
            }

            UpdateCameraTransform(false);
        }

        public void ToggleOrbit()
        {
            autoRotate = !autoRotate;
            Debug.Log($"<color=#00f0ff>[Z-Buffer]</color> Camera Orbit: {(autoRotate ? "ACTIVE" : "PAUSED")}");
        }

        public void ResetView()
        {
            targetYaw = -35f;
            targetPitch = 15f;
            targetDistance = 16f;
            Debug.Log("<color=#00f0ff>[Z-Buffer]</color> Camera View Reset to default perspective.");
        }

        private void UpdateCameraTransform(bool instant)
        {
            if (instant)
            {
                currentYaw = targetYaw;
                currentPitch = targetPitch;
                distance = targetDistance;
            }
            else
            {
                float t = Mathf.Clamp01(Time.deltaTime * smoothing);
                currentYaw = Mathf.Lerp(currentYaw, targetYaw, t);
                currentPitch = Mathf.Lerp(currentPitch, targetPitch, t);
                distance = Mathf.Lerp(distance, targetDistance, t);
            }

            Quaternion rotation = Quaternion.Euler(currentPitch, currentYaw, 0f);
            Vector3 offset = rotation * new Vector3(0f, 0f, -distance);
            transform.position = focalPoint + offset;
            transform.LookAt(focalPoint);
        }
    }
}
