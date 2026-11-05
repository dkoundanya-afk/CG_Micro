using UnityEngine;
using UnityEditor;
using UnityEditor.SceneManagement;

namespace ZBufferDemo.Editor
{
    /// <summary>
    /// Editor utilities and menu actions for generating and managing the Z-Buffer demonstration scene and depth visualizer.
    /// </summary>
    public static class ZBufferSceneMenu
    {
        private const string MENU_ROOT = "Tools/Z-Buffer System/";

        [MenuItem(MENU_ROOT + "Setup Futuristic Scene (SampleScene)", false, 1)]
        public static void SetupFuturisticScene()
        {
            // Ensure Materials folder exists
            if (!AssetDatabase.IsValidFolder("Assets/Materials"))
            {
                AssetDatabase.CreateFolder("Assets", "Materials");
            }

            // Find or create builder host in the scene
            ZBufferSceneBuilder builder = Object.FindFirstObjectByType<ZBufferSceneBuilder>();
            if (builder == null)
            {
                GameObject hostGo = new GameObject("ZBuffer_Scene_Manager");
                builder = hostGo.AddComponent<ZBufferSceneBuilder>();
                Undo.RegisterCreatedObjectUndo(hostGo, "Create Z-Buffer Manager");
            }

            // 1. Build scene and save materials as permanent assets in Assets/Materials
            GameObject root = builder.BuildScene(true);

            // 2. Ensure Main Camera has DepthVisualizer component (defer selection)
            AttachDepthVisualizerToCamera(false);

            // 3. Create or Refresh Futuristic Sci-Fi HUD in Screen Space - Overlay (defer selection)
            FuturisticHUDBuilder.BuildOrRefreshHUD(false);

            // 4. Mark the scene dirty so Unity records changes
            var activeScene = EditorSceneManager.GetActiveScene();
            EditorSceneManager.MarkSceneDirty(activeScene);

            // 5. Select root once safely on the next editor tick to prevent UI Toolkit Inspector race
            EditorApplication.delayCall += () =>
            {
                if (root != null)
                {
                    Selection.activeGameObject = root;
                    EditorGUIUtility.PingObject(root);
                }
            };

            Debug.Log("<color=#00f0ff><b>[Z-Buffer Demo]</b> Futuristic Scene & HUD successfully constructed!</color>\n" +
                      "- Multi-layer occlusion: Foreground Drone/Asteroid -> Player Spaceship -> Asteroid Belt -> Orbital Megastructure.\n" +
                      "- OrbitCamera and DepthVisualizer attached to Main Camera.\n" +
                      "- Sci-Fi HUD operating in Screen Space - Overlay.\n" +
                      "- <b>Press PLAY</b>, then press <b>[Z]</b> to toggle Real-Time Depth Buffer Visualization!");
        }

        [MenuItem(MENU_ROOT + "Create or Refresh Futuristic HUD", false, 2)]
        public static void CreateOrRefreshHUD()
        {
            FuturisticHUDBuilder.BuildOrRefreshHUD(true);
        }

        [MenuItem(MENU_ROOT + "Attach Depth Visualizer to Main Camera", false, 3)]
        public static void AttachDepthVisualizerToCamera()
        {
            AttachDepthVisualizerToCamera(true);
        }

        public static void AttachDepthVisualizerToCamera(bool select)
        {
            Camera cam = Camera.main;
            if (cam == null)
            {
                cam = Object.FindFirstObjectByType<Camera>();
            }

            if (cam == null)
            {
                Debug.LogError("[Z-Buffer] No Camera found in the current scene!");
                return;
            }

            DepthVisualizer visualizer = cam.GetComponent<DepthVisualizer>();
            if (visualizer == null)
            {
                visualizer = Undo.AddComponent<DepthVisualizer>(cam.gameObject);
                Debug.Log($"<color=#00ff88>[Z-Buffer]</color> Attached DepthVisualizer to '{cam.gameObject.name}'.");
            }
            else
            {
                Debug.Log($"<color=#00ff88>[Z-Buffer]</color> DepthVisualizer already present on '{cam.gameObject.name}'.");
            }

            cam.depthTextureMode |= DepthTextureMode.Depth;
            var activeScene = EditorSceneManager.GetActiveScene();
            EditorSceneManager.MarkSceneDirty(activeScene);

            if (select)
            {
                EditorApplication.delayCall += () =>
                {
                    if (cam != null)
                    {
                        Selection.activeGameObject = cam.gameObject;
                        EditorGUIUtility.PingObject(cam.gameObject);
                    }
                };
            }
        }

        [MenuItem(MENU_ROOT + "Toggle Occlusion Demonstration (O)", false, 4)]
        public static void ToggleOcclusionDemonstration()
        {
            OcclusionDemoController demo = Object.FindFirstObjectByType<OcclusionDemoController>();
            if (demo != null)
            {
                demo.ToggleOcclusionDemo();
            }
            else
            {
                Debug.LogWarning("[Z-Buffer] OcclusionDemoController not found in scene. Please run 'Setup Futuristic Scene' first.");
            }
        }

        [MenuItem(MENU_ROOT + "Toggle Z-Fighting Lab (F)", false, 5)]
        public static void ToggleZFightingLab()
        {
            DepthPrecisionDemoController demo = Object.FindFirstObjectByType<DepthPrecisionDemoController>();
            if (demo != null)
            {
                demo.ToggleDemoActive();
            }
            else
            {
                Debug.LogWarning("[Z-Buffer] DepthPrecisionDemoController not found in scene. Please run 'Setup Futuristic Scene' first.");
            }
        }

        [MenuItem(MENU_ROOT + "Toggle Z-Fighting Correction (G)", false, 6)]
        public static void ToggleZFightingCorrection()
        {
            DepthPrecisionDemoController demo = Object.FindFirstObjectByType<DepthPrecisionDemoController>();
            if (demo != null)
            {
                demo.ToggleCorrectionMode();
            }
            else
            {
                Debug.LogWarning("[Z-Buffer] DepthPrecisionDemoController not found in scene. Please run 'Setup Futuristic Scene' first.");
            }
        }

        [MenuItem(MENU_ROOT + "Clear Generated Scene and HUD Objects", false, 20)]
        public static void ClearGeneratedScene()
        {
            // Safely clear selection if currently inspecting any object that is about to be destroyed
            if (Selection.activeGameObject != null)
            {
                GameObject currentHud = GameObject.Find(FuturisticHUDBuilder.HUD_ROOT_NAME);
                if (currentHud == null) currentHud = GameObject.Find("[" + FuturisticHUDBuilder.HUD_ROOT_NAME + "]");

                GameObject currentSceneRoot = GameObject.Find(ZBufferSceneBuilder.ROOT_NAME);

                if ((currentHud != null && (Selection.activeGameObject == currentHud || Selection.activeTransform.IsChildOf(currentHud.transform))) ||
                    (currentSceneRoot != null && (Selection.activeGameObject == currentSceneRoot || Selection.activeTransform.IsChildOf(currentSceneRoot.transform))))
                {
                    Selection.activeObject = null;
                }
            }

            ZBufferSceneBuilder.ClearExistingScene();

            GameObject hud = GameObject.Find(FuturisticHUDBuilder.HUD_ROOT_NAME);
            if (hud == null)
            {
                hud = GameObject.Find("[" + FuturisticHUDBuilder.HUD_ROOT_NAME + "]");
            }
            if (hud != null)
            {
                Undo.DestroyObjectImmediate(hud);
            }

            ZBufferSceneBuilder builder = Object.FindFirstObjectByType<ZBufferSceneBuilder>();
            if (builder != null)
            {
                Undo.DestroyObjectImmediate(builder.gameObject);
            }

            var activeScene = EditorSceneManager.GetActiveScene();
            EditorSceneManager.MarkSceneDirty(activeScene);

            Debug.Log("[Z-Buffer Demo] Cleared all generated scene objects.");
        }
    }

    /// <summary>
    /// Custom Inspector for ZBufferSceneBuilder to provide convenient one-click action buttons.
    /// </summary>
    [CustomEditor(typeof(ZBufferSceneBuilder))]
    public class ZBufferSceneBuilderEditor : UnityEditor.Editor
    {
        public override void OnInspectorGUI()
        {
            DrawDefaultInspector();

            EditorGUILayout.Space(12);
            EditorGUILayout.LabelField("Z-Buffer Scene Actions", EditorStyles.boldLabel);

            GUI.backgroundColor = new Color(0.2f, 0.85f, 1.0f);
            if (GUILayout.Button("Build / Rebuild Futuristic Scene", GUILayout.Height(36)))
            {
                ZBufferSceneBuilder builder = (ZBufferSceneBuilder)target;
                builder.BuildScene(true);
                ZBufferSceneMenu.AttachDepthVisualizerToCamera();
                var activeScene = EditorSceneManager.GetActiveScene();
                EditorSceneManager.MarkSceneDirty(activeScene);
            }

            GUI.backgroundColor = new Color(1.0f, 0.4f, 0.4f);
            if (GUILayout.Button("Clear Generated Objects", GUILayout.Height(28)))
            {
                ZBufferSceneBuilder.ClearExistingScene();
                var activeScene = EditorSceneManager.GetActiveScene();
                EditorSceneManager.MarkSceneDirty(activeScene);
            }
            GUI.backgroundColor = Color.white;
        }
    }

    /// <summary>
    /// Custom Inspector for DepthVisualizer with one-click toggles and range presets.
    /// </summary>
    [CustomEditor(typeof(DepthVisualizer))]
    public class DepthVisualizerEditor : UnityEditor.Editor
    {
        public override void OnInspectorGUI()
        {
            DrawDefaultInspector();

            DepthVisualizer dv = (DepthVisualizer)target;

            EditorGUILayout.Space(12);
            EditorGUILayout.LabelField("Interactive Depth Controls", EditorStyles.boldLabel);

            // Prominent Toggle Button
            bool isActive = dv.IsDepthActive;
            GUI.backgroundColor = isActive ? new Color(0.2f, 1.0f, 0.4f) : new Color(0.2f, 0.85f, 1.0f);
            string btnText = isActive ? "DEPTH VISUALIZATION: [ACTIVE] (Click to Turn OFF)" : "DEPTH VISUALIZATION: [OFF] (Click to Turn ON)";

            if (GUILayout.Button(btnText, GUILayout.Height(38)))
            {
                dv.ToggleDepth();
                EditorUtility.SetDirty(dv);
            }
            GUI.backgroundColor = Color.white;

            EditorGUILayout.Space(6);
            EditorGUILayout.LabelField("Range Presets (Eye-Space Distance):", EditorStyles.miniBoldLabel);

            EditorGUILayout.BeginHorizontal();
            if (GUILayout.Button("Close-Up (0.5 - 15m)"))
            {
                Undo.RecordObject(dv, "Set Depth Preset");
                dv.nearDisplayDistance = 0.5f;
                dv.farDisplayDistance = 15.0f;
                EditorUtility.SetDirty(dv);
            }
            if (GUILayout.Button("Balanced (0.5 - 65m)"))
            {
                Undo.RecordObject(dv, "Set Depth Preset");
                dv.nearDisplayDistance = 0.5f;
                dv.farDisplayDistance = 65.0f;
                EditorUtility.SetDirty(dv);
            }
            if (GUILayout.Button("Deep Scene (0.5 - 120m)"))
            {
                Undo.RecordObject(dv, "Set Depth Preset");
                dv.nearDisplayDistance = 0.5f;
                dv.farDisplayDistance = 120.0f;
                EditorUtility.SetDirty(dv);
            }
            EditorGUILayout.EndHorizontal();

            EditorGUILayout.Space(6);
            EditorGUILayout.HelpBox("In Play Mode, you can also press:\n- [Z] Toggle Depth Mode\n- [X] Invert Intensity (Near White vs Near Black)\n- [C] Cycle Depth Algorithm (Eye / 0-1 / Hardware Z)", MessageType.Info);
        }
    }
}
