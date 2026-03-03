using System.IO;
using UnityEngine;

namespace LazyCat.BetterUnity
{
    public static class HierarchyStorage
    {
        static readonly string Path = "ProjectSettings/BetterUnity_Hierarchy.json";
        static HierarchyDatabase _db;

        public static HierarchyDatabase DB
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
                try   { _db = JsonUtility.FromJson<HierarchyDatabase>(File.ReadAllText(Path)); }
                catch { _db = new HierarchyDatabase(); }
            }
            else
            {
                _db = new HierarchyDatabase();
            }
        }

        public static void Save() => File.WriteAllText(Path, JsonUtility.ToJson(_db, true));
    }
}
