using UnityEngine;
using UnityEditor;

namespace GravityBox.ColorPicker
{
    [CustomEditor(typeof(ColorImage))]
    public class ColorImageEditor : Editor
    {
        /// <summary>
        /// This editor draws only three properties, which is exactly what I need to draw inside
        /// ColorButton Editor
        /// </summary>
        public override void OnInspectorGUI()
        {
            serializedObject.Update();
            SerializedProperty hdr = serializedObject.FindProperty("_isHDR");
            SerializedProperty alpha = serializedObject.FindProperty("_hasAlpha");
            SerializedProperty color = serializedObject.FindProperty("hdrColor");
            //color
            color.colorValue = EditorGUILayout.ColorField(new GUIContent("Color"), color.colorValue, true, alpha.boolValue, hdr.boolValue);
            //hdr
            EditorGUILayout.PropertyField(hdr);
            //alpha
            EditorGUILayout.BeginHorizontal();
            EditorGUILayout.PropertyField(alpha);
            if (alpha.boolValue)
            {
                SerializedProperty aHeight = serializedObject.FindProperty("alphaIndicatorHeight");
                EditorGUILayout.PropertyField(aHeight, new GUIContent("Indicator Height"));
            }
            EditorGUILayout.EndHorizontal();
            //default, but skip material and color. uncomment if necessary
            //EditorGUILayout.Space();
            //EditorGUILayout.PropertyField(serializedObject.FindProperty("m_RaycastTarget"));
            //EditorGUILayout.PropertyField(serializedObject.FindProperty("m_Maskable"));
            //EditorGUILayout.PropertyField(serializedObject.FindProperty("colorPicker"));
            serializedObject.ApplyModifiedProperties();
        }
    }
}