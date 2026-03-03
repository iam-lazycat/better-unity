using System;
using System.Collections.Generic;
using UnityEngine;

namespace LazyCat.BetterUnity
{
    [Serializable]
    public class SceneBookmark
    {
        public string id        = Guid.NewGuid().ToString();
        public string label     = "Bookmark";
        public string scenePath = "";
        public Vector3 position;
        public Quaternion rotation;
        public float size       = 10f;
        public bool orthographic = false;
    }

    [Serializable]
    public class SceneBookmarkDatabase
    {
        public List<SceneBookmark> bookmarks = new List<SceneBookmark>();
    }
}
