using System.IO;
using UnityEngine;

namespace LazyCat.BetterUnity
{
    public static class ScreenshotStorage
    {
        static readonly string Path = "ProjectSettings/BetterUnity_Screenshot.json";
        static ScreenshotSettings _settings;

        public static ScreenshotSettings Settings
        {
            get
            {
                if (_settings == null) Load();
                return _settings;
            }
        }

        public static void Load()
        {
            if (File.Exists(Path))
            {
                try   { _settings = JsonUtility.FromJson<ScreenshotSettings>(File.ReadAllText(Path)); }
                catch { _settings = new ScreenshotSettings(); }
            }
            else
            {
                _settings = new ScreenshotSettings();
            }
        }

        public static void Save() => File.WriteAllText(Path, JsonUtility.ToJson(_settings, true));
    }
}
