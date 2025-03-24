using UnityEditor;

namespace GravityBox.ColorPicker
{
    /// <summary>
    /// Color Button Editor. Directly edit ColorImage properties through thid Inspector
    /// and avoid making duplicate fields in ColorButton like Color, Has Alpha, Is HDR
    /// </summary>
    [CustomEditor(typeof(ColorButton))]
    public class ColorButtonEditor : Editor
    {
        private static bool foldout;
        private Editor imageEditor;

        public override void OnInspectorGUI()
        {
            serializedObject.Update();
            SerializedProperty colorImage = serializedObject.FindProperty("_colorImage");
            EditorGUILayout.PropertyField(colorImage);

            if (colorImage.objectReferenceValue != null)
            {
                foldout = EditorGUILayout.Foldout(foldout, "Color Image Settings");
                if (foldout)
                {
                    //the key here is Cached Editor which allows drowing other custom editors inside one another
                    CreateCachedEditor(colorImage.objectReferenceValue, typeof(ColorImageEditor), ref imageEditor);
                    imageEditor.OnInspectorGUI();
                    EditorGUILayout.Space();
                }
            }

            EditorGUILayout.PropertyField(serializedObject.FindProperty("colorPicker"));
            EditorGUILayout.PropertyField(serializedObject.FindProperty("onColorUpdated"), false);
            serializedObject.ApplyModifiedProperties();
        }
    }
}
