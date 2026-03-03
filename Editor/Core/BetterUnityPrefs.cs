using UnityEditor;
using UnityEngine;

namespace LazyCat.BetterUnity
{
    public static class BetterUnityPrefs
    {
        private const string Prefix = "LazyCat.BetterUnity.";

        // ── Auto Save ─────────────────────────────────────────────────────

        private const string K_AutoSaveEnabled   = Prefix + "AutoSave.Enabled";
        private const string K_AutoSaveInterval  = Prefix + "AutoSave.Interval";
        private const string K_AutoSaveShowNotif = Prefix + "AutoSave.ShowNotification";

        public static bool AutoSaveEnabled
        {
            get => EditorPrefs.GetBool(K_AutoSaveEnabled, true);
            set => EditorPrefs.SetBool(K_AutoSaveEnabled, value);
        }

        public static float AutoSaveInterval
        {
            get => EditorPrefs.GetFloat(K_AutoSaveInterval, 120f);
            set => EditorPrefs.SetFloat(K_AutoSaveInterval, value);
        }

        public static bool AutoSaveShowNotification
        {
            get => EditorPrefs.GetBool(K_AutoSaveShowNotif, true);
            set => EditorPrefs.SetBool(K_AutoSaveShowNotif, value);
        }

        // ── Task List ─────────────────────────────────────────────────────

        private const string K_TaskListEnabled = Prefix + "TaskList.Enabled";
        public static bool TaskListEnabled
        {
            get => EditorPrefs.GetBool(K_TaskListEnabled, true);
            set => EditorPrefs.SetBool(K_TaskListEnabled, value);
        }

        // ── Bulk Rename ───────────────────────────────────────────────────

        private const string K_BulkRenameEnabled = Prefix + "BulkRename.Enabled";
        public static bool BulkRenameEnabled
        {
            get => EditorPrefs.GetBool(K_BulkRenameEnabled, true);
            set => EditorPrefs.SetBool(K_BulkRenameEnabled, value);
        }

        // ── Align To Terrain ──────────────────────────────────────────────

        private const string K_AlignEnabled      = Prefix + "Align.Enabled";
        private const string K_AlignSamples      = Prefix + "Align.SamplesPerAxis";
        private const string K_AlignRotation     = Prefix + "Align.RotationToNormal";
        private const string K_AlignMinNormalDot = Prefix + "Align.MinNormalDot";
        private const string K_AlignGroundLayers = Prefix + "Align.GroundLayers";

        public static bool AlignToGroundEnabled
        {
            get => EditorPrefs.GetBool(K_AlignEnabled, true);
            set => EditorPrefs.SetBool(K_AlignEnabled, value);
        }

        public static int AlignSamplesPerAxis
        {
            get => EditorPrefs.GetInt(K_AlignSamples, 7);
            set => EditorPrefs.SetInt(K_AlignSamples, Mathf.Clamp(value, 1, 20));
        }

        public static bool AlignRotationToNormal
        {
            get => EditorPrefs.GetBool(K_AlignRotation, true);
            set => EditorPrefs.SetBool(K_AlignRotation, value);
        }

        public static float AlignMinNormalDot
        {
            get => EditorPrefs.GetFloat(K_AlignMinNormalDot, 0.3f);
            set => EditorPrefs.SetFloat(K_AlignMinNormalDot, value);
        }

        public static int AlignGroundLayers
        {
            get => EditorPrefs.GetInt(K_AlignGroundLayers, ~0);
            set => EditorPrefs.SetInt(K_AlignGroundLayers, value);
        }

        // ── Transform Copy Paste ──────────────────────────────────────────

        private const string K_TransformCopyPasteEnabled = Prefix + "TransformCopyPaste.Enabled";
        public static bool TransformCopyPasteEnabled
        {
            get => EditorPrefs.GetBool(K_TransformCopyPasteEnabled, true);
            set => EditorPrefs.SetBool(K_TransformCopyPasteEnabled, value);
        }

        // ── Toolbar ───────────────────────────────────────────────────────

        private const string K_ToolbarEnabled       = Prefix + "Toolbar.Enabled";
        private const string K_ToolbarScreenshot    = Prefix + "Toolbar.Screenshot";
        private const string K_ToolbarSceneSwitcher = Prefix + "Toolbar.SceneSwitcher";
        private const string K_ToolbarBookmarks     = Prefix + "Toolbar.Bookmarks";
        private const string K_ToolbarTimeScale     = Prefix + "Toolbar.TimeScale";
        private const string K_ToolbarFpsCap        = Prefix + "Toolbar.FpsCap";
        private const string K_ToolbarFpsCapValue   = Prefix + "Toolbar.FpsCapValue";

        public static bool ToolbarEnabled
        {
            get => EditorPrefs.GetBool(K_ToolbarEnabled, true);
            set => EditorPrefs.SetBool(K_ToolbarEnabled, value);
        }

        public static bool ToolbarScreenshotEnabled
        {
            get => EditorPrefs.GetBool(K_ToolbarScreenshot, true);
            set => EditorPrefs.SetBool(K_ToolbarScreenshot, value);
        }

        public static bool ToolbarSceneSwitcherEnabled
        {
            get => EditorPrefs.GetBool(K_ToolbarSceneSwitcher, true);
            set => EditorPrefs.SetBool(K_ToolbarSceneSwitcher, value);
        }

        public static bool ToolbarBookmarksEnabled
        {
            get => EditorPrefs.GetBool(K_ToolbarBookmarks, true);
            set => EditorPrefs.SetBool(K_ToolbarBookmarks, value);
        }

        public static bool ToolbarTimeScaleEnabled
        {
            get => EditorPrefs.GetBool(K_ToolbarTimeScale, true);
            set => EditorPrefs.SetBool(K_ToolbarTimeScale, value);
        }

        public static bool ToolbarFpsCapEnabled
        {
            get => EditorPrefs.GetBool(K_ToolbarFpsCap, true);
            set => EditorPrefs.SetBool(K_ToolbarFpsCap, value);
        }

        public static int ToolbarFpsCapValue
        {
            get => EditorPrefs.GetInt(K_ToolbarFpsCapValue, 0);
            set => EditorPrefs.SetInt(K_ToolbarFpsCapValue, value);
        }

        // ── Folder Icons ──────────────────────────────────────────────────

        private const string K_FolderIconEnabled = Prefix + "FolderIcon.Enabled";
        private const string K_FolderIconScale   = Prefix + "FolderIcon.Scale";

        public static bool FolderIconEnabled
        {
            get => EditorPrefs.GetBool(K_FolderIconEnabled, true);
            set => EditorPrefs.SetBool(K_FolderIconEnabled, value);
        }

        public static float FolderIconScale
        {
            get => EditorPrefs.GetFloat(K_FolderIconScale, 1f);
            set => EditorPrefs.SetFloat(K_FolderIconScale, Mathf.Clamp(value, 0.25f, 1f));
        }

        // ── Hierarchy ─────────────────────────────────────────────────────

        private const string K_HierarchyEnabled        = Prefix + "Hierarchy.Enabled";
        private const string K_HierarchyZebra          = Prefix + "Hierarchy.Zebra";
        private const string K_HierarchyZebraColor     = Prefix + "Hierarchy.ZebraColor";
        private const string K_HierarchyLines          = Prefix + "Hierarchy.Lines";
        private const string K_HierarchyLineColor      = Prefix + "Hierarchy.LineColor";
        private const string K_HierarchyIndentWidth    = Prefix + "Hierarchy.IndentWidth";
        private const string K_HierarchyComponentIcons = Prefix + "Hierarchy.ComponentIcons";
        private const string K_HierarchyIconCap        = Prefix + "Hierarchy.IconCap";
        private const string K_HierarchyIconOpacity    = Prefix + "Hierarchy.IconOpacity";
        private const string K_HierarchyActiveToggle   = Prefix + "Hierarchy.ActiveToggle";
        private const string K_HierarchyDimInactive    = Prefix + "Hierarchy.DimInactive";

        public static bool HierarchyEnabled
        {
            get => EditorPrefs.GetBool(K_HierarchyEnabled, true);
            set => EditorPrefs.SetBool(K_HierarchyEnabled, value);
        }

        public static bool HierarchyZebraEnabled
        {
            get => EditorPrefs.GetBool(K_HierarchyZebra, true);
            set => EditorPrefs.SetBool(K_HierarchyZebra, value);
        }

        public static Color HierarchyZebraColor
        {
            get
            {
                string h = EditorPrefs.GetString(K_HierarchyZebraColor, "00000012");
                return ColorUtility.TryParseHtmlString("#" + h, out var c) ? c : new Color(0f, 0f, 0f, 0.07f);
            }
            set => EditorPrefs.SetString(K_HierarchyZebraColor, ColorUtility.ToHtmlStringRGBA(value));
        }

        public static bool HierarchyLinesEnabled
        {
            get => EditorPrefs.GetBool(K_HierarchyLines, true);
            set => EditorPrefs.SetBool(K_HierarchyLines, value);
        }

        public static Color HierarchyLineColor
        {
            get
            {
                string h = EditorPrefs.GetString(K_HierarchyLineColor, "5252619E");
                return ColorUtility.TryParseHtmlString("#" + h, out var c) ? c : new Color(0.32f, 0.32f, 0.38f, 0.62f);
            }
            set => EditorPrefs.SetString(K_HierarchyLineColor, ColorUtility.ToHtmlStringRGBA(value));
        }

        public static float HierarchyIndentWidth
        {
            get => EditorPrefs.GetFloat(K_HierarchyIndentWidth, 14f);
            set => EditorPrefs.SetFloat(K_HierarchyIndentWidth, Mathf.Clamp(value, 10f, 22f));
        }

        public static bool HierarchyComponentIconsEnabled
        {
            get => EditorPrefs.GetBool(K_HierarchyComponentIcons, true);
            set => EditorPrefs.SetBool(K_HierarchyComponentIcons, value);
        }

        public static int HierarchyIconCap
        {
            get => EditorPrefs.GetInt(K_HierarchyIconCap, 4);
            set => EditorPrefs.SetInt(K_HierarchyIconCap, Mathf.Clamp(value, 1, 8));
        }

        public static float HierarchyIconOpacity
        {
            get => EditorPrefs.GetFloat(K_HierarchyIconOpacity, 0.75f);
            set => EditorPrefs.SetFloat(K_HierarchyIconOpacity, Mathf.Clamp01(value));
        }

        public static bool HierarchyActiveToggleEnabled
        {
            get => EditorPrefs.GetBool(K_HierarchyActiveToggle, true);
            set => EditorPrefs.SetBool(K_HierarchyActiveToggle, value);
        }

        public static bool HierarchyDimInactiveEnabled
        {
            get => EditorPrefs.GetBool(K_HierarchyDimInactive, true);
            set => EditorPrefs.SetBool(K_HierarchyDimInactive, value);
        }

    }
}