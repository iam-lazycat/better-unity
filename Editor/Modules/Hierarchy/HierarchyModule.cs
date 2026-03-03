using System;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;

namespace LazyCat.BetterUnity
{
    [InitializeOnLoad]
    public static class HierarchyModule
    {
        const float IconSz     = 14f;
        const float IconGap    = 1f;
        const float ToggleW    = 16f;
        const float SettingsSz = 13f;

        static readonly (Type type, string icon)[] ComponentIconMap =
        {
            (typeof(Camera),                      "d_Camera Icon"),
            (typeof(Light),                       "d_Light Icon"),
            (typeof(ReflectionProbe),             "d_ReflectionProbe Icon"),
            (typeof(Rigidbody),                   "d_Rigidbody Icon"),
            (typeof(Rigidbody2D),                 "d_Rigidbody2D Icon"),
            (typeof(Collider),                    "d_BoxCollider Icon"),
            (typeof(Collider2D),                  "d_BoxCollider2D Icon"),
            (typeof(CharacterController),         "d_CharacterController Icon"),
            (typeof(AudioSource),                 "d_AudioSource Icon"),
            (typeof(AudioListener),               "d_AudioListener Icon"),
            (typeof(Animator),                    "d_Animator Icon"),
            (typeof(Animation),                   "d_Animation Icon"),
            (typeof(Canvas),                      "d_Canvas Icon"),
            (typeof(ParticleSystem),              "d_ParticleSystem Icon"),
            (typeof(MeshRenderer),                "d_MeshRenderer Icon"),
            (typeof(SkinnedMeshRenderer),         "d_SkinnedMeshRenderer Icon"),
            (typeof(SpriteRenderer),              "d_SpriteRenderer Icon"),
            (typeof(LineRenderer),                "d_LineRenderer Icon"),
            (typeof(TrailRenderer),               "d_TrailRenderer Icon"),
            (typeof(LODGroup),                    "d_LODGroup Icon"),
            (typeof(UnityEngine.AI.NavMeshAgent), "d_NavMeshAgent Icon"),
            (typeof(MonoBehaviour),               "d_cs Script Icon"),
        };

        static readonly Dictionary<int, string>      _keyCache  = new();
        static readonly Dictionary<int, Texture2D[]> _iconCache = new();

        static float _lastRowY = float.MinValue;
        static int   _rowIndex = 0;

        static HierarchyModule()
        {
            EditorApplication.hierarchyWindowItemOnGUI -= OnItem;
            EditorApplication.hierarchyWindowItemOnGUI += OnItem;
            EditorApplication.hierarchyChanged         += InvalidateCaches;
        }

        public static void InvalidateCaches()
        {
            _keyCache.Clear();
            _iconCache.Clear();
        }

        public static string GetKey(GameObject go)
        {
            int id = go.GetInstanceID();
            if (_keyCache.TryGetValue(id, out var k)) return k;
            k = go.scene.path + "|" + GetPath(go.transform);
            _keyCache[id] = k;
            return k;
        }

        static string GetPath(Transform t) =>
            t.parent == null ? t.name : GetPath(t.parent) + "/" + t.name;

        static void OnItem(int id, Rect r)
        {
            if (!BetterUnityPrefs.HierarchyEnabled) return;

#pragma warning disable CS0618
            var go = EditorUtility.InstanceIDToObject(id) as GameObject;
#pragma warning restore CS0618
            if (go == null) return;

            bool repaint = Event.current.type == EventType.Repaint;
            bool hover   = r.Contains(Event.current.mousePosition);

            if (repaint)
            {
                if (r.y <= _lastRowY) _rowIndex = 0;
                _lastRowY = r.y;
                _rowIndex++;
            }

            var  data     = HierarchyStorage.DB.Get(GetKey(go));
            bool isHeader = data != null && data.style == HierarchyItemStyle.Header;

            if (repaint)
            {
                if (isHeader)
                {
                    DrawHeaderBG(r, go, data);
                }
                else
                {
                    if (BetterUnityPrefs.HierarchyZebraEnabled && _rowIndex % 2 == 0)
                        EditorGUI.DrawRect(new Rect(0, r.y, Screen.width, r.height),
                            BetterUnityPrefs.HierarchyZebraColor);

                    if (BetterUnityPrefs.HierarchyLinesEnabled)
                        DrawLines(r, go);

                    if (BetterUnityPrefs.HierarchyDimInactiveEnabled && !go.activeSelf)
                        EditorGUI.DrawRect(new Rect(0, r.y, Screen.width, r.height),
                            new Color(0f, 0f, 0f, 0.28f));
                }
            }

            float rightX = r.xMax;

            if (BetterUnityPrefs.HierarchyActiveToggleEnabled)
            {
                rightX -= ToggleW + 2f;
                HandleActiveToggle(new Rect(rightX, r.y, ToggleW, r.height), go);
            }

            var settingsR = new Rect(rightX - SettingsSz - 2f,
                                     r.y + (r.height - SettingsSz) * 0.5f,
                                     SettingsSz, SettingsSz);
            rightX -= SettingsSz + 3f;

            if (repaint && (hover || isHeader))
                DrawSettingsIcon(settingsR, isHeader);

            if (Event.current.type == EventType.MouseDown
                && settingsR.Contains(Event.current.mousePosition))
            {
                HierarchyHeaderPopup.Open(go);
                Event.current.Use();
            }

            EditorGUIUtility.AddCursorRect(settingsR, MouseCursor.Link);

            if (!isHeader && BetterUnityPrefs.HierarchyComponentIconsEnabled && repaint)
                DrawComponentIcons(r, rightX, go);
        }

        static void DrawHeaderBG(Rect r, GameObject go, HierarchyItemData data)
        {
            EditorGUI.DrawRect(new Rect(0, r.y, Screen.width, r.height), data.color);

            if (!string.IsNullOrEmpty(data.icon))
            {
                var ic = EditorGUIUtility.IconContent(data.icon);
                if (ic?.image != null)
                {
                    float sz = r.height - 4f;
                    GUI.DrawTexture(new Rect(r.x + 2, r.y + 2, sz, sz),
                        (Texture2D)ic.image, ScaleMode.ScaleToFit, true);
                }
            }

            float lum     = 0.299f * data.color.r + 0.587f * data.color.g + 0.114f * data.color.b;
            Color textCol = lum > 0.55f ? new Color(0.08f, 0.08f, 0.08f) : Color.white;

            GUI.Label(new Rect(0, r.y, Screen.width, r.height), go.name,
                new GUIStyle(EditorStyles.boldLabel)
                {
                    alignment = TextAnchor.MiddleCenter,
                    normal    = { textColor = textCol }
                });
        }

        // Positions derived from r.x and realDepth — no hardcoded base offset needed.
        //
        //   ownX  = r.x - indentW * 0.5          (connector for this item)
        //   ancX  = ownX - indentW * steps        (connector for an ancestor steps levels up)
        //
        // This is correct regardless of Unity's actual tree indent values because we
        // measure realDepth from the transform and treat r.x as ground truth.
        static void DrawLines(Rect r, GameObject go)
        {
            int realDepth = 0;
            for (Transform p = go.transform.parent; p != null; p = p.parent) realDepth++;
            if (realDepth == 0) return;

            float indentW = BetterUnityPrefs.HierarchyIndentWidth;
            Color col     = BetterUnityPrefs.HierarchyLineColor;
            float midY    = r.y + r.height * 0.5f;
            float ownX    = r.x - indentW * 0.5f;

            bool isLast = go.transform.parent != null &&
                          go.transform.GetSiblingIndex() == go.transform.parent.childCount - 1;

            EditorGUI.DrawRect(new Rect(ownX, r.y, 1f, isLast ? r.height * 0.5f + 1f : r.height), col);
            EditorGUI.DrawRect(new Rect(ownX + 1f, midY, indentW * 0.5f - 1f, 1f), col);

            Transform ancestor = go.transform.parent;
            for (int steps = 1; steps < realDepth; steps++)
            {
                if (ancestor == null) break;
                ancestor = ancestor.parent;
                if (ancestor == null) break;

                float ancX = ownX - steps * indentW;

                bool ancIsLast;
                if (ancestor.parent != null)
                    ancIsLast = ancestor.GetSiblingIndex() == ancestor.parent.childCount - 1;
                else
                    ancIsLast = ancestor.GetSiblingIndex() >= ancestor.gameObject.scene.rootCount - 1;

                if (!ancIsLast)
                    EditorGUI.DrawRect(new Rect(ancX, r.y, 1f, r.height), col);
            }
        }

        static void HandleActiveToggle(Rect r, GameObject go)
        {
            var toggleR = new Rect(r.x, r.y + (r.height - 14f) * 0.5f, 14f, 14f);

            if (Event.current.type == EventType.MouseDown
                && toggleR.Contains(Event.current.mousePosition))
            {
                Undo.RecordObject(go, "Toggle Active");
                go.SetActive(!go.activeSelf);
                Event.current.Use();
            }

            if (Event.current.type == EventType.Repaint)
                EditorGUI.Toggle(toggleR, go.activeSelf);
        }

        static void DrawSettingsIcon(Rect r, bool isHeader)
        {
            var ic = EditorGUIUtility.IconContent("d_SettingsIcon");
            if (ic?.image == null) return;
            var prev  = GUI.color;
            GUI.color = new Color(1f, 1f, 1f, isHeader ? 0.9f : 0.5f);
            GUI.DrawTexture(r, (Texture2D)ic.image, ScaleMode.ScaleToFit, true);
            GUI.color = prev;
        }

        static void DrawComponentIcons(Rect r, float rightEdge, GameObject go)
        {
            var   icons   = GetComponentIcons(go);
            if (icons.Length == 0) return;
            float opacity = BetterUnityPrefs.HierarchyIconOpacity;
            float x       = rightEdge;
            for (int i = icons.Length - 1; i >= 0; i--)
            {
                x -= IconSz + IconGap;
                if (x < r.x + 80f) break;
                var prev  = GUI.color;
                GUI.color = new Color(1f, 1f, 1f, opacity);
                GUI.DrawTexture(new Rect(x, r.y + (r.height - IconSz) * 0.5f, IconSz, IconSz),
                    icons[i], ScaleMode.ScaleToFit, true);
                GUI.color = prev;
            }
        }

        static Texture2D[] GetComponentIcons(GameObject go)
        {
            int id = go.GetInstanceID();
            if (_iconCache.TryGetValue(id, out var cached)) return cached;

            var comps = go.GetComponents<Component>();
            var list  = new List<Texture2D>();
            int cap   = BetterUnityPrefs.HierarchyIconCap;

            foreach (var comp in comps)
            {
                if (list.Count >= cap) break;

                if (comp == null)
                {
                    var warn = EditorGUIUtility.IconContent("console.warnicon.sml");
                    if (warn?.image is Texture2D wt) list.Add(wt);
                    continue;
                }

                var type = comp.GetType();
                if (type == typeof(Transform) || type == typeof(RectTransform)) continue;

                string iconName = null;
                foreach (var (mapType, mapIcon) in ComponentIconMap)
                {
                    if (mapType.IsAssignableFrom(type)) { iconName = mapIcon; break; }
                }
                if (iconName == null) continue;

                var content = EditorGUIUtility.IconContent(iconName);
                if (content?.image is Texture2D tex) list.Add(tex);
            }

            var arr = list.ToArray();
            _iconCache[id] = arr;
            return arr;
        }
    }
}
