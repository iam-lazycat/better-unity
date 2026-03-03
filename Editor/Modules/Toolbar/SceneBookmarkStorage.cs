using System.IO;
using UnityEngine;

namespace LazyCat.BetterUnity
{
    public static class SceneBookmarkStorage
    {
        static readonly string Path = "ProjectSettings/BetterUnity_Bookmarks.json";
        static SceneBookmarkDatabase _db;

        public static SceneBookmarkDatabase DB
        {
            get
            {
                if (_db == null) Load();
                return _db;
            }
        }

        public static void Load()
        {
            if (File.Exists(Path))
            {
                try   { _db = JsonUtility.FromJson<SceneBookmarkDatabase>(File.ReadAllText(Path)); }
                catch { _db = new SceneBookmarkDatabase(); }
            }
            else
            {
                _db = new SceneBookmarkDatabase();
            }
        }

        public static void Save() => File.WriteAllText(Path, JsonUtility.ToJson(_db, true));
    }
}
