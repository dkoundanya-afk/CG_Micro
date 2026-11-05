using UnityEngine;

namespace ZBufferDemo
{
    /// <summary>
    /// Smooth sinusoidal hovering, bobbing, and subtle roll oscillation for floating spaceships and drones.
    /// </summary>
    public class FloatBob : MonoBehaviour
    {
        [Header("Hover Settings")]
        [Tooltip("Vertical bobbing amplitude.")]
        public float amplitude = 0.25f;

        [Tooltip("Frequency of vertical bobbing in cycles per second.")]
        public float frequency = 0.75f;

        [Tooltip("Random phase offset to prevent synchrony across multiple objects.")]
        public float phaseOffset = 0f;

        [Header("Tilt & Drift (Optional)")]
        [Tooltip("Subtle roll and pitch oscillation amplitude in degrees.")]
        public Vector3 tiltAmplitude = new Vector3(1.5f, 0f, 2.5f);

        [Tooltip("Tilt oscillation speed multiplier.")]
        public float tiltFrequency = 0.5f;

        private Vector3 initialPosition;
        private Quaternion initialRotation;

        private void Start()
        {
            initialPosition = transform.localPosition;
            initialRotation = transform.localRotation;

            if (phaseOffset == 0f)
            {
                // Give a subtle unique offset based on instance ID if not customized
                phaseOffset = (GetInstanceID() % 100) * 0.1f;
            }
        }

        private void Update()
        {
            float t = Time.time + phaseOffset;

            // Vertical oscillation
            float yOffset = Mathf.Sin(t * frequency * Mathf.PI * 2f) * amplitude;
            transform.localPosition = initialPosition + new Vector3(0f, yOffset, 0f);

            // Subtle pitch and roll swaying
            if (tiltAmplitude != Vector3.zero)
            {
                float pitch = Mathf.Sin(t * tiltFrequency * Mathf.PI * 1.7f) * tiltAmplitude.x;
                float yaw = Mathf.Sin(t * tiltFrequency * Mathf.PI * 1.1f) * tiltAmplitude.y;
                float roll = Mathf.Cos(t * tiltFrequency * Mathf.PI * 2.1f) * tiltAmplitude.z;

                transform.localRotation = initialRotation * Quaternion.Euler(pitch, yaw, roll);
            }
        }
    }
}
