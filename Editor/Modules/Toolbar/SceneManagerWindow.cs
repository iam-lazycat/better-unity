using System.IO;
using UnityEditor;
using UnityEngine;

namespace LazyCat.BetterUnity
{
    public class SceneManagerWindow : EditorWindow
    {
        Vector2 _scroll;

        public static void Open()
        {
            var w = GetWindow<SceneManagerWindow>();
            w.titleContent = new GUIContent("Scene List", EditorGUIUtility.IconContent("d_SceneAsset Icon").image);
            w.minSize = new Vector2(300, 300);
            w.Show();
        }

        void OnGUI()
        {
            if (Event.current.type == EventType.MouseMove) Repaint();

            var    hidden  = ToolbarStorage.Data.hiddenScenes;
            var    paths   = ToolbarModule.GetAllScenePaths();
            string activeScene = UnityEditor.SceneManagement.EditorSceneManager.GetActiveScene().path;

            // ── toolbar ───────────────────────────────────────────────────
            EditorGUILayout.BeginHorizontal(EditorStyles.toolbar);
            GUILayout.Label("Scene List", EditorStyles.boldLabel);
            GUILayout.FlexibleSpace();
            if (hidden.Count > 0)
                GUILayout.Label($"{hidden.Count} hidden", EditorStyles.centeredGreyMiniLabel, GUILayout.Width(60));
            if (hidden.Count > 0 && GUILayout.Button("Show All", EditorStyles.toolbarButton, GUILayout.Width(64)))
            {
                hidden.Clear();
                ToolbarStorage.Save();
            }
            EditorGUILayout.EndHorizontal();

            // ── list ──────────────────────────────────────────────────────
            float toolbarH = EditorStyles.toolbar.fixedHeight;
            float rowH     = 22f;
            float totalH   = Mathf.Max(position.height - toolbarH, paths.Length * rowH + 4f);

            _scroll = GUI.BeginScrollView(
                new Rect(0, toolbarH, position.width, position.height - toolbarH),
                _scroll,
                new Rect(0, 0, position.width - 16, totalH));

            if (paths.Length == 0)
            {
                EditorGUI.LabelField(new Rect(0, 8, position.width - 16, 24),
                    "No scenes found in Assets/", EditorStyles.centeredGreyMiniLabel);
            }

            float y = 2f;
            bool dirty = false;

            foreach (var path in paths)
            {
                string guid    = AssetDatabase.AssetPathToGUID(path);
                bool   isHidden = hidden.Contains(guid);
                bool   isActive = path == activeScene;
                string name    = Path.GetFileNameWithoutExtension(path);
                string folder  = Path.GetDirectoryName(path) ?? "";
                if (folder.StartsWith("Assets/")) folder = folder.Substring(7);
                if (folder == "Assets")            folder = "";

                var rowR = new Rect(0, y, position.width - 16, rowH - 1);
                bool hov = rowR.Contains(Event.current.mousePosition);

                if (isActive)
                    EditorGUI.DrawRect(rowR, new Color(0.24f, 0.37f, 0.58f, 0.35f));
                else if (hov)
                    EditorGUI.DrawRect(rowR, new Color(0.5f, 0.5f, 0.5f, 0.1f));

                // eye icon button
                var eyeIcon = EditorGUIUtility.IconContent(isHidden ? "d_scenevis_hidden_hover" : "d_scenevis_visible_hover");
                if (GUI.Button(new Rect(4, y + 3, 16, 16), eyeIcon, GUIStyle.none))
                {
                    if (isHidden) hidden.Remove(guid);
                    else          hidden.Add(guid);
                    ToolbarStorage.Save();
                    dirty = true;
                }

                // scene name
                var nameStyle = new GUIStyle(EditorStyles.label)
                {
                    normal = { textColor = isHidden
                        ? new Color(0.45f, 0.45f, 0.45f)
                        : EditorStyles.label.normal.textColor }
                };
                EditorGUI.LabelField(new Rect(24, y + 2, position.width - 120, rowH - 2), name, nameStyle);

                // folder — right aligned, dimmed
                if (!string.IsNullOrEmpty(folder))
                    EditorGUI.LabelField(new Rect(24, y + 2, position.width - 36, rowH - 2), folder,
                        new GUIStyle(EditorStyles.miniLabel)
                        {
                            alignment = TextAnchor.MiddleRight,
                            normal    = { textColor = new Color(0.45f, 0.45f, 0.5f) }
                        });

                // bottom row divider
                EditorGUI.DrawRect(new Rect(0, y + rowH - 1, position.width - 16, 1), new Color(0f, 0f, 0f, 0.08f));

                y += rowH;
            }

            GUI.EndScrollView();

            if (dirty) Repaint();
        }
    }
}
