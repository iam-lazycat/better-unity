using UnityEditor;
using UnityEngine;

namespace LazyCat.BetterUnity
{
    public static class BetterUnityMenu
    {
        // ── top menu ──────────────────────────────────────────────────────

        [MenuItem("Tools/Lazy Cat/Better Unity/Task List", false, 0)]
        public static void OpenTaskList() => TaskListWindow.Open();

        [MenuItem("Tools/Lazy Cat/Better Unity/Bulk Rename", false, 1)]
        public static void OpenBulkRename() => BulkRenameWindow.Open();

        [MenuItem("Tools/Lazy Cat/Better Unity/Align To Ground", false, 2)]
        public static void MenuAlignToGround() => AlignToGroundModule.AlignSelected();

        [MenuItem("Tools/Lazy Cat/Better Unity/Align To Ground", true)]
        static bool MenuAlignToGroundValidate() =>
            Selection.gameObjects.Length > 0 && BetterUnityPrefs.AlignToGroundEnabled;


        [MenuItem("Tools/Lazy Cat/Better Unity/Settings", false, 50)]
        public static void OpenSettings() => SettingsService.OpenProjectSettings("Project/Better Unity");

        [MenuItem("Tools/Lazy Cat/Better Unity/About", false, 51)]
        public static void OpenAbout() => AboutWindow.Open();

        // ── Scene view right-click (GameObjectToolContext = default tool) ──
        // This is the correct Unity 6 API for flat items in the scene view menu.

        [MenuItem("CONTEXT/GameObjectToolContext/Align To Ground", false, 0)]
        static void SceneViewAlignToGround(MenuCommand cmd) => AlignToGroundModule.AlignSelected();

        [MenuItem("CONTEXT/GameObjectToolContext/Align To Ground", true)]
        static bool SceneViewAlignToGroundValidate(MenuCommand cmd) =>
            Selection.gameObjects.Length > 0 && BetterUnityPrefs.AlignToGroundEnabled;

        // ── Hierarchy right-click ─────────────────────────────────────────

        [MenuItem("GameObject/Align To Ground %#t", false, 0)]
        static void GOAlignToGround() => AlignToGroundModule.AlignSelected();

        [MenuItem("GameObject/Align To Ground %#t", true)]
        static bool GOAlignToGroundValidate() =>
            Selection.gameObjects.Length > 0 && BetterUnityPrefs.AlignToGroundEnabled;

        [MenuItem("GameObject/Bulk Rename", false, 0)]
        static void GOBulkRename() => BulkRenameWindow.Open();

        [MenuItem("GameObject/Bulk Rename", true)]
        static bool GOBulkRenameValidate() =>
            Selection.gameObjects.Length > 0 && BetterUnityPrefs.BulkRenameEnabled;

        // ── Transform Inspector context menu ──────────────────────────────

        [MenuItem("CONTEXT/Transform/Align To Ground", false, -99)]
        static void TransformContextAlign(MenuCommand cmd)
        {
            var go   = ((Transform)cmd.context).gameObject;
            var prev = Selection.gameObjects;
            Selection.objects = new Object[] { go };
            AlignToGroundModule.AlignSelected();
            Selection.objects = prev;
        }

        [MenuItem("CONTEXT/Transform/Align To Ground", true)]
        static bool TransformContextAlignValidate(MenuCommand cmd) =>
            BetterUnityPrefs.AlignToGroundEnabled;

        // ── Project window right-click ────────────────────────────────────

        [MenuItem("Assets/Bulk Rename", false, 0)]
        static void AssetsBulkRename() => BulkRenameWindow.Open();

        [MenuItem("Assets/Bulk Rename", true)]
        static bool AssetsBulkRenameValidate() =>
            Selection.objects.Length > 0 && BetterUnityPrefs.BulkRenameEnabled;
    }
}
