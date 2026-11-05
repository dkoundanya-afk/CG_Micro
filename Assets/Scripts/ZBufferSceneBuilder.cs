using System.Collections.Generic;
using UnityEngine;
#if UNITY_EDITOR
using UnityEditor;
#endif

namespace ZBufferDemo
{
    /// <summary>
    /// Procedural scene builder for the Z-Buffer Computer Graphics demonstration.
    /// Builds a futuristic space scene using Unity primitives without external assets.
    /// Supports both Built-in Render Pipeline (BiRP) and URP.
    /// </summary>
    public class ZBufferSceneBuilder : MonoBehaviour
    {
        public const string ROOT_NAME = "[ZBuffer_Scene_Root]";

        [Header("Scene Configuration")]
        [Tooltip("If true, builds the scene automatically on Start if not already present in the hierarchy.")]
        public bool buildOnStartIfMissing = true;

        [Header("Material References (Auto-created if empty)")]
        public Material matHullDark;
        public Material matHullArmor;
        public Material matCockpitGlass;
        public Material matNeonCyan;
        public Material matNeonMagenta;
        public Material matNeonViolet;
        public Material matNeonBlue;
        public Material matAsteroid;
        public Material matTechGold;
        public Material matStationRing;

        private void Start()
        {
            if (buildOnStartIfMissing)
            {
                GameObject existingRoot = GameObject.Find(ROOT_NAME);
                if (existingRoot == null || existingRoot.transform.childCount == 0)
                {
                    Debug.Log("[ZBuffer] Scene root not found or empty. Procedurally generating futuristic scene...");
                    BuildScene(false);
                }
            }
        }

        /// <summary>
        /// Detects if the current project is running Universal Render Pipeline.
        /// </summary>
        public static bool IsURP()
        {
            return UnityEngine.Rendering.GraphicsSettings.currentRenderPipeline != null;
        }

        /// <summary>
        /// Gets the appropriate default Lit shader depending on active render pipeline.
        /// </summary>
        public static Shader GetLitShader()
        {
            if (IsURP())
            {
                Shader urpShader = Shader.Find("Universal Render Pipeline/Lit");
                if (urpShader != null) return urpShader;
            }

            Shader standard = Shader.Find("Standard");
            if (standard != null) return standard;

            return Shader.Find("Diffuse");
        }

        /// <summary>
        /// Gets the appropriate Unlit shader depending on active render pipeline.
        /// </summary>
        public static Shader GetUnlitShader()
        {
            if (IsURP())
            {
                Shader urpUnlit = Shader.Find("Universal Render Pipeline/Unlit");
                if (urpUnlit != null) return urpUnlit;
            }

            Shader unlit = Shader.Find("Unlit/Color");
            if (unlit != null) return unlit;

            return Shader.Find("Sprites/Default");
        }

        /// <summary>
        /// Creates or retrieves a material with custom color, metallic, smoothness, and emission.
        /// </summary>
        public static Material CreateMaterial(string name, Color baseColor, float metallic, float smoothness, Color emissionColor, float emissionIntensity, bool isAsset = false)
        {
#if UNITY_EDITOR
            if (isAsset)
            {
                string path = $"Assets/Materials/{name}.mat";
                Material existing = AssetDatabase.LoadAssetAtPath<Material>(path);
                if (existing != null)
                {
                    ConfigureMaterialProperties(existing, baseColor, metallic, smoothness, emissionColor, emissionIntensity);
                    EditorUtility.SetDirty(existing);
                    return existing;
                }
            }
#endif

            Shader shader = GetLitShader();
            Material mat = new Material(shader) { name = name };
            ConfigureMaterialProperties(mat, baseColor, metallic, smoothness, emissionColor, emissionIntensity);

#if UNITY_EDITOR
            if (isAsset)
            {
                if (!AssetDatabase.IsValidFolder("Assets/Materials"))
                {
                    AssetDatabase.CreateFolder("Assets", "Materials");
                }
                string path = $"Assets/Materials/{name}.mat";
                AssetDatabase.CreateAsset(mat, path);
                AssetDatabase.SaveAssets();
            }
#endif
            return mat;
        }

        private static void ConfigureMaterialProperties(Material mat, Color baseColor, float metallic, float smoothness, Color emissionColor, float emissionIntensity)
        {
            bool urp = IsURP();

            if (urp)
            {
                mat.SetColor("_BaseColor", baseColor);
                mat.SetFloat("_Metallic", metallic);
                mat.SetFloat("_Smoothness", smoothness);

                if (emissionIntensity > 0f)
                {
                    mat.EnableKeyword("_EMISSION");
                    mat.SetColor("_EmissionColor", emissionColor * emissionIntensity);
                }
            }
            else
            {
                // Built-in Standard Shader
                mat.SetColor("_Color", baseColor);
                mat.SetFloat("_Metallic", metallic);
                mat.SetFloat("_Glossiness", smoothness);

                if (emissionIntensity > 0f)
                {
                    mat.EnableKeyword("_EMISSION");
                    mat.SetColor("_EmissionColor", emissionColor * emissionIntensity);
                    mat.globalIlluminationFlags = MaterialGlobalIlluminationFlags.RealtimeEmissive;
                }
            }
        }

        /// <summary>
        /// Initializes and returns the complete material library.
        /// </summary>
        public void InitializeMaterials(bool saveAsAssets)
        {
            matHullDark = CreateMaterial("Mat_HullDark", new Color(0.10f, 0.12f, 0.16f), 0.85f, 0.75f, Color.black, 0f, saveAsAssets);
            matHullArmor = CreateMaterial("Mat_HullArmor", new Color(0.24f, 0.28f, 0.35f), 0.70f, 0.60f, Color.black, 0f, saveAsAssets);
            matCockpitGlass = CreateMaterial("Mat_CockpitGlass", new Color(0.06f, 0.12f, 0.18f), 0.95f, 0.95f, new Color(0.0f, 0.4f, 0.6f), 0.8f, saveAsAssets);

            // Neon emissive materials
            matNeonCyan = CreateMaterial("Mat_NeonCyan", new Color(0.0f, 0.95f, 1.0f), 0.2f, 0.9f, new Color(0.0f, 0.95f, 1.0f), 2.8f, saveAsAssets);
            matNeonMagenta = CreateMaterial("Mat_NeonMagenta", new Color(1.0f, 0.05f, 0.55f), 0.2f, 0.9f, new Color(1.0f, 0.05f, 0.55f), 2.8f, saveAsAssets);
            matNeonViolet = CreateMaterial("Mat_NeonViolet", new Color(0.55f, 0.15f, 0.95f), 0.2f, 0.9f, new Color(0.55f, 0.15f, 0.95f), 2.4f, saveAsAssets);
            matNeonBlue = CreateMaterial("Mat_NeonBlue", new Color(0.12f, 0.45f, 1.0f), 0.2f, 0.9f, new Color(0.12f, 0.45f, 1.0f), 2.4f, saveAsAssets);

            // Tech & Environment materials
            matAsteroid = CreateMaterial("Mat_AsteroidRock", new Color(0.26f, 0.24f, 0.22f), 0.05f, 0.15f, Color.black, 0f, saveAsAssets);
            matTechGold = CreateMaterial("Mat_TechGold", new Color(0.85f, 0.70f, 0.22f), 0.90f, 0.85f, Color.black, 0f, saveAsAssets);
            matStationRing = CreateMaterial("Mat_StationRing", new Color(0.15f, 0.17f, 0.22f), 0.80f, 0.70f, new Color(0.2f, 0.4f, 0.8f), 0.5f, saveAsAssets);
        }

        /// <summary>
        /// Clears all previously generated scene objects under the root.
        /// </summary>
        public static void ClearExistingScene()
        {
            GameObject root = GameObject.Find(ROOT_NAME);
            if (root != null)
            {
#if UNITY_EDITOR
                if (!Application.isPlaying)
                {
                    Undo.DestroyObjectImmediate(root);
                    return;
                }
#endif
                Destroy(root);
            }
        }

        /// <summary>
        /// Main builder method. Constructs the entire procedural futuristic scene.
        /// </summary>
        public GameObject BuildScene(bool saveAssets)
        {
            ClearExistingScene();
            InitializeMaterials(saveAssets);

            GameObject root = new GameObject(ROOT_NAME);
#if UNITY_EDITOR
            if (!Application.isPlaying)
            {
                Undo.RegisterCreatedObjectUndo(root, "Build Z-Buffer Scene");
            }
#endif

            // 1. Setup Camera and Lighting
            SetupEnvironment(root);

            // 2. Build Layer 2: Central Futuristic Player Spaceship (Z: 0)
            GameObject playerShip = BuildPlayerSpaceship(root);

            // 3. Build Layer 1: Foreground Occluders (Z: -4 to -1)
            BuildForegroundOccluders(root);

            // 4. Build Escort Drones (Z: 3 to 5)
            BuildEscortDrones(root);

            // 5. Build Layer 3: Intermediate Asteroid Belt & Beacon (Z: 10 to 25)
            BuildAsteroidBelt(root);

            // 6. Build Layer 4: Deep Background Orbital Megastructure (Z: 45 to 65)
            BuildMegastructureRing(root);

            // 7. Build Procedural Starfield
            BuildStarfield(root);

            // 8. Build Dedicated Occlusion Demonstration Rig (Phase 4)
            BuildOcclusionDemoRig(root);

            // 9. Build Z-Fighting and Depth Precision Laboratory Rig (Phase 5)
            BuildDepthPrecisionDemoRig(root);

            Debug.Log("[ZBuffer] Futuristic Scene successfully generated with multi-layer depth composition!");
            return root;
        }

        private void SetupEnvironment(GameObject root)
        {
            // Configure Main Camera
            Camera cam = Camera.main;
            if (cam == null)
            {
                cam = FindFirstObjectByType<Camera>();
            }

            if (cam != null)
            {
                cam.clearFlags = CameraClearFlags.SolidColor;
                cam.backgroundColor = new Color(0.015f, 0.025f, 0.05f); // Deep midnight void
                cam.nearClipPlane = 0.3f;
                cam.farClipPlane = 400f;
                cam.fieldOfView = 55f;

                // Configure or add OrbitCamera
                OrbitCamera orbit = cam.GetComponent<OrbitCamera>();
                if (orbit == null)
                {
                    orbit = cam.gameObject.AddComponent<OrbitCamera>();
                }
                orbit.focalPoint = new Vector3(0f, 0.5f, 0f);
                orbit.distance = 15f;
                orbit.currentYaw = -38f;
                orbit.currentPitch = 14f;
                orbit.autoRotate = true;
                orbit.autoRotateSpeed = 5f;

                // Configure or add DepthVisualizer (Phase 2 Real-Time Depth Buffer)
                DepthVisualizer visualizer = cam.GetComponent<DepthVisualizer>();
                if (visualizer == null)
                {
                    visualizer = cam.gameObject.AddComponent<DepthVisualizer>();
                }
                visualizer.nearDisplayDistance = 0.5f;
                visualizer.farDisplayDistance = 65.0f;
                visualizer.invertDepth = true;
                visualizer.depthMode = DepthVisualizer.DepthMode.LinearEyeRange;
            }

            // Configure Key Directional Light
            Light mainLight = null;
            Light[] lights = FindObjectsByType<Light>(FindObjectsSortMode.None);
            foreach (Light l in lights)
            {
                if (l.type == LightType.Directional)
                {
                    mainLight = l;
                    break;
                }
            }

            if (mainLight == null)
            {
                GameObject lightGo = new GameObject("Directional Light - Key");
                lightGo.transform.SetParent(root.transform);
                mainLight = lightGo.AddComponent<Light>();
                mainLight.type = LightType.Directional;
            }

            mainLight.color = new Color(0.82f, 0.90f, 1.0f); // Cool blue-white star
            mainLight.intensity = 1.15f;
            mainLight.transform.rotation = Quaternion.Euler(42f, -35f, 0f);

            // Add Secondary Rim/Fill Light (Deep Violet/Magenta)
            GameObject rimLightGo = new GameObject("Directional Light - Rim Fill");
            rimLightGo.transform.SetParent(root.transform);
            Light rimLight = rimLightGo.AddComponent<Light>();
            rimLight.type = LightType.Directional;
            rimLight.color = new Color(0.55f, 0.20f, 0.85f); // Neon violet rim
            rimLight.intensity = 0.65f;
            rimLightGo.transform.rotation = Quaternion.Euler(-25f, 145f, 0f);
        }

        private GameObject BuildPlayerSpaceship(GameObject parent)
        {
            GameObject shipRoot = new GameObject("PlayerSpaceship_ValkyrieZ1");
            shipRoot.transform.SetParent(parent.transform);
            shipRoot.transform.position = Vector3.zero;
            shipRoot.transform.rotation = Quaternion.Euler(5f, 15f, -3f);

            // Idle floating animation
            FloatBob bob = shipRoot.AddComponent<FloatBob>();
            bob.amplitude = 0.18f;
            bob.frequency = 0.5f;
            bob.tiltAmplitude = new Vector3(1.0f, 0.5f, 2.0f);

            // --- Fuselage Main Body ---
            GameObject fuselage = GameObject.CreatePrimitive(PrimitiveType.Capsule);
            fuselage.name = "Fuselage";
            fuselage.transform.SetParent(shipRoot.transform);
            fuselage.transform.localPosition = Vector3.zero;
            fuselage.transform.localRotation = Quaternion.Euler(90f, 0f, 0f);
            fuselage.transform.localScale = new Vector3(1.2f, 2.8f, 0.85f);
            fuselage.GetComponent<MeshRenderer>().sharedMaterial = matHullDark;

            // Forward Nose Cone Wedge
            GameObject nose = GameObject.CreatePrimitive(PrimitiveType.Cylinder);
            nose.name = "NoseCone";
            nose.transform.SetParent(shipRoot.transform);
            nose.transform.localPosition = new Vector3(0f, 0.05f, 2.6f);
            nose.transform.localRotation = Quaternion.Euler(90f, 0f, 0f);
            nose.transform.localScale = new Vector3(0.7f, 0.9f, 0.35f);
            nose.GetComponent<MeshRenderer>().sharedMaterial = matHullArmor;

            // Sleek Cockpit Canopy
            GameObject canopy = GameObject.CreatePrimitive(PrimitiveType.Sphere);
            canopy.name = "CockpitCanopy";
            canopy.transform.SetParent(shipRoot.transform);
            canopy.transform.localPosition = new Vector3(0f, 0.38f, 0.9f);
            canopy.transform.localRotation = Quaternion.identity;
            canopy.transform.localScale = new Vector3(0.65f, 0.45f, 1.4f);
            canopy.GetComponent<MeshRenderer>().sharedMaterial = matCockpitGlass;

            // Canopy Neon Trim
            GameObject canopyTrim = GameObject.CreatePrimitive(PrimitiveType.Cube);
            canopyTrim.name = "CanopyTrim_Neon";
            canopyTrim.transform.SetParent(shipRoot.transform);
            canopyTrim.transform.localPosition = new Vector3(0f, 0.52f, 0.85f);
            canopyTrim.transform.localScale = new Vector3(0.08f, 0.08f, 1.35f);
            canopyTrim.GetComponent<MeshRenderer>().sharedMaterial = matNeonCyan;

            // --- Swept Delta Wings ---
            BuildWings(shipRoot);

            // --- Twin Ion Engines & Neon Exhaust ---
            BuildEngines(shipRoot);

            // --- Dorsal Sensor Spine & Radiator Fins ---
            BuildSensorSpine(shipRoot);

            return shipRoot;
        }

        private void BuildWings(GameObject shipRoot)
        {
            // Port (Left) Wing
            GameObject leftWing = GameObject.CreatePrimitive(PrimitiveType.Cube);
            leftWing.name = "Wing_Port";
            leftWing.transform.SetParent(shipRoot.transform);
            leftWing.transform.localPosition = new Vector3(-1.9f, 0.02f, -0.4f);
            leftWing.transform.localRotation = Quaternion.Euler(0f, -22f, -3f);
            leftWing.transform.localScale = new Vector3(2.6f, 0.12f, 1.8f);
            leftWing.GetComponent<MeshRenderer>().sharedMaterial = matHullArmor;

            // Port Wing Neon Edge (Cyan)
            GameObject leftEdge = GameObject.CreatePrimitive(PrimitiveType.Cube);
            leftEdge.name = "WingEdge_Neon_Port";
            leftEdge.transform.SetParent(leftWing.transform);
            leftEdge.transform.localPosition = new Vector3(-0.48f, 0f, 0.48f);
            leftEdge.transform.localRotation = Quaternion.Euler(0f, 40f, 0f);
            leftEdge.transform.localScale = new Vector3(0.06f, 1.3f, 1.1f);
            leftEdge.GetComponent<MeshRenderer>().sharedMaterial = matNeonCyan;

            // Port Wingtip Stabilizer Fin
            GameObject leftFin = GameObject.CreatePrimitive(PrimitiveType.Cube);
            leftFin.name = "StabilizerFin_Port";
            leftFin.transform.SetParent(shipRoot.transform);
            leftFin.transform.localPosition = new Vector3(-3.2f, 0.6f, -0.9f);
            leftFin.transform.localRotation = Quaternion.Euler(-15f, -12f, 25f);
            leftFin.transform.localScale = new Vector3(0.1f, 1.3f, 0.9f);
            leftFin.GetComponent<MeshRenderer>().sharedMaterial = matHullDark;

            // Port Fin Tip (Magenta)
            GameObject leftFinTip = GameObject.CreatePrimitive(PrimitiveType.Cube);
            leftFinTip.name = "FinTip_Neon_Port";
            leftFinTip.transform.SetParent(leftFin.transform);
            leftFinTip.transform.localPosition = new Vector3(0f, 0.48f, 0f);
            leftFinTip.transform.localScale = new Vector3(1.4f, 0.12f, 0.95f);
            leftFinTip.GetComponent<MeshRenderer>().sharedMaterial = matNeonMagenta;

            // Starboard (Right) Wing
            GameObject rightWing = GameObject.CreatePrimitive(PrimitiveType.Cube);
            rightWing.name = "Wing_Starboard";
            rightWing.transform.SetParent(shipRoot.transform);
            rightWing.transform.localPosition = new Vector3(1.9f, 0.02f, -0.4f);
            rightWing.transform.localRotation = Quaternion.Euler(0f, 22f, 3f);
            rightWing.transform.localScale = new Vector3(2.6f, 0.12f, 1.8f);
            rightWing.GetComponent<MeshRenderer>().sharedMaterial = matHullArmor;

            // Starboard Wing Neon Edge (Cyan)
            GameObject rightEdge = GameObject.CreatePrimitive(PrimitiveType.Cube);
            rightEdge.name = "WingEdge_Neon_Starboard";
            rightEdge.transform.SetParent(rightWing.transform);
            rightEdge.transform.localPosition = new Vector3(0.48f, 0f, 0.48f);
            rightEdge.transform.localRotation = Quaternion.Euler(0f, -40f, 0f);
            rightEdge.transform.localScale = new Vector3(0.06f, 1.3f, 1.1f);
            rightEdge.GetComponent<MeshRenderer>().sharedMaterial = matNeonCyan;

            // Starboard Wingtip Stabilizer Fin
            GameObject rightFin = GameObject.CreatePrimitive(PrimitiveType.Cube);
            rightFin.name = "StabilizerFin_Starboard";
            rightFin.transform.SetParent(shipRoot.transform);
            rightFin.transform.localPosition = new Vector3(3.2f, 0.6f, -0.9f);
            rightFin.transform.localRotation = Quaternion.Euler(-15f, 12f, -25f);
            rightFin.transform.localScale = new Vector3(0.1f, 1.3f, 0.9f);
            rightFin.GetComponent<MeshRenderer>().sharedMaterial = matHullDark;

            // Starboard Fin Tip (Magenta)
            GameObject rightFinTip = GameObject.CreatePrimitive(PrimitiveType.Cube);
            rightFinTip.name = "FinTip_Neon_Starboard";
            rightFinTip.transform.SetParent(rightFin.transform);
            rightFinTip.transform.localPosition = new Vector3(0f, 0.48f, 0f);
            rightFinTip.transform.localScale = new Vector3(1.4f, 0.12f, 0.95f);
            rightFinTip.GetComponent<MeshRenderer>().sharedMaterial = matNeonMagenta;
        }

        private void BuildEngines(GameObject shipRoot)
        {
            float[] xOffsets = new float[] { -0.7f, 0.7f };

            for (int i = 0; i < xOffsets.Length; i++)
            {
                float x = xOffsets[i];
                string side = i == 0 ? "Port" : "Starboard";

                // Engine Housing
                GameObject housing = GameObject.CreatePrimitive(PrimitiveType.Cylinder);
                housing.name = $"EngineHousing_{side}";
                housing.transform.SetParent(shipRoot.transform);
                housing.transform.localPosition = new Vector3(x, 0.08f, -2.1f);
                housing.transform.localRotation = Quaternion.Euler(90f, 0f, 0f);
                housing.transform.localScale = new Vector3(0.6f, 0.85f, 0.6f);
                housing.GetComponent<MeshRenderer>().sharedMaterial = matHullDark;

                // Engine Cowling Rim (Gold)
                GameObject rim = GameObject.CreatePrimitive(PrimitiveType.Cylinder);
                rim.name = $"EngineCowling_{side}";
                rim.transform.SetParent(shipRoot.transform);
                rim.transform.localPosition = new Vector3(x, 0.08f, -2.85f);
                rim.transform.localRotation = Quaternion.Euler(90f, 0f, 0f);
                rim.transform.localScale = new Vector3(0.65f, 0.12f, 0.65f);
                rim.GetComponent<MeshRenderer>().sharedMaterial = matTechGold;

                // Glowing Thruster Exhaust (Neon Cyan / Blue)
                GameObject exhaust = GameObject.CreatePrimitive(PrimitiveType.Cylinder);
                exhaust.name = $"ThrusterExhaust_{side}";
                exhaust.transform.SetParent(shipRoot.transform);
                exhaust.transform.localPosition = new Vector3(x, 0.08f, -3.2f);
                exhaust.transform.localRotation = Quaternion.Euler(90f, 0f, 0f);
                exhaust.transform.localScale = new Vector3(0.42f, 0.45f, 0.42f);
                exhaust.GetComponent<MeshRenderer>().sharedMaterial = matNeonCyan;

                // Local point light for engine exhaust glow
                GameObject thrusterLightGo = new GameObject($"ThrusterLight_{side}");
                thrusterLightGo.transform.SetParent(shipRoot.transform);
                thrusterLightGo.transform.localPosition = new Vector3(x, 0.08f, -3.4f);
                Light tLight = thrusterLightGo.AddComponent<Light>();
                tLight.type = LightType.Point;
                tLight.color = new Color(0.0f, 0.85f, 1.0f);
                tLight.range = 3.5f;
                tLight.intensity = 2.0f;
            }
        }

        private void BuildSensorSpine(GameObject shipRoot)
        {
            // Spinal dorsal spine
            GameObject spine = GameObject.CreatePrimitive(PrimitiveType.Cube);
            spine.name = "DorsalSpine";
            spine.transform.SetParent(shipRoot.transform);
            spine.transform.localPosition = new Vector3(0f, 0.42f, -0.6f);
            spine.transform.localRotation = Quaternion.Euler(-8f, 0f, 0f);
            spine.transform.localScale = new Vector3(0.22f, 0.35f, 2.2f);
            spine.GetComponent<MeshRenderer>().sharedMaterial = matHullArmor;

            // Spine neon channel (Violet)
            GameObject spineGlow = GameObject.CreatePrimitive(PrimitiveType.Cube);
            spineGlow.name = "DorsalSpine_Glow_Violet";
            spineGlow.transform.SetParent(spine.transform);
            spineGlow.transform.localPosition = new Vector3(0f, 0.52f, 0f);
            spineGlow.transform.localScale = new Vector3(0.35f, 0.1f, 0.95f);
            spineGlow.GetComponent<MeshRenderer>().sharedMaterial = matNeonViolet;

            // Forward sensor antenna
            GameObject antenna = GameObject.CreatePrimitive(PrimitiveType.Cylinder);
            antenna.name = "NoseAntenna";
            antenna.transform.SetParent(shipRoot.transform);
            antenna.transform.localPosition = new Vector3(0f, 0.12f, 3.4f);
            antenna.transform.localRotation = Quaternion.Euler(90f, 0f, 0f);
            antenna.transform.localScale = new Vector3(0.05f, 0.5f, 0.05f);
            antenna.GetComponent<MeshRenderer>().sharedMaterial = matTechGold;
        }

        private void BuildForegroundOccluders(GameObject root)
        {
            // --- Foreground Object 1: Autonomous Recon Drone Probe ---
            // Positioned at Z: -3.2, partially occluding the ship's left wing from camera view!
            GameObject drone = new GameObject("Foreground_ReconDrone");
            drone.transform.SetParent(root.transform);
            drone.transform.position = new Vector3(-2.8f, -0.6f, -3.2f);
            drone.transform.rotation = Quaternion.Euler(15f, 35f, -10f);

            FloatBob droneBob = drone.AddComponent<FloatBob>();
            droneBob.amplitude = 0.22f;
            droneBob.frequency = 0.8f;
            droneBob.phaseOffset = 1.5f;

            // Drone spherical chassis
            GameObject droneBody = GameObject.CreatePrimitive(PrimitiveType.Sphere);
            droneBody.name = "DroneCore";
            droneBody.transform.SetParent(drone.transform);
            droneBody.transform.localPosition = Vector3.zero;
            droneBody.transform.localScale = new Vector3(1.1f, 1.1f, 1.1f);
            droneBody.GetComponent<MeshRenderer>().sharedMaterial = matHullDark;

            // Drone glowing optical sensor eye (Magenta)
            GameObject droneEye = GameObject.CreatePrimitive(PrimitiveType.Sphere);
            droneEye.name = "DroneOpticEye_Magenta";
            droneEye.transform.SetParent(drone.transform);
            droneEye.transform.localPosition = new Vector3(0.1f, 0.1f, 0.5f);
            droneEye.transform.localScale = new Vector3(0.42f, 0.42f, 0.25f);
            droneEye.GetComponent<MeshRenderer>().sharedMaterial = matNeonMagenta;

            // Rotating scanner ring around drone
            GameObject ringHolder = new GameObject("ScannerRing");
            ringHolder.transform.SetParent(drone.transform);
            ringHolder.transform.localPosition = Vector3.zero;
            ringHolder.transform.localRotation = Quaternion.Euler(45f, 20f, 0f);

            Rotator ringRot = ringHolder.AddComponent<Rotator>();
            ringRot.rotationSpeed = new Vector3(0f, 60f, 0f);

            // Construct ring segments
            int ringSegments = 12;
            float rRadius = 0.95f;
            for (int s = 0; s < ringSegments; s++)
            {
                float angle = (s / (float)ringSegments) * Mathf.PI * 2f;
                Vector3 pos = new Vector3(Mathf.Cos(angle) * rRadius, 0f, Mathf.Sin(angle) * rRadius);

                GameObject seg = GameObject.CreatePrimitive(PrimitiveType.Cube);
                seg.name = $"RingNode_{s}";
                seg.transform.SetParent(ringHolder.transform);
                seg.transform.localPosition = pos;
                seg.transform.localRotation = Quaternion.Euler(0f, -angle * Mathf.Rad2Deg, 0f);
                seg.transform.localScale = new Vector3(0.12f, 0.12f, 0.35f);
                seg.GetComponent<MeshRenderer>().sharedMaterial = (s % 3 == 0) ? matNeonCyan : matTechGold;
            }

            // --- Foreground Object 2: Near Floating Asteroid Fragment ---
            // Positioned at Z: -2.0 on the right, occluding the background space
            GameObject nearRock = CreateCompositeAsteroid("Foreground_Asteroid_Fragment", new Vector3(3.4f, 1.6f, -2.0f), 1.25f, root);
            Rotator rockRot = nearRock.AddComponent<Rotator>();
            rockRot.rotationSpeed = new Vector3(18f, 25f, -12f);
        }

        private void BuildEscortDrones(GameObject root)
        {
            // Escort Bit Left (Z: +3.0)
            GameObject bitLeft = CreateEscortDrone("EscortBit_Alpha", new Vector3(-4.8f, 2.2f, 3.0f), root);
            // Escort Bit Right (Z: +4.5)
            GameObject bitRight = CreateEscortDrone("EscortBit_Beta", new Vector3(4.5f, -1.6f, 4.5f), root);
        }

        private GameObject CreateEscortDrone(string name, Vector3 position, GameObject parent)
        {
            GameObject drone = new GameObject(name);
            drone.transform.SetParent(parent.transform);
            drone.transform.position = position;

            FloatBob bob = drone.AddComponent<FloatBob>();
            bob.amplitude = 0.28f;
            bob.frequency = 0.65f;
            bob.phaseOffset = position.x;

            Rotator rot = drone.AddComponent<Rotator>();
            rot.rotationSpeed = new Vector3(10f, 35f, 15f);

            // Diamond / Octahedral Core
            GameObject core1 = GameObject.CreatePrimitive(PrimitiveType.Cube);
            core1.name = "DiamondCore_A";
            core1.transform.SetParent(drone.transform);
            core1.transform.localPosition = Vector3.zero;
            core1.transform.localRotation = Quaternion.Euler(45f, 45f, 0f);
            core1.transform.localScale = new Vector3(0.75f, 0.75f, 0.75f);
            core1.GetComponent<MeshRenderer>().sharedMaterial = matHullArmor;

            // Glowing central power sphere (Neon Cyan)
            GameObject powerCore = GameObject.CreatePrimitive(PrimitiveType.Sphere);
            powerCore.name = "PowerCore_Cyan";
            powerCore.transform.SetParent(drone.transform);
            powerCore.transform.localPosition = Vector3.zero;
            powerCore.transform.localScale = new Vector3(0.55f, 0.55f, 0.55f);
            powerCore.GetComponent<MeshRenderer>().sharedMaterial = matNeonCyan;

            // Orbiting sensor ring
            GameObject ring = GameObject.CreatePrimitive(PrimitiveType.Cylinder);
            ring.name = "ShieldHalo";
            ring.transform.SetParent(drone.transform);
            ring.transform.localPosition = Vector3.zero;
            ring.transform.localRotation = Quaternion.Euler(60f, 0f, 30f);
            ring.transform.localScale = new Vector3(1.4f, 0.04f, 1.4f);
            ring.GetComponent<MeshRenderer>().sharedMaterial = matNeonViolet;

            return drone;
        }

        private struct AsteroidData
        {
            public string name;
            public Vector3 pos;
            public float scale;
            public Vector3 rotSpeed;

            public AsteroidData(string name, Vector3 pos, float scale, Vector3 rotSpeed)
            {
                this.name = name;
                this.pos = pos;
                this.scale = scale;
                this.rotSpeed = rotSpeed;
            }
        }

        private void BuildAsteroidBelt(GameObject root)
        {
            GameObject beltGroup = new GameObject("Layer3_AsteroidBelt");
            beltGroup.transform.SetParent(root.transform);

            // Staggered asteroid field across Z: 10 to 26 with deliberate overlaps
            AsteroidData[] asteroids = new AsteroidData[]
            {
                new AsteroidData("Asteroid_Cluster_A", new Vector3(2.8f, 2.2f, 11.5f), 2.2f, new Vector3(15f, 22f, -8f)),
                new AsteroidData("Asteroid_Cluster_B", new Vector3(-3.8f, -2.5f, 14.0f), 3.0f, new Vector3(-12f, 18f, 20f)),
                new AsteroidData("Asteroid_Cluster_C", new Vector3(1.2f, -3.8f, 17.5f), 2.4f, new Vector3(24f, -14f, 10f)),
                new AsteroidData("Asteroid_Cluster_D", new Vector3(-2.2f, 3.8f, 21.0f), 3.8f, new Vector3(-16f, 20f, -14f)),
                new AsteroidData("Asteroid_Cluster_E", new Vector3(5.2f, 1.2f, 23.5f), 2.8f, new Vector3(10f, -25f, 18f)),
                new AsteroidData("Asteroid_Cluster_F", new Vector3(-5.8f, 1.0f, 25.0f), 3.4f, new Vector3(18f, 15f, -12f)),
                new AsteroidData("Asteroid_Cluster_G", new Vector3(0.0f, 5.0f, 26.5f), 2.6f, new Vector3(-20f, 10f, 15f))
            };

            for (int i = 0; i < asteroids.Length; i++)
            {
                AsteroidData data = asteroids[i];
                GameObject asteroid = CreateCompositeAsteroid(data.name, data.pos, data.scale, beltGroup);
                Rotator rot = asteroid.AddComponent<Rotator>();
                rot.rotationSpeed = data.rotSpeed;
            }

            // Intermediate Tech Monolith / Navigation Pylon (Z: 19.0)
            GameObject pylon = new GameObject("NavigationPylon_TechMonolith");
            pylon.transform.SetParent(beltGroup.transform);
            pylon.transform.position = new Vector3(-6.2f, -1.8f, 19.0f);
            pylon.transform.rotation = Quaternion.Euler(20f, -40f, 15f);

            Rotator pylonRot = pylon.AddComponent<Rotator>();
            pylonRot.rotationSpeed = new Vector3(5f, 12f, 3f);

            // Tall hexagonal/cubic obelisk
            GameObject obelisk = GameObject.CreatePrimitive(PrimitiveType.Cube);
            obelisk.name = "ObeliskCore";
            obelisk.transform.SetParent(pylon.transform);
            obelisk.transform.localPosition = Vector3.zero;
            obelisk.transform.localScale = new Vector3(1.1f, 5.5f, 1.1f);
            obelisk.GetComponent<MeshRenderer>().sharedMaterial = matHullDark;

            // Emissive central conduit (Neon Violet)
            GameObject conduit = GameObject.CreatePrimitive(PrimitiveType.Cube);
            conduit.name = "EnergyConduit_Violet";
            conduit.transform.SetParent(pylon.transform);
            conduit.transform.localPosition = Vector3.zero;
            conduit.transform.localScale = new Vector3(1.15f, 4.8f, 0.25f);
            conduit.GetComponent<MeshRenderer>().sharedMaterial = matNeonViolet;

            // Beacon emitter beacon top
            GameObject beaconCap = GameObject.CreatePrimitive(PrimitiveType.Sphere);
            beaconCap.name = "BeaconTop_Magenta";
            beaconCap.transform.SetParent(pylon.transform);
            beaconCap.transform.localPosition = new Vector3(0f, 3.0f, 0f);
            beaconCap.transform.localScale = new Vector3(0.7f, 0.7f, 0.7f);
            beaconCap.GetComponent<MeshRenderer>().sharedMaterial = matNeonMagenta;
        }

        private GameObject CreateCompositeAsteroid(string name, Vector3 position, float scale, GameObject parent)
        {
            GameObject astRoot = new GameObject(name);
            astRoot.transform.SetParent(parent.transform);
            astRoot.transform.position = position;

            // Cluster 3 overlapping distorted primitives to form an irregular faceted rocky shape
            GameObject part1 = GameObject.CreatePrimitive(PrimitiveType.Sphere);
            part1.name = "RockPart_Main";
            part1.transform.SetParent(astRoot.transform);
            part1.transform.localPosition = Vector3.zero;
            part1.transform.localRotation = Quaternion.Euler(25f, 45f, 10f);
            part1.transform.localScale = new Vector3(scale * 1.1f, scale * 0.85f, scale * 1.0f);
            part1.GetComponent<MeshRenderer>().sharedMaterial = matAsteroid;

            GameObject part2 = GameObject.CreatePrimitive(PrimitiveType.Cube);
            part2.name = "RockPart_Facet1";
            part2.transform.SetParent(astRoot.transform);
            part2.transform.localPosition = new Vector3(scale * 0.22f, -scale * 0.15f, scale * 0.1f);
            part2.transform.localRotation = Quaternion.Euler(55f, 20f, 40f);
            part2.transform.localScale = new Vector3(scale * 0.85f, scale * 0.8f, scale * 0.9f);
            part2.GetComponent<MeshRenderer>().sharedMaterial = matAsteroid;

            GameObject part3 = GameObject.CreatePrimitive(PrimitiveType.Sphere);
            part3.name = "RockPart_Lobe";
            part3.transform.SetParent(astRoot.transform);
            part3.transform.localPosition = new Vector3(-scale * 0.3f, scale * 0.2f, -scale * 0.15f);
            part3.transform.localRotation = Quaternion.Euler(-30f, 60f, -20f);
            part3.transform.localScale = new Vector3(scale * 0.75f, scale * 0.75f, scale * 0.8f);
            part3.GetComponent<MeshRenderer>().sharedMaterial = matAsteroid;

            return astRoot;
        }

        private void BuildMegastructureRing(GameObject root)
        {
            // Giant Space Station Gateway Ring (Layer 4 - Z: ~52)
            GameObject station = new GameObject("Layer4_Megastructure_OrbitalRing");
            station.transform.SetParent(root.transform);
            station.transform.position = new Vector3(0f, 8f, 52f);
            station.transform.rotation = Quaternion.Euler(22f, -15f, 8f);

            Rotator ringRot = station.AddComponent<Rotator>();
            ringRot.rotationSpeed = new Vector3(0f, 0f, 2.5f);

            int segments = 28;
            float ringRadius = 22f;

            for (int i = 0; i < segments; i++)
            {
                float angle = (i / (float)segments) * Mathf.PI * 2f;
                Vector3 localPos = new Vector3(Mathf.Cos(angle) * ringRadius, Mathf.Sin(angle) * ringRadius, 0f);
                float angleDeg = angle * Mathf.Rad2Deg;

                // Hull Segment
                GameObject seg = GameObject.CreatePrimitive(PrimitiveType.Cube);
                seg.name = $"RingSegment_{i}";
                seg.transform.SetParent(station.transform);
                seg.transform.localPosition = localPos;
                seg.transform.localRotation = Quaternion.Euler(0f, 0f, angleDeg + 90f);
                seg.transform.localScale = new Vector3(1.8f, 5.2f, 2.2f);
                seg.GetComponent<MeshRenderer>().sharedMaterial = matStationRing;

                // Alternate neon energy nodes along the ring perimeter
                if (i % 2 == 0)
                {
                    GameObject node = GameObject.CreatePrimitive(PrimitiveType.Cylinder);
                    node.name = $"EnergyNode_{i}";
                    node.transform.SetParent(station.transform);
                    node.transform.localPosition = localPos * 1.04f;
                    node.transform.localRotation = Quaternion.Euler(90f, 0f, angleDeg);
                    node.transform.localScale = new Vector3(0.7f, 0.35f, 0.7f);
                    node.GetComponent<MeshRenderer>().sharedMaterial = (i % 4 == 0) ? matNeonCyan : matNeonViolet;
                }
            }

            // Central Station Hub
            GameObject hub = GameObject.CreatePrimitive(PrimitiveType.Cylinder);
            hub.name = "StationCentralHub";
            hub.transform.SetParent(station.transform);
            hub.transform.localPosition = Vector3.zero;
            hub.transform.localRotation = Quaternion.Euler(90f, 0f, 0f);
            hub.transform.localScale = new Vector3(5.5f, 1.8f, 5.5f);
            hub.GetComponent<MeshRenderer>().sharedMaterial = matHullDark;

            // Hub Neon Spire
            GameObject spire = GameObject.CreatePrimitive(PrimitiveType.Cylinder);
            spire.name = "HubSpire_Cyan";
            spire.transform.SetParent(station.transform);
            spire.transform.localPosition = new Vector3(0f, 0f, -2.5f);
            spire.transform.localRotation = Quaternion.Euler(90f, 0f, 0f);
            spire.transform.localScale = new Vector3(1.2f, 3.5f, 1.2f);
            spire.GetComponent<MeshRenderer>().sharedMaterial = matNeonBlue;
        }

        private void BuildStarfield(GameObject root)
        {
            GameObject starfieldGo = new GameObject("ProceduralStarfield_Dome");
            starfieldGo.transform.SetParent(root.transform);
            starfieldGo.transform.position = Vector3.zero;

            ProceduralStarfield ps = starfieldGo.AddComponent<ProceduralStarfield>();
            ps.starCount = 650;
            ps.sphereRadius = 150f;
            ps.starSize = 0.85f;
            ps.celestialRotationSpeed = new Vector3(0.08f, 0.2f, 0.04f);
            ps.BuildStarfieldMesh();
        }

        private GameObject BuildOcclusionDemoRig(GameObject root)
        {
            // Dedicated Child of scene root for Phase 4 Occlusion Demonstration
            GameObject rigRoot = new GameObject("OcclusionDemo_TestRig");
            rigRoot.transform.SetParent(root.transform);
            rigRoot.transform.position = Vector3.zero;
            rigRoot.transform.rotation = Quaternion.identity;

            GameObject container = new GameObject("Rig_Calibration_Container");
            container.transform.SetParent(rigRoot.transform);
            container.transform.position = Vector3.zero;
            container.transform.rotation = Quaternion.identity;

            // 1. Near Slab (~6m distance from default camera)
            // Color: Gold trim, Dark Gunmetal body
            GameObject nearSlab = CreateCalibrationSlab("Near_Depth_Slab_6m",
                new Vector3(1.6f, 0.4f, -2.2f),
                new Vector3(2.0f, 2.6f, 0.15f),
                matHullDark, matTechGold, container);

            // 2. Mid Slab (~10m distance, overlaps ~40% behind Near Slab)
            // Color: Magenta trim, Hull Armor body
            GameObject midSlab = CreateCalibrationSlab("Mid_Depth_Slab_10m",
                new Vector3(2.8f, 0.9f, 1.2f),
                new Vector3(2.8f, 3.4f, 0.15f),
                matHullArmor, matNeonMagenta, container);

            // 3. Far Slab (~16m distance, overlaps ~45% behind Mid Slab)
            // Color: Cyan trim, Deep Station Ring body
            GameObject farSlab = CreateCalibrationSlab("Far_Depth_Slab_16m",
                new Vector3(4.2f, 1.4f, 5.0f),
                new Vector3(3.6f, 4.2f, 0.15f),
                matStationRing, matNeonCyan, container);

            // 4. Oscillating Depth Probe (passes behind Near Slab, in front of Mid Slab)
            GameObject probe = BuildSlidingProbe(container);

            // 5. Attach OcclusionDemoController
            OcclusionDemoController ctrl = rigRoot.AddComponent<OcclusionDemoController>();
            ctrl.rigContainer = container;
            ctrl.nearSlab = nearSlab.transform;
            ctrl.midSlab = midSlab.transform;
            ctrl.farSlab = farSlab.transform;
            ctrl.slidingProbe = probe.transform;
            ctrl.probeSpeed = 1.4f;
            ctrl.probeAmplitude = 2.4f;
            ctrl.alignCameraOnEnable = false;

            return rigRoot;
        }

        private GameObject CreateCalibrationSlab(string name, Vector3 pos, Vector3 scale, Material bodyMat, Material borderMat, GameObject parent)
        {
            GameObject slabGo = new GameObject(name);
            slabGo.transform.SetParent(parent.transform);
            slabGo.transform.localPosition = pos;
            slabGo.transform.localRotation = Quaternion.Euler(-5f, 25f, 0f);

            // Main Slab body
            GameObject body = GameObject.CreatePrimitive(PrimitiveType.Cube);
            body.name = "Body";
            body.transform.SetParent(slabGo.transform);
            body.transform.localPosition = Vector3.zero;
            body.transform.localRotation = Quaternion.identity;
            body.transform.localScale = scale;
            body.GetComponent<MeshRenderer>().sharedMaterial = bodyMat;

            // Diagnostic Frame Top/Sides
            GameObject frameTop = GameObject.CreatePrimitive(PrimitiveType.Cube);
            frameTop.name = "Frame_TopAccent";
            frameTop.transform.SetParent(slabGo.transform);
            frameTop.transform.localPosition = new Vector3(0f, scale.y * 0.5f, 0f);
            frameTop.transform.localScale = new Vector3(scale.x * 1.02f, 0.12f, scale.z * 1.3f);
            frameTop.GetComponent<MeshRenderer>().sharedMaterial = borderMat;

            GameObject frameEdge = GameObject.CreatePrimitive(PrimitiveType.Cube);
            frameEdge.name = "Frame_LeftAccent";
            frameEdge.transform.SetParent(slabGo.transform);
            frameEdge.transform.localPosition = new Vector3(-scale.x * 0.5f, 0f, 0f);
            frameEdge.transform.localScale = new Vector3(0.12f, scale.y * 1.02f, scale.z * 1.3f);
            frameEdge.GetComponent<MeshRenderer>().sharedMaterial = borderMat;

            return slabGo;
        }

        private GameObject BuildSlidingProbe(GameObject parent)
        {
            GameObject probeGo = new GameObject("Sliding_Depth_Probe");
            probeGo.transform.SetParent(parent.transform);
            probeGo.transform.localPosition = new Vector3(2.2f, 0.8f, -0.5f);
            probeGo.transform.localRotation = Quaternion.identity;

            // Glowing core sphere (Neon Magenta)
            GameObject core = GameObject.CreatePrimitive(PrimitiveType.Sphere);
            core.name = "ProbeCore";
            core.transform.SetParent(probeGo.transform);
            core.transform.localPosition = Vector3.zero;
            core.transform.localScale = new Vector3(0.7f, 0.7f, 0.7f);
            core.GetComponent<MeshRenderer>().sharedMaterial = matNeonMagenta;

            // Lateral sensor wings (Neon Violet)
            GameObject wing = GameObject.CreatePrimitive(PrimitiveType.Cube);
            wing.name = "SensorBar";
            wing.transform.SetParent(probeGo.transform);
            wing.transform.localPosition = Vector3.zero;
            wing.transform.localScale = new Vector3(1.3f, 0.15f, 0.15f);
            wing.GetComponent<MeshRenderer>().sharedMaterial = matNeonViolet;

            return probeGo;
        }

        private GameObject BuildDepthPrecisionDemoRig(GameObject root)
        {
            // Dedicated Child of scene root for Phase 5 Z-Fighting and Depth Precision Laboratory
            GameObject rigRoot = new GameObject("DepthPrecisionDemo_TestRig");
            rigRoot.transform.SetParent(root.transform);
            rigRoot.transform.position = Vector3.zero;
            rigRoot.transform.rotation = Quaternion.identity;

            // Container placed on the port (left) side, tilted to face the default camera perspective
            GameObject container = new GameObject("Rig_Precision_Container");
            container.transform.SetParent(rigRoot.transform);
            container.transform.position = new Vector3(-2.8f, 0.6f, -1.5f);
            container.transform.rotation = Quaternion.Euler(0f, 32f, 0f);

            // 1. High-tech Mounting Pedestal & Structural Frame
            GameObject pedestal = GameObject.CreatePrimitive(PrimitiveType.Cube);
            pedestal.name = "Pedestal_Base";
            pedestal.transform.SetParent(container.transform);
            pedestal.transform.localPosition = new Vector3(0f, -1.45f, 0f);
            pedestal.transform.localRotation = Quaternion.identity;
            pedestal.transform.localScale = new Vector3(2.8f, 0.14f, 0.5f);
            pedestal.GetComponent<MeshRenderer>().sharedMaterial = matHullDark;

            // Left Support Pylon
            GameObject pylonL = GameObject.CreatePrimitive(PrimitiveType.Cube);
            pylonL.name = "Pylon_Left";
            pylonL.transform.SetParent(container.transform);
            pylonL.transform.localPosition = new Vector3(-1.25f, 0f, 0f);
            pylonL.transform.localRotation = Quaternion.identity;
            pylonL.transform.localScale = new Vector3(0.12f, 2.8f, 0.18f);
            pylonL.GetComponent<MeshRenderer>().sharedMaterial = matHullArmor;

            // Right Support Pylon
            GameObject pylonR = GameObject.CreatePrimitive(PrimitiveType.Cube);
            pylonR.name = "Pylon_Right";
            pylonR.transform.SetParent(container.transform);
            pylonR.transform.localPosition = new Vector3(1.25f, 0f, 0f);
            pylonR.transform.localRotation = Quaternion.identity;
            pylonR.transform.localScale = new Vector3(0.12f, 2.8f, 0.18f);
            pylonR.GetComponent<MeshRenderer>().sharedMaterial = matHullArmor;

            // Header Bracket with Tech Gold accent
            GameObject header = GameObject.CreatePrimitive(PrimitiveType.Cube);
            header.name = "Header_Bracket";
            header.transform.SetParent(container.transform);
            header.transform.localPosition = new Vector3(0f, 1.45f, 0f);
            header.transform.localRotation = Quaternion.identity;
            header.transform.localScale = new Vector3(2.6f, 0.12f, 0.16f);
            header.GetComponent<MeshRenderer>().sharedMaterial = matTechGold;

            // Status Beacon (Neon Violet)
            GameObject beacon = GameObject.CreatePrimitive(PrimitiveType.Sphere);
            beacon.name = "PrecisionBeacon";
            beacon.transform.SetParent(container.transform);
            beacon.transform.localPosition = new Vector3(0f, 1.62f, 0f);
            beacon.transform.localRotation = Quaternion.identity;
            beacon.transform.localScale = new Vector3(0.22f, 0.22f, 0.22f);
            beacon.GetComponent<MeshRenderer>().sharedMaterial = matNeonViolet;

            // 2. Surface A: Cyan Reference Surface
            GameObject surfaceAGo = GameObject.CreatePrimitive(PrimitiveType.Cube);
            surfaceAGo.name = "SurfaceA_Cyan_Reference";
            surfaceAGo.transform.SetParent(container.transform);
            surfaceAGo.transform.localPosition = Vector3.zero;
            surfaceAGo.transform.localRotation = Quaternion.identity;
            surfaceAGo.transform.localScale = new Vector3(2.2f, 2.6f, 0.04f);
            surfaceAGo.GetComponent<MeshRenderer>().sharedMaterial = matNeonCyan;

            // 3. Surface B: Magenta Target Surface (nearly coincident in Z)
            GameObject surfaceBGo = GameObject.CreatePrimitive(PrimitiveType.Cube);
            surfaceBGo.name = "SurfaceB_Magenta_Target";
            surfaceBGo.transform.SetParent(container.transform);
            surfaceBGo.transform.localPosition = Vector3.zero;
            surfaceBGo.transform.localRotation = Quaternion.identity;
            surfaceBGo.transform.localScale = new Vector3(2.0f, 2.4f, 0.04f);
            surfaceBGo.GetComponent<MeshRenderer>().sharedMaterial = matNeonMagenta;

            // 4. Attach DepthPrecisionDemoController
            DepthPrecisionDemoController ctrl = rigRoot.AddComponent<DepthPrecisionDemoController>();
            ctrl.rigContainer = container;
            ctrl.surfaceA = surfaceAGo.transform;
            ctrl.surfaceB = surfaceBGo.transform;
            ctrl.coincidentOffset = 0.0002f;
            ctrl.correctedOffset = 0.08f;
            ctrl.IsDemoActive = true;
            ctrl.IsCoincident = true;

            return rigRoot;
        }
    }
}
