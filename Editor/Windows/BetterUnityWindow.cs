using UnityEditor;
using UnityEngine;

namespace LazyCat.BetterUnity
{
    public class BetterUnityWindow : EditorWindow
    {
        private Vector2 _scroll;

        public static void Open()
        {
            var w = GetWindow<BetterUnityWindow>();
            w.titleContent = new GUIContent("Better Unity", EditorGUIUtility.IconContent("d_UnityEditor.InspectorWindow").image);
            w.minSize = new Vector2(300, 200);
            w.Show();
        }

        void OnGUI()
        {
            DrawHeader();
            DrawLine();

            _scroll = EditorGUILayout.BeginScrollView(_scroll);
            AutoSaveModule.DrawModuleGUI();
            EditorGUILayout.EndScrollView();
        }

        void DrawHeader()
        {
            var r = EditorGUILayout.GetControlRect(false, 36);
            EditorGUI.DrawRect(r, new Color(0.15f, 0.15f, 0.18f));

            var catIcon = EditorGUIUtility.IconContent("d_UnityEditor.InspectorWindow");
            if (catIcon?.image != null)
                GUI.DrawTexture(new Rect(r.x + 8, r.y + 8, 18, 18), catIcon.image, ScaleMode.ScaleToFit, true);

            GUI.Label(new Rect(r.x + 32, r.y, r.width - 80, r.height), "Better Unity",
                new GUIStyle(EditorStyles.boldLabel)
                {
                    fontSize  = 14,
                    alignment = TextAnchor.MiddleLeft,
                    normal    = { textColor = new Color(0.95f, 0.75f, 0.3f) }
                });

            float bw = 28;
            float bx = r.xMax - 8;

            bx -= bw;
            if (GUI.Button(new Rect(bx, r.y + 4, bw, 28),
                new GUIContent(EditorGUIUtility.IconContent("_Help").image, "About"),
                EditorStyles.miniButton))
                AboutWindow.Open();

            bx -= bw + 2;
            if (GUI.Button(new Rect(bx, r.y + 4, bw, 28),
                new GUIContent(EditorGUIUtility.IconContent("d_SettingsIcon").image, "Settings"),
                EditorStyles.miniButton))
                SettingsService.OpenProjectSettings("Project/Better Unity");
        }

        void DrawLine()
        {
            var r = EditorGUILayout.GetControlRect(false, 1);
            EditorGUI.DrawRect(r, new Color(0.1f, 0.1f, 0.12f));
        }
    }
}
