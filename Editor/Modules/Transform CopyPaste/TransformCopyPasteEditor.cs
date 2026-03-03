using UnityEditor;
using UnityEngine;

namespace LazyCat.BetterUnity
{
    [CustomEditor(typeof(Transform))]
    [CanEditMultipleObjects]
    public class TransformCopyPasteEditor : Editor
    {
        static Vector3 _clipPos, _clipRot, _clipScale;
        static bool    _hasPos, _hasRot, _hasScale;

        static GUIContent _copyIcon, _pasteIcon;

        static GUIContent CopyIcon
        {
            get
            {
                if (_copyIcon != null) return _copyIcon;
                var ic = EditorGUIUtility.IconContent("TreeEditor.Duplicate");
                _copyIcon = ic?.image != null ? new GUIContent(ic.image, "Copy") : new GUIContent("C", "Copy");
                return _copyIcon;
            }
        }

        static GUIContent PasteIcon
        {
            get
            {
                if (_pasteIcon != null) return _pasteIcon;
                var ic = EditorGUIUtility.IconContent("Clipboard");
                _pasteIcon = ic?.image != null ? new GUIContent(ic.image, "Paste") : new GUIContent("P", "Paste");
                return _pasteIcon;
            }
        }

        static readonly GUILayoutOption _btnW = GUILayout.Width(22);
        static readonly GUILayoutOption _btnH = GUILayout.Height(EditorGUIUtility.singleLineHeight);

        public override void OnInspectorGUI()
        {
            serializedObject.Update();

            var  t        = (Transform)target;
            bool showBtns = BetterUnityPrefs.TransformCopyPasteEnabled;

            EditorGUI.BeginChangeCheck();
            Vector3 pos = VecRow("Position", t.localPosition, showBtns, ref _clipPos, ref _hasPos);
            if (EditorGUI.EndChangeCheck())
            {
                Undo.RecordObjects(targets, "Move");
                foreach (Object o in targets) ((Transform)o).localPosition = pos;
            }

            EditorGUI.BeginChangeCheck();
            Vector3 rot = VecRow("Rotation", t.localEulerAngles, showBtns, ref _clipRot, ref _hasRot);
            if (EditorGUI.EndChangeCheck())
            {
                Undo.RecordObjects(targets, "Rotate");
                foreach (Object o in targets) ((Transform)o).localEulerAngles = rot;
            }

            EditorGUI.BeginChangeCheck();
            Vector3 scale = VecRow("Scale", t.localScale, showBtns, ref _clipScale, ref _hasScale);
            if (EditorGUI.EndChangeCheck())
            {
                Undo.RecordObjects(targets, "Scale");
                foreach (Object o in targets) ((Transform)o).localScale = scale;
            }

            serializedObject.ApplyModifiedProperties();
        }

        Vector3 VecRow(string label, Vector3 val, bool showBtns, ref Vector3 clip, ref bool hasClip)
        {
            if (!showBtns)
                return EditorGUILayout.Vector3Field(label, val);

            Vector3 result = val;

            EditorGUILayout.BeginHorizontal();
            result = EditorGUILayout.Vector3Field(label, val);

            if (GUILayout.Button(CopyIcon, EditorStyles.miniButtonLeft, _btnW, _btnH))
            {
                clip    = val;
                hasClip = true;
            }

            using (new EditorGUI.DisabledGroupScope(!hasClip))
            {
                if (GUILayout.Button(PasteIcon, EditorStyles.miniButtonRight, _btnW, _btnH))
                    result = clip;
            }

            EditorGUILayout.EndHorizontal();
            return result;
        }
    }
}
