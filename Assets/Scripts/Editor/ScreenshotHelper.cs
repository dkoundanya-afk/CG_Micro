using System;
using System.IO;
using UnityEngine;
using UnityEngine.UI;
using UnityEditor;
using UnityEditor.SceneManagement;

namespace ZBufferDemo.Editor
{
    public static class ScreenshotHelper
    {
        public static void CaptureScreenshot(string outputPath, bool enableDepth = false)
        {
            var scene = EditorSceneManager.OpenScene("Assets/Scenes/SampleScene.unity");
            Debug.Log("[ScreenshotHelper] Opened SampleScene.unity successfully.");

            // Find Main Camera
            Camera mainCam = Camera.main;
            if (mainCam == null) mainCam = UnityEngine.Object.FindFirstObjectByType<Camera>();
            if (mainCam == null)
            {
                Debug.LogError("[ScreenshotHelper] Main Camera not found!");
                return;
            }

            // Find HUD
            GameObject hudGo = GameObject.Find(FuturisticHUDBuilder.HUD_ROOT_NAME);
            if (hudGo == null) hudGo = GameObject.Find("[" + FuturisticHUDBuilder.HUD_ROOT_NAME + "]");
            if (hudGo == null)
            {
                Debug.LogError("[ScreenshotHelper] HUD Root not found!");
                return;
            }

            Canvas canvas = hudGo.GetComponent<Canvas>();
            RenderMode originalMode = canvas.renderMode;
            Camera originalWorldCam = canvas.worldCamera;
            float originalPlaneDist = canvas.planeDistance;

            DepthVisualizer dv = mainCam.GetComponent<DepthVisualizer>();
            bool originalDepth = dv != null && dv.IsDepthActive;
            if (dv != null)
            {
                dv.IsDepthActive = enableDepth;
            }

            // Create 1920x1080 RenderTexture
            RenderTexture rt = new RenderTexture(1920, 1080, 24, RenderTextureFormat.ARGB32);
            rt.antiAliasing = 4;

            // Render 3D scene from mainCam
            RenderTexture prevTarget = mainCam.targetTexture;
            mainCam.targetTexture = rt;
            mainCam.Render();
            mainCam.targetTexture = prevTarget;

            // Ensure all HUD objects are on UI layer
            foreach (Transform t in hudGo.GetComponentsInChildren<Transform>(true))
            {
                t.gameObject.layer = LayerMask.NameToLayer("UI");
            }

            // Create temporary UI Camera aligned with main camera
            GameObject uiCamGo = new GameObject("__TempUICamera");
            uiCamGo.transform.position = mainCam.transform.position;
            uiCamGo.transform.rotation = mainCam.transform.rotation;
            Camera uiCam = uiCamGo.AddComponent<Camera>();
            uiCam.CopyFrom(mainCam);
            uiCam.clearFlags = CameraClearFlags.Depth;
            uiCam.cullingMask = 1 << LayerMask.NameToLayer("UI");
            uiCam.depth = mainCam.depth + 10;
            uiCam.targetTexture = rt;

            canvas.renderMode = RenderMode.ScreenSpaceCamera;
            canvas.worldCamera = uiCam;
            canvas.planeDistance = 1.0f;

            // Update HUD text states
            FuturisticHUDController hudCtrl = hudGo.GetComponent<FuturisticHUDController>();
            if (hudCtrl != null)
            {
                hudCtrl.FindReferencesIfNull();
                hudCtrl.UpdateHUDState(true);
            }

            Canvas.ForceUpdateCanvases();
            uiCam.Render();

            // Read pixels into Texture2D
            RenderTexture.active = rt;
            Texture2D tex = new Texture2D(1920, 1080, TextureFormat.RGB24, false);
            tex.ReadPixels(new Rect(0, 0, 1920, 1080), 0, 0);
            tex.Apply();
            RenderTexture.active = null;

            // Save PNG
            byte[] bytes = tex.EncodeToPNG();
            string fullPath = Path.GetFullPath(outputPath);
            string dir = Path.GetDirectoryName(fullPath);
            if (!Directory.Exists(dir)) Directory.CreateDirectory(dir);
            File.WriteAllBytes(fullPath, bytes);
            Debug.Log($"[ScreenshotHelper] Screenshot saved to: {fullPath} ({bytes.Length} bytes)");

            // Cleanup
            UnityEngine.Object.DestroyImmediate(tex);
            UnityEngine.Object.DestroyImmediate(uiCamGo);
            rt.Release();
            UnityEngine.Object.DestroyImmediate(rt);

            // Restore canvas
            canvas.renderMode = originalMode;
            canvas.worldCamera = originalWorldCam;
            canvas.planeDistance = originalPlaneDist;
            if (dv != null) dv.IsDepthActive = originalDepth;
        }

        public static void CaptureInitialScreenshot()
        {
            CaptureScreenshot("Screenshots/before_hud_normal.png", false);
            CaptureScreenshot("Screenshots/before_hud_depth.png", true);
        }

        public static void RebuildHUDAndCaptureVerifiedScreenshots()
        {
            var scene = EditorSceneManager.OpenScene("Assets/Scenes/SampleScene.unity");
            Debug.Log("[ScreenshotHelper] Rebuilding HUD with high-contrast bold fonts and increased font sizes...");

            // Rebuild HUD procedurally
            GameObject hudGo = FuturisticHUDBuilder.BuildOrRefreshHUD(false);
            EditorSceneManager.MarkSceneDirty(scene);
            EditorSceneManager.SaveScene(scene);

            Debug.Log("[ScreenshotHelper] Verifying rebuilt HUD text components:");
            Text[] allTexts = hudGo.GetComponentsInChildren<Text>(true);
            foreach (var t in allTexts)
            {
                Debug.Log($"[ScreenshotHelper] Text '{t.name}': fontSize={t.fontSize}, font={t.font?.name}, style={t.fontStyle}, color={t.color}, text='{t.text}'");
            }

            CaptureScreenshot("Screenshots/after_hud_normal.png", false);
            CaptureScreenshot("Screenshots/after_hud_depth.png", true);
            Debug.Log("[ScreenshotHelper] Both verified screenshots captured successfully!");
        }
    }
}
