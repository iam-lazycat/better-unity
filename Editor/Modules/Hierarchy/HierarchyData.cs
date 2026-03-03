using System;
using System.Collections.Generic;
using UnityEngine;

namespace LazyCat.BetterUnity
{
    public enum HierarchyItemStyle { Default, Header }

    [Serializable]
    public class HierarchyItemData
    {
        public string             key   = "";
        public HierarchyItemStyle style = HierarchyItemStyle.Default;
        public Color              color = new Color(0.95f, 0.75f, 0.3f, 0.85f);
        public string             icon  = "";
        public string             note  = "";
    }

    [Serializable]
    public class HierarchyDatabase
    {
        public List<HierarchyItemData> items = new List<HierarchyItemData>();

        public HierarchyItemData Get(string key)
        {
            for (int i = 0; i < items.Count; i++)
                if (items[i].key == key) return items[i];
            return null;
        }

        public HierarchyItemData GetOrCreate(string key)
        {
            var d = Get(key);
            if (d != null) return d;
            d = new HierarchyItemData { key = key };
            items.Add(d);
            return d;
        }

        public void Remove(string key) => items.RemoveAll(d => d.key == key);
    }
}
