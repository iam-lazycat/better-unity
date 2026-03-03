using System;
using System.Collections.Generic;
using UnityEngine;

namespace LazyCat.BetterUnity
{
    public enum FolderIconType { Default, Color, Custom }

    [Serializable]
    public class FolderIconEntry
    {
        public string         guid           = "";
        public FolderIconType iconType       = FolderIconType.Default;
        public Color          tintColor      = Color.white;
        public string         customIconGuid = "";   // GUID of a Texture2D in the project
        public bool           hasOverlay     = false;
        public string         overlayIcon    = "";   // EditorGUIUtility icon name
    }

    [Serializable]
    public class FolderIconDatabase
    {
        public List<FolderIconEntry> entries = new List<FolderIconEntry>();

        public FolderIconEntry Get(string guid)
        {
            for (int i = 0; i < entries.Count; i++)
                if (entries[i].guid == guid) return entries[i];
            return null;
        }

        public FolderIconEntry GetOrCreate(string guid)
        {
            var e = Get(guid);
            if (e != null) return e;
            e = new FolderIconEntry { guid = guid };
            entries.Add(e);
            return e;
        }

        public void Remove(string guid) => entries.RemoveAll(e => e.guid == guid);
    }

    // ── Built-in presets ──────────────────────────────────────────────────────

    public static class FolderIconPresets
    {
        public static readonly (string name, Color color)[] Colors =
        {
            ("Red",     new Color(0.86f, 0.27f, 0.27f)),
            ("Orange",  new Color(0.92f, 0.54f, 0.18f)),
            ("Yellow",  new Color(0.90f, 0.80f, 0.18f)),
            ("Lime",    new Color(0.48f, 0.82f, 0.22f)),
            ("Green",   new Color(0.16f, 0.66f, 0.32f)),
            ("Teal",    new Color(0.14f, 0.72f, 0.64f)),
            ("Cyan",    new Color(0.18f, 0.66f, 0.88f)),
            ("Blue",    new Color(0.22f, 0.42f, 0.92f)),
            ("Indigo",  new Color(0.36f, 0.22f, 0.82f)),
            ("Purple",  new Color(0.58f, 0.24f, 0.86f)),
            ("Pink",    new Color(0.92f, 0.34f, 0.68f)),
            ("Gray",    new Color(0.48f, 0.48f, 0.52f)),
        };

        public static readonly (string label, string icon)[] Overlays =
        {
            ("None",        ""),
            ("Star",        "d_Favorite"),
            ("Warning",     "d_console.warnicon.sml"),
            ("Error",       "d_console.erroricon.sml"),
            ("Info",        "d_console.infoicon.sml"),
            ("Tag",         "d_FilterByLabel"),
            ("Link",        "d_Linked"),
            ("Settings",    "d_SettingsIcon"),
            ("Scene",       "d_SceneAsset Icon"),
            ("Script",      "d_cs Script Icon"),
        };
    }
}
