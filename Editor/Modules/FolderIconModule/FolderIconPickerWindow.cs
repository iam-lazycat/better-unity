using System.IO;
using UnityEditor;
using UnityEngine;

namespace LazyCat.BetterUnity
{
    public class FolderIconPickerWindow : EditorWindow
    {
        string          _guid;
        string          _folderPath;
        FolderIconEntry _draft;
        Vector2         _scroll;

        const float W = 300f;
        const float H = 360f;

        public static void Open(string guid, string folderPath)
        {
            var w = GetWindow<FolderIconPickerWindow>(true, "Folder Style", true);
            w.minSize = w.maxSize = new Vector2(W, H);
            w._guid       = guid;
            w._folderPath = folderPath;
            var existing  = FolderIconStorage.DB.Get(guid);
            w._draft      = Clone(existing) ?? new FolderIconEntry { guid = guid };
            w.ShowUtility();
        }

        static FolderIconEntry Clone(FolderIconEntry src) => src == null ? null : new FolderIconEntry
        {
            guid           = src.guid,
            iconType       = src.iconType,
            tintColor      = src.tintColor,
            customIconGuid = src.customIconGuid,
            hasOverlay     = src.hasOverlay,
            overlayIcon    = src.overlayIcon,
        };

        void OnGUI()
        {
            // ── Header toolbar ─────────────────────────────────────────────────
            EditorGUILayout.BeginHorizontal(EditorStyles.toolbar);
            GUILayout.Label(Path.GetFileName(_folderPath), EditorStyles.boldLabel);
            GUILayout.FlexibleSpace();
            EditorGUILayout.EndHorizontal();

            _scroll = EditorGUILayout.BeginScrollView(_scroll);

            EditorGUILayout.Space(4);
            EditorGUILayout.LabelField("Style", EditorStyles.boldLabel);

            int cur = (int)_draft.iconType;
            int nxt = GUILayout.SelectionGrid(cur, new[] { "Default", "Color", "Custom" }, 3,
                EditorStyles.miniButton, GUILayout.Height(22));
            if (nxt != cur) _draft.iconType = (FolderIconType)nxt;

            EditorGUILayout.Space(8);

            // ── Color section ──────────────────────────────────────────────────
            if (_draft.iconType == FolderIconType.Color)
            {
                EditorGUILayout.LabelField("Preset Colors", EditorStyles.boldLabel);

                const int   cols = 6;
                const float sz   = 26f;
                const float gap  = 3f;

                for (int row = 0; row * cols < FolderIconPresets.Colors.Length; row++)
                {
                    var rowR = EditorGUILayout.GetControlRect(false, sz);
                    for (int col = 0; col < cols; col++)
                    {
                        int idx = row * cols + col;
                        if (idx >= FolderIconPresets.Colors.Length) break;

                        var (_, c) = FolderIconPresets.Colors[idx];
                        var r = new Rect(rowR.x + col * (sz + gap), rowR.y, sz, sz);

                        EditorGUI.DrawRect(r, c);

                        if (ColorClose(_draft.tintColor, c))
                            DrawBorder(r, Color.white, 2);

                        if (Event.current.type == EventType.MouseDown
                            && r.Contains(Event.current.mousePosition))
                        {
                            _draft.tintColor = c;
                            _draft.iconType  = FolderIconType.Color;
                            Event.current.Use();
                            Repaint();
                        }
                    }
                }

                EditorGUILayout.Space(4);
                EditorGUILayout.LabelField("Custom Color", EditorStyles.miniLabel);
                _draft.tintColor = EditorGUILayout.ColorField(_draft.tintColor, GUILayout.Width(160));
                EditorGUILayout.Space(4);
            }

            // ── Custom texture section ─────────────────────────────────────────
            if (_draft.iconType == FolderIconType.Custom)
            {
                EditorGUILayout.LabelField("Texture", EditorStyles.boldLabel);

                string texPath    = AssetDatabase.GUIDToAssetPath(_draft.customIconGuid);
                var    currentTex = string.IsNullOrEmpty(texPath)
                    ? null
                    : AssetDatabase.LoadAssetAtPath<Texture2D>(texPath);

                var newTex = EditorGUILayout.ObjectField("Icon Texture", currentTex,
                    typeof(Texture2D), false) as Texture2D;

                if (newTex != currentTex)
                {
                    _draft.customIconGuid = newTex != null
                        ? AssetDatabase.AssetPathToGUID(AssetDatabase.GetAssetPath(newTex))
                        : "";
                    FolderIconModule.InvalidateCustomCache();
                }

                if (newTex != null)
                {
                    var prevRect = GUILayoutUtility.GetRect(48, 48, GUILayout.ExpandWidth(false));
                    EditorGUI.DrawRect(new Rect(prevRect.x - 1, prevRect.y - 1,
                        prevRect.width + 2, prevRect.height + 2), new Color(0.3f, 0.3f, 0.35f));
                    GUI.DrawTexture(prevRect, newTex, ScaleMode.ScaleToFit, true);
                }

                EditorGUILayout.Space(4);
            }

            // ── Overlay section ────────────────────────────────────────────────
            EditorGUILayout.LabelField("Overlay Badge", EditorStyles.boldLabel);

            const float iconSz  = 24f;
            const float iconGap = 3f;
            float contentW = position.width - 20f;
            int   icols    = Mathf.Max(1, Mathf.FloorToInt((contentW + iconGap) / (iconSz + iconGap)));

            for (int row2 = 0; row2 * icols < FolderIconPresets.Overlays.Length; row2++)
            {
                var rowR = EditorGUILayout.GetControlRect(false, iconSz);
                for (int col = 0; col < icols; col++)
                {
                    int idx = row2 * icols + col;
                    if (idx >= FolderIconPresets.Overlays.Length) break;

                    var (olabel, oicon) = FolderIconPresets.Overlays[idx];
                    bool isNone = string.IsNullOrEmpty(oicon);
                    bool isSel  = isNone
                        ? !_draft.hasOverlay
                        : (_draft.hasOverlay && _draft.overlayIcon == oicon);

                    var r = new Rect(rowR.x + col * (iconSz + iconGap), rowR.y, iconSz, iconSz);

                    if (isSel)
                        EditorGUI.DrawRect(r, new Color(0.24f, 0.37f, 0.58f, 0.55f));

                    GUIContent oc;
                    if (isNone)
                        oc = new GUIContent("—", "No overlay");
                    else
                    {
                        var loaded = EditorGUIUtility.IconContent(oicon);
                        oc = loaded?.image != null
                            ? new GUIContent(loaded.image, olabel)
                            : new GUIContent(olabel[0].ToString(), olabel);
                    }

                    if (GUI.Button(r, oc, EditorStyles.miniButton))
                    {
                        if (isNone) { _draft.hasOverlay = false; _draft.overlayIcon = ""; }
                        else        { _draft.hasOverlay = true;  _draft.overlayIcon = oicon; }
                        Repaint();
                    }
                }
            }

            EditorGUILayout.EndScrollView();

            // ── Footer ─────────────────────────────────────────────────────────
            EditorGUILayout.Space(4);
            EditorGUI.DrawRect(EditorGUILayout.GetControlRect(false, 1), new Color(0.1f, 0.1f, 0.12f));
            EditorGUILayout.BeginHorizontal();
            if (GUILayout.Button("Reset", EditorStyles.miniButton, GUILayout.Height(22)))
            {
                FolderIconStorage.DB.Remove(_guid);
                FolderIconStorage.Save();
                EditorApplication.RepaintProjectWindow();
                Close();
            }
            GUILayout.FlexibleSpace();
            if (GUILayout.Button("Apply", EditorStyles.miniButton, GUILayout.Width(70), GUILayout.Height(22)))
            {
                Commit();
                Close();
            }
            EditorGUILayout.EndHorizontal();
            EditorGUILayout.Space(4);
        }

        void Commit()
        {
            if (_draft.iconType == FolderIconType.Default)
            {
                FolderIconStorage.DB.Remove(_guid);
            }
            else
            {
                var e = FolderIconStorage.DB.GetOrCreate(_guid);
                e.iconType       = _draft.iconType;
                e.tintColor      = _draft.tintColor;
                e.customIconGuid = _draft.customIconGuid;
                e.hasOverlay     = _draft.hasOverlay;
                e.overlayIcon    = _draft.overlayIcon;
            }
            FolderIconStorage.Save();
            FolderIconModule.InvalidateCustomCache();
            EditorApplication.RepaintProjectWindow();
        }

        static bool ColorClose(Color a, Color b) =>
            Mathf.Abs(a.r - b.r) < 0.01f &&
            Mathf.Abs(a.g - b.g) < 0.01f &&
            Mathf.Abs(a.b - b.b) < 0.01f;

        static void DrawBorder(Rect r, Color c, int t)
        {
            EditorGUI.DrawRect(new Rect(r.x,        r.y,        r.width, t), c);
            EditorGUI.DrawRect(new Rect(r.x,        r.yMax - t, r.width, t), c);
            EditorGUI.DrawRect(new Rect(r.x,        r.y,        t, r.height), c);
            EditorGUI.DrawRect(new Rect(r.xMax - t, r.y,        t, r.height), c);
        }
    }
}
