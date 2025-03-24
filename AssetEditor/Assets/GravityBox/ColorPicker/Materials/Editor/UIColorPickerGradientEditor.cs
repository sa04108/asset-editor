using UnityEngine;
using UnityEditor;

namespace GravityBox.ColorPicker
{
    /// <summary>
    /// Material Editor to simplify setting up gradients for Color Picker
    /// </summary>
    public class UIColorPickerGradientEditor : ShaderGUI
    {
        public enum ShaderMode
        {
            PickerRing,
            PickerBox,
            Hue,
            Saturation,
            Value,
            Alpha
        }

        public enum AlphaMode
        {
            Gradient,
            Swatch,
            Solid,
            None
        }

        private static readonly string[] keywords = new string[]
        {
        "_RING","_BOX", "_LINE", "_SAT", "_", "_ALPHA"
        };

        private static readonly string[] keywordsAlpha = new string[]
        {
        "_GRADIENT","_SWATCH", "_SOLID"
        };

        MaterialProperty ringThickness;
        MaterialProperty checkerSize;
        MaterialProperty colorMask;

        MaterialEditor m_MaterialEditor;

        public override void OnGUI(MaterialEditor materialEditor, MaterialProperty[] properties)
        {
            FindProperties(properties);
            m_MaterialEditor = materialEditor;
            Material material = m_MaterialEditor.target as Material;

            ShaderPropertiesGUI(material);
        }

        public void FindProperties(MaterialProperty[] props)
        {
            ringThickness = FindProperty("_Thickness", props);
            checkerSize = FindProperty("_Checker", props);
            colorMask = FindProperty("_Mask", props);
        }

        public void ShaderPropertiesGUI(Material material)
        {
            ShaderMode mode = GetShaderMode(material);
            AlphaMode amode = GetAlphaMode(material, mode);
            EditorGUI.BeginChangeCheck();
            {
                mode = (ShaderMode)EditorGUILayout.EnumPopup(mode);
                switch (mode)
                {
                    case ShaderMode.PickerRing:
                    case ShaderMode.PickerBox:
                        ringThickness.floatValue = EditorGUILayout.Slider(ringThickness.floatValue, 0.005f, 0.5f);
                        break;
                    case ShaderMode.Alpha:
                        amode = (AlphaMode)EditorGUILayout.EnumPopup(amode);
                        checkerSize.vectorValue = EditorGUILayout.Vector2Field("Checker Size", checkerSize.vectorValue);
                        break;
                    case ShaderMode.Hue:
                        colorMask.vectorValue = new Vector4(0, 1, 1, 0);
                        break;
                    case ShaderMode.Saturation:
                        colorMask.vectorValue = new Vector4(1, 0, 1, 0);
                        break;
                    case ShaderMode.Value:
                        colorMask.vectorValue = new Vector4(1, 1, 0, 0);
                        break;
                }
            }
            if (EditorGUI.EndChangeCheck())
            {
                MaterialChanged(material, mode, amode);
            }
        }

        void MaterialChanged(Material material, ShaderMode shaderMode, AlphaMode alphaMode)
        {
            SetShaderMode(material, shaderMode);
            SetAlphaMode(material, shaderMode, alphaMode);
        }

        ShaderMode GetShaderMode(Material material)
        {
            for (int i = 0; i < keywords.Length; i++)
            {
                if (material.IsKeywordEnabled(keywords[i]))
                    return (ShaderMode)i;
            }

            return ShaderMode.Value;
        }

        void SetShaderMode(Material material, ShaderMode shaderMode)
        {
            string keyword = keywords[(int)shaderMode];
            if (material.IsKeywordEnabled(keyword))
                return;

            foreach (var k in keywords)
            {
                if (material.IsKeywordEnabled(k))
                    material.DisableKeyword(k);
            }

            material.EnableKeyword(keyword);
        }

        AlphaMode GetAlphaMode(Material material, ShaderMode shaderMode)
        {
            if (shaderMode != ShaderMode.Alpha) return AlphaMode.None;

            for (int i = 0; i < keywordsAlpha.Length; i++)
            {
                if (material.IsKeywordEnabled(keywordsAlpha[i]))
                    return (AlphaMode)i;
            }

            return AlphaMode.None;
        }

        void SetAlphaMode(Material material, ShaderMode shaderMode, AlphaMode alphaMode)
        {
            if (shaderMode == ShaderMode.Alpha)
            {
                if (alphaMode == AlphaMode.None)
                    alphaMode = AlphaMode.Gradient;

                string keyword = keywordsAlpha[(int)alphaMode];
                if (material.IsKeywordEnabled(keyword))
                    return;

                foreach (var k in keywordsAlpha)
                {
                    if (material.IsKeywordEnabled(k))
                        material.DisableKeyword(k);
                }

                material.EnableKeyword(keyword);
            }
            else
            {
                foreach (var k in keywordsAlpha)
                {
                    if (material.IsKeywordEnabled(k))
                        material.DisableKeyword(k);
                }
            }
        }
    }
}