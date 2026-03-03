using System.IO;
using UnityEditor;
using UnityEditor.Toolbars;
using UnityEngine;

namespace LazyCat.BetterUnity
{
    public class BookmarkManagerWindow : EditorWindow
    {
        Vector2 _scroll;
        int     _renamingIdx = -1;
        string  _renameBuffer = "";

        public static void Open()
        {
            var w = GetWindow<BookmarkManagerWindow>();
            w.titleContent = new GUIContent("Bookmarks", EditorGUIUtility.IconContent("d_Favorite").image);
            w.minSize = new Vector2(300, 300);
            w.Show();
        }

        void OnGUI()
        {
            if (Event.current.type == EventType.MouseMove) Repaint();

            var db = SceneBookmarkStorage.DB;

            // ── toolbar ───────────────────────────────────────────────────
            EditorGUILayout.BeginHorizontal(EditorStyles.toolbar);
            GUILayout.Label("Bookmarks", EditorStyles.boldLabel);
            GUILayout.FlexibleSpace();
            if (db.bookmarks.Count > 0)
                GUILayout.Label($"{db.bookmarks.Count} saved", EditorStyles.centeredGreyMiniLabel, GUILayout.Width(56));
            if (db.bookmarks.Count > 0 && GUILayout.Button("Clear All", EditorStyles.toolbarButton, GUILayout.Width(62)))
            {
                if (EditorUtility.DisplayDialog("Clear Bookmarks", "Delete all bookmarks?", "Clear", "Cancel"))
                {
                    db.bookmarks.Clear();
                    SceneBookmarkStorage.Save();
                    MainToolbar.Refresh("BetterUnity/Bookmarks");
                    _renamingIdx = -1;
                }
            }
            EditorGUILayout.EndHorizontal();

            // ── list ──────────────────────────────────────────────────────
            float toolbarH = EditorStyles.toolbar.fixedHeight;
            float rowH     = 28f;
            float totalH   = Mathf.Max(position.height - toolbarH, db.bookmarks.Count * rowH + 4f);

            _scroll = GUI.BeginScrollView(
                new Rect(0, toolbarH, position.width, position.height - toolbarH),
                _scroll,
                new Rect(0, 0, position.width - 16, totalH));

            if (db.bookmarks.Count == 0)
            {
                EditorGUI.LabelField(new Rect(0, 8, position.width - 16, 24),
                    "No bookmarks saved yet.", EditorStyles.centeredGreyMiniLabel);
                GUI.EndScrollView();
                return;
            }

            float y    = 2f;
            bool  dirty = false;

            for (int i = 0; i < db.bookmarks.Count; i++)
            {
                var    bm    = db.bookmarks[i];
                string scene = Path.GetFileNameWithoutExtension(bm.scenePath);

                var  rowR = new Rect(0, y, position.width - 16, rowH - 1);
                bool hov  = rowR.Contains(Event.current.mousePosition);

                if (hov && _renamingIdx != i)
                    EditorGUI.DrawRect(rowR, new Color(0.5f, 0.5f, 0.5f, 0.1f));

                // bookmark icon
                var ic = EditorGUIUtility.IconContent("d_Favorite");
                if (ic?.image != null)
                    GUI.DrawTexture(new Rect(4, y + 6, 14, 14), (Texture2D)ic.image, ScaleMode.ScaleToFit, true);

                if (_renamingIdx == i)
                {
                    // ── inline rename ─────────────────────────────────────
                    EditorGUI.DrawRect(rowR, new Color(0.18f, 0.28f, 0.45f, 0.35f));

                    string ctrlName = $"BmRename{i}";
                    GUI.SetNextControlName(ctrlName);
                    _renameBuffer = EditorGUI.TextField(
                        new Rect(22, y + 5, position.width - 122, 18), _renameBuffer);

                    bool confirm = GUI.Button(new Rect(position.width - 96, y + 5, 36, 18), "OK",     EditorStyles.miniButton);
                    bool cancel  = GUI.Button(new Rect(position.width - 56, y + 5, 50, 18), "Cancel", EditorStyles.miniButton);

                    if (Event.current.type == EventType.KeyDown)
                    {
                        if (Event.current.keyCode == KeyCode.Return || Event.current.keyCode == KeyCode.KeypadEnter)
                            { confirm = true; Event.current.Use(); }
                        if (Event.current.keyCode == KeyCode.Escape)
                            { cancel = true; Event.current.Use(); }
                    }

                    if (confirm)
                    {
                        string trimmed = _renameBuffer.Trim();
                        if (trimmed.Length > 0) bm.label = trimmed;
                        SceneBookmarkStorage.Save();
                        MainToolbar.Refresh("BetterUnity/Bookmarks");
                        _renamingIdx = -1;
                        dirty = true;
                    }
                    else if (cancel)
                    {
                        _renamingIdx = -1;
                    }
                }
                else
                {
                    // ── normal row ────────────────────────────────────────
                    // label
                    EditorGUI.LabelField(new Rect(22, y + 2, position.width - 120, 16),
                        bm.label,
                        new GUIStyle(EditorStyles.label) { fontStyle = FontStyle.Normal });

                    // scene name — right aligned, dimmed
                    if (!string.IsNullOrEmpty(scene))
                        EditorGUI.LabelField(new Rect(22, y + 2, position.width - 36, 16), scene,
                            new GUIStyle(EditorStyles.miniLabel)
                            {
                                alignment = TextAnchor.MiddleRight,
                                normal    = { textColor = new Color(0.45f, 0.45f, 0.5f) }
                            });

                    if (hov)
                    {
                        float btnX = position.width - 16;

                        btnX -= 48;
                        if (GUI.Button(new Rect(btnX, y + 5, 46, 18), "Delete", EditorStyles.miniButton))
                        {
                            if (_renamingIdx == i) _renamingIdx = -1;
                            db.bookmarks.RemoveAt(i);
                            SceneBookmarkStorage.Save();
                            MainToolbar.Refresh("BetterUnity/Bookmarks");
                            dirty = true;
                            break;
                        }

                        btnX -= 62;
                        if (GUI.Button(new Rect(btnX, y + 5, 60, 18), "Rename", EditorStyles.miniButton))
                        {
                            _renamingIdx  = i;
                            _renameBuffer = bm.label;
                            EditorGUI.FocusTextInControl($"BmRename{i}");
                        }
                    }
                }

                // divider
                EditorGUI.DrawRect(new Rect(0, y + rowH - 1, position.width - 16, 1), new Color(0f, 0f, 0f, 0.08f));

                y += rowH;
            }

            GUI.EndScrollView();

            if (dirty) Repaint();
        }
    }
}
