using System.Collections.Generic;
using UnityEditor;
using UnityEngine;

namespace LazyCat.BetterUnity
{
    [InitializeOnLoad]
    public static class FolderIconModule
    {
        static Texture2D _folderTex;
        static readonly Dictionary<string, Texture2D> _customCache = new Dictionary<string, Texture2D>();

        static FolderIconModule()
        {
            EditorApplication.projectWindowItemOnGUI -= OnProjectWindowItem;
            EditorApplication.projectWindowItemOnGUI += OnProjectWindowItem;
        }

        static void OnProjectWindowItem(string guid, Rect selectionRect)
        {
            if (!BetterUnityPrefs.FolderIconEnabled) return;
            if (Event.current.type != EventType.Repaint) return;

            string path = AssetDatabase.GUIDToAssetPath(guid);
            if (string.IsNullOrEmpty(path) || !AssetDatabase.IsValidFolder(path)) return;

            var entry = FolderIconStorage.DB.Get(guid);
            if (entry == null || entry.iconType == FolderIconType.Default) return;

            bool isLargeIcon = selectionRect.height > 20f;
            Rect iconRect    = CalcIconRect(selectionRect, isLargeIcon);

            DrawIcon(iconRect, entry, isLargeIcon);
        }

        static Rect CalcIconRect(Rect selectionRect, bool largeIcon)
        {
            float scale = Mathf.Clamp(BetterUnityPrefs.FolderIconScale, 0.25f, 1f);

            if (!largeIcon)
            {
                float fullSz = selectionRect.height;
                float sz     = fullSz * scale;
                // Keep vertically centred, left-aligned like Unity's own icon
                float offsetY = (fullSz - sz) * 0.5f;
                return new Rect(selectionRect.x, selectionRect.y + offsetY, sz, sz);
            }

            const float labelH = 16f;
            float fullSize = selectionRect.height - labelH;
            float size     = fullSize * scale;
            // Centre horizontally and vertically in the available icon space
            float cx = selectionRect.x + (selectionRect.width - size) * 0.5f;
            float cy = selectionRect.y + (fullSize - size) * 0.5f;
            return new Rect(cx, cy, size, size);
        }

        // Erase Unity's pre-drawn folder icon by painting the real project-window
        // background over it. We ask Unity's own GUISkin for the project browser
        // row style so the colour is correct for any theme / Unity version.
        static Color? _cachedBG;
        static Color ProjectWindowBG
        {
            get
            {
                if (_cachedBG.HasValue) return _cachedBG.Value;

                // "ProjectBrowserTableBGEven" is Unity's internal style for list rows.
                // In grid/icon view Unity uses "ProjectBrowserIconAreaBg".
                // Both live in the built-in GUISkin exposed via EditorGUIUtility.
                GUISkin skin = EditorGUIUtility.GetBuiltinSkin(
                    EditorGUIUtility.isProSkin ? EditorSkin.Scene : EditorSkin.Inspector);

                Color fallback = EditorGUIUtility.isProSkin
                    ? new Color(0.19f, 0.19f, 0.19f, 1f)
                    : new Color(0.76f, 0.76f, 0.76f, 1f);

                GUIStyle style = skin?.FindStyle("ProjectBrowserIconAreaBg");
                if (style?.normal?.background != null)
                {
                    Texture2D t = style.normal.background;
                    try
                    {
                        Color s = t.GetPixel(t.width / 2, t.height / 2);
                        s.a = 1f;
                        _cachedBG = s;
                        return s;
                    }
                    catch { }
                }

                _cachedBG = fallback;
                return fallback;
            }
        }

        static void EraseUnityIcon(Rect r) => EditorGUI.DrawRect(r, ProjectWindowBG);

        static void DrawIcon(Rect iconRect, FolderIconEntry entry, bool largeIcon)
        {
            EraseUnityIcon(iconRect);

            switch (entry.iconType)
            {
                case FolderIconType.Color:
                {
                    // If a custom texture is assigned, tint that texture with the chosen
                    // colour. If not, fall back to tinting the built-in folder icon.
                    var tex = GetCustomTexture(entry.customIconGuid);
                    var prev = GUI.color;
                    GUI.color = entry.tintColor;
                    GUI.DrawTexture(iconRect, tex != null ? tex : FolderTex, ScaleMode.ScaleToFit, true);
                    GUI.color = prev;
                    break;
                }

                case FolderIconType.Custom:
                {
                    // Raw custom texture — no colour tint applied.
                    var tex = GetCustomTexture(entry.customIconGuid);
                    GUI.DrawTexture(iconRect, tex != null ? tex : FolderTex, ScaleMode.ScaleToFit, true);
                    break;
                }
            }

            if (entry.hasOverlay && !string.IsNullOrEmpty(entry.overlayIcon))
            {
                var oc = EditorGUIUtility.IconContent(entry.overlayIcon);
                if (oc?.image != null)
                {
                    float os    = Mathf.Max(8f, iconRect.width * 0.36f);
                    var   oRect = new Rect(iconRect.xMax - os, iconRect.yMax - os, os, os);
                    GUI.DrawTexture(oRect, (Texture2D)oc.image, ScaleMode.ScaleToFit, true);
                }
            }
        }

        static Texture2D FolderTex
        {
            get
            {
                if (_folderTex == null)
                    _folderTex = EditorGUIUtility.FindTexture("d_Folder Icon")
                              ?? EditorGUIUtility.FindTexture("Folder Icon");
                return _folderTex;
            }
        }

        // Kept for any external callers; internally FolderTex property is used.
        static void DrawTintedFolder(Rect iconRect, Color tint)
        {
            if (FolderTex == null) return;
            var prev  = GUI.color;
            GUI.color = tint;
            GUI.DrawTexture(iconRect, FolderTex, ScaleMode.ScaleToFit, true);
            GUI.color = prev;
        }

        static Texture2D GetCustomTexture(string guid)
        {
            if (string.IsNullOrEmpty(guid)) return null;
            if (_customCache.TryGetValue(guid, out var t) && t != null) return t;
            string p = AssetDatabase.GUIDToAssetPath(guid);
            if (string.IsNullOrEmpty(p)) return null;
            t = AssetDatabase.LoadAssetAtPath<Texture2D>(p);
            _customCache[guid] = t;
            return t;
        }

        public static void InvalidateCustomCache() => _customCache.Clear();

        // ── Quick-colour context menu ──────────────────────────────────────────

        [MenuItem("Assets/Folder Style/Color/Red",    false, 1110)] static void CR()  => ApplyColor(0);
        [MenuItem("Assets/Folder Style/Color/Orange", false, 1111)] static void CO()  => ApplyColor(1);
        [MenuItem("Assets/Folder Style/Color/Yellow", false, 1112)] static void CY()  => ApplyColor(2);
        [MenuItem("Assets/Folder Style/Color/Lime",   false, 1113)] static void CLi() => ApplyColor(3);
        [MenuItem("Assets/Folder Style/Color/Green",  false, 1114)] static void CG()  => ApplyColor(4);
        [MenuItem("Assets/Folder Style/Color/Teal",   false, 1115)] static void CT()  => ApplyColor(5);
        [MenuItem("Assets/Folder Style/Color/Cyan",   false, 1116)] static void CCy() => ApplyColor(6);
        [MenuItem("Assets/Folder Style/Color/Blue",   false, 1117)] static void CB()  => ApplyColor(7);
        [MenuItem("Assets/Folder Style/Color/Indigo", false, 1118)] static void CI()  => ApplyColor(8);
        [MenuItem("Assets/Folder Style/Color/Purple", false, 1119)] static void CP()  => ApplyColor(9);
        [MenuItem("Assets/Folder Style/Color/Pink",   false, 1120)] static void CPk() => ApplyColor(10);
        [MenuItem("Assets/Folder Style/Color/Gray",   false, 1121)] static void CGr() => ApplyColor(11);

        [MenuItem("Assets/Folder Style/Color/Red",    true)] static bool VR()  => IsFolderSelected();
        [MenuItem("Assets/Folder Style/Color/Orange", true)] static bool VO()  => IsFolderSelected();
        [MenuItem("Assets/Folder Style/Color/Yellow", true)] static bool VY()  => IsFolderSelected();
        [MenuItem("Assets/Folder Style/Color/Lime",   true)] static bool VLi() => IsFolderSelected();
        [MenuItem("Assets/Folder Style/Color/Green",  true)] static bool VG()  => IsFolderSelected();
        [MenuItem("Assets/Folder Style/Color/Teal",   true)] static bool VTl() => IsFolderSelected();
        [MenuItem("Assets/Folder Style/Color/Cyan",   true)] static bool VCy() => IsFolderSelected();
        [MenuItem("Assets/Folder Style/Color/Blue",   true)] static bool VBl() => IsFolderSelected();
        [MenuItem("Assets/Folder Style/Color/Indigo", true)] static bool VIn() => IsFolderSelected();
        [MenuItem("Assets/Folder Style/Color/Purple", true)] static bool VPr() => IsFolderSelected();
        [MenuItem("Assets/Folder Style/Color/Pink",   true)] static bool VPk() => IsFolderSelected();
        [MenuItem("Assets/Folder Style/Color/Gray",   true)] static bool VGr() => IsFolderSelected();

        static void ApplyColor(int idx)
        {
            foreach (var obj in Selection.objects)
            {
                string p = AssetDatabase.GetAssetPath(obj);
                if (!AssetDatabase.IsValidFolder(p)) continue;
                var e = FolderIconStorage.DB.GetOrCreate(AssetDatabase.AssetPathToGUID(p));
                e.iconType  = FolderIconType.Color;
                e.tintColor = FolderIconPresets.Colors[idx].color;
            }
            FolderIconStorage.Save();
            EditorApplication.RepaintProjectWindow();
        }

        [MenuItem("Assets/Folder Style/Set Style...", false, 1100)]
        static void OpenPicker()
        {
            var obj = Selection.activeObject;
            if (obj == null) return;
            string p = AssetDatabase.GetAssetPath(obj);
            FolderIconPickerWindow.Open(AssetDatabase.AssetPathToGUID(p), p);
        }
        [MenuItem("Assets/Folder Style/Set Style...", true)]
        static bool OpenPickerValidate() => IsFolderSelected();

        [MenuItem("Assets/Folder Style/Clear Style", false, 1130)]
        static void ClearStyle()
        {
            foreach (var obj in Selection.objects)
            {
                string p = AssetDatabase.GetAssetPath(obj);
                if (!AssetDatabase.IsValidFolder(p)) continue;
                FolderIconStorage.DB.Remove(AssetDatabase.AssetPathToGUID(p));
            }
            FolderIconStorage.Save();
            EditorApplication.RepaintProjectWindow();
        }
        [MenuItem("Assets/Folder Style/Clear Style", true)]
        static bool ClearStyleValidate() => IsFolderSelected();

        static bool IsFolderSelected()
        {
            if (!BetterUnityPrefs.FolderIconEnabled || Selection.activeObject == null) return false;
            return AssetDatabase.IsValidFolder(AssetDatabase.GetAssetPath(Selection.activeObject));
        }
    }
}
