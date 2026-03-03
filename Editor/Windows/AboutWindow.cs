using UnityEditor;
using UnityEngine;

namespace LazyCat.BetterUnity
{
    public class AboutWindow : EditorWindow
    {
        // ── Constants ─────────────────────────────────────────────────────────

        const string Version     = "5.3.0v";
        const string Creator     = "Lazy Cat";
        const string Description = "A collection of Unity editor tools built to make your workflow faster, cleaner, and a little less painful.";

        const string URL_GitHub  = "https://github.com/iam-lazycat";
        const string URL_X       = "https://x.com/iam_lazycat";
        const string URL_Discord = "https://discord.com/users/iam_lazycat";
        const string URL_Insta   = "https://instagram.com/iam_lazycat";
        const string URL_Reddit  = "https://reddit.com/user/iam_lazycat";

        const float W = 320f;
        const float H = 310f;

        // ── Open ──────────────────────────────────────────────────────────────

        [MenuItem("Help/Better Unity/About", false, 9000)]
        [MenuItem("Tools/Better Unity/About", false, 9000)]
        public static void Open()
        {
            var w = GetWindow<AboutWindow>(true, "About Better Unity", true);
            w.minSize = w.maxSize = new Vector2(W, H);
            w.ShowUtility();
        }

        // ── GUI ───────────────────────────────────────────────────────────────

        void OnGUI()
        {
            // ── Header ────────────────────────────────────────────────────────
            var headerRect = new Rect(0, 0, W, 64f);
            EditorGUI.DrawRect(headerRect, new Color(0.13f, 0.13f, 0.15f, 1f));
            EditorGUI.DrawRect(new Rect(0, headerRect.yMax - 1, W, 1f), new Color(0f, 0f, 0f, 0.4f));

            // Logo / package icon
            var logoIcon = EditorGUIUtility.IconContent("d_Package Manager@2x");
            if (logoIcon?.image != null)
                GUI.DrawTexture(new Rect(14f, 13f, 38f, 38f), (Texture2D)logoIcon.image,
                    ScaleMode.ScaleToFit, true);

            // Title
            GUI.Label(new Rect(62f, 14f, 240f, 22f), "Better Unity",
                new GUIStyle(EditorStyles.boldLabel)
                {
                    fontSize = 16,
                    normal   = { textColor = new Color(0.95f, 0.75f, 0.3f) }
                });

            // Version badge
            var badgeStyle = new GUIStyle(EditorStyles.centeredGreyMiniLabel)
            {
                normal   = { textColor = new Color(0.55f, 0.55f, 0.60f) },
                fontSize = 10
            };
            GUI.Label(new Rect(63f, 38f, 100f, 16f), "v" + Version, badgeStyle);

            float y = headerRect.yMax + 16f;

            // ── Description ───────────────────────────────────────────────────
            var descStyle = new GUIStyle(EditorStyles.wordWrappedLabel)
            {
                fontSize  = 11,
                alignment = TextAnchor.UpperLeft,
                wordWrap  = true,
                normal    = { textColor = new Color(0.72f, 0.72f, 0.75f) }
            };

            var descRect = new Rect(16f, y, W - 32f, 46f);
            GUI.Label(descRect, Description, descStyle);
            y = descRect.yMax + 14f;

            // ── Divider ───────────────────────────────────────────────────────
            EditorGUI.DrawRect(new Rect(16f, y, W - 32f, 1f), new Color(0.25f, 0.25f, 0.28f, 1f));
            y += 12f;

            // ── Creator row ───────────────────────────────────────────────────
            var labelStyle = new GUIStyle(EditorStyles.miniLabel)
            {
                normal = { textColor = new Color(0.48f, 0.48f, 0.52f) }
            };
            var valueStyle = new GUIStyle(EditorStyles.miniLabel)
            {
                fontStyle = FontStyle.Bold,
                normal    = { textColor = new Color(0.85f, 0.85f, 0.88f) }
            };

            DrawInfoRow(ref y, "Created by", Creator,    labelStyle, valueStyle);
            DrawInfoRow(ref y, "Version",    Version,    labelStyle, valueStyle);
            DrawInfoRow(ref y, "Unity",      "6.3+",     labelStyle, valueStyle);

            y += 6f;

            // ── Divider ───────────────────────────────────────────────────────
            EditorGUI.DrawRect(new Rect(16f, y, W - 32f, 1f), new Color(0.25f, 0.25f, 0.28f, 1f));
            y += 14f;

            // ── Social links ──────────────────────────────────────────────────
            GUI.Label(new Rect(16f, y, 80f, 16f), "Links", labelStyle);
            y += 16f;

            float linkX = 16f;
            linkX = DrawLinkButton(linkX, y, "GitHub",  "d_BuildSettings.WebGL.Small", URL_GitHub);
            linkX = DrawLinkButton(linkX, y, "X",       "d_BuildSettings.WebGL.Small", URL_X);
            linkX = DrawLinkButton(linkX, y, "Discord", "d_BuildSettings.WebGL.Small", URL_Discord);
            linkX = DrawLinkButton(linkX, y, "Insta",   "d_BuildSettings.WebGL.Small", URL_Insta);
            linkX = DrawLinkButton(linkX, y, "Reddit",  "d_BuildSettings.WebGL.Small", URL_Reddit);

            // ── Footer ────────────────────────────────────────────────────────
            var footerRect = new Rect(0f, H - 28f, W, 28f);
            EditorGUI.DrawRect(footerRect, new Color(0.11f, 0.11f, 0.13f, 1f));
            EditorGUI.DrawRect(new Rect(0f, footerRect.y, W, 1f), new Color(0f, 0f, 0f, 0.35f));

            GUI.Label(new Rect(16f, footerRect.y + 6f, W - 32f, 16f),
                "it works. sometimes. idk.  --  Lazy Cat",
                new GUIStyle(EditorStyles.centeredGreyMiniLabel)
                {
                    normal = { textColor = new Color(0.38f, 0.38f, 0.42f) }
                });
        }

        // ── Helpers ───────────────────────────────────────────────────────────

        void DrawInfoRow(ref float y, string label, string value,
                         GUIStyle labelStyle, GUIStyle valueStyle)
        {
            GUI.Label(new Rect(16f,  y, 90f,       16f), label, labelStyle);
            GUI.Label(new Rect(110f, y, W - 126f,  16f), value, valueStyle);
            y += 18f;
        }

        float DrawLinkButton(float x, float y, string label, string iconName, string url)
        {
            // Try to get a real platform/social icon; fall back to link icon
            var linkIc = EditorGUIUtility.IconContent("d_Linked");

            float btnW = 52f;
            float btnH = 22f;

            var r = new Rect(x, y, btnW, btnH);

            bool hover = r.Contains(Event.current.mousePosition);
            if (hover) EditorGUI.DrawRect(r, new Color(0.24f, 0.37f, 0.58f, 0.22f));

            var style = new GUIStyle(EditorStyles.miniButton)
            {
                fontSize  = 10,
                alignment = TextAnchor.MiddleCenter,
            };

            // Draw icon + label stacked
            GUIContent content;
            if (linkIc?.image != null)
                content = new GUIContent(" " + label, linkIc.image, url);
            else
                content = new GUIContent(label, url);

            if (GUI.Button(r, content, style))
                Application.OpenURL(url);

            EditorGUIUtility.AddCursorRect(r, MouseCursor.Link);

            if (Event.current.type == EventType.MouseMove) Repaint();

            return x + btnW + 4f;
        }
    }
}
