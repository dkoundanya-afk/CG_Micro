using UnityEngine;

namespace ZBufferDemo
{
    /// <summary>
    /// Generates a procedural starfield mesh on a sphere around the scene.
    /// Uses a single draw call with vertex colors for maximum performance on student laptops.
    /// </summary>
    [RequireComponent(typeof(MeshFilter), typeof(MeshRenderer))]
    public class ProceduralStarfield : MonoBehaviour
    {
        [Header("Starfield Settings")]
        [Range(100, 1500)]
        public int starCount = 600;

        [Range(50f, 300f)]
        public float sphereRadius = 140f;

        [Range(0.2f, 2f)]
        public float starSize = 0.8f;

        [Tooltip("Slowly rotate the celestial sphere for dynamic background motion.")]
        public Vector3 celestialRotationSpeed = new Vector3(0.1f, 0.3f, 0.05f);

        private void Start()
        {
            BuildStarfieldMesh();
        }

        private void Update()
        {
            transform.Rotate(celestialRotationSpeed * Time.deltaTime, Space.Self);
        }

        /// <summary>
        /// Generates a single combined quad mesh containing all stars with randomized colors and sizes.
        /// </summary>
        public void BuildStarfieldMesh()
        {
            MeshFilter mf = GetComponent<MeshFilter>();
            if (mf == null) mf = gameObject.AddComponent<MeshFilter>();

            MeshRenderer mr = GetComponent<MeshRenderer>();
            if (mr == null) mr = gameObject.AddComponent<MeshRenderer>();

            // Find an unlit or default shader for the stars
            Shader starShader = Shader.Find("Unlit/Color");
            if (starShader == null) starShader = Shader.Find("Sprites/Default");
            if (starShader == null) starShader = Shader.Find("Mobile/Particles/Additive");
            if (starShader == null) starShader = Shader.Find("Standard");

            Material starMaterial = new Material(starShader);
            starMaterial.name = "Mat_ProceduralStars";
            if (starMaterial.HasProperty("_Color"))
            {
                starMaterial.SetColor("_Color", Color.white);
            }
            mr.sharedMaterial = starMaterial;

            int vertexCount = starCount * 4;
            int triangleCount = starCount * 6;

            Vector3[] vertices = new Vector3[vertexCount];
            Vector2[] uvs = new Vector2[vertexCount];
            Color[] colors = new Color[vertexCount];
            int[] triangles = new int[triangleCount];

            // Palette of space star colors (white, cyan, ice-blue, soft magenta/violet)
            Color[] starPalette = new Color[]
            {
                new Color(0.95f, 0.98f, 1.0f, 1f),      // Pure white/cool white
                new Color(0.40f, 0.85f, 1.0f, 1f),      // Neon Cyan
                new Color(0.60f, 0.70f, 1.0f, 0.9f),    // Soft Blue
                new Color(0.85f, 0.60f, 1.0f, 0.85f),   // Soft Violet
                new Color(1.00f, 0.50f, 0.85f, 0.8f)    // Subtle Magenta
            };

            Random.State originalState = Random.state;
            Random.InitState(42); // Stable seed for consistent stars

            for (int i = 0; i < starCount; i++)
            {
                // Uniform distribution on sphere
                Vector3 dir = Random.onUnitSphere;
                Vector3 center = dir * sphereRadius;

                // Tangent frame for facing camera/center
                Vector3 normal = -dir;
                Vector3 right = Vector3.Cross(normal, Vector3.up);
                if (right.sqrMagnitude < 0.001f)
                {
                    right = Vector3.Cross(normal, Vector3.right);
                }
                right.Normalize();
                Vector3 up = Vector3.Cross(right, normal).normalized;

                float size = starSize * Random.Range(0.4f, 1.6f);
                Color starCol = starPalette[Random.Range(0, starPalette.Length)];
                // Modulate brightness
                starCol *= Random.Range(0.7f, 1.3f);

                int vBase = i * 4;
                vertices[vBase + 0] = center + (-right - up) * (size * 0.5f);
                vertices[vBase + 1] = center + (right - up) * (size * 0.5f);
                vertices[vBase + 2] = center + (right + up) * (size * 0.5f);
                vertices[vBase + 3] = center + (-right + up) * (size * 0.5f);

                uvs[vBase + 0] = new Vector2(0, 0);
                uvs[vBase + 1] = new Vector2(1, 0);
                uvs[vBase + 2] = new Vector2(1, 1);
                uvs[vBase + 3] = new Vector2(0, 1);

                colors[vBase + 0] = starCol;
                colors[vBase + 1] = starCol;
                colors[vBase + 2] = starCol;
                colors[vBase + 3] = starCol;

                int tBase = i * 6;
                triangles[tBase + 0] = vBase + 0;
                triangles[tBase + 1] = vBase + 2;
                triangles[tBase + 2] = vBase + 1;
                triangles[tBase + 3] = vBase + 0;
                triangles[tBase + 4] = vBase + 3;
                triangles[tBase + 5] = vBase + 2;
            }

            Random.state = originalState;

            Mesh mesh = new Mesh();
            mesh.name = "StarfieldMesh";
            mesh.vertices = vertices;
            mesh.uv = uvs;
            mesh.colors = colors;
            mesh.triangles = triangles;
            mesh.RecalculateBounds();

            mf.sharedMesh = mesh;
        }
    }
}
