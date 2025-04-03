using System;
using System.Collections.Generic;
using System.IO;
using UnityEngine;
using UnityEngine.Rendering;

namespace Merlin
{
    public enum eMaterialPropertyType
    {
        Number,
        Color,
        Nothing
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
        public bool doubleSidedGI;
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
                    case ShaderPropertyType.Color:
                        propData.propertyType = eMaterialPropertyType.Color;
                        // Color를 Vector4로 저장 (x=r, y=g, z=b, w=a)
                        propData.colorValue = mat.GetColor(propName);
                        break;

                    case ShaderPropertyType.Float:
                    case ShaderPropertyType.Range:
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
            data.doubleSidedGI = mat.doubleSidedGI;
            data.mainTextureScale = mat.mainTextureScale;
            data.mainTextureOffset = mat.mainTextureOffset;

            // JSON 문자열로 변환 (가독성을 위해 prettyPrint true)
            return JsonUtility.ToJson(data, true);
        }

        /// <summary>
        /// JSON 데이터를 Material에 적용하여 역직렬화합니다.
        /// </summary>
        public static void Apply(Material mat, string json)
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
            mat.doubleSidedGI = data.doubleSidedGI;
            mat.SetTextureScale("_BaseMap", data.mainTextureScale);
            mat.SetTextureScale("_MainTex", data.mainTextureScale);
            mat.SetTextureOffset("_BaseMap", data.mainTextureOffset);
            mat.SetTextureOffset("_MainTex", data.mainTextureOffset);
        }

        /// <summary>
        /// Material의 변경점을 바이너리 데이터(byte array)로 직렬화합니다.
        /// </summary>
        public static byte[] SerializeBinary(Material mat)
        {
            using (MemoryStream ms = new MemoryStream())
            {
                using (BinaryWriter writer = new BinaryWriter(ms))
                {
                    // 1. Shader 이름 기록
                    writer.Write(mat.shader.name);

                    // 2. Shader 프로퍼티 기록
                    int propCount = mat.shader.GetPropertyCount();
                    writer.Write(propCount);
                    for (int i = 0; i < propCount; i++)
                    {
                        string propName = mat.shader.GetPropertyName(i);
                        writer.Write(propName);

                        var propType = mat.shader.GetPropertyType(i);
                        eMaterialPropertyType sPropType;
                        switch (propType)
                        {
                            case ShaderPropertyType.Color:
                                sPropType = eMaterialPropertyType.Color;
                                break;

                            case ShaderPropertyType.Float:
                            case ShaderPropertyType.Range:
                                sPropType = eMaterialPropertyType.Number;
                                break;

                            default:
                                sPropType = eMaterialPropertyType.Nothing;
                                break;
                        }
                        writer.Write((byte)sPropType);

                        // 각 타입에 맞게 값 기록
                        switch (sPropType)
                        {
                            case eMaterialPropertyType.Color:
                                Color col = mat.GetColor(propName);
                                writer.Write(col.r);
                                writer.Write(col.g);
                                writer.Write(col.b);
                                writer.Write(col.a);
                                break;

                            case eMaterialPropertyType.Number:
                                writer.Write(mat.GetFloat(propName));
                                break;
                        }
                    }

                    // 3. 활성화된 키워드 기록
                    string[] keywords = mat.shaderKeywords;
                    writer.Write(keywords.Length);
                    foreach (var kw in keywords)
                    {
                        writer.Write(kw);
                    }

                    // 4. Override 태그 기록 (미리 지정한 태그 목록)
                    string[] tagKeys = new string[] { "RenderType", "LightMode", "Queue" };
                    // 유효한 태그만 기록
                    int validTagCount = 0;
                    foreach (var key in tagKeys)
                    {
                        string tagValue = mat.GetTag(key, false);
                        if (!string.IsNullOrEmpty(tagValue))
                            validTagCount++;
                    }
                    writer.Write(validTagCount);
                    foreach (var key in tagKeys)
                    {
                        string tagValue = mat.GetTag(key, false);
                        if (!string.IsNullOrEmpty(tagValue))
                        {
                            writer.Write(key);
                            writer.Write(tagValue);
                        }
                    }

                    // 5. Shader Pass 활성화 상태 기록
                    int passCount = mat.passCount;
                    writer.Write(passCount);
                    for (int i = 0; i < passCount; i++)
                    {
                        string passName = mat.GetPassName(i);
                        writer.Write(passName);
                        bool enabled = mat.GetShaderPassEnabled(passName);
                        writer.Write(enabled);
                    }

                    // 5. 기타 특수 타입
                    writer.Write(mat.renderQueue);
                    writer.Write((int)mat.globalIlluminationFlags);
                    writer.Write(mat.doubleSidedGI);
                    writer.Write(mat.mainTextureScale.x);
                    writer.Write(mat.mainTextureScale.y);
                    writer.Write(mat.mainTextureOffset.x);
                    writer.Write(mat.mainTextureOffset.y);
                }
                return ms.ToArray();
            }
        }

        /// <summary>
        /// 바이너리 데이터를 읽어 Material에 변경점을 적용합니다.
        /// </summary>
        public static void ApplyBinary(Material mat, byte[] data)
        {
            using (MemoryStream ms = new MemoryStream(data))
            {
                using (BinaryReader reader = new BinaryReader(ms))
                {
                    var shader = Shader.Find(reader.ReadString());
                    if (shader == null)
                    {
                        Debug.LogWarning("Could not find shader by shader name.");
                        return;
                    }

                    mat.shader = shader;

                    // 1. 프로퍼티 복원
                    int propCount = reader.ReadInt32();
                    for (int i = 0; i < propCount; i++)
                    {
                        string propName = reader.ReadString();
                        eMaterialPropertyType sPropType = (eMaterialPropertyType)reader.ReadByte();
                        switch (sPropType)
                        {
                            case eMaterialPropertyType.Color:
                                float r = reader.ReadSingle();
                                float g = reader.ReadSingle();
                                float b = reader.ReadSingle();
                                float a = reader.ReadSingle();
                                mat.SetColor(propName, new Color(r, g, b, a));
                                break;

                            case eMaterialPropertyType.Number:
                                float f = reader.ReadSingle();
                                mat.SetFloat(propName, f);
                                break;
                        }
                    }

                    // 2. 키워드 복원
                    int kwCount = reader.ReadInt32();
                    string[] keywords = new string[kwCount];
                    for (int i = 0; i < kwCount; i++)
                    {
                        keywords[i] = reader.ReadString();
                    }
                    mat.shaderKeywords = keywords;

                    // 3. Override 태그 복원
                    int tagCount = reader.ReadInt32();
                    for (int i = 0; i < tagCount; i++)
                    {
                        string key = reader.ReadString();
                        string tagValue = reader.ReadString();
                        mat.SetOverrideTag(key, tagValue);
                    }

                    // 4. Shader Pass 활성화 상태 복원
                    int passCount = reader.ReadInt32();
                    for (int i = 0; i < passCount; i++)
                    {
                        string passName = reader.ReadString();
                        bool enabled = reader.ReadBoolean();
                        mat.SetShaderPassEnabled(passName, enabled);
                    }

                    // 5. 기타 특수 타입
                    mat.renderQueue = reader.ReadInt32();
                    mat.globalIlluminationFlags = (MaterialGlobalIlluminationFlags)reader.ReadInt32();
                    mat.doubleSidedGI = reader.ReadBoolean();
                    Vector2 mainTextureScale = new(reader.ReadSingle(), reader.ReadSingle());
                    Vector2 mainTextureOffset = new(reader.ReadSingle(), reader.ReadSingle());
                    mat.SetTextureScale("_BaseMap", mainTextureScale);
                    mat.SetTextureScale("_MainTex", mainTextureScale);
                    mat.SetTextureOffset("_BaseMap", mainTextureOffset);
                    mat.SetTextureOffset("_MainTex", mainTextureOffset);
                }
            }
        }
    }
}