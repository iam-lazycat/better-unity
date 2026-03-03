using System;
using System.Collections.Generic;
using System.Linq;
using UnityEditor;
using UnityEngine;

namespace LazyCat.BetterUnity
{
    public class TaskListWindow : EditorWindow
    {
        // ── state ─────────────────────────────────────────────────────────
        int     _tab         = 0;
        int     _sortMode    = 0;
        string  _filterLabel = "";
        bool    _showClosed  = false;

        string  _selectedId  = null;
        bool    _detailOpen  = false;

        Vector2 _listScroll;
        Vector2 _detailScroll;
        Vector2 _settingsScroll;

        string _quickAddName = "";

        // attachment add state
        AttachmentType       _newAttachType  = AttachmentType.Link;
        string               _newAttachLabel = "";
        string               _newAttachLink  = "";
        UnityEngine.Object   _newAttachObj   = null;

        static readonly string[] SORT_OPTS = { "Manual", "Priority", "Deadline", "Name" };
        static readonly string[] TAB_NAMES = { "Current", "Backlog", "Settings" };

        // ── open ──────────────────────────────────────────────────────────
        public static void Open()
        {
            if (!BetterUnityPrefs.TaskListEnabled)
            {
                EditorUtility.DisplayDialog("Module Disabled",
                    "Task List is disabled. Enable it in Project Settings → Better Unity.", "OK");
                return;
            }
            var w = GetWindow<TaskListWindow>();
            w.titleContent = new GUIContent("Task List", EditorGUIUtility.IconContent("d_UnityEditor.ConsoleWindow").image);
            w.minSize = new Vector2(380, 300);
            w.Show();
        }

        void OnEnable()  => TaskListStorage.Load();
        void OnDisable() => TaskListStorage.Save();

        // ── main ──────────────────────────────────────────────────────────
        void OnGUI()
        {
            var db = TaskListStorage.DB;
            if (db.priorities == null || db.priorities.Count == 0)
                db.priorities = TaskDatabase.DefaultPriorities();

            // native toolbar at top
            DrawTopToolbar();

            float splitFrac = 0.44f;
            float splitX    = _detailOpen ? Mathf.Max(200, position.width * splitFrac) : position.width;
            float bodyY     = EditorStyles.toolbar.fixedHeight + EditorStyles.toolbar.fixedHeight; // two toolbars
            float bodyH     = position.height - bodyY - 22f; // 22 = quick-add

            if (_tab == 0 || _tab == 1)
            {
                // list column
                var listR = new Rect(0, bodyY, splitX, bodyH);
                DrawTaskList(listR);

                // quick-add at bottom
                DrawQuickAdd(new Rect(0, position.height - 22f, splitX, 22f));

                // detail column
                if (_detailOpen && _selectedId != null)
                {
                    float dx = splitX;
                    DrawDetailPanel(new Rect(dx, bodyY, position.width - dx, position.height - bodyY));
                }
            }
            else
            {
                DrawSettingsTab(new Rect(0, bodyY, position.width, position.height - bodyY));
            }

            // keep hover repaints cheap
            if (Event.current.type == EventType.MouseMove) Repaint();
        }

        // ── top toolbar (tab bar + filters, all native) ───────────────────
        void DrawTopToolbar()
        {
            // row 1: tabs
            EditorGUILayout.BeginHorizontal(EditorStyles.toolbar);

            for (int i = 0; i < TAB_NAMES.Length; i++)
            {
                bool sel = _tab == i;
                var style = sel ? EditorStyles.toolbarButton : EditorStyles.toolbarButton;
                if (GUILayout.Toggle(sel, TAB_NAMES[i], EditorStyles.toolbarButton, GUILayout.Width(72)) && !sel)
                    _tab = i;
            }

            GUILayout.FlexibleSpace();

            // active task count badge (native mini label)
            if (_tab < 2)
            {
                var list = GetCurrentList();
                int active = list.Count(t => !t.completed && t.status == TaskStatus.Active);
                if (active > 0)
                    GUILayout.Label($"{active} active", EditorStyles.centeredGreyMiniLabel, GUILayout.Width(55));
            }

            EditorGUILayout.EndHorizontal();

            // row 2: filter/sort toolbar — only on list tabs
            if (_tab == 0 || _tab == 1)
            {
                EditorGUILayout.BeginHorizontal(EditorStyles.toolbar);

                _showClosed = GUILayout.Toggle(_showClosed, "Show Closed", EditorStyles.toolbarButton, GUILayout.Width(84));

                GUILayout.Space(4);
                GUILayout.Label("Sort", EditorStyles.miniLabel, GUILayout.Width(28));
                _sortMode = EditorGUILayout.Popup(_sortMode, SORT_OPTS, EditorStyles.toolbarPopup, GUILayout.Width(76));

                GUILayout.Space(4);
                GUILayout.Label("Label", EditorStyles.miniLabel, GUILayout.Width(34));
                _filterLabel = EditorGUILayout.TextField(_filterLabel, EditorStyles.toolbarSearchField, GUILayout.MinWidth(60), GUILayout.ExpandWidth(true));
                if (!string.IsNullOrEmpty(_filterLabel))
                    if (GUILayout.Button("✕", EditorStyles.toolbarButton, GUILayout.Width(20)))
                        _filterLabel = "";

                EditorGUILayout.EndHorizontal();
            }
            else
            {
                // spacer row so layout stays consistent
                EditorGUILayout.BeginHorizontal(EditorStyles.toolbar);
                GUILayout.FlexibleSpace();
                EditorGUILayout.EndHorizontal();
            }
        }

        // ── task list ─────────────────────────────────────────────────────
        void DrawTaskList(Rect r)
        {
            var tasks = GetFilteredSortedList();

            float rowH   = 38f;
            var viewRect = new Rect(0, 0, r.width - 14, Mathf.Max(r.height, tasks.Count * rowH + 4));

            GUI.BeginGroup(r);
            _listScroll = GUI.BeginScrollView(new Rect(0, 0, r.width, r.height), _listScroll, viewRect);

            if (tasks.Count == 0)
            {
                var emptyR = new Rect(0, 20, r.width - 14, 24);
                EditorGUI.LabelField(emptyR,
                    _tab == 0 ? "No tasks yet. Type below to add one." : "Backlog is empty.",
                    EditorStyles.centeredGreyMiniLabel);
            }

            float y = 2f;
            for (int i = 0; i < tasks.Count; i++)
            {
                DrawTaskRow(tasks[i], new Rect(0, y, r.width - 14, rowH - 2), i);
                y += rowH;
            }

            GUI.EndScrollView();
            GUI.EndGroup();
        }

        void DrawTaskRow(TaskItem task, Rect r, int idx)
        {
            var db     = TaskListStorage.DB;
            bool sel   = _selectedId == task.id;
            bool hov   = r.Contains(Event.current.mousePosition);
            bool done  = task.completed;
            bool closed= task.status == TaskStatus.Closed;

            // background — use Unity's selection style or alternating rows
            if (sel)
                EditorGUI.DrawRect(r, new Color(0.24f, 0.37f, 0.58f, 0.4f));
            else if (hov)
                EditorGUI.DrawRect(r, new Color(0.5f, 0.5f, 0.5f, 0.1f));
            else if (idx % 2 == 1)
                EditorGUI.DrawRect(r, new Color(0f, 0f, 0f, 0.06f));

            // priority stripe
            var pdef   = db.GetPriority(task.priorityId);
            Color stripe = (done || closed) ? new Color(0.4f, 0.4f, 0.4f) : pdef.color;
            EditorGUI.DrawRect(new Rect(r.x, r.y + 4, 3, r.height - 8), stripe);

            float x = r.x + 8;

            // checkbox — native toggle style
            var cbR = new Rect(x, r.y + (r.height - 14) * 0.5f, 14, 14);
            bool newDone = EditorGUI.Toggle(cbR, done);
            if (newDone != done)
            {
                task.completed = newDone;
                if (newDone && db.settings.autoMoveCompletedToBacklog && task.list == TaskList.Current)
                    task.list = TaskList.Backlog;
                TaskListStorage.MarkDirty();
            }
            x += 20;

            // name + label
            float infoW = r.width - (x - r.x) - 106;
            var nameStyle = new GUIStyle(EditorStyles.label)
            {
                fontStyle = (!done && !closed) ? FontStyle.Normal : FontStyle.Normal,
                normal    = { textColor = (done || closed)
                    ? new Color(0.5f, 0.5f, 0.5f)
                    : EditorStyles.label.normal.textColor }
            };
            EditorGUI.LabelField(new Rect(x, r.y + 3, infoW, 16), task.name, nameStyle);

            if (!string.IsNullOrEmpty(task.label))
            {
                var tagStyle = new GUIStyle(EditorStyles.miniLabel)
                {
                    normal = { textColor = new Color(0.55f, 0.55f, 0.6f) }
                };
                EditorGUI.LabelField(new Rect(x, r.y + 20, infoW, 13), task.label, tagStyle);
            }

            // deadline
            if (!string.IsNullOrEmpty(task.deadline))
            {
                bool overdue = DateTime.TryParse(task.deadline, out DateTime dl) && dl < DateTime.Today && !done && !closed;
                var dlStyle  = new GUIStyle(EditorStyles.miniLabel)
                {
                    alignment = TextAnchor.MiddleRight,
                    normal    = { textColor = overdue ? new Color(0.9f, 0.35f, 0.35f) : new Color(0.55f, 0.55f, 0.55f) }
                };
                EditorGUI.LabelField(new Rect(r.xMax - 106, r.y + 3, 64, 16), task.deadline, dlStyle);
            }

            // priority chip
            var chipStyle = new GUIStyle(EditorStyles.miniLabel)
            {
                alignment = TextAnchor.MiddleRight,
                normal    = { textColor = stripe }
            };
            EditorGUI.LabelField(new Rect(r.xMax - 42, r.y + 3, 38, 16), pdef.name, chipStyle);

            // bottom divider
            EditorGUI.DrawRect(new Rect(r.x, r.yMax, r.width, 1), new Color(0f, 0f, 0f, 0.12f));

            // row click
            if (Event.current.type == EventType.MouseDown && r.Contains(Event.current.mousePosition))
            {
                if (_selectedId == task.id && _detailOpen) _detailOpen = false;
                else { _selectedId = task.id; _detailOpen = true; _detailScroll = Vector2.zero; }
                GUI.FocusControl(null);
                Repaint();
                Event.current.Use();
            }
        }

        // ── quick-add bar ─────────────────────────────────────────────────
        void DrawQuickAdd(Rect r)
        {
            EditorGUI.DrawRect(r, EditorGUIUtility.isProSkin
                ? new Color(0.22f, 0.22f, 0.22f)
                : new Color(0.76f, 0.76f, 0.76f));

            float pad = 4f;
            float btnW = 48f;
            float fieldW = r.width - pad * 2 - btnW - 4;

            GUI.SetNextControlName("QuickAdd");
            _quickAddName = EditorGUI.TextField(
                new Rect(r.x + pad, r.y + 2, fieldW, r.height - 4),
                _quickAddName, EditorStyles.toolbarSearchField);

            if (string.IsNullOrEmpty(_quickAddName) && GUI.GetNameOfFocusedControl() != "QuickAdd")
                EditorGUI.LabelField(new Rect(r.x + pad + 18, r.y + 2, fieldW, r.height - 4),
                    "Add a task...", EditorStyles.centeredGreyMiniLabel);

            bool enter = Event.current.type == EventType.KeyDown &&
                         Event.current.keyCode == KeyCode.Return &&
                         GUI.GetNameOfFocusedControl() == "QuickAdd";

            EditorGUI.BeginDisabledGroup(string.IsNullOrWhiteSpace(_quickAddName));
            bool clicked = GUI.Button(new Rect(r.xMax - btnW - pad, r.y + 2, btnW, r.height - 4),
                "Add", EditorStyles.miniButton);
            EditorGUI.EndDisabledGroup();

            if ((enter || clicked) && !string.IsNullOrWhiteSpace(_quickAddName))
            {
                var db = TaskListStorage.DB;
                var defaultPriId = db.SortedPriorities().Count > 0 ? db.SortedPriorities()[1 % db.priorities.Count].id : "";
                var t = new TaskItem
                {
                    name       = _quickAddName.Trim(),
                    list       = (TaskList)_tab,
                    priorityId = defaultPriId,
                    label      = db.settings.defaultLabel
                };
                db.tasks.Add(t);
                TaskListStorage.MarkDirty();
                _quickAddName = "";
                _selectedId   = t.id;
                _detailOpen   = true;
                _detailScroll = Vector2.zero;
                GUI.FocusControl(null);
                Event.current.Use();
                Repaint();
            }
        }

        // ── detail panel ──────────────────────────────────────────────────
        void DrawDetailPanel(Rect r)
        {
            var task = TaskListStorage.DB.tasks.FirstOrDefault(t => t.id == _selectedId);
            if (task == null) { _detailOpen = false; return; }
            var db = TaskListStorage.DB;

            // vertical divider
            EditorGUI.DrawRect(new Rect(r.x, r.y, 1, r.height), new Color(0f, 0f, 0f, 0.25f));
            var inner = new Rect(r.x + 1, r.y, r.width - 1, r.height);

            GUI.BeginGroup(inner);

            // action toolbar
            float tbH = EditorStyles.toolbar.fixedHeight;
            GUILayout.BeginArea(new Rect(0, 0, inner.width, tbH));
            EditorGUILayout.BeginHorizontal(EditorStyles.toolbar);

            // complete
            if (GUILayout.Button(task.completed ? "✓ Done" : "Mark Done", EditorStyles.toolbarButton, GUILayout.Width(72)))
            {
                task.completed = !task.completed;
                if (task.completed && db.settings.autoMoveCompletedToBacklog && task.list == TaskList.Current)
                    task.list = TaskList.Backlog;
                TaskListStorage.MarkDirty();
            }

            // move list
            string moveLabel = task.list == TaskList.Current ? "→ Backlog" : "→ Current";
            if (GUILayout.Button(moveLabel, EditorStyles.toolbarButton, GUILayout.Width(70)))
            {
                task.list = task.list == TaskList.Current ? TaskList.Backlog : TaskList.Current;
                TaskListStorage.MarkDirty();
            }

            // close/reopen
            if (GUILayout.Button(task.status == TaskStatus.Closed ? "Reopen" : "Close", EditorStyles.toolbarButton, GUILayout.Width(56)))
            {
                task.status = task.status == TaskStatus.Closed ? TaskStatus.Active : TaskStatus.Closed;
                TaskListStorage.MarkDirty();
            }

            GUILayout.FlexibleSpace();

            // delete
            var oldColor = GUI.color;
            GUI.color = new Color(1f, 0.5f, 0.5f);
            bool deletePressed = GUILayout.Button("Delete", EditorStyles.toolbarButton, GUILayout.Width(48));
            GUI.color = oldColor;

            // close panel
            if (GUILayout.Button("✕", EditorStyles.toolbarButton, GUILayout.Width(24)))
                _detailOpen = false;

            EditorGUILayout.EndHorizontal();
            GUILayout.EndArea();

            // handle delete after all layout groups are closed
            if (deletePressed)
            {
                bool doDelete = !db.settings.confirmOnDelete ||
                    EditorUtility.DisplayDialog("Delete Task", $"Delete \"{task.name}\"?", "Delete", "Cancel");
                if (doDelete)
                {
                    db.tasks.Remove(task);
                    TaskListStorage.MarkDirty();
                    _selectedId = null;
                    _detailOpen = false;
                    GUI.EndGroup();
                    return;
                }
            }

            // scrollable fields
            var fieldsR = new Rect(0, tbH, inner.width, inner.height - tbH);
            _detailScroll = GUI.BeginScrollView(fieldsR, _detailScroll,
                new Rect(0, tbH, inner.width - 14, tbH + 560));

            float y = tbH + 8;
            float pad = 8f;
            float fw = inner.width - pad * 2;

            // name
            y = NativeField(y, pad, fw, inner.width, "Name", ref task.name);

            // description
            y = NativeTextArea(y, pad, fw, inner.width, "Description", ref task.description);

            // priority dropdown
            y = NativePriorityField(y, pad, fw, inner.width, task, db);

            // label
            y = NativeField(y, pad, fw, inner.width, "Label", ref task.label);

            // deadline
            y = NativeField(y, pad, fw, inner.width, "Deadline  (yyyy-MM-dd)", ref task.deadline);

            // status
            EditorGUI.LabelField(new Rect(pad, y, fw, 14), "Status", EditorStyles.miniLabel);
            y += 15;
            var statusNames = new[] { "Active", "Closed" };
            int curStatus   = (int)task.status;
            int newStatus   = GUI.Toolbar(new Rect(pad, y, fw, 20), curStatus, statusNames, EditorStyles.miniButton);
            if (newStatus != curStatus) { task.status = (TaskStatus)newStatus; TaskListStorage.MarkDirty(); }
            y += 26;

            // divider
            EditorGUI.DrawRect(new Rect(pad, y, fw, 1), new Color(0f, 0f, 0f, 0.2f));
            y += 8;

            // attachments
            EditorGUI.LabelField(new Rect(pad, y, fw, 14), "Attachments", EditorStyles.miniLabel);
            y += 18;
            y = DrawAttachments(y, pad, fw, inner.width, task, db);

            GUI.EndScrollView();
            GUI.EndGroup();
        }

        // ── detail field helpers ──────────────────────────────────────────
        float NativeField(float y, float pad, float fw, float totalW, string label, ref string val)
        {
            EditorGUI.LabelField(new Rect(pad, y, fw, 14), label, EditorStyles.miniLabel);
            y += 15;
            string nv = EditorGUI.TextField(new Rect(pad, y, fw, 18), val);
            if (nv != val) { val = nv; TaskListStorage.MarkDirty(); }
            return y + 24;
        }

        float NativeTextArea(float y, float pad, float fw, float totalW, string label, ref string val)
        {
            EditorGUI.LabelField(new Rect(pad, y, fw, 14), label, EditorStyles.miniLabel);
            y += 15;
            string nv = EditorGUI.TextArea(new Rect(pad, y, fw, 48), val);
            if (nv != val) { val = nv; TaskListStorage.MarkDirty(); }
            return y + 54;
        }

        float NativePriorityField(float y, float pad, float fw, float totalW, TaskItem task, TaskDatabase db)
        {
            EditorGUI.LabelField(new Rect(pad, y, fw, 14), "Priority", EditorStyles.miniLabel);
            y += 15;

            var sorted   = db.SortedPriorities();
            var names    = sorted.Select(p => p.name).ToArray();
            int curIdx   = sorted.FindIndex(p => p.id == task.priorityId);
            if (curIdx < 0) curIdx = 0;

            int newIdx = EditorGUI.Popup(new Rect(pad, y, fw, 18), curIdx, names);
            if (newIdx != curIdx && newIdx < sorted.Count)
            {
                task.priorityId = sorted[newIdx].id;
                TaskListStorage.MarkDirty();
            }
            return y + 24;
        }

        // ── attachments ───────────────────────────────────────────────────
        float DrawAttachments(float y, float pad, float fw, float totalW, TaskItem task, TaskDatabase db)
        {
            foreach (var att in task.attachments.ToList())
            {
                var rowR = new Rect(pad, y, fw, 20);
                EditorGUI.DrawRect(rowR, new Color(0f, 0f, 0f, 0.1f));

                // type
                EditorGUI.LabelField(new Rect(pad + 3, y + 2, 60, 16), att.type.ToString(), EditorStyles.miniLabel);

                // label or url
                string dispLabel = !string.IsNullOrEmpty(att.label) ? att.label : att.linkUrl;
                EditorGUI.LabelField(new Rect(pad + 66, y + 2, fw - 110, 16), dispLabel, EditorStyles.miniLabel);

                // ping / open
                if (GUI.Button(new Rect(fw - 38 + pad, y + 2, 18, 16), "↗", EditorStyles.miniButton))
                    OpenAttachment(att);

                // remove
                if (GUI.Button(new Rect(fw - 18 + pad, y + 2, 16, 16), "✕", EditorStyles.miniButton))
                {
                    task.attachments.Remove(att);
                    TaskListStorage.MarkDirty();
                    break;
                }
                y += 24;
            }

            y += 4;
            EditorGUI.LabelField(new Rect(pad, y, fw, 14), "Add Attachment", EditorStyles.miniLabel);
            y += 16;

            float typeW = 96f;
            _newAttachType = (AttachmentType)EditorGUI.EnumPopup(
                new Rect(pad, y, typeW, 18), _newAttachType);

            float objW = fw - typeW - 6;
            if (_newAttachType == AttachmentType.Link)
                _newAttachLink = EditorGUI.TextField(new Rect(pad + typeW + 4, y, objW, 18), _newAttachLink);
            else
                _newAttachObj  = EditorGUI.ObjectField(new Rect(pad + typeW + 4, y, objW, 18),
                    _newAttachObj, GetAttachType(_newAttachType), false);
            y += 22;

            _newAttachLabel = EditorGUI.TextField(new Rect(pad, y, fw - 24, 18), _newAttachLabel);
            if (string.IsNullOrEmpty(_newAttachLabel))
                EditorGUI.LabelField(new Rect(pad + 2, y, fw - 28, 18), "Label (optional)", EditorStyles.centeredGreyMiniLabel);

            if (GUI.Button(new Rect(pad + fw - 22, y, 20, 18), "+", EditorStyles.miniButton))
            {
                var att = new TaskAttachment { type = _newAttachType, label = _newAttachLabel };
                if (_newAttachType == AttachmentType.Link)
                    att.linkUrl = _newAttachLink;
                else if (_newAttachObj != null)
                {
                    att.assetGuid = AssetDatabase.AssetPathToGUID(AssetDatabase.GetAssetPath(_newAttachObj));
                    if (string.IsNullOrEmpty(att.label)) att.label = _newAttachObj.name;
                }
                task.attachments.Add(att);
                TaskListStorage.MarkDirty();
                _newAttachLabel = ""; _newAttachLink = ""; _newAttachObj = null;
            }
            return y + 28;
        }

        Type GetAttachType(AttachmentType t)
        {
            switch (t)
            {
                case AttachmentType.Script:          return typeof(MonoScript);
                case AttachmentType.Scene:
                case AttachmentType.SceneGameObject: return typeof(SceneAsset);
                default:                             return typeof(UnityEngine.Object);
            }
        }

        void OpenAttachment(TaskAttachment att)
        {
            if (att.type == AttachmentType.Link) { Application.OpenURL(att.linkUrl); return; }
            var path = AssetDatabase.GUIDToAssetPath(att.assetGuid);
            var obj  = AssetDatabase.LoadAssetAtPath<UnityEngine.Object>(path);
            if (obj != null) EditorGUIUtility.PingObject(obj);
        }

        // ── settings tab ──────────────────────────────────────────────────
        void DrawSettingsTab(Rect r)
        {
            var db = TaskListStorage.DB;
            var s  = db.settings;

            GUI.BeginGroup(r);
            _settingsScroll = GUI.BeginScrollView(new Rect(0, 0, r.width, r.height), _settingsScroll,
                new Rect(0, 0, r.width - 14, 700));

            float y   = 8f;
            float pad = 12f;
            float fw  = r.width - pad * 2 - 14;

            // ── Behaviour ─────────────────────────────────────────────────
            y = SectionHeader(y, pad, fw, "Behaviour");

            y = NativeBoolField(y, pad, fw, "Confirm before deleting tasks", ref s.confirmOnDelete, db);
            y = NativeBoolField(y, pad, fw, "Auto-move completed tasks to Backlog", ref s.autoMoveCompletedToBacklog, db);
            y = NativeBoolField(y, pad, fw, "Show deadline warning on tasks", ref s.showDeadlineWarning, db);

            if (s.showDeadlineWarning)
            {
                EditorGUI.LabelField(new Rect(pad + 16, y, fw - 16, 16), "Warn days before deadline", EditorStyles.miniLabel);
                int newDays = EditorGUI.IntSlider(new Rect(pad + 16, y + 16, fw - 16, 18), s.deadlineWarnDays, 1, 14);
                if (newDays != s.deadlineWarnDays) { s.deadlineWarnDays = newDays; TaskListStorage.MarkDirty(); }
                y += 38;
            }

            EditorGUI.LabelField(new Rect(pad, y, fw, 16), "Default label for new tasks", EditorStyles.miniLabel);
            string newLabel = EditorGUI.TextField(new Rect(pad, y + 16, fw, 18), s.defaultLabel);
            if (newLabel != s.defaultLabel) { s.defaultLabel = newLabel; TaskListStorage.MarkDirty(); }
            y += 40;

            EditorGUI.LabelField(new Rect(pad, y, fw, 16), "Default sort mode", EditorStyles.miniLabel);
            int newSort = EditorGUI.Popup(new Rect(pad, y + 16, fw, 18), s.defaultSortMode, SORT_OPTS);
            if (newSort != s.defaultSortMode) { s.defaultSortMode = newSort; _sortMode = newSort; TaskListStorage.MarkDirty(); }
            y += 40;

            y += 4;

            // ── Priorities ────────────────────────────────────────────────
            y = SectionHeader(y, pad, fw, "Priorities");

            var sorted = db.SortedPriorities();
            for (int i = 0; i < sorted.Count; i++)
            {
                var p  = sorted[i];
                var pr = new Rect(pad, y, fw, 24);
                EditorGUI.DrawRect(pr, new Color(0f, 0f, 0f, 0.08f));

                // color swatch
                Color nc = EditorGUI.ColorField(new Rect(pad + 2, y + 3, 38, 18),
                    GUIContent.none, p.color, false, false, false);
                if (nc != p.color) { p.color = nc; TaskListStorage.MarkDirty(); }

                // name
                string nn = EditorGUI.TextField(new Rect(pad + 46, y + 3, fw - 134, 18), p.name);
                if (nn != p.name) { p.name = nn; TaskListStorage.MarkDirty(); }

                // move up
                EditorGUI.BeginDisabledGroup(i == 0);
                if (GUI.Button(new Rect(fw + pad - 82, y + 3, 22, 18), "↑", EditorStyles.miniButton))
                {
                    p.order--; sorted[i - 1].order++;
                    TaskListStorage.MarkDirty();
                }
                EditorGUI.EndDisabledGroup();

                // move down
                EditorGUI.BeginDisabledGroup(i == sorted.Count - 1);
                if (GUI.Button(new Rect(fw + pad - 58, y + 3, 22, 18), "↓", EditorStyles.miniButton))
                {
                    p.order++; sorted[i + 1].order--;
                    TaskListStorage.MarkDirty();
                }
                EditorGUI.EndDisabledGroup();

                // delete (only if more than 1)
                EditorGUI.BeginDisabledGroup(db.priorities.Count <= 1);
                if (GUI.Button(new Rect(fw + pad - 32, y + 3, 28, 18), "Del", EditorStyles.miniButton))
                {
                    db.priorities.Remove(p);
                    TaskListStorage.MarkDirty();
                    break;
                }
                EditorGUI.EndDisabledGroup();

                y += 28;
            }

            // add new priority
            if (GUI.Button(new Rect(pad, y, 110, 20), "+ Add Priority", EditorStyles.miniButton))
            {
                db.priorities.Add(new PriorityDef
                {
                    name  = "New",
                    color = Color.grey,
                    order = db.priorities.Count
                });
                TaskListStorage.MarkDirty();
            }

            if (GUI.Button(new Rect(pad + 116, y, 110, 20), "Reset to Defaults", EditorStyles.miniButton))
            {
                if (EditorUtility.DisplayDialog("Reset Priorities",
                    "Reset all priorities to defaults? Task priority assignments will be cleared.", "Reset", "Cancel"))
                {
                    db.priorities = TaskDatabase.DefaultPriorities();
                    foreach (var t in db.tasks) t.priorityId = "";
                    TaskListStorage.MarkDirty();
                }
            }
            y += 30;

            y += 4;

            // ── Data ──────────────────────────────────────────────────────
            y = SectionHeader(y, pad, fw, "Data");

            if (GUI.Button(new Rect(pad, y, 150, 20), "Clear Completed Tasks", EditorStyles.miniButton))
            {
                if (EditorUtility.DisplayDialog("Clear Completed", "Remove all completed tasks?", "Clear", "Cancel"))
                { db.tasks.RemoveAll(t => t.completed); TaskListStorage.MarkDirty(); }
            }
            y += 28;

            var oldC = GUI.color;
            GUI.color = new Color(1f, 0.7f, 0.7f);
            if (GUI.Button(new Rect(pad, y, 110, 20), "Delete All Tasks", EditorStyles.miniButton))
            {
                GUI.color = oldC;
                if (EditorUtility.DisplayDialog("Delete All Tasks", "Delete ALL tasks? This cannot be undone.", "Delete All", "Cancel"))
                {
                    db.tasks.Clear();
                    TaskListStorage.MarkDirty();
                    _selectedId = null; _detailOpen = false;
                }
            }
            GUI.color = oldC;
            y += 30;

            GUI.EndScrollView();
            GUI.EndGroup();
        }

        // ── settings helpers ──────────────────────────────────────────────
        float SectionHeader(float y, float pad, float fw, string title)
        {
            EditorGUI.LabelField(new Rect(pad, y, fw, 18), title, EditorStyles.boldLabel);
            EditorGUI.DrawRect(new Rect(pad, y + 18, fw, 1), new Color(0f, 0f, 0f, 0.2f));
            return y + 24;
        }

        float NativeBoolField(float y, float pad, float fw, string label, ref bool val, TaskDatabase db)
        {
            bool nv = EditorGUI.Toggle(new Rect(pad, y + 1, 16, 16), val);
            EditorGUI.LabelField(new Rect(pad + 20, y, fw - 20, 18), label, EditorStyles.label);
            if (nv != val) { val = nv; TaskListStorage.MarkDirty(); }
            return y + 22;
        }

        // ── data helpers ──────────────────────────────────────────────────
        List<TaskItem> GetCurrentList()
        {
            var target = (TaskList)Mathf.Clamp(_tab, 0, 1);
            return TaskListStorage.DB.tasks.Where(t => t.list == target).ToList();
        }

        List<TaskItem> GetFilteredSortedList()
        {
            var list = GetCurrentList()
                .Where(t => _showClosed || t.status != TaskStatus.Closed)
                .Where(t => string.IsNullOrEmpty(_filterLabel) ||
                            (t.label ?? "").IndexOf(_filterLabel, StringComparison.OrdinalIgnoreCase) >= 0)
                .ToList();

            var db = TaskListStorage.DB;
            switch (_sortMode)
            {
                case 1: // Priority — sort by order of priority def
                    return list.OrderByDescending(t =>
                    {
                        var p = db.GetPriority(t.priorityId);
                        return p.order;
                    }).ToList();
                case 2: // Deadline
                    return list.OrderBy(t => string.IsNullOrEmpty(t.deadline) ? "9999" : t.deadline).ToList();
                case 3: // Name
                    return list.OrderBy(t => t.name).ToList();
                default:
                    return list;
            }
        }
    }
}
