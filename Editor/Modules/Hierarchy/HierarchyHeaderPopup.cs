using UnityEditor;
using UnityEngine;

namespace LazyCat.BetterUnity
{
    public class HierarchyHeaderPopup : EditorWindow
    {
        static GameObject     _target;
        static string         _key;
        HierarchyItemData     _draft;
        Vector2               _iconScroll;

        const float W = 286f;
        const float H = 340f;

        static readonly string[] IconOptions =
        {
            "",
            "d_Favorite",
            "d_Warning",
            "d_console.erroricon.sml",
            "d_console.infoicon.sml",
            "d_console.warnicon.sml",
            "d_SettingsIcon",
            "d_SceneAsset Icon",
            "d_cs Script Icon",
            "d_Camera Icon",
            "d_Light Icon",
            "d_Rigidbody Icon",
            "d_AudioSource Icon",
            "d_Animator Icon",
            "d_ParticleSystem Icon",
            "d_Canvas Icon",
            "d_Folder Icon",
            "d_GameObject Icon",
            "d_Prefab Icon",
            "d_FilterByType",
            "d_TransformTool",
            "d_NavMeshAgent Icon",
            "sv_icon_dot0_sml",
            "sv_icon_dot3_sml",
            "sv_icon_dot6_sml",
            "sv_icon_dot9_sml",
            "sv_icon_dot11_sml",
        };

        public static void Open(GameObject go)
        {
            _target = go;
            _key    = HierarchyModule.GetKey(go);

            var w = GetWindow<HierarchyHeaderPopup>(true, "Object Style", true);
            w.minSize = w.maxSize = new Vector2(W, H);

            var existing = HierarchyStorage.DB.Get(_key);
            w._draft     = Clone(existing) ?? new HierarchyItemData { key = _key };
            w.ShowUtility();
        }

        static HierarchyItemData Clone(HierarchyItemData src)
        {
            if (src == null) return null;
            return new HierarchyItemData
            {
                key   = src.key,
                style = src.style,
                color = src.color,
                icon  = src.icon,
                note  = src.note
            };
        }

        void OnGUI()
        {
            EditorGUILayout.BeginHorizontal(EditorStyles.toolbar);
            GUILayout.Label(_target != null ? _target.name : "Object Style", EditorStyles.boldLabel);
            GUILayout.FlexibleSpace();
            EditorGUILayout.EndHorizontal();

            EditorGUILayout.Space(6);

            EditorGUILayout.LabelField("Style", EditorStyles.boldLabel);
            int cur = (int)_draft.style;
            int nxt = GUILayout.SelectionGrid(cur, new[] { "Default", "Header" }, 2,
                EditorStyles.miniButton, GUILayout.Height(22));
            if (nxt != cur) _draft.style = (HierarchyItemStyle)nxt;

            EditorGUILayout.Space(10);

            if (_draft.style == HierarchyItemStyle.Header)
            {
                EditorGUILayout.LabelField("Color", EditorStyles.boldLabel);
                _draft.color = EditorGUILayout.ColorField(_draft.color, GUILayout.Width(W - 20));

                EditorGUILayout.Space(10);

                EditorGUILayout.LabelField("Icon", EditorStyles.boldLabel);

                const int   cols  = 7;
                const float sz    = 26f;
                const float gap   = 3f;

                _iconScroll = EditorGUILayout.BeginScrollView(_iconScroll, GUILayout.Height(sz * 4 + gap * 3 + 4));

                for (int row = 0; row * cols < IconOptions.Length; row++)
                {
                    var rowR = EditorGUILayout.GetControlRect(false, sz);
                    for (int col = 0; col < cols; col++)
                    {
                        int idx = row * cols + col;
                        if (idx >= IconOptions.Length) break;

                        string iconName = IconOptions[idx];
                        var    cellR    = new Rect(rowR.x + col * (sz + gap), rowR.y, sz, sz);
                        bool   isSel    = _draft.icon == iconName;

                        if (isSel)
                            EditorGUI.DrawRect(cellR, new Color(0.24f, 0.37f, 0.58f, 0.55f));

                        if (string.IsNullOrEmpty(iconName))
                        {
                            if (GUI.Button(cellR, new GUIContent("–", "No icon"), EditorStyles.miniLabel))
                                _draft.icon = "";
                        }
                        else
                        {
                            var ic = EditorGUIUtility.IconContent(iconName);
                            GUIContent btn = ic?.image != null
                                ? new GUIContent(ic.image, iconName)
                                : new GUIContent("?", iconName);

                            if (GUI.Button(cellR, btn, GUIStyle.none))
                                _draft.icon = iconName;
                        }
                    }
                }

                EditorGUILayout.EndScrollView();

                EditorGUILayout.Space(10);
            }

            EditorGUILayout.LabelField("Note", EditorStyles.boldLabel);
            _draft.note = EditorGUILayout.TextArea(_draft.note, GUILayout.Height(38));

            GUILayout.FlexibleSpace();

            EditorGUI.DrawRect(EditorGUILayout.GetControlRect(false, 1), new Color(0.1f, 0.1f, 0.12f));

            EditorGUILayout.BeginHorizontal();
            if (GUILayout.Button("Reset", EditorStyles.miniButton, GUILayout.Height(22)))
            {
                HierarchyStorage.DB.Remove(_key);
                HierarchyStorage.Save();
                HierarchyModule.InvalidateCaches();
                EditorApplication.RepaintHierarchyWindow();
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
            if (_draft.style == HierarchyItemStyle.Default && string.IsNullOrEmpty(_draft.note))
            {
                HierarchyStorage.DB.Remove(_key);
            }
            else
            {
                var e = HierarchyStorage.DB.GetOrCreate(_key);
                e.style = _draft.style;
                e.color = _draft.color;
                e.icon  = _draft.icon;
                e.note  = _draft.note;
            }
            HierarchyStorage.Save();
            HierarchyModule.InvalidateCaches();
            EditorApplication.RepaintHierarchyWindow();
        }
    }
}
