using System.IO;
using UnityEngine;

namespace LazyCat.BetterUnity
{
    public static class FolderIconStorage
    {
        static readonly string Path = "ProjectSettings/BetterUnity_FolderIcons.json";
        static FolderIconDatabase _db;

        public static FolderIconDatabase DB
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
                try   { _db = JsonUtility.FromJson<FolderIconDatabase>(File.ReadAllText(Path)); }
                catch { _db = new FolderIconDatabase(); }
            }
            else
            {
                _db = new FolderIconDatabase();
            }
        }

        public static void Save() => File.WriteAllText(Path, JsonUtility.ToJson(_db, true));
    }
}
