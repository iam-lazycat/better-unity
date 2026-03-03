using System;
using System.IO;
using UnityEditor;
using UnityEngine;

namespace LazyCat.BetterUnity
{
    public static class ScreenshotModule
    {
        public static void Capture()
        {
            var s = ToolbarStorage.Screenshot;

            if (s.target == ScreenshotTarget.GameView)
                CaptureGameView(s);
            else
                CaptureSceneView(s);
        }

        static void CaptureGameView(ScreenshotSettings s)
        {
            int  w    = GetWidth(s);
            int  h    = GetHeight(s);
            var  path = GetOutputPath(s);

            if (s.hideUI)
            {
                HideCanvases(true);
                EditorApplication.delayCall += () =>
                {
                    DoGameViewCapture(w, h, path, s.openAfterCapture);
                    HideCanvases(false);
                };
            }
            else
            {
                DoGameViewCapture(w, h, path, s.openAfterCapture);
            }
        }

        static void DoGameViewCapture(int w, int h, string path, bool open)
        {
            var cam = Camera.main;
            if (cam == null)
            {
                Debug.LogWarning("[Better Unity] Screenshot: no main camera found.");
                return;
            }

            var rt  = new RenderTexture(w, h, 24);
            var prev = cam.targetTexture;
            cam.targetTexture = rt;
            cam.Render();
            cam.targetTexture = prev;

            RenderTexture.active = rt;
            var tex = new Texture2D(w, h, TextureFormat.RGB24, false);
            tex.ReadPixels(new Rect(0, 0, w, h), 0, 0);
            tex.Apply();
            RenderTexture.active = null;
            UnityEngine.Object.DestroyImmediate(rt);

            File.WriteAllBytes(path, tex.EncodeToPNG());
            UnityEngine.Object.DestroyImmediate(tex);
            AssetDatabase.Refresh();
            Log(path, open);
        }

        static void CaptureSceneView(ScreenshotSettings s)
        {
            var sv = SceneView.lastActiveSceneView;
            if (sv == null)
            {
                Debug.LogWarning("[Better Unity] Screenshot: no active Scene View.");
                return;
            }

            int  w    = GetWidth(s);
            int  h    = GetHeight(s);
            var  path = GetOutputPath(s);

            var rt = new RenderTexture(w, h, 24);
            sv.camera.targetTexture = rt;
            sv.camera.Render();
            sv.camera.targetTexture = null;

            RenderTexture.active = rt;
            var tex = new Texture2D(w, h, TextureFormat.RGB24, false);
            tex.ReadPixels(new Rect(0, 0, w, h), 0, 0);
            tex.Apply();
            RenderTexture.active = null;
            UnityEngine.Object.DestroyImmediate(rt);

            File.WriteAllBytes(path, tex.EncodeToPNG());
            UnityEngine.Object.DestroyImmediate(tex);
            AssetDatabase.Refresh();
            Log(path, s.openAfterCapture);
        }

        static void HideCanvases(bool hide)
        {
            foreach (var canvas in UnityEngine.Object.FindObjectsByType<Canvas>(UnityEngine.FindObjectsSortMode.None))
            {
                if (canvas.renderMode != RenderMode.WorldSpace)
                    canvas.enabled = !hide;
            }
        }

        static int GetWidth(ScreenshotSettings s)
        {
            if (s.useCustomSize) return s.customWidth;
            var p = GetPreset(s);
            return p?.width ?? 1920;
        }

        static int GetHeight(ScreenshotSettings s)
        {
            if (s.useCustomSize) return s.customHeight;
            var p = GetPreset(s);
            return p?.height ?? 1080;
        }

        static ScreenshotPreset GetPreset(ScreenshotSettings s)
        {
            if (s.presets == null || s.presets.Count == 0) return null;
            return s.presets[Mathf.Clamp(s.selectedPresetIndex, 0, s.presets.Count - 1)];
        }

        static string GetOutputPath(ScreenshotSettings s)
        {
            var dir = s.outputPath;
            if (!Path.IsPathRooted(dir))
                dir = Path.Combine(Application.dataPath, "..", dir);
            Directory.CreateDirectory(dir);
            string stamp = DateTime.Now.ToString("yyyy-MM-dd_HH-mm-ss");
            return Path.GetFullPath(Path.Combine(dir, $"screenshot_{stamp}.png"));
        }

        static void Log(string path, bool open)
        {
            Debug.Log($"[Better Unity] Screenshot saved: {path}");
            if (open) EditorUtility.RevealInFinder(path);
        }
    }
}
