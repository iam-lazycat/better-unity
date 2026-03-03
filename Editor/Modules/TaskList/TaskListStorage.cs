using System.IO;
using UnityEngine;
using UnityEditor;

namespace LazyCat.BetterUnity
{
    public static class TaskListStorage
    {
        static readonly string Path = "ProjectSettings/BetterUnity_Tasks.json";

        static TaskDatabase _db;

        public static TaskDatabase DB
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
                try   { _db = JsonUtility.FromJson<TaskDatabase>(File.ReadAllText(Path)); }
                catch { _db = new TaskDatabase(); }
            }
            else
            {
                _db = new TaskDatabase();
            }
        }

        public static void Save()
        {
            File.WriteAllText(Path, JsonUtility.ToJson(_db, true));
        }

        public static void MarkDirty()
        {
            Save();
        }
    }
}
