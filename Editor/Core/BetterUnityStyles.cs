using UnityEditor;
using UnityEngine;

namespace LazyCat.BetterUnity
{
    public static class BetterUnityStyles
    {
        private static GUIStyle _header;
        private static GUIStyle _title;
        private static GUIStyle _iconButton;
        private static GUIStyle _moduleBox;
        private static GUIStyle _moduleHeader;

        public static GUIStyle Header => _header ??= new GUIStyle()
        {
            padding = new RectOffset(8, 8, 6, 6),
            normal = { background = MakeTex(1, 1, new Color(0.15f, 0.15f, 0.17f)) }
        };

        public static GUIStyle Title => _title ??= new GUIStyle(EditorStyles.boldLabel)
        {
            fontSize = 15,
            normal = { textColor = new Color(0.95f, 0.75f, 0.3f) },
            padding = new RectOffset(2, 0, 0, 0)
        };

        public static GUIStyle IconButton => _iconButton ??= new GUIStyle(EditorStyles.miniButton)
        {
            padding = new RectOffset(2, 2, 2, 2),
            margin = new RectOffset(2, 0, 0, 0)
        };

        public static GUIStyle ModuleBox => _moduleBox ??= new GUIStyle("HelpBox")
        {
            padding = new RectOffset(10, 10, 8, 8),
            margin = new RectOffset(6, 6, 4, 4)
        };

        public static GUIStyle ModuleHeader => _moduleHeader ??= new GUIStyle(EditorStyles.boldLabel)
        {
            fontSize = 12,
            normal = { textColor = Color.white }
        };

        private static Texture2D MakeTex(int width, int height, Color col)
        {
            var pix = new Color[width * height];
            for (int i = 0; i < pix.Length; i++) pix[i] = col;
            var result = new Texture2D(width, height);
            result.SetPixels(pix);
            result.Apply();
            return result;
        }
    }
}
