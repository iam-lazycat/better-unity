using System;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;

namespace LazyCat.BetterUnity
{
    [InitializeOnLoad]
    public static class AutoSaveModule
    {
        private static double _nextSaveTime;

        static AutoSaveModule()
        {
            EditorApplication.update -= Tick;
            EditorApplication.update += Tick;
            ResetTimer();
        }

        public static void OnSettingsChanged() => ResetTimer();

        public static void ForceSave()
        {
            PerformSave();
            ResetTimer();
        }

        private static void ResetTimer()
        {
            _nextSaveTime = EditorApplication.timeSinceStartup + BetterUnityPrefs.AutoSaveInterval;
        }

        private static void Tick()
        {
            if (!BetterUnityPrefs.AutoSaveEnabled) return;
            if (EditorApplication.isPlaying) return;
            if (EditorApplication.timeSinceStartup < _nextSaveTime) return;

            PerformSave();
            ResetTimer();
        }

        private static void PerformSave()
        {
            try
            {
                EditorSceneManager.SaveOpenScenes();
                AssetDatabase.SaveAssets();

                if (BetterUnityPrefs.AutoSaveShowNotification)
                {
                    foreach (var w in Resources.FindObjectsOfTypeAll<EditorWindow>())
                    {
                        w.ShowNotification(new GUIContent("Auto Saved"), 1.5f);
                        break;
                    }
                    Debug.Log($"[Better Unity] Auto Saved at {DateTime.Now:HH:mm:ss}");
                }
            }
            catch (Exception e)
            {
                Debug.LogWarning($"[Better Unity] AutoSave failed: {e.Message}");
            }
        }

        public static void DrawModuleGUI()
        {
            EditorGUILayout.BeginVertical(EditorStyles.helpBox);

            EditorGUILayout.BeginHorizontal();

            var icon = EditorGUIUtility.IconContent("d_SaveAs");
            if (icon?.image != null)
                GUILayout.Label(icon, GUILayout.Width(18), GUILayout.Height(18));

            EditorGUILayout.LabelField("Auto Save", EditorStyles.boldLabel);
            GUILayout.FlexibleSpace();
            bool enabled    = BetterUnityPrefs.AutoSaveEnabled;
            bool newEnabled = EditorGUILayout.Toggle(enabled, GUILayout.Width(20));
            if (newEnabled != enabled) { BetterUnityPrefs.AutoSaveEnabled = newEnabled; OnSettingsChanged(); }
            EditorGUILayout.EndHorizontal();

            using (new EditorGUI.DisabledGroupScope(!BetterUnityPrefs.AutoSaveEnabled))
            {
                float interval    = BetterUnityPrefs.AutoSaveInterval;
                float newInterval = EditorGUILayout.Slider("Interval", interval, 30f, 600f);
                if (!Mathf.Approximately(newInterval, interval)) { BetterUnityPrefs.AutoSaveInterval = newInterval; OnSettingsChanged(); }

                bool notif    = BetterUnityPrefs.AutoSaveShowNotification;
                bool newNotif = EditorGUILayout.Toggle("Notify", notif);
                if (newNotif != notif) BetterUnityPrefs.AutoSaveShowNotification = newNotif;

                double remaining = System.Math.Max(0, _nextSaveTime - EditorApplication.timeSinceStartup);
                EditorGUILayout.HelpBox($"Next save in {FormatTime((float)remaining)}", MessageType.None);

                if (GUILayout.Button("Save Now")) ForceSave();
            }

            EditorGUILayout.EndVertical();
        }

        static string FormatTime(float s)
        {
            int m   = Mathf.FloorToInt(s / 60f);
            int sec = Mathf.RoundToInt(s % 60f);
            return m > 0 ? $"{m}m {sec}s" : $"{sec}s";
        }
    }
}
