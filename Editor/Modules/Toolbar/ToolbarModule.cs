using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEditor.Toolbars;
using UnityEngine;
using UnityEngine.LowLevel;
using UnityEngine.SceneManagement;

namespace LazyCat.BetterUnity
{
    // ── Editor FPS Cap via Player Loop + background thread ────────────────────
    // Application.targetFrameRate is ignored by the editor entirely.
    // The only reliable way to cap editor FPS is to stall the player loop
    // using Thread.Sleep on a background thread synced to EarlyUpdate.

    [InitializeOnLoad]
    public static class ToolbarModule
    {
        const string K_Screenshot    = "BetterUnity/Screenshot";
        const string K_SceneSwitcher = "BetterUnity/SceneSwitcher";
        const string K_Bookmarks     = "BetterUnity/Bookmarks";
        const string K_TimeScale     = "BetterUnity/TimeScale";
        const string K_FpsCap        = "BetterUnity/FpsCap";

        // ── FPS cap internals ─────────────────────────────────────────────
        static AutoResetEvent _sync;
        static int            _intervalMs = 0;   // 0 = uncapped

        static ToolbarModule()
        {
            // Start the background throttle thread
            var t = new Thread(ThrottleThread) { IsBackground = true };
            t.Start();

            // Inject our update system into EarlyUpdate
            var system = new PlayerLoopSystem
            {
                type           = typeof(ToolbarModule),
                updateDelegate = FpsCapUpdate
            };

            var loop = PlayerLoop.GetCurrentPlayerLoop();
            for (int i = 0; i < loop.subSystemList.Length; i++)
            {
                ref var phase = ref loop.subSystemList[i];
                if (phase.type == typeof(UnityEngine.PlayerLoop.EarlyUpdate))
                {
                    phase.subSystemList = phase.subSystemList.Concat(new[] { system }).ToArray();
                    break;
                }
            }
            PlayerLoop.SetPlayerLoop(loop);

            // Apply saved cap value on load
            ApplyFpsCap(BetterUnityPrefs.ToolbarFpsCapValue);
        }

        static void ThrottleThread()
        {
            _sync = new AutoResetEvent(true);
            while (true)
            {
                int ms = _intervalMs;
                Thread.Sleep(ms > 0 ? ms : 1);
                _sync.Set();
            }
        }

        static void FpsCapUpdate()
        {
            if (_sync == null) return;
            if (!BetterUnityPrefs.ToolbarEnabled) return;
            if (!BetterUnityPrefs.ToolbarFpsCapEnabled) return;
            if (_intervalMs <= 0) return;       // uncapped — don't stall
            if (Time.captureDeltaTime != 0) return; // recording mode — don't interfere
            _sync.WaitOne();
        }

        // ── Screenshot ────────────────────────────────────────────────────

        [MainToolbarElement(K_Screenshot, defaultDockPosition = MainToolbarDockPosition.Left)]
        public static IEnumerable<MainToolbarElement> ScreenshotButton()
        {
            if (!BetterUnityPrefs.ToolbarEnabled || !BetterUnityPrefs.ToolbarScreenshotEnabled)
                yield break;

            var icon    = EditorGUIUtility.IconContent("d_SceneViewCamera").image as Texture2D;
            var content = new MainToolbarContent(null, icon, "Screenshot");
            yield return new MainToolbarButton(content, ScreenshotWindow.Open);
        }

        // ── Scene Switcher ────────────────────────────────────────────────

        [MainToolbarElement(K_SceneSwitcher, defaultDockPosition = MainToolbarDockPosition.Left)]
        public static IEnumerable<MainToolbarElement> SceneSwitcherDropdown()
        {
            if (!BetterUnityPrefs.ToolbarEnabled || !BetterUnityPrefs.ToolbarSceneSwitcherEnabled)
                yield break;

            string activeName = Application.isPlaying
                ? SceneManager.GetActiveScene().name
                : EditorSceneManager.GetActiveScene().name;
            if (string.IsNullOrEmpty(activeName)) activeName = "Untitled";

            var icon    = EditorGUIUtility.IconContent("d_SceneAsset Icon").image as Texture2D;
            var content = new MainToolbarContent(activeName, icon, "Switch Scene");
            yield return new MainToolbarDropdown(content, ShowSceneSwitcherMenu);
        }

        static void ShowSceneSwitcherMenu(Rect rect)
        {
            var    menu        = new GenericMenu();
            var    scenePaths  = GetAllScenePaths();
            var    hidden      = ToolbarStorage.Data.hiddenScenes;
            string activeScene = Application.isPlaying
                ? SceneManager.GetActiveScene().path
                : EditorSceneManager.GetActiveScene().path;

            foreach (var path in scenePaths)
            {
                var    local = path;
                string guid  = AssetDatabase.AssetPathToGUID(path);
                if (hidden.Contains(guid)) continue;

                string name   = Path.GetFileNameWithoutExtension(path);
                bool   active = path == activeScene;
                menu.AddItem(new GUIContent(name), active, () => LoadScene(local));
            }

            if (scenePaths.Length == 0)
                menu.AddDisabledItem(new GUIContent("No scenes found"));

            menu.AddSeparator("");
            menu.AddItem(new GUIContent("New Scene"), false, () =>
            {
                if (EditorSceneManager.SaveCurrentModifiedScenesIfUserWantsTo())
                    EditorSceneManager.NewScene(NewSceneSetup.DefaultGameObjects, NewSceneMode.Single);
            });
            menu.AddItem(new GUIContent("Manage Scenes..."), false, OpenSceneManager);

            menu.DropDown(rect);
        }

        static void LoadScene(string path)
        {
            if (Application.isPlaying) { SceneManager.LoadScene(path); return; }
            if (EditorSceneManager.SaveCurrentModifiedScenesIfUserWantsTo())
            {
                EditorSceneManager.OpenScene(path);
                MainToolbar.Refresh(K_SceneSwitcher);
            }
        }

        static void OpenSceneManager() => SceneManagerWindow.Open();

        public static string[] GetAllScenePaths()
        {
            var guids = AssetDatabase.FindAssets("t:Scene");
            var paths = new List<string>(guids.Length);
            foreach (var guid in guids)
            {
                var p = AssetDatabase.GUIDToAssetPath(guid);
                if (!string.IsNullOrEmpty(p) && p.StartsWith("Assets/"))
                    paths.Add(p);
            }
            paths.Sort();
            return paths.ToArray();
        }

        // ── Scene View Bookmarks ──────────────────────────────────────────

        [MainToolbarElement(K_Bookmarks, defaultDockPosition = MainToolbarDockPosition.Left)]
        public static IEnumerable<MainToolbarElement> BookmarksDropdown()
        {
            if (!BetterUnityPrefs.ToolbarEnabled || !BetterUnityPrefs.ToolbarBookmarksEnabled)
                yield break;

            var icon    = EditorGUIUtility.IconContent("d_Favorite").image as Texture2D;
            var content = new MainToolbarContent("Bookmarks", icon, "Scene View Bookmarks");
            yield return new MainToolbarDropdown(content, ShowBookmarksMenu);
        }

        static void ShowBookmarksMenu(Rect rect)
        {
            var menu = new GenericMenu();
            var db   = SceneBookmarkStorage.DB;

            if (db.bookmarks.Count == 0)
            {
                menu.AddDisabledItem(new GUIContent("No bookmarks saved"));
            }
            else
            {
                foreach (var bm in db.bookmarks)
                {
                    var local = bm;
                    menu.AddItem(new GUIContent(local.label), false, () => ApplyBookmark(local));
                }
                menu.AddSeparator("");
                foreach (var bm in db.bookmarks)
                {
                    var local = bm;
                    menu.AddItem(new GUIContent($"Delete/{local.label}"), false, () =>
                    {
                        db.bookmarks.Remove(local);
                        SceneBookmarkStorage.Save();
                        MainToolbar.Refresh(K_Bookmarks);
                    });
                }
                menu.AddSeparator("");
            }

            menu.AddItem(new GUIContent("Save Current View..."), false, SaveCurrentView);
            menu.AddItem(new GUIContent("Manage Bookmarks..."),  false, () => BookmarkManagerWindow.Open());
            menu.DropDown(rect);
        }

        static void SaveCurrentView()
        {
            var sv = SceneView.lastActiveSceneView;
            if (sv == null) { Debug.LogWarning("[Better Unity] No active Scene View."); return; }

            string scenePath = EditorSceneManager.GetActiveScene().path;
            string label     = $"{Path.GetFileNameWithoutExtension(scenePath)} View {SceneBookmarkStorage.DB.bookmarks.Count + 1}";

            SceneBookmarkStorage.DB.bookmarks.Add(new SceneBookmark
            {
                label        = label,
                scenePath    = scenePath,
                position     = sv.pivot,
                rotation     = sv.rotation,
                size         = sv.size,
                orthographic = sv.orthographic
            });
            SceneBookmarkStorage.Save();
            MainToolbar.Refresh(K_Bookmarks);
        }

        static void ApplyBookmark(SceneBookmark bm)
        {
            if (!string.IsNullOrEmpty(bm.scenePath) && bm.scenePath != EditorSceneManager.GetActiveScene().path)
            {
                if (!EditorSceneManager.SaveCurrentModifiedScenesIfUserWantsTo()) return;
                EditorSceneManager.OpenScene(bm.scenePath);
            }
            EditorApplication.delayCall += () =>
            {
                var sv = SceneView.lastActiveSceneView;
                if (sv == null) return;
                sv.orthographic = bm.orthographic;
                sv.size         = bm.size;
                sv.LookAt(bm.position, bm.rotation, bm.size, bm.orthographic, true);
                sv.Repaint();
            };
        }

        // ── Time Scale ────────────────────────────────────────────────────

        [MainToolbarElement(K_TimeScale, defaultDockPosition = MainToolbarDockPosition.Right)]
        public static IEnumerable<MainToolbarElement> TimeScaleElements()
        {
            if (!BetterUnityPrefs.ToolbarEnabled || !BetterUnityPrefs.ToolbarTimeScaleEnabled)
                yield break;

            yield return new MainToolbarLabel(new MainToolbarContent("Time"));

            var slider = new MainToolbarSlider(
                new MainToolbarContent("Time Scale", "Adjust Time.timeScale  (0 = paused, 1 = normal)"),
                Time.timeScale, 0f, 5f,
                v => Time.timeScale = v);

            slider.populateContextMenu = menu =>
            {
                menu.AppendAction("Reset  1x",  _ => { Time.timeScale = 1f;    MainToolbar.Refresh(K_TimeScale); });
                menu.AppendAction("0.25x",      _ => { Time.timeScale = 0.25f; MainToolbar.Refresh(K_TimeScale); });
                menu.AppendAction("0.5x",       _ => { Time.timeScale = 0.5f;  MainToolbar.Refresh(K_TimeScale); });
                menu.AppendAction("2x",         _ => { Time.timeScale = 2f;    MainToolbar.Refresh(K_TimeScale); });
            };

            yield return slider;
        }

        // ── FPS Cap ───────────────────────────────────────────────────────

        [MainToolbarElement(K_FpsCap, defaultDockPosition = MainToolbarDockPosition.Right)]
        public static IEnumerable<MainToolbarElement> FpsCapElements()
        {
            if (!BetterUnityPrefs.ToolbarEnabled || !BetterUnityPrefs.ToolbarFpsCapEnabled)
                yield break;

            yield return new MainToolbarLabel(new MainToolbarContent("FPS"));

            int savedCap     = BetterUnityPrefs.ToolbarFpsCapValue;
            int displayValue = savedCap <= 0 ? 0 : savedCap;

            var slider = new MainToolbarSlider(
                new MainToolbarContent("FPS Cap", "Target frame rate for the editor  (0 = uncapped)."),
                displayValue, 0f, 300f,
                v =>
                {
                    int cap = Mathf.RoundToInt(v);
                    ApplyFpsCap(cap);
                });

            slider.populateContextMenu = menu =>
            {
                menu.AppendAction("Uncapped",  _ => ApplyFpsCap(0));
                menu.AppendAction("30",        _ => ApplyFpsCap(30));
                menu.AppendAction("60",        _ => ApplyFpsCap(60));
                menu.AppendAction("120",       _ => ApplyFpsCap(120));
                menu.AppendAction("144",       _ => ApplyFpsCap(144));
            };

            yield return slider;
        }

        static void ApplyFpsCap(int cap)
        {
            BetterUnityPrefs.ToolbarFpsCapValue = cap;
            _intervalMs = cap > 0 ? Mathf.Max(1, 1000 / cap) : 0;
            MainToolbar.Refresh(K_FpsCap);
        }
    }
}
