using UnityEditor;
using UnityEngine;

namespace LazyCat.BetterUnity
{
    public class ScreenshotWindow : EditorWindow
    {
        public static void Open()
        {
            var w = GetWindow<ScreenshotWindow>();
            w.titleContent = new GUIContent("Screenshot", EditorGUIUtility.IconContent("d_SceneViewCamera").image);
            w.minSize = new Vector2(300, 320);
            w.Show();
        }

        void OnGUI()
        {
            var s   = ToolbarStorage.Screenshot;
            float y = 0;

            // ── toolbar ───────────────────────────────────────────────────
            EditorGUILayout.BeginHorizontal(EditorStyles.toolbar);
            GUILayout.Label("Screenshot", EditorStyles.boldLabel);
            GUILayout.FlexibleSpace();
            EditorGUILayout.EndHorizontal();

            float pad = 8f;
            float fw  = position.width - pad * 2;

            y = 4f + EditorStyles.toolbar.fixedHeight;

            GUI.BeginGroup(new Rect(0, EditorStyles.toolbar.fixedHeight, position.width, position.height - EditorStyles.toolbar.fixedHeight - 38));

            y = 4f;

            // ── Target ────────────────────────────────────────────────────
            y = SectionHeader(y, pad, fw, "Target");

            int curTarget = s.target == ScreenshotTarget.GameView ? 0 : 1;
            int newTarget = GUI.SelectionGrid(new Rect(pad, y, fw, 22), curTarget,
                new[] { "Game View", "Scene View" }, 2, EditorStyles.miniButton);
            if (newTarget != curTarget)
            {
                s.target = newTarget == 0 ? ScreenshotTarget.GameView : ScreenshotTarget.SceneView;
                ToolbarStorage.Save();
            }
            y += 28;

            if (s.target == ScreenshotTarget.GameView)
            {
                bool hideUI = EditorGUI.Toggle(new Rect(pad, y + 1, 16, 16), s.hideUI);
                EditorGUI.LabelField(new Rect(pad + 20, y, fw - 20, 18), "Hide UI on capture", EditorStyles.label);
                if (hideUI != s.hideUI) { s.hideUI = hideUI; ToolbarStorage.Save(); }
                y += 22;
            }

            y += 6;

            // ── Resolution ────────────────────────────────────────────────
            y = SectionHeader(y, pad, fw, "Resolution");

            bool custom = EditorGUI.Toggle(new Rect(pad, y + 1, 16, 16), s.useCustomSize);
            EditorGUI.LabelField(new Rect(pad + 20, y, fw - 20, 18), "Custom size", EditorStyles.label);
            if (custom != s.useCustomSize) { s.useCustomSize = custom; ToolbarStorage.Save(); }
            y += 22;

            if (s.useCustomSize)
            {
                EditorGUI.LabelField(new Rect(pad, y, fw, 16), "Width", EditorStyles.miniLabel);
                int cw = EditorGUI.IntField(new Rect(pad, y + 16, Mathf.Min(fw, 120), 18), s.customWidth);
                if (cw != s.customWidth) { s.customWidth = Mathf.Max(1, cw); ToolbarStorage.Save(); }
                y += 38;

                EditorGUI.LabelField(new Rect(pad, y, fw, 16), "Height", EditorStyles.miniLabel);
                int ch = EditorGUI.IntField(new Rect(pad, y + 16, Mathf.Min(fw, 120), 18), s.customHeight);
                if (ch != s.customHeight) { s.customHeight = Mathf.Max(1, ch); ToolbarStorage.Save(); }
                y += 38;
            }
            else if (s.presets != null && s.presets.Count > 0)
            {
                var names = new string[s.presets.Count];
                for (int i = 0; i < s.presets.Count; i++)
                    names[i] = $"{s.presets[i].name}   {s.presets[i].width} x {s.presets[i].height}";

                EditorGUI.LabelField(new Rect(pad, y, fw, 16), "Preset", EditorStyles.miniLabel);
                int sel = EditorGUI.Popup(new Rect(pad, y + 16, fw, 18), s.selectedPresetIndex, names);
                if (sel != s.selectedPresetIndex) { s.selectedPresetIndex = sel; ToolbarStorage.Save(); }
                y += 38;
            }

            y += 6;

            // ── Output ────────────────────────────────────────────────────
            y = SectionHeader(y, pad, fw, "Output");

            EditorGUI.LabelField(new Rect(pad, y, fw, 16), "Folder", EditorStyles.miniLabel);
            y += 16;
            float browseW = 54f;
            string newPath = EditorGUI.TextField(new Rect(pad, y, fw - browseW - 4, 18), s.outputPath);
            if (newPath != s.outputPath) { s.outputPath = newPath; ToolbarStorage.Save(); }
            if (GUI.Button(new Rect(pad + fw - browseW, y, browseW, 18), "Browse", EditorStyles.miniButton))
            {
                string picked = EditorUtility.OpenFolderPanel("Output Folder", s.outputPath, "");
                if (!string.IsNullOrEmpty(picked)) { s.outputPath = picked; ToolbarStorage.Save(); }
            }
            y += 24;

            bool openAfter = EditorGUI.Toggle(new Rect(pad, y + 1, 16, 16), s.openAfterCapture);
            EditorGUI.LabelField(new Rect(pad + 20, y, fw - 20, 18), "Reveal after capture", EditorStyles.label);
            if (openAfter != s.openAfterCapture) { s.openAfterCapture = openAfter; ToolbarStorage.Save(); }
            y += 22;

            GUI.EndGroup();

            // ── capture button ────────────────────────────────────────────
            EditorGUI.DrawRect(new Rect(0, position.height - 38, position.width, 1), new Color(0f, 0f, 0f, 0.2f));
            if (GUI.Button(new Rect(8, position.height - 30, position.width - 16, 22), "Capture", EditorStyles.miniButton))
            {
                ScreenshotModule.Capture();
                ShowNotification(new GUIContent("Captured"), 1.2f);
            }
        }

        float SectionHeader(float y, float pad, float fw, string title)
        {
            EditorGUI.LabelField(new Rect(pad, y, fw, 18), title, EditorStyles.boldLabel);
            EditorGUI.DrawRect(new Rect(pad, y + 18, fw, 1), new Color(0f, 0f, 0f, 0.2f));
            return y + 24;
        }
    }
}
