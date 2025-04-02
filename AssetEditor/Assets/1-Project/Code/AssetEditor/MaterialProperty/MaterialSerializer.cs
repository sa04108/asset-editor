using System;
using System.Collections.Generic;
using UnityEngine;

namespace Merlin
{
    public enum eMaterialPropertyType
    {
        Number,
        Color
    }

    [Serializable]
    public class MaterialPropertyData
    {
        public string propertyName;
        public eMaterialPropertyType propertyType;
        public float numberValue;
        public Color colorValue;
    }

    [Serializable]
    public class TagData
    {
        public string tagName;
        public string tagValue;
    }

    [Serializable]
    public class ShaderPassData
    {
        public string passName;
        public bool enabled;
    }

    [Serializable]
    public class MaterialData
    {
        public string shaderName;
        public List<MaterialPropertyData> properties = new List<MaterialPropertyData>();
        public List<string> keywords = new List<string>();
        public List<TagData> tags = new List<TagData>();
        public List<ShaderPassData> shaderPasses = new List<ShaderPassData>();
        public int renderQueue;
        public int lightMapFlag;
        public Vector2 mainTextureScale;
        public Vector2 mainTextureOffset;
    }

    public class MaterialSerializer
    {
        public static string Serialize(Material mat)
        {
            MaterialData data = new MaterialData();
            data.shaderName = mat.shader.name;

            // 1. Shader 프로퍼티 직렬화
            int propCount = mat.shader.GetPropertyCount();
            for (int i = 0; i < propCount; i++)
            {
                string propName = mat.shader.GetPropertyName(i);
                MaterialPropertyData propData = new MaterialPropertyData();
                propData.propertyName = propName;

                var propType = mat.shader.GetPropertyType(i);
                switch (propType)
                {
                    case UnityEngine.Rendering.ShaderPropertyType.Color:
                        propData.propertyType = eMaterialPropertyType.Color;
                        // Color를 Vector4로 저장 (x=r, y=g, z=b, w=a)
                        propData.colorValue = mat.GetColor(propName);
                        break;

                    case UnityEngine.Rendering.ShaderPropertyType.Float:
                    case UnityEngine.Rendering.ShaderPropertyType.Range:
                        propData.propertyType = eMaterialPropertyType.Number;
                        propData.numberValue = mat.GetFloat(propName);
                        break;

                    default:
                        continue;
                }
                data.properties.Add(propData);
            }

            // 2. 활성화된 키워드 직렬화
            data.keywords.AddRange(mat.shaderKeywords);

            // 3. Override 태그 직렬화 (미리 지정한 태그들)
            string[] tagKeys = new string[] { "RenderType", "LightMode", "Queue" };
            foreach (var key in tagKeys)
            {
                string tagValue = mat.GetTag(key, false);
                if (!string.IsNullOrEmpty(tagValue))
                {
                    data.tags.Add(new TagData { tagName = key, tagValue = tagValue });
                }
            }

            // 4. Shader Pass 활성화 상태 직렬화
            int passCount = mat.passCount;
            for (int i = 0; i < passCount; i++)
            {
                string passName = mat.GetPassName(i);
                ShaderPassData passData = new ShaderPassData();
                passData.passName = passName;
                passData.enabled = mat.GetShaderPassEnabled(passName);
                data.shaderPasses.Add(passData);
            }

            // 5. 기타 특수 타입
            data.renderQueue = mat.renderQueue;
            data.lightMapFlag = (int)mat.globalIlluminationFlags;
            data.mainTextureScale = mat.mainTextureScale;
            data.mainTextureOffset = mat.mainTextureOffset;

            // JSON 문자열로 변환 (가독성을 위해 prettyPrint true)
            return JsonUtility.ToJson(data, true);
        }

        /// <summary>
        /// JSON 데이터를 Material에 적용하여 역직렬화합니다.
        /// (Material의 shader는 변경되지 않는다고 가정합니다.)
        /// </summary>
        public static void Deserialize(Material mat, string json)
        {
            MaterialData data = JsonUtility.FromJson<MaterialData>(json);

            var shader = Shader.Find(data.shaderName);
            if (shader == null)
            {
                Debug.LogWarning("Could not find shader by shader name.");
                return;
            }

            mat.shader = shader;

            // 1. 프로퍼티 복원
            foreach (var prop in data.properties)
            {
                switch (prop.propertyType)
                {
                    case eMaterialPropertyType.Color:
                        mat.SetColor(prop.propertyName, prop.colorValue);
                        break;

                    case eMaterialPropertyType.Number:
                        mat.SetFloat(prop.propertyName, prop.numberValue);
                        break;
                }
            }

            // 2. 키워드 복원: 기존 키워드를 초기화하고 새로 적용합니다.
            mat.shaderKeywords = data.keywords.ToArray();

            // 3. Override 태그 복원
            foreach (var tag in data.tags)
            {
                mat.SetOverrideTag(tag.tagName, tag.tagValue);
            }

            // 4. Shader Pass 활성화 상태 복원
            foreach (var pass in data.shaderPasses)
            {
                mat.SetShaderPassEnabled(pass.passName, pass.enabled);
            }

            // 5. 기타 특수 타입
            mat.renderQueue = data.renderQueue;
            mat.globalIlluminationFlags = (MaterialGlobalIlluminationFlags)data.lightMapFlag;
            mat.mainTextureScale = data.mainTextureScale;
            mat.mainTextureOffset = data.mainTextureOffset;
        }
    }
}