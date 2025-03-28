﻿using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.UI;

namespace Merlin
{
    using ShaderPropertyIndex = Dictionary<string, int>;

    public class AssetInspector : MonoBehaviour
    {
        [SerializeField]
        private Transform memberParent;

        [SerializeField]
        private PropertyMemberCreator memberCreator;

        [SerializeField]
        private Button saveButton;

        [SerializeField]
        private Button resetButton;

        private AssetModifier modifier;
        private IMaterialPropertyMember[] materialProperties;
        private int presetCount;

        private void Start()
        {
            presetCount = memberParent.transform.childCount;
            saveButton.onClick.AddListener(SaveAsset);
            resetButton.onClick.AddListener(ResetAsset);
        }

        public void SetModelInstance(GameObject go, Texture[] textures)
        {
            ClearMembers();
            LoadMembers(go);
        }

        private void ClearMembers()
        {
            for (int i = memberParent.childCount - 1; i >= presetCount; i--)
            {
                Destroy(memberParent.GetChild(i).gameObject);
            }
        }

        private void LoadMembers(GameObject go)
        {
            modifier = new(go);
            AssetWindow.Show(go.transform, modifier.GetSharedMaterials(), InspectMaterialProperties);
        }

        private void InspectMaterialProperties(Material mat)
        {
            if (mat.shader.name == "Universal Render Pipeline/Lit")
                InspectLitMaterialProperties(mat);
            else
                InspectAllMaterialProperties(mat);

            materialProperties = memberParent.GetComponentsInChildren<IMaterialPropertyMember>();
        }

        private void InspectLitMaterialProperties(Material mat)
        {
            ClearMembers();

            ShaderPropertyIndex shaderPropIdx = new();
            for (int i = 0; i < mat.shader.GetPropertyCount(); i++)
            {
                shaderPropIdx.Add(mat.shader.GetPropertyName(i), i);
            }

            var group = memberCreator.CreateGroupMember("Lit Options", memberParent);
            var propertyState = new MaterialPropertyState();

            var workflowMode = mat.GetInt("_WorkflowMode");
            memberCreator.CreateEnumMember(mat, "Workflow Mode", typeof(eShaderWorkflowMode), workflowMode, group)
                .OnValueChanged(value => propertyState.SetWorkflowMode(mat, (eShaderWorkflowMode)value));

            var surfaceType = mat.GetInt("_Surface");
            memberCreator.CreateEnumMember(mat, "Surface Type", typeof(eShaderSurfaceType), surfaceType, group)
                .OnValueChanged(value => propertyState.SetSurfaceType(mat, (eShaderSurfaceType)value));

            var renderFace = mat.GetInt("_Cull");
            memberCreator.CreateEnumMember(mat, "Render Face", typeof(eShaderRenderFace), renderFace, group)
                .OnValueChanged(value => propertyState.SetRenderFace(mat, (eShaderRenderFace)value));

            var alphaCliping = mat.GetInt("_AlphaClip");
            memberCreator.CreateBoolMember(mat, "Alpha Clip", alphaCliping == 1 ? true : false, group)
                .OnValueChanged(value => propertyState.SetAlphaClipping(mat, value));

            var alphaCutoff = mat.GetFloat("_Cutoff");
            Vector2 rangeCutoff = mat.shader.GetPropertyRangeLimits(shaderPropIdx["_Cutoff"]);
            memberCreator.CreateFloatMember(mat, "Alpha Cutoff", alphaCutoff, rangeCutoff.x, rangeCutoff.y, group)
                .OnValueChanged(value => propertyState.SetAlphaCutoff(mat, value));

            var receiveShadows = mat.GetInt("_ReceiveShadows");
            memberCreator.CreateBoolMember(mat, "Receive Shadows", receiveShadows == 1 ? true : false, group)
                .OnValueChanged(value => propertyState.SetReceiveShadows(mat, value));

            var mainTex = mat.GetTexture("_BaseMap");
            memberCreator.CreateTexturePropertyMember(mat, "Base Map", mainTex, group)
                .OnValueChanged(value => propertyState.SetTextureMap(mat, eShaderTextureMap.BaseMap, value));

            var metallicTex = mat.GetTexture("_MetallicGlossMap");
            memberCreator.CreateTexturePropertyMember(mat, "Metallic Map", metallicTex, group);

            var metallicDegree = mat.GetFloat("_Metallic");
            Vector2 rangeMetallic = mat.shader.GetPropertyRangeLimits(shaderPropIdx["_Metallic"]);
            memberCreator.CreateFloatMember(mat, "_Metallic", metallicDegree, rangeMetallic.x, rangeMetallic.y, group);

            var smoothness = mat.GetFloat("_Smoothness");
            Vector2 rangeSmoothness = mat.shader.GetPropertyRangeLimits(shaderPropIdx["_Smoothness"]);
            memberCreator.CreateFloatMember(mat, "_Smoothness", smoothness, rangeSmoothness.x, rangeSmoothness.y, group);

            var bumpText = mat.GetTexture("_BumpMap");
            memberCreator.CreateTexturePropertyMember(mat, "Normal Map", bumpText, group)
                .OnValueChanged(value => propertyState.SetTextureMap(mat, eShaderTextureMap.NormalMap, value));

            var mainColor = mat.GetColor("_BaseColor");
            memberCreator.CreateColorMember(mat, "_BaseColor", mainColor, true, false, group);

            var maskTex = mat.GetTexture("_DetailMask");
            memberCreator.CreateTexturePropertyMember(mat, "_DetailMask", maskTex, group)
                .OnValueChanged(value => propertyState.SetTextureMap(mat, eShaderTextureMap.DetailMaskMap, value));

            var emsGIMode = mat.globalIlluminationFlags;
            memberCreator.CreateEnumMember(mat, "Emission Global Illumination", typeof(eEmissionGlobalIllumination), (int)emsGIMode, group)
                .OnValueChanged(value => propertyState.SetEmission(mat, (eEmissionGlobalIllumination)value));

            var emsTex = mat.GetTexture("_EmissionMap");
            memberCreator.CreateTexturePropertyMember(mat, "Emission Map", emsTex, group)
                .OnValueChanged(value => propertyState.SetTextureMap(mat, eShaderTextureMap.EmissionMap, value));

            var emsColor = mat.GetColor("_EmissionColor");
            memberCreator.CreateColorMember(mat, "_EmissionColor", mainColor, false, true, group);
        }

        private void InspectAllMaterialProperties(Material mat)
        {
            ClearMembers();

            ShaderPropertyIndex shaderPropIdx = new();
            for (int i = 0; i < mat.shader.GetPropertyCount(); i++)
            {
                shaderPropIdx.Add(mat.shader.GetPropertyName(i), i);
            }

            var group = memberCreator.CreateGroupMember("Options", memberParent);

            var textureProps = mat.GetTexturePropertyNames();
            foreach (string prop in textureProps)
            {
                var tex = mat.GetTexture(prop);
                memberCreator.CreateTexturePropertyMember(mat, prop, tex, group);
            }

            var floatProps = mat.GetPropertyNames(MaterialPropertyType.Float);
            foreach (string prop in floatProps)
            {
                var value = mat.GetFloat(prop);
                if (shaderPropIdx.ContainsKey(prop) &&
                    mat.shader.GetPropertyType(shaderPropIdx[prop]) == UnityEngine.Rendering.ShaderPropertyType.Range)
                {
                    Vector2 rangeVec = mat.shader.GetPropertyRangeLimits(shaderPropIdx[prop]);
                    float min = rangeVec.x;
                    float max = rangeVec.y;
                    memberCreator.CreateFloatMember(mat, prop, value, min, max, group);
                }
                else
                {
                    memberCreator.CreateFloatMember(mat, prop, value, group);
                }
            }

            var intProps = mat.GetPropertyNames(MaterialPropertyType.Int);
            foreach (string prop in intProps)
            {
                var value = mat.GetInteger(prop);
                memberCreator.CreateIntMember(mat, prop, value, group);
            }

            var vecProps = mat.GetPropertyNames(MaterialPropertyType.Vector);
            foreach (string prop in vecProps)
            {
                var value = mat.GetVector(prop);
                if (shaderPropIdx.ContainsKey(prop) &&
                    mat.shader.GetPropertyType(shaderPropIdx[prop]) == UnityEngine.Rendering.ShaderPropertyType.Color)
                {
                    bool isHDR = mat.shader.GetPropertyFlags(shaderPropIdx[prop]) == UnityEngine.Rendering.ShaderPropertyFlags.HDR;
                    memberCreator.CreateColorMember(mat, prop, value, true, isHDR, group);
                }
                else
                {
                    memberCreator.CreateVectorMember(mat, prop, value, group);
                }
            }

            var matrixProps = mat.GetPropertyNames(MaterialPropertyType.Matrix);
            foreach (string prop in matrixProps)
            {
                var value = mat.GetMatrix(prop);
                memberCreator.CreateMatrixMember(mat, prop, value, group);
            }
        }

        private bool IsPropertyHideInInspector(Material mat, ShaderPropertyIndex dict, string prop)
        {
            return dict.ContainsKey(prop) &&
                mat.shader.GetPropertyFlags(dict[prop]) == UnityEngine.Rendering.ShaderPropertyFlags.HideInInspector;
        }

        public void SaveAsset()
        {
            // API를 사용하여 서버로 전송
            modifier?.Save();
        }

        public void ResetAsset()
        {
            modifier?.Reset();
            materialProperties?.ToList().ForEach(prop => prop.ResetProperty());
        }
    }
}