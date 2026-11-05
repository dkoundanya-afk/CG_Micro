using UnityEngine;

namespace ZBufferDemo
{
    /// <summary>
    /// Simple, high-performance rotation script for asteroids, rings, and orbital objects.
    /// </summary>
    public class Rotator : MonoBehaviour
    {
        [Header("Rotation Settings")]
        [Tooltip("Rotation speed in degrees per second for each axis.")]
        public Vector3 rotationSpeed = new Vector3(0f, 25f, 0f);

        [Tooltip("Coordinate space to apply rotation.")]
        public Space rotationSpace = Space.Self;

        private void Update()
        {
            transform.Rotate(rotationSpeed * Time.deltaTime, rotationSpace);
        }
    }
}
