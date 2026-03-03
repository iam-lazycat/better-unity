using System;
using System.Collections.Generic;
using UnityEngine;

namespace LazyCat.BetterUnity
{
    public enum TaskStatus     { Active, Closed }
    public enum TaskList       { Current, Backlog }
    public enum AttachmentType { Object, Script, Scene, Link, SceneGameObject }

    [Serializable]
    public class PriorityDef
    {
        public string id    = Guid.NewGuid().ToString();
        public string name  = "Priority";
        public Color  color = Color.grey;
        public int    order = 0;
    }

    [Serializable]
    public class TaskAttachment
    {
        public string         id        = Guid.NewGuid().ToString();
        public AttachmentType type      = AttachmentType.Link;
        public string         label     = "";
        public string         assetGuid = "";
        public string         linkUrl   = "";
        public string         sceneGuid = "";
        public string         objectPath= "";
    }

    [Serializable]
    public class TaskItem
    {
        public string id              = Guid.NewGuid().ToString();
        public string name            = "New Task";
        public string description     = "";
        public string priorityId      = "";   // references PriorityDef.id
        public string label           = "";
        public string deadline        = "";
        public TaskStatus status      = TaskStatus.Active;
        public TaskList list          = TaskList.Current;
        public bool completed         = false;
        public string createdAt       = DateTime.Now.ToString("o");
        public List<TaskAttachment> attachments = new List<TaskAttachment>();
    }

    [Serializable]
    public class TaskSettings
    {
        public bool  showCompletedInList    = false;
        public bool  confirmOnDelete        = true;
        public bool  autoMoveCompletedToBacklog = false;
        public int   defaultSortMode        = 0;
        public bool  showDeadlineWarning    = true;
        public int   deadlineWarnDays       = 2;
        public string defaultLabel          = "";
    }

    [Serializable]
    public class TaskDatabase
    {
        public List<TaskItem>    tasks      = new List<TaskItem>();
        public List<PriorityDef> priorities = DefaultPriorities();
        public TaskSettings      settings   = new TaskSettings();

        public static List<PriorityDef> DefaultPriorities() => new List<PriorityDef>
        {
            new PriorityDef { name = "Low",      color = new Color(0.35f, 0.72f, 0.40f), order = 0 },
            new PriorityDef { name = "Medium",   color = new Color(0.90f, 0.78f, 0.25f), order = 1 },
            new PriorityDef { name = "High",     color = new Color(0.95f, 0.52f, 0.20f), order = 2 },
            new PriorityDef { name = "Critical", color = new Color(0.88f, 0.25f, 0.25f), order = 3 },
        };

        public PriorityDef GetPriority(string id)
        {
            if (priorities == null || priorities.Count == 0) priorities = DefaultPriorities();
            var p = priorities.Find(x => x.id == id);
            return p ?? (priorities.Count > 0 ? priorities[0] : new PriorityDef());
        }

        public List<PriorityDef> SortedPriorities()
        {
            if (priorities == null || priorities.Count == 0) priorities = DefaultPriorities();
            var copy = new List<PriorityDef>(priorities);
            copy.Sort((a, b) => a.order.CompareTo(b.order));
            return copy;
        }
    }
}
