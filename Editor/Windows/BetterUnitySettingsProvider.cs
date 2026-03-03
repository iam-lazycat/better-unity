using System.IO;
using UnityEditor;
using UnityEditor.Toolbars;
using UnityEngine;

namespace LazyCat.BetterUnity
{
    public static class BetterUnitySettingsProvider
    {
        static int _selectedModule = 0;

        const float LEFT_W  = 148f;
        const float DIVIDER = 1f;

        static readonly string[] MODULE_NAMES = { "Auto Save", "Task List", "Bulk Rename", "Align To Ground", "Transform Copy Paste", "Toolbar", "Folder Icons", "Hierarchy" };
        static readonly string[] MODULE_ICONS = { "d_SaveAs", "d_UnityEditor.ConsoleWindow", "d_editicon.sml", "d_Terrain Icon", "d_Transform Icon", "d_CustomTool", "d_Folder Icon", "d_UnityEditor.HierarchyWindow" };

        static bool GetModuleEnabled(int idx)
        {
            switch (idx)
            {
                case 0: return BetterUnityPrefs.AutoSaveEnabled;
                case 1: return BetterUnityPrefs.TaskListEnabled;
                case 2: return BetterUnityPrefs.BulkRenameEnabled;
                case 3: return BetterUnityPrefs.AlignToGroundEnabled;
                case 4: return BetterUnityPrefs.TransformCopyPasteEnabled;
                case 5: return BetterUnityPrefs.ToolbarEnabled;
                case 6: return BetterUnityPrefs.FolderIconEnabled;
                case 7: return BetterUnityPrefs.HierarchyEnabled;
                default: return true;
            }
        }

        static void SetModuleEnabled(int idx, bool val)
        {
            switch (idx)
            {
                case 0: BetterUnityPrefs.AutoSaveEnabled           = val; AutoSaveModule.OnSettingsChanged(); break;
                case 1: BetterUnityPrefs.TaskListEnabled            = val; break;
                case 2: BetterUnityPrefs.BulkRenameEnabled          = val; break;
                case 3: BetterUnityPrefs.AlignToGroundEnabled = val; break;
                case 4: BetterUnityPrefs.TransformCopyPasteEnabled  = val; break;
                case 5: BetterUnityPrefs.ToolbarEnabled = val;
                    MainToolbar.Refresh("BetterUnity/Screenshot");
                    MainToolbar.Refresh("BetterUnity/SceneSwitcher");
                    MainToolbar.Refresh("BetterUnity/Bookmarks");
                    MainToolbar.Refresh("BetterUnity/TimeScale");
                    MainToolbar.Refresh("BetterUnity/FpsCap");
                    break;
                case 6: BetterUnityPrefs.FolderIconEnabled = val; EditorApplication.RepaintProjectWindow(); break;
                case 7: BetterUnityPrefs.HierarchyEnabled = val; EditorApplication.RepaintHierarchyWindow(); break;

            }
        }

        [SettingsProvider]
        public static SettingsProvider CreateSettingsProvider()
        {
            return new SettingsProvider("Project/Better Unity", SettingsScope.Project)
            {
                label      = "Better Unity",
                guiHandler = _ => DrawGUI(),
                keywords   = new System.Collections.Generic.HashSet<string>
                    { "Better", "Unity", "Lazy", "Cat", "AutoSave", "Auto", "Save", "Task", "List", "Align", "Terrain", "Toolbar", "Screenshot", "Rename" }
            };
        }

        static void DrawGUI()
        {
            var   totalRect = EditorGUILayout.GetControlRect(false, GUILayout.ExpandHeight(true), GUILayout.ExpandWidth(true));
            float fullH     = Screen.height;

            var leftR  = new Rect(totalRect.x,                        totalRect.y, LEFT_W,                                               fullH);
            var divR   = new Rect(totalRect.x + LEFT_W,                totalRect.y, DIVIDER,                                              fullH);
            var rightR = new Rect(totalRect.x + LEFT_W + DIVIDER + 8,  totalRect.y, Mathf.Max(10, totalRect.width - LEFT_W - DIVIDER - 8), fullH);

            DrawLeftPanel(leftR);
            EditorGUI.DrawRect(divR, new Color(0f, 0f, 0f, 0.15f));
            DrawRightPanel(rightR);
        }

        // ── left panel ────────────────────────────────────────────────────

        static void DrawLeftPanel(Rect r)
        {
            GUI.BeginGroup(r);
            float y = 0;

            EditorGUI.LabelField(new Rect(0, y, r.width, 18), "Modules", EditorStyles.boldLabel);
            y += 22;

            for (int i = 0; i < MODULE_NAMES.Length; i++)
            {
                bool sel     = _selectedModule == i;
                bool enabled = GetModuleEnabled(i);
                var  rowR    = new Rect(0, y, r.width - 2, 28);

                if (sel)
                    EditorGUI.DrawRect(rowR, new Color(0.24f, 0.37f, 0.58f, 0.35f));
                else if (rowR.Contains(Event.current.mousePosition))
                    EditorGUI.DrawRect(rowR, new Color(0.5f, 0.5f, 0.5f, 0.1f));

                var icon = EditorGUIUtility.IconContent(MODULE_ICONS[i]);
                if (icon?.image != null)
                    GUI.DrawTexture(new Rect(6, y + 6, 16, 16), icon.image, ScaleMode.ScaleToFit, true);

                var nameStyle = new GUIStyle(EditorStyles.label)
                {
                    normal = { textColor = sel
                        ? EditorStyles.label.normal.textColor
                        : (enabled ? EditorStyles.label.normal.textColor : new Color(0.5f, 0.5f, 0.5f)) }
                };
                EditorGUI.LabelField(new Rect(26, y + 6, r.width - 52, 16), MODULE_NAMES[i], nameStyle);

                Color dotBg = enabled ? new Color(0.22f, 0.6f, 0.28f) : new Color(0.35f, 0.35f, 0.35f);
                var   dotR  = new Rect(r.width - 28, y + 8, 22, 12);
                EditorGUI.DrawRect(dotR, dotBg);
                EditorGUI.LabelField(dotR, enabled ? "ON" : "OFF",
                    new GUIStyle(EditorStyles.miniLabel)
                    {
                        alignment = TextAnchor.MiddleCenter,
                        normal    = { textColor = new Color(0.92f, 0.92f, 0.92f) },
                        fontSize  = 8
                    });

                if (Event.current.type == EventType.MouseDown && rowR.Contains(Event.current.mousePosition))
                {
                    _selectedModule = i;
                    Event.current.Use();
                    GUI.changed = true;
                }

                y += 30;
            }

            GUI.EndGroup();
        }

        // ── right panel ───────────────────────────────────────────────────

        static void DrawRightPanel(Rect r)
        {
            GUI.BeginGroup(r);

            switch (_selectedModule)
            {
                case 0: DrawAutoSaveSettings(r.width);        break;
                case 1: DrawTaskListSettings(r.width);        break;
                case 2: DrawSimpleModuleSettings(r.width, 2, "Bulk Rename",          "Rename multiple assets or GameObjects at once.",                                                        BulkRenameWindow.Open); break;
                case 3: DrawAlignToGroundSettings(r.width);     break;
                case 4: DrawSimpleModuleSettings(r.width, 4, "Transform Copy Paste", "Adds Copy and Paste buttons to each row of the Transform inspector. Clipboard is per-session.",         null); break;
                case 5: DrawToolbarSettings(r.width);         break;
                case 6: DrawFolderIconSettings(r.width); break;
                case 7: DrawSmartHierarchySettings(r.width); break;
            }

            GUI.EndGroup();
        }

        // ── Auto Save ─────────────────────────────────────────────────────

        static void DrawAutoSaveSettings(float w)
        {
            float y = 0;
            y = ModuleHeader(y, "Auto Save", 0, ref y);

            using (new EditorGUI.DisabledGroupScope(!GetModuleEnabled(0)))
            {
                float interval    = BetterUnityPrefs.AutoSaveInterval;
                float newInterval = SettingsSlider(ref y, w,
                    new GUIContent("Interval", "How long between each automatic save. Shorter = safer, but may feel disruptive."),
                    interval, 30f, 600f);
                if (!Mathf.Approximately(newInterval, interval))
                {
                    BetterUnityPrefs.AutoSaveInterval = newInterval;
                    AutoSaveModule.OnSettingsChanged();
                }

                EditorGUI.LabelField(new Rect(0, y, w, 16),
                    FormatInterval(BetterUnityPrefs.AutoSaveInterval),
                    new GUIStyle(EditorStyles.centeredGreyMiniLabel));
                y += 20;

                bool showNotif = BetterUnityPrefs.AutoSaveShowNotification;
                bool newNotif  = SettingsToggle(ref y, w, new GUIContent("Show Notification"), showNotif);
                if (newNotif != showNotif) BetterUnityPrefs.AutoSaveShowNotification = newNotif;

                y += 12;
                if (GUI.Button(new Rect(0, y, 90, 22), "Save Now", EditorStyles.miniButton))
                    AutoSaveModule.ForceSave();
                if (GUI.Button(new Rect(96, y, 90, 22), "Reset Defaults", EditorStyles.miniButton))
                {
                    BetterUnityPrefs.AutoSaveInterval         = 120f;
                    BetterUnityPrefs.AutoSaveShowNotification = true;
                    AutoSaveModule.OnSettingsChanged();
                }
                y += 30;
            }

            y += 4;
            DrawHelpBox(y, w, "Saves all open scenes and project assets on the set interval. Pauses automatically during Play Mode.", MessageType.Info);
        }

        // ── Task List ─────────────────────────────────────────────────────

        static void DrawTaskListSettings(float w)
        {
            float y = 0;
            ModuleHeader(y, "Task List", 1, ref y);

            using (new EditorGUI.DisabledGroupScope(!GetModuleEnabled(1)))
            {
                var db = TaskListStorage.DB;
                var s  = db.settings;

                y = SectionLabel(y, "Behaviour");

                bool confirmDel = SettingsToggle(ref y, w, new GUIContent("Confirm before delete", "Shows a confirmation dialog before permanently deleting a task."), s.confirmOnDelete);
                if (confirmDel != s.confirmOnDelete) { s.confirmOnDelete = confirmDel; TaskListStorage.MarkDirty(); }

                bool autoMove = SettingsToggle(ref y, w, new GUIContent("Auto-move completed to Backlog"), s.autoMoveCompletedToBacklog);
                if (autoMove != s.autoMoveCompletedToBacklog) { s.autoMoveCompletedToBacklog = autoMove; TaskListStorage.MarkDirty(); }

                bool dlWarn = SettingsToggle(ref y, w, new GUIContent("Deadline warning", "Highlights tasks in red when their deadline is approaching."), s.showDeadlineWarning);
                if (dlWarn != s.showDeadlineWarning) { s.showDeadlineWarning = dlWarn; TaskListStorage.MarkDirty(); }

                if (s.showDeadlineWarning)
                {
                    int newDays = SettingsIntSlider(ref y, w, new GUIContent("  Warn days before", "How many days before the deadline to start showing the warning colour."), s.deadlineWarnDays, 1, 14);
                    if (newDays != s.deadlineWarnDays) { s.deadlineWarnDays = newDays; TaskListStorage.MarkDirty(); }
                }

                y += 8;
                EditorGUI.LabelField(new Rect(0, y, 160, 16), new GUIContent("Default label", "Applied automatically to every new task."), EditorStyles.miniLabel);
                y += 17;
                string newLabel = EditorGUI.TextField(new Rect(0, y, Mathf.Min(w - 4, 220), 18), s.defaultLabel);
                if (newLabel != s.defaultLabel) { s.defaultLabel = newLabel; TaskListStorage.MarkDirty(); }
                y += 26;
                y += 4;

                if (GUI.Button(new Rect(0, y, 150, 22), "Open Task List Window", EditorStyles.miniButton))
                    TaskListWindow.Open();
                if (GUI.Button(new Rect(156, y, 100, 22), "Reset Defaults", EditorStyles.miniButton))
                {
                    s.confirmOnDelete            = true;
                    s.autoMoveCompletedToBacklog = false;
                    s.showDeadlineWarning        = true;
                    s.deadlineWarnDays           = 2;
                    s.defaultLabel               = "";
                    TaskListStorage.MarkDirty();
                }
                y += 32;

                y = SectionLabel(y, "Data");

                if (GUI.Button(new Rect(0, y, 150, 22), "Clear Completed Tasks", EditorStyles.miniButton))
                {
                    if (EditorUtility.DisplayDialog("Clear Completed", "Remove all completed tasks?", "Clear", "Cancel"))
                    { db.tasks.RemoveAll(t => t.completed); TaskListStorage.MarkDirty(); }
                }

                var oldC = GUI.color;
                GUI.color = new Color(1f, 0.7f, 0.7f);
                if (GUI.Button(new Rect(158, y, 120, 22), "Delete All Tasks", EditorStyles.miniButton))
                {
                    GUI.color = oldC;
                    if (EditorUtility.DisplayDialog("Delete All Tasks", "Delete ALL tasks? This cannot be undone.", "Delete All", "Cancel"))
                    { db.tasks.Clear(); TaskListStorage.MarkDirty(); }
                }
                GUI.color = oldC;
                y += 30;
            }
        }

        // ── Align To Ground ──────────────────────────────────────────────

        static void DrawAlignToGroundSettings(float w)
        {
            float y = 0;
            ModuleHeader(y, "Align To Ground", 3, ref y);

            using (new EditorGUI.DisabledGroupScope(!GetModuleEnabled(3)))
            {
                y = SectionLabel(y, "Behaviour");

                bool alignRot = SettingsToggle(ref y, w,
                    new GUIContent("Align rotation to terrain normal", "Tilts the object to match the average slope of the ground beneath it."),
                    BetterUnityPrefs.AlignRotationToNormal);
                if (alignRot != BetterUnityPrefs.AlignRotationToNormal) BetterUnityPrefs.AlignRotationToNormal = alignRot;

                y += 4;
                y = SectionLabel(y, "Sampling");

                int samples = SettingsIntSlider(ref y, w,
                    new GUIContent("Samples per axis", "Grid density for raycasting across the object's footprint. 7 = 7x7 = 49 rays. Higher = more accurate on irregular terrain but slower."),
                    BetterUnityPrefs.AlignSamplesPerAxis, 1, 20);
                if (samples != BetterUnityPrefs.AlignSamplesPerAxis) BetterUnityPrefs.AlignSamplesPerAxis = samples;

                EditorGUI.LabelField(new Rect(0, y, w, 16),
                    $"{samples * samples} raycast{(samples * samples == 1 ? "" : "s")} per object",
                    new GUIStyle(EditorStyles.centeredGreyMiniLabel));
                y += 20;

                float minDot = SettingsSlider(ref y, w,
                    new GUIContent("Min surface angle", "Surfaces with a normal dot product below this are ignored. Filters out walls and steep slopes."),
                    BetterUnityPrefs.AlignMinNormalDot, 0f, 1f);
                if (!Mathf.Approximately(minDot, BetterUnityPrefs.AlignMinNormalDot)) BetterUnityPrefs.AlignMinNormalDot = minDot;

                float deg = Mathf.Acos(Mathf.Clamp01(BetterUnityPrefs.AlignMinNormalDot)) * Mathf.Rad2Deg;
                EditorGUI.LabelField(new Rect(0, y, w, 16),
                    $"Ignores surfaces steeper than {deg:F0}°",
                    new GUIStyle(EditorStyles.centeredGreyMiniLabel));
                y += 20;

                y += 4;
                y = SectionLabel(y, "Ground Layers");

                EditorGUI.LabelField(new Rect(0, y, w, 16), new GUIContent("Layers", "Which physics layers are considered ground."), EditorStyles.miniLabel);
                y += 17;
                LayerMask cur = BetterUnityPrefs.AlignGroundLayers;
                LayerMask upd = EditorGUI.MaskField(new Rect(0, y, Mathf.Min(w - 4, 280), 18), cur,
                    UnityEditorInternal.InternalEditorUtility.layers);
                if (upd != cur) BetterUnityPrefs.AlignGroundLayers = upd;
                y += 26;

                y += 8;
                if (GUI.Button(new Rect(0, y, 130, 22), "Align Selection Now", EditorStyles.miniButton))
                    AlignToGroundModule.AlignSelected();
                if (GUI.Button(new Rect(136, y, 100, 22), "Reset Defaults", EditorStyles.miniButton))
                {
                    BetterUnityPrefs.AlignRotationToNormal = true;
                    BetterUnityPrefs.AlignSamplesPerAxis   = 7;
                    BetterUnityPrefs.AlignMinNormalDot     = 0.3f;
                    BetterUnityPrefs.AlignGroundLayers     = ~0;
                }
                y += 30;

                y += 4;
                DrawHelpBox(y, w, "Right-click a GameObject in the Hierarchy → Align To Ground, or press Ctrl+Shift+T.", MessageType.Info);
            }
        }

        // ── Toolbar ───────────────────────────────────────────────────────

        static void DrawToolbarSettings(float w)
        {
            float y = 0;
            ModuleHeader(y, "Toolbar", 5, ref y);

            using (new EditorGUI.DisabledGroupScope(!GetModuleEnabled(5)))
            {
                y = SectionLabel(y, "Elements");

                bool ss = SettingsToggle(ref y, w, new GUIContent("Screenshot Button"),  BetterUnityPrefs.ToolbarScreenshotEnabled);
                if (ss != BetterUnityPrefs.ToolbarScreenshotEnabled) { BetterUnityPrefs.ToolbarScreenshotEnabled = ss; MainToolbar.Refresh("BetterUnity/Screenshot"); }

                bool sc = SettingsToggle(ref y, w, new GUIContent("Scene Switcher"),     BetterUnityPrefs.ToolbarSceneSwitcherEnabled);
                if (sc != BetterUnityPrefs.ToolbarSceneSwitcherEnabled) { BetterUnityPrefs.ToolbarSceneSwitcherEnabled = sc; MainToolbar.Refresh("BetterUnity/SceneSwitcher"); }

                bool bk = SettingsToggle(ref y, w, new GUIContent("Scene Bookmarks"),    BetterUnityPrefs.ToolbarBookmarksEnabled);
                if (bk != BetterUnityPrefs.ToolbarBookmarksEnabled) { BetterUnityPrefs.ToolbarBookmarksEnabled = bk; MainToolbar.Refresh("BetterUnity/Bookmarks"); }

                bool ts = SettingsToggle(ref y, w, new GUIContent("Time Scale Slider"),  BetterUnityPrefs.ToolbarTimeScaleEnabled);
                if (ts != BetterUnityPrefs.ToolbarTimeScaleEnabled) { BetterUnityPrefs.ToolbarTimeScaleEnabled = ts; MainToolbar.Refresh("BetterUnity/TimeScale"); }

                bool fc = SettingsToggle(ref y, w, new GUIContent("FPS Cap Slider"),     BetterUnityPrefs.ToolbarFpsCapEnabled);
                if (fc != BetterUnityPrefs.ToolbarFpsCapEnabled) { BetterUnityPrefs.ToolbarFpsCapEnabled = fc; MainToolbar.Refresh("BetterUnity/FpsCap"); }

                y += 8;
                DrawHelpBox(y, w, "Changes require a domain reload. Use the toolbar overflow menu ( \u22ee ) to pin elements.", MessageType.Info);
                y += 48;

                y = SectionLabel(y, "Screenshot");

                var s    = ToolbarStorage.Screenshot;
                bool game = s.target == ScreenshotTarget.GameView;

                EditorGUI.LabelField(new Rect(0, y, w, 16), "Target", EditorStyles.miniLabel);
                y += 17;
                int newTarget = GUI.SelectionGrid(new Rect(0, y, Mathf.Min(w - 4, 240), 20), game ? 0 : 1,
                    new[] { "Game View", "Scene View" }, 2, EditorStyles.miniButton);
                if (newTarget != (game ? 0 : 1)) { s.target = newTarget == 0 ? ScreenshotTarget.GameView : ScreenshotTarget.SceneView; ToolbarStorage.Save(); }
                y += 26;

                if (s.target == ScreenshotTarget.GameView)
                {
                    bool hideUI = SettingsToggle(ref y, w, new GUIContent("Hide UI on capture"), s.hideUI);
                    if (hideUI != s.hideUI) { s.hideUI = hideUI; ToolbarStorage.Save(); }
                }

                bool custom = SettingsToggle(ref y, w, new GUIContent("Custom resolution"), s.useCustomSize);
                if (custom != s.useCustomSize) { s.useCustomSize = custom; ToolbarStorage.Save(); }

                if (s.useCustomSize)
                {
                    EditorGUI.LabelField(new Rect(0, y, w, 16), "Width", EditorStyles.miniLabel);  y += 17;
                    int cw = EditorGUI.IntField(new Rect(0, y, Mathf.Min(w - 4, 120), 18), s.customWidth);
                    if (cw != s.customWidth) { s.customWidth = Mathf.Max(1, cw); ToolbarStorage.Save(); }
                    y += 24;

                    EditorGUI.LabelField(new Rect(0, y, w, 16), "Height", EditorStyles.miniLabel); y += 17;
                    int ch = EditorGUI.IntField(new Rect(0, y, Mathf.Min(w - 4, 120), 18), s.customHeight);
                    if (ch != s.customHeight) { s.customHeight = Mathf.Max(1, ch); ToolbarStorage.Save(); }
                    y += 24;
                }
                else if (s.presets != null && s.presets.Count > 0)
                {
                    var names = new string[s.presets.Count];
                    for (int i = 0; i < s.presets.Count; i++)
                        names[i] = $"{s.presets[i].name}  ({s.presets[i].width}x{s.presets[i].height})";

                    EditorGUI.LabelField(new Rect(0, y, w, 16), "Preset", EditorStyles.miniLabel); y += 17;
                    int sel = EditorGUI.Popup(new Rect(0, y, Mathf.Min(w - 4, 260), 18), s.selectedPresetIndex, names);
                    if (sel != s.selectedPresetIndex) { s.selectedPresetIndex = sel; ToolbarStorage.Save(); }
                    y += 24;
                }

                EditorGUI.LabelField(new Rect(0, y, w, 16), "Output folder", EditorStyles.miniLabel); y += 17;
                float browseW = 50f;
                string newPath = EditorGUI.TextField(new Rect(0, y, Mathf.Min(w - 4, 260) - browseW - 4, 18), s.outputPath);
                if (newPath != s.outputPath) { s.outputPath = newPath; ToolbarStorage.Save(); }
                if (GUI.Button(new Rect(Mathf.Min(w - 4, 260) - browseW, y, browseW, 18), "Browse", EditorStyles.miniButton))
                {
                    string picked = EditorUtility.OpenFolderPanel("Screenshot Output", s.outputPath, "");
                    if (!string.IsNullOrEmpty(picked)) { s.outputPath = picked; ToolbarStorage.Save(); }
                }
                y += 26;

                bool openAfter = SettingsToggle(ref y, w, new GUIContent("Reveal after capture"), s.openAfterCapture);
                if (openAfter != s.openAfterCapture) { s.openAfterCapture = openAfter; ToolbarStorage.Save(); }

                y += 8;
                if (GUI.Button(new Rect(0, y, 120, 22), "Capture Now", EditorStyles.miniButton))
                    ScreenshotModule.Capture();
                if (GUI.Button(new Rect(126, y, 100, 22), "Reset Defaults", EditorStyles.miniButton))
                {
                    var def = new ScreenshotSettings();
                    s.target             = def.target;
                    s.hideUI             = def.hideUI;
                    s.useCustomSize      = def.useCustomSize;
                    s.customWidth        = def.customWidth;
                    s.customHeight       = def.customHeight;
                    s.selectedPresetIndex= def.selectedPresetIndex;
                    s.outputPath         = def.outputPath;
                    s.openAfterCapture   = def.openAfterCapture;
                    ToolbarStorage.Save();
                }
                y += 32;

                y = SectionLabel(y, "Scene Bookmarks");

                var db = SceneBookmarkStorage.DB;
                if (db.bookmarks.Count == 0)
                {
                    EditorGUI.LabelField(new Rect(0, y, w, 18), "No bookmarks saved yet.", EditorStyles.miniLabel);
                    y += 20;
                }
                else
                {
                    for (int i = 0; i < db.bookmarks.Count; i++)
                    {
                        var bm    = db.bookmarks[i];
                        string scene = Path.GetFileNameWithoutExtension(bm.scenePath);
                        EditorGUI.LabelField(new Rect(0, y, Mathf.Min(w - 54, 300), 18),
                            $"{bm.label}  —  {scene}", EditorStyles.miniLabel);
                        if (GUI.Button(new Rect(Mathf.Min(w - 54, 300) + 4, y, 46, 18), "Delete", EditorStyles.miniButton))
                        {
                            db.bookmarks.RemoveAt(i);
                            SceneBookmarkStorage.Save();
                            break;
                        }
                        y += 22;
                    }
                    y += 4;
                    if (GUI.Button(new Rect(0, y, 130, 22), "Clear All Bookmarks", EditorStyles.miniButton))
                    {
                        if (EditorUtility.DisplayDialog("Clear Bookmarks", "Delete all bookmarks?", "Clear", "Cancel"))
                        { db.bookmarks.Clear(); SceneBookmarkStorage.Save(); }
                    }
                }
            }
        }

        // ── Folder Icons ──────────────────────────────────────────────────────

        static void DrawFolderIconSettings(float w)
        {
            float y = 0;
            ModuleHeader(y, "Folder Icons", 6, ref y);

            using (new EditorGUI.DisabledGroupScope(!GetModuleEnabled(6)))
            {
                EditorGUI.LabelField(new Rect(0, y, w - 4, 32),
                    "Right-click any folder > Folder Style to apply a colour, custom texture, or overlay badge.",
                    new GUIStyle(EditorStyles.wordWrappedMiniLabel)
                    { normal = { textColor = new Color(0.6f, 0.6f, 0.6f) } });
                y += 40;

                // ── Icon Size ──────────────────────────────────────────────
                y = SectionLabel(y, "Icon Size");

                float curScale = BetterUnityPrefs.FolderIconScale;
                float newScale = SettingsSlider(ref y, w,
                    new GUIContent("Scale", "Shrink folder icons relative to their slot. 1 = full size, 0.25 = quarter size."),
                    curScale, 0.25f, 1f);
                if (!Mathf.Approximately(newScale, curScale))
                {
                    BetterUnityPrefs.FolderIconScale = newScale;
                    EditorApplication.RepaintProjectWindow();
                }

                int pct = Mathf.RoundToInt(BetterUnityPrefs.FolderIconScale * 100f);
                EditorGUI.LabelField(new Rect(0, y, w, 16), $"{pct}%",
                    new GUIStyle(EditorStyles.centeredGreyMiniLabel));
                y += 20;

                if (GUI.Button(new Rect(0, y, 100, 20), "Reset Defaults", EditorStyles.miniButton))
                {
                    BetterUnityPrefs.FolderIconScale = 1f;
                    EditorApplication.RepaintProjectWindow();
                }
                y += 28;

                // ── Styled Folders ─────────────────────────────────────────
                y = SectionLabel(y, "Styled Folders");

                var db = FolderIconStorage.DB;
                if (db.entries.Count == 0)
                {
                    EditorGUI.LabelField(new Rect(0, y, w, 18), "No folders styled yet.", EditorStyles.miniLabel);
                    y += 20;
                }
                else
                {
                    for (int i = 0; i < db.entries.Count; i++)
                    {
                        var e = db.entries[i];
                        string p = AssetDatabase.GUIDToAssetPath(e.guid);
                        if (string.IsNullOrEmpty(p)) continue;

                        if (e.iconType == FolderIconType.Color)
                            EditorGUI.DrawRect(new Rect(0, y + 3, 12, 12), e.tintColor);

                        EditorGUI.LabelField(new Rect(16, y, Mathf.Min(w - 64, 300), 18),
                            System.IO.Path.GetFileName(p), EditorStyles.miniLabel);

                        if (GUI.Button(new Rect(Mathf.Min(w - 64, 300) + 20, y, 44, 18),
                                "Clear", EditorStyles.miniButton))
                        {
                            db.Remove(e.guid);
                            FolderIconStorage.Save();
                            EditorApplication.RepaintProjectWindow();
                            break;
                        }
                        y += 22;
                    }

                    y += 4;
                    if (GUI.Button(new Rect(0, y, 130, 22), "Clear All Styles", EditorStyles.miniButton))
                    {
                        if (EditorUtility.DisplayDialog("Clear All Folder Styles",
                            "Remove all custom folder icons?", "Clear", "Cancel"))
                        {
                            db.entries.Clear();
                            FolderIconStorage.Save();
                            EditorApplication.RepaintProjectWindow();
                        }
                    }
                }
            }
        }



        // ── Hierarchy ────────────────────────────────────────────────────────────

        static void DrawSmartHierarchySettings(float w)
        {
            float y = 0;
            ModuleHeader(y, "Hierarchy", 7, ref y);

            using (new EditorGUI.DisabledGroupScope(!GetModuleEnabled(7)))
            {
                // ── Appearance ────────────────────────────────────────────
                y = SectionLabel(y, "Rows");

                bool zebra = SettingsToggle(ref y, w, new GUIContent("Zebra Striping", "Alternating row tint to make rows easier to scan."), BetterUnityPrefs.HierarchyZebraEnabled);
                if (zebra != BetterUnityPrefs.HierarchyZebraEnabled) { BetterUnityPrefs.HierarchyZebraEnabled = zebra; EditorApplication.RepaintHierarchyWindow(); }

                using (new EditorGUI.DisabledGroupScope(!BetterUnityPrefs.HierarchyZebraEnabled))
                {
                    EditorGUI.LabelField(new Rect(16, y, 80, 16), "Color", EditorStyles.miniLabel);
                    Color newZC = EditorGUI.ColorField(new Rect(100, y, Mathf.Min(w - 104, 140), 16), BetterUnityPrefs.HierarchyZebraColor);
                    if (newZC != BetterUnityPrefs.HierarchyZebraColor) { BetterUnityPrefs.HierarchyZebraColor = newZC; EditorApplication.RepaintHierarchyWindow(); }
                    y += 22;
                }

                bool dimInactive = SettingsToggle(ref y, w, new GUIContent("Dim Inactive", "Darkens rows for GameObjects that are currently inactive."), BetterUnityPrefs.HierarchyDimInactiveEnabled);
                if (dimInactive != BetterUnityPrefs.HierarchyDimInactiveEnabled) { BetterUnityPrefs.HierarchyDimInactiveEnabled = dimInactive; EditorApplication.RepaintHierarchyWindow(); }

                y += 6;

                // ── Tree lines ────────────────────────────────────────────
                y = SectionLabel(y, "Tree Lines");

                bool lines = SettingsToggle(ref y, w, new GUIContent("Show Lines", "Draws L-shaped connecting lines in the indent area."), BetterUnityPrefs.HierarchyLinesEnabled);
                if (lines != BetterUnityPrefs.HierarchyLinesEnabled) { BetterUnityPrefs.HierarchyLinesEnabled = lines; EditorApplication.RepaintHierarchyWindow(); }

                using (new EditorGUI.DisabledGroupScope(!BetterUnityPrefs.HierarchyLinesEnabled))
                {
                    EditorGUI.LabelField(new Rect(16, y, 80, 16), "Color", EditorStyles.miniLabel);
                    Color newLC = EditorGUI.ColorField(new Rect(100, y, Mathf.Min(w - 104, 140), 16), BetterUnityPrefs.HierarchyLineColor);
                    if (newLC != BetterUnityPrefs.HierarchyLineColor) { BetterUnityPrefs.HierarchyLineColor = newLC; EditorApplication.RepaintHierarchyWindow(); }
                    y += 22;
                }

                y += 6;

                // ── Right-side elements ───────────────────────────────────
                y = SectionLabel(y, "Right-side Elements");

                bool icons = SettingsToggle(ref y, w, new GUIContent("Component Icons", "Shows small icons for key components on the right of each row."), BetterUnityPrefs.HierarchyComponentIconsEnabled);
                if (icons != BetterUnityPrefs.HierarchyComponentIconsEnabled) { BetterUnityPrefs.HierarchyComponentIconsEnabled = icons; EditorApplication.RepaintHierarchyWindow(); }

                using (new EditorGUI.DisabledGroupScope(!BetterUnityPrefs.HierarchyComponentIconsEnabled))
                {
                    int cap = SettingsIntSlider(ref y, w, new GUIContent("  Icon Cap", "Maximum number of component icons shown per row."), BetterUnityPrefs.HierarchyIconCap, 1, 8);
                    if (cap != BetterUnityPrefs.HierarchyIconCap) { BetterUnityPrefs.HierarchyIconCap = cap; HierarchyModule.InvalidateCaches(); EditorApplication.RepaintHierarchyWindow(); }

                    float opacity = SettingsSlider(ref y, w, new GUIContent("  Icon Opacity"), BetterUnityPrefs.HierarchyIconOpacity, 0.1f, 1f);
                    if (!Mathf.Approximately(opacity, BetterUnityPrefs.HierarchyIconOpacity)) { BetterUnityPrefs.HierarchyIconOpacity = opacity; EditorApplication.RepaintHierarchyWindow(); }
                }

                bool toggle = SettingsToggle(ref y, w, new GUIContent("Active Toggle", "Checkbox on the right to toggle active state without selecting the object."), BetterUnityPrefs.HierarchyActiveToggleEnabled);
                if (toggle != BetterUnityPrefs.HierarchyActiveToggleEnabled) { BetterUnityPrefs.HierarchyActiveToggleEnabled = toggle; EditorApplication.RepaintHierarchyWindow(); }

                y += 8;
                if (GUI.Button(new Rect(0, y, 100, 22), "Reset Defaults", EditorStyles.miniButton))
                {
                    BetterUnityPrefs.HierarchyZebraEnabled          = true;
                    BetterUnityPrefs.HierarchyZebraColor            = new Color(0f, 0f, 0f, 0.07f);
                    BetterUnityPrefs.HierarchyLinesEnabled          = true;
                    BetterUnityPrefs.HierarchyLineColor             = new Color(0.32f, 0.32f, 0.38f, 0.62f);
                    BetterUnityPrefs.HierarchyComponentIconsEnabled = true;
                    BetterUnityPrefs.HierarchyIconCap               = 4;
                    BetterUnityPrefs.HierarchyIconOpacity           = 0.75f;
                    BetterUnityPrefs.HierarchyActiveToggleEnabled   = true;
                    BetterUnityPrefs.HierarchyDimInactiveEnabled    = true;
                    HierarchyModule.InvalidateCaches();
                    EditorApplication.RepaintHierarchyWindow();
                }
                y += 30;

                // ── Object styles ─────────────────────────────────────────
                y = SectionLabel(y, "Object Styles");

                EditorGUI.LabelField(new Rect(0, y, w - 4, 32),
                    "Hover any object in the Hierarchy and click the ⚙ icon to set it as a Header or add a note.",
                    new GUIStyle(EditorStyles.wordWrappedMiniLabel) { normal = { textColor = new Color(0.6f, 0.6f, 0.6f) } });
                y += 38;

                var db = HierarchyStorage.DB;
                if (db.items.Count == 0)
                {
                    EditorGUI.LabelField(new Rect(0, y, w, 18), "No styled objects yet.", EditorStyles.miniLabel);
                    y += 20;
                }
                else
                {
                    for (int i = 0; i < db.items.Count; i++)
                    {
                        var    item  = db.items[i];
                        string label = item.key.Contains("|") ? item.key.Substring(item.key.LastIndexOf('/') + 1) : item.key;

                        if (item.style == HierarchyItemStyle.Header)
                            EditorGUI.DrawRect(new Rect(0, y + 3, 4, 14), item.color);

                        EditorGUI.LabelField(new Rect(8, y, Mathf.Min(w - 64, 300), 18),
                            $"{label}  [{item.style}]", EditorStyles.miniLabel);

                        if (GUI.Button(new Rect(Mathf.Min(w - 64, 300) + 12, y, 50, 18), "Remove", EditorStyles.miniButton))
                        {
                            db.items.RemoveAt(i);
                            HierarchyStorage.Save();
                            HierarchyModule.InvalidateCaches();
                            EditorApplication.RepaintHierarchyWindow();
                            break;
                        }
                        y += 22;
                    }

                    y += 4;
                    if (GUI.Button(new Rect(0, y, 150, 22), "Clear All Styles", EditorStyles.miniButton))
                    {
                        if (EditorUtility.DisplayDialog("Clear All Object Styles",
                            "Remove all Hierarchy object styles?", "Clear", "Cancel"))
                        {
                            db.items.Clear();
                            HierarchyStorage.Save();
                            HierarchyModule.InvalidateCaches();
                            EditorApplication.RepaintHierarchyWindow();
                        }
                    }
                }
            }
        }

        // ── Simple module ─────────────────────────────────────────────────

        static void DrawSimpleModuleSettings(float w, int idx, string name, string description, System.Action openWindow)
        {
            float y = 0;
            ModuleHeader(y, name, idx, ref y);

            using (new EditorGUI.DisabledGroupScope(!GetModuleEnabled(idx)))
            {
                EditorGUI.LabelField(new Rect(0, y, w - 4, 32), description,
                    new GUIStyle(EditorStyles.wordWrappedMiniLabel) { normal = { textColor = new Color(0.6f, 0.6f, 0.6f) } });
                y += 40;

                if (openWindow != null && GUI.Button(new Rect(0, y, 130, 22), $"Open {name}", EditorStyles.miniButton))
                    openWindow.Invoke();
            }
        }

        // ── Helpers ───────────────────────────────────────────────────────

        static float ModuleHeader(float y, string name, int moduleIdx, ref float yOut)
        {
            var nameStyle = new GUIStyle(EditorStyles.boldLabel) { fontSize = 13 };
            EditorGUI.LabelField(new Rect(0, y, 200, 22), name, nameStyle);

            bool cur     = GetModuleEnabled(moduleIdx);
            bool toggled = EditorGUI.Toggle(new Rect(210, y + 3, 16, 16), cur);
            EditorGUI.LabelField(new Rect(228, y + 3, 60, 16), cur ? "Enabled" : "Disabled", EditorStyles.miniLabel);
            if (toggled != cur) SetModuleEnabled(moduleIdx, toggled);

            y += 26;
            EditorGUI.DrawRect(new Rect(0, y, 500, 1), new Color(0f, 0f, 0f, 0.15f));
            y += 10;

            yOut = y;
            return y;
        }

        static float SettingsSlider(ref float y, float w, GUIContent label, float val, float min, float max)
        {
            EditorGUI.LabelField(new Rect(0, y, w, 16), label, EditorStyles.miniLabel);
            y += 17;
            float nv = EditorGUI.Slider(new Rect(0, y, Mathf.Min(w - 4, 300), 18), val, min, max);
            y += 24;
            return nv;
        }

        static int SettingsIntSlider(ref float y, float w, GUIContent label, int val, int min, int max)
        {
            EditorGUI.LabelField(new Rect(0, y, w, 16), label, EditorStyles.miniLabel);
            y += 17;
            int nv = EditorGUI.IntSlider(new Rect(0, y, Mathf.Min(w - 4, 300), 18), val, min, max);
            y += 24;
            return nv;
        }

        static bool SettingsToggle(ref float y, float w, GUIContent label, bool val)
        {
            bool nv = EditorGUI.Toggle(new Rect(0, y + 1, 16, 16), val);
            EditorGUI.LabelField(new Rect(20, y, w - 20, 18), label, EditorStyles.label);
            y += 22;
            return nv;
        }

        static float SectionLabel(float y, string title)
        {
            EditorGUI.LabelField(new Rect(0, y, 300, 18), title, EditorStyles.boldLabel);
            y += 20;
            EditorGUI.DrawRect(new Rect(0, y, 300, 1), new Color(0f, 0f, 0f, 0.15f));
            return y + 8;
        }

        static void DrawHelpBox(float y, float w, string msg, MessageType type)
        {
            EditorGUI.HelpBox(new Rect(0, y, Mathf.Min(w - 4, 380), 40), msg, type);
        }

        static string FormatInterval(float seconds)
        {
            int m = Mathf.FloorToInt(seconds / 60f);
            int s = Mathf.RoundToInt(seconds % 60f);
            return m > 0 ? $"Every {m}m {s}s" : $"Every {s}s";
        }
    }
}
