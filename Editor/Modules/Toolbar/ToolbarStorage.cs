using System.Collections.Generic;
using System.IO;
using UnityEngine;

namespace LazyCat.BetterUnity
{
    [System.Serializable]
    public class ToolbarData
    {
        public ScreenshotSettings    screenshot   = new ScreenshotSettings();
        public List<string>          hiddenScenes = new List<string>();   // asset GUIDs to hide from scene switcher
    }

    public static class ToolbarStorage
    {
        static readonly string Path = "ProjectSettings/BetterUnity_Toolbar.json";
        static ToolbarData _data;

        public static ToolbarData Data
        {
            get
            {
                if (_data == null) Load();
                return _data;
            }
        }

        public static ScreenshotSettings Screenshot => Data.screenshot;

        public static void Load()
        {
            if (File.Exists(Path))
            {
                try   { _data = JsonUtility.FromJson<ToolbarData>(File.ReadAllText(Path)); }
                catch { _data = new ToolbarData(); }
            }
            else
            {
                _data = new ToolbarData();
            }
        }

        public static void Save() => File.WriteAllText(Path, JsonUtility.ToJson(_data, true));
    }
}
