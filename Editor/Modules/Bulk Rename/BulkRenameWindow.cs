using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;
using UnityEditor;
using UnityEngine;
using Object = UnityEngine.Object;

namespace LazyCat.BetterUnity
{
    public class BulkRenameWindow : EditorWindow
    {
        enum RenameMode { Replace, Prefix, Suffix, Sequence, Regex, Case }
        enum CaseMode   { Lower, Upper, Title, Camel, Pascal }
        enum TargetType { Selection, Folder }

        // ── mode state ────────────────────────────────────────────────────
        RenameMode _mode       = RenameMode.Replace;
        TargetType _targetType = TargetType.Selection;

        string _find        = "";
        string _replaceWith = "";
        bool   _caseSens    = false;

        string _prefix = "";
        string _suffix = "";

        string _seqBase    = "";
        int    _seqStart   = 1;
        int    _seqStep    = 1;
        int    _seqPad     = 2;
        bool   _seqAppend  = true;

        string _rxPattern = "";
        string _rxReplace = "";

        CaseMode _caseMode = CaseMode.Title;

        // ── preview ───────────────────────────────────────────────────────
        class Entry { public string orig, next; public bool unchanged, conflict; public Object obj; public bool isAsset; }
        List<Entry> _entries     = new List<Entry>();
        bool        _dirty       = true;
        Vector2     _prevScroll;

        // ── layout constants ──────────────────────────────────────────────
        const float MODE_W   = 110f;
        const float DIVIDER  = 1f;
        const float TB_H     = 21f;   // toolbar height

        static readonly string[] MODE_LABELS = { "Find & Replace", "Add Prefix", "Add Suffix", "Number Sequence", "Regex Replace", "Change Case" };
        static readonly string[] TARGET_OPTS  = { "Selection", "Assets in Folder" };

        // ── open ──────────────────────────────────────────────────────────
        public static void Open()
        {
            if (!BetterUnityPrefs.BulkRenameEnabled)
            {
                EditorUtility.DisplayDialog("Module Disabled",
                    "Bulk Rename is disabled. Enable it in Project Settings → Better Unity.", "OK");
                return;
            }
            var w = GetWindow<BulkRenameWindow>();
            w.titleContent = new GUIContent("Bulk Rename");
            w.minSize = new Vector2(500, 380);
            w.Show();
        }

        void OnEnable()  { Selection.selectionChanged += Dirty; Dirty(); }
        void OnDisable() { Selection.selectionChanged -= Dirty; }
        void Dirty()     { _dirty = true; Repaint(); }

        // ── OnGUI ─────────────────────────────────────────────────────────
        void OnGUI()
        {
            if (_dirty) Refresh();

            float tbY  = 0;
            float bodyY = TB_H;
            float bodyH = position.height - TB_H;

            DrawTopBar(new Rect(0, tbY, position.width, TB_H));

            // three columns: mode list | fields | preview
            float fieldsW  = Mathf.Clamp(position.width * 0.38f, 160, 300);
            float previewX = MODE_W + DIVIDER + fieldsW + DIVIDER;
            float previewW = position.width - previewX;

            DrawModeList (new Rect(0,                      bodyY, MODE_W,  bodyH));
            Line         (new Rect(MODE_W,                 bodyY, DIVIDER, bodyH));
            DrawFields   (new Rect(MODE_W + DIVIDER,       bodyY, fieldsW, bodyH));
            Line         (new Rect(MODE_W + DIVIDER + fieldsW, bodyY, DIVIDER, bodyH));
            DrawPreview  (new Rect(previewX,               bodyY, previewW, bodyH));

            if (Event.current.type == EventType.MouseMove) Repaint();
        }

        // ── top bar ───────────────────────────────────────────────────────
        void DrawTopBar(Rect r)
        {
            EditorGUILayout.BeginHorizontal(EditorStyles.toolbar);

            GUILayout.Label("Target:", EditorStyles.miniLabel, GUILayout.Width(42));
            var nt = (TargetType)EditorGUILayout.Popup((int)_targetType, TARGET_OPTS,
                EditorStyles.toolbarPopup, GUILayout.Width(120));
            if (nt != _targetType) { _targetType = nt; Dirty(); }

            GUILayout.FlexibleSpace();

            // stats
            int willChange  = _entries.Count(e => !e.unchanged && !e.conflict);
            int conflicts   = _entries.Count(e => e.conflict);
            if (conflicts > 0)
                GUILayout.Label($"⚠ {conflicts} conflict{(conflicts > 1 ? "s" : "")}", EditorStyles.miniLabel, GUILayout.Width(80));

            EditorGUI.BeginDisabledGroup(willChange == 0);
            if (GUILayout.Button($"Rename  {willChange}", EditorStyles.toolbarButton, GUILayout.Width(80)))
                Apply();
            EditorGUI.EndDisabledGroup();

            EditorGUILayout.EndHorizontal();
        }

        // ── mode list (left sidebar) ──────────────────────────────────────
        void DrawModeList(Rect r)
        {
            GUI.BeginGroup(r);
            EditorGUI.DrawRect(new Rect(0, 0, r.width, r.height), new Color(0f, 0f, 0f, 0.05f));

            float y = 6f;
            for (int i = 0; i < MODE_LABELS.Length; i++)
            {
                bool sel = (int)_mode == i;
                var  row = new Rect(0, y, r.width, 26);
                bool hov = row.Contains(Event.current.mousePosition);

                if (sel)        EditorGUI.DrawRect(row, new Color(0.24f, 0.37f, 0.58f, 0.35f));
                else if (hov)   EditorGUI.DrawRect(row, new Color(0.5f, 0.5f, 0.5f, 0.12f));

                // left accent bar on selected
                if (sel) EditorGUI.DrawRect(new Rect(0, y + 2, 2, 22), new Color(0.95f, 0.75f, 0.3f));

                var style = new GUIStyle(EditorStyles.label)
                {
                    fontSize = 11,
                    normal   = { textColor = sel ? EditorStyles.label.normal.textColor : new Color(0.6f, 0.6f, 0.6f) },
                    fontStyle= sel ? FontStyle.Bold : FontStyle.Normal
                };
                EditorGUI.LabelField(new Rect(10, y + 4, r.width - 12, 18), MODE_LABELS[i], style);

                if (Event.current.type == EventType.MouseDown && row.Contains(Event.current.mousePosition))
                {
                    _mode = (RenameMode)i;
                    Dirty();
                    Event.current.Use();
                }
                EditorGUIUtility.AddCursorRect(row, MouseCursor.Link);
                y += 28;
            }

            GUI.EndGroup();
        }

        // ── fields panel (middle) ─────────────────────────────────────────
        void DrawFields(Rect r)
        {
            GUI.BeginGroup(r);

            // use GUILayout inside a fixed area so text fields fill width cleanly
            GUILayout.BeginArea(new Rect(0, 0, r.width, r.height));
            GUILayout.Space(8);

            switch (_mode)
            {
                case RenameMode.Replace:  DrawReplaceFields();  break;
                case RenameMode.Prefix:   DrawPrefixFields();   break;
                case RenameMode.Suffix:   DrawSuffixFields();   break;
                case RenameMode.Sequence: DrawSequenceFields(); break;
                case RenameMode.Regex:    DrawRegexFields();    break;
                case RenameMode.Case:     DrawCaseFields();     break;
            }

            GUILayout.EndArea();
            GUI.EndGroup();
        }

        // -- field helpers use GUILayout so width is automatic ─────────────
        void FieldLabel(string text, string tooltip = null)
        {
            GUILayout.Label(tooltip != null ? new GUIContent(text, tooltip) : new GUIContent(text),
                EditorStyles.miniLabel);
        }

        string TextField(string val)
        {
            var nv = EditorGUILayout.TextField(val);
            if (nv != val) Dirty();
            return nv;
        }

        bool Toggle(bool val, string label, string tooltip = null)
        {
            EditorGUILayout.BeginHorizontal();
            var content = tooltip != null ? new GUIContent(label, tooltip) : new GUIContent(label);
            var nv = EditorGUILayout.ToggleLeft(content, val);
            EditorGUILayout.EndHorizontal();
            if (nv != val) Dirty();
            return nv;
        }

        void DrawReplaceFields()
        {
            FieldLabel("Find");
            _find = TextField(_find);
            GUILayout.Space(6);

            FieldLabel("Replace With");
            _replaceWith = TextField(_replaceWith);
            GUILayout.Space(8);

            _caseSens = Toggle(_caseSens, "Case Sensitive", "Match exact letter casing when searching.");
        }

        void DrawPrefixFields()
        {
            FieldLabel("Prefix Text", "Added to the beginning of every name.");
            _prefix = TextField(_prefix);
            GUILayout.Space(6);
            EditorGUILayout.HelpBox("Result:  <prefix> + original name", MessageType.None);
        }

        void DrawSuffixFields()
        {
            FieldLabel("Suffix Text", "Added to the end of every name, before the file extension.");
            _suffix = TextField(_suffix);
            GUILayout.Space(6);
            EditorGUILayout.HelpBox("Result:  original name + <suffix>", MessageType.None);
        }

        void DrawSequenceFields()
        {
            FieldLabel("Base Name", "Leave empty to keep each object's original name.");
            _seqBase = TextField(_seqBase);
            GUILayout.Space(6);

            FieldLabel("Start");
            var ns = EditorGUILayout.IntField(_seqStart);
            if (ns != _seqStart) { _seqStart = Mathf.Max(0, ns); Dirty(); }
            GUILayout.Space(4);

            FieldLabel("Step", "How much the number increases per item.");
            var nst = EditorGUILayout.IntField(_seqStep);
            if (nst != _seqStep) { _seqStep = Mathf.Max(1, nst); Dirty(); }
            GUILayout.Space(4);

            FieldLabel("Zero Padding", "Pad with leading zeros. e.g. padding 3 → 001");
            var np = EditorGUILayout.IntSlider(_seqPad, 1, 6);
            if (np != _seqPad) { _seqPad = np; Dirty(); }
            GUILayout.Space(8);

            _seqAppend = Toggle(_seqAppend, "Number after name");
        }

        void DrawRegexFields()
        {
            FieldLabel("Pattern", "Regular expression to match in each name.");
            var np = EditorGUILayout.TextField(_rxPattern);
            if (np != _rxPattern) { _rxPattern = np; Dirty(); }
            GUILayout.Space(6);

            FieldLabel("Replacement", "Replacement text. Use $1, $2 for capture groups.");
            var nr = EditorGUILayout.TextField(_rxReplace);
            if (nr != _rxReplace) { _rxReplace = nr; Dirty(); }
            GUILayout.Space(8);

            if (!string.IsNullOrEmpty(_rxPattern))
            {
                bool ok = IsValidRegex(_rxPattern);
                EditorGUILayout.HelpBox(ok ? "Pattern valid." : "Invalid regex pattern.", ok ? MessageType.None : MessageType.Error);
            }
        }

        void DrawCaseFields()
        {
            GUILayout.Label("Case Style", EditorStyles.miniLabel);
            GUILayout.Space(4);

            string[] labels = { "lowercase", "UPPERCASE", "Title Case", "camelCase", "PascalCase" };
            for (int i = 0; i < labels.Length; i++)
            {
                bool sel = (int)_caseMode == i;

                EditorGUILayout.BeginHorizontal();
                var prev = GUI.backgroundColor;
                GUI.backgroundColor = sel ? new Color(0.95f, 0.75f, 0.3f) : prev;
                if (GUILayout.Button(labels[i], EditorStyles.miniButton))
                {
                    _caseMode = (CaseMode)i;
                    Dirty();
                }
                GUI.backgroundColor = prev;
                EditorGUILayout.EndHorizontal();
                GUILayout.Space(2);
            }
        }

        // ── preview panel (right) ─────────────────────────────────────────
        void DrawPreview(Rect r)
        {
            GUI.BeginGroup(r);

            // column headers
            float hH   = 22f;
            float col2 = r.width * 0.5f;
            EditorGUI.DrawRect(new Rect(0, 0, r.width, hH), new Color(0f, 0f, 0f, 0.07f));
            EditorGUI.LabelField(new Rect(6,    3, col2 - 10, 16), "Original",     EditorStyles.boldLabel);
            EditorGUI.LabelField(new Rect(col2, 3, col2 - 6,  16), "Result",       EditorStyles.boldLabel);
            EditorGUI.DrawRect(new Rect(0, hH - 1, r.width, 1), new Color(0f, 0f, 0f, 0.15f));

            if (_entries.Count == 0)
            {
                EditorGUI.LabelField(new Rect(0, hH + 10, r.width, 20),
                    "Select objects or assets to rename.", EditorStyles.centeredGreyMiniLabel);
                GUI.EndGroup();
                return;
            }

            const float ROW_H = 22f;
            float viewH = _entries.Count * ROW_H + 4f;

            _prevScroll = GUI.BeginScrollView(
                new Rect(0, hH, r.width, r.height - hH),
                _prevScroll,
                new Rect(0, hH, r.width - 14, viewH + hH));

            float y = hH + 2f;
            for (int i = 0; i < _entries.Count; i++)
            {
                var e    = _entries[i];
                var rowR = new Rect(0, y, r.width - 14, ROW_H);

                if (i % 2 == 1) EditorGUI.DrawRect(rowR, new Color(0f, 0f, 0f, 0.04f));

                // original
                EditorGUI.LabelField(new Rect(6, y + 3, col2 - 10, 16), e.orig, EditorStyles.miniLabel);

                // result
                Color rc;
                string label;
                if (e.conflict)        { rc = new Color(0.9f, 0.35f, 0.35f); label = "⚠ conflict"; }
                else if (e.unchanged)  { rc = new Color(0.45f, 0.45f, 0.45f); label = e.orig; }
                else                   { rc = new Color(0.35f, 0.78f, 0.4f);  label = e.next; }

                var rs = new GUIStyle(EditorStyles.miniLabel) { normal = { textColor = rc } };
                EditorGUI.LabelField(new Rect(col2, y + 3, col2 - 6, 16), label, rs);

                y += ROW_H;
            }

            GUI.EndScrollView();
            GUI.EndGroup();
        }

        // ── logic ─────────────────────────────────────────────────────────
        void Refresh()
        {
            _dirty = false;
            _entries.Clear();

            var targets = GatherTargets();
            int seq = _seqStart;
            foreach (var (name, obj, isAsset) in targets)
            {
                string next = Compute(name, seq);
                seq += _seqStep;
                _entries.Add(new Entry { orig = name, next = next, unchanged = next == name, obj = obj, isAsset = isAsset });
            }

            var byName = _entries.GroupBy(e => e.next.ToLowerInvariant());
            foreach (var g in byName)
                if (g.Count() > 1)
                    foreach (var e in g) if (!e.unchanged) e.conflict = true;

            Repaint();
        }

        List<(string name, Object obj, bool isAsset)> GatherTargets()
        {
            var list = new List<(string, Object, bool)>();

            if (_targetType == TargetType.Selection)
            {
                foreach (var go in Selection.gameObjects)
                    list.Add((go.name, go, false));

                foreach (var o in Selection.objects)
                {
                    if (o == null) continue;
                    string p = AssetDatabase.GetAssetPath(o);
                    if (!string.IsNullOrEmpty(p) && !list.Any(r => r.Item2 == o))
                        list.Add((System.IO.Path.GetFileNameWithoutExtension(p), o, true));
                }
            }
            else
            {
                foreach (var o in Selection.objects)
                {
                    string p = AssetDatabase.GetAssetPath(o);
                    if (string.IsNullOrEmpty(p) || !System.IO.Directory.Exists(p)) continue;
                    foreach (var guid in AssetDatabase.FindAssets("", new[] { p }))
                    {
                        string ap = AssetDatabase.GUIDToAssetPath(guid);
                        if (System.IO.Directory.Exists(ap)) continue;
                        var a = AssetDatabase.LoadAssetAtPath<Object>(ap);
                        if (a != null) list.Add((System.IO.Path.GetFileNameWithoutExtension(ap), a, true));
                    }
                }
            }
            return list;
        }

        string Compute(string original, int seq)
        {
            switch (_mode)
            {
                case RenameMode.Replace:
                    if (string.IsNullOrEmpty(_find)) return original;
                    return _caseSens
                        ? original.Replace(_find, _replaceWith)
                        : Regex.Replace(original, Regex.Escape(_find), _replaceWith ?? "", RegexOptions.IgnoreCase);

                case RenameMode.Prefix:
                    return _prefix + original;

                case RenameMode.Suffix:
                    return original + _suffix;

                case RenameMode.Sequence:
                    string num  = seq.ToString().PadLeft(_seqPad, '0');
                    string base_ = string.IsNullOrEmpty(_seqBase) ? original : _seqBase;
                    return _seqAppend ? base_ + num : num + base_;

                case RenameMode.Regex:
                    if (string.IsNullOrEmpty(_rxPattern) || !IsValidRegex(_rxPattern)) return original;
                    return Regex.Replace(original, _rxPattern, _rxReplace ?? "");

                case RenameMode.Case:
                    return ApplyCase(original);

                default: return original;
            }
        }

        string ApplyCase(string s)
        {
            switch (_caseMode)
            {
                case CaseMode.Lower: return s.ToLower();
                case CaseMode.Upper: return s.ToUpper();
                case CaseMode.Title:
                    return System.Globalization.CultureInfo.CurrentCulture.TextInfo.ToTitleCase(s.ToLower());
                case CaseMode.Camel:
                    var cw = Regex.Split(s, @"[\s_\-]+").Where(w => w.Length > 0).ToArray();
                    if (cw.Length == 0) return s;
                    return cw[0].ToLower() + string.Concat(cw.Skip(1).Select(w => char.ToUpper(w[0]) + w.Substring(1).ToLower()));
                case CaseMode.Pascal:
                    var pw = Regex.Split(s, @"[\s_\-]+").Where(w => w.Length > 0).ToArray();
                    return string.Concat(pw.Select(w => char.ToUpper(w[0]) + w.Substring(1).ToLower()));
                default: return s;
            }
        }

        void Apply()
        {
            bool anyAsset = false;
            foreach (var e in _entries)
            {
                if (e.unchanged || e.conflict) continue;
                if (e.isAsset)
                {
                    string path = AssetDatabase.GetAssetPath(e.obj);
                    AssetDatabase.RenameAsset(path, e.next + System.IO.Path.GetExtension(path));
                    anyAsset = true;
                }
                else if (e.obj is GameObject go)
                {
                    Undo.RecordObject(go, "Bulk Rename");
                    go.name = e.next;
                }
            }
            if (anyAsset) AssetDatabase.SaveAssets();
            _dirty = true;
        }

        bool IsValidRegex(string p)
        {
            try { new Regex(p); return true; } catch { return false; }
        }

        void Line(Rect r) => EditorGUI.DrawRect(r, new Color(0f, 0f, 0f, 0.18f));
    }
}
