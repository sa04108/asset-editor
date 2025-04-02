using System.Collections.Generic;
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

        private void Start()
        {
            ClearMembers();
            saveButton.onClick.AddListener(SaveAsset);
            resetButton.onClick.AddListener(ResetAsset);
        }

        private void ClearMembers()
        {
            for (int i = memberParent.childCount - 1; i >= 0; i--)
            {
                Destroy(memberParent.GetChild(i).gameObject);
            }
        }

        public void LoadModel(GameObject go)
        {
            modifier = new(go);
            AssetWindow.Show(go.transform, modifier.GetSharedMaterials(), InspectMaterialProperties);
        }

        private void InspectMaterialProperties(Material mat)
        {
            memberCreator.SetBindingPointer(memberParent);

            if (mat.shader.name == "Universal Render Pipeline/Lit")
                InspectLitMaterialProperties(mat);
            else
                InspectAllMaterialProperties(mat);

            materialProperties = memberParent.GetComponentsInChildren<IMaterialPropertyMember>();
        }

        private void InspectLitMaterialProperties(Material mat)
        {
            ShaderPropertyIndex shaderPropIdx = new();
            for (int i = 0; i < mat.shader.GetPropertyCount(); i++)
            {
                shaderPropIdx.Add(mat.shader.GetPropertyName(i), i);
            }

            var group = memberCreator.CreateGroupMember("Lit Options", memberParent);
            var shaderModifier = new URPLitShaderModifier();

            var workflowMode = mat.GetInt("_WorkflowMode");
            memberCreator.CreateEnumMember("Workflow Mode", mat, typeof(eShaderWorkflowMode), workflowMode, group, "")
                .OnValueChanged.AddListener(value => shaderModifier.SetWorkflowMode(mat, (eShaderWorkflowMode)value));

            var surfaceType = mat.GetInt("_Surface");
            var surfaceMember = memberCreator.CreateEnumMember("Surface Type", mat, typeof(eShaderSurfaceType), surfaceType, group, "");
            surfaceMember.OnValueChanged.AddListener(value => shaderModifier.SetSurfaceType(mat, (eShaderSurfaceType)value));

            var blendGroup = memberCreator.CreateSubGroup(surfaceMember, value => value == (int)eShaderSurfaceType.Transparent, surfaceType == (int)eShaderSurfaceType.Transparent);

            var blend = mat.GetInt("_Blend");
            memberCreator.CreateEnumMember("Blending Mode", mat, typeof(eBlendMode), blend, blendGroup, "")
                .OnValueChanged.AddListener(value => shaderModifier.SetBlendMode(mat, (eBlendMode)value));

            var renderFace = mat.GetInt("_Cull");
            memberCreator.CreateEnumMember("Render Face", mat, typeof(eShaderRenderFace), renderFace, group, "")
                .OnValueChanged.AddListener(value => shaderModifier.SetRenderFace(mat, (eShaderRenderFace)value));

            var alphaCliping = mat.GetInt("_AlphaClip");
            memberCreator.CreateBoolMember("Alpha Clip", mat, alphaCliping == 1 ? true : false, group, "")
                .OnValueChanged.AddListener(value => shaderModifier.SetAlphaClipping(mat, value));

            var alphaCutoff = mat.GetFloat("_Cutoff");
            Vector2 rangeCutoff = mat.shader.GetPropertyRangeLimits(shaderPropIdx["_Cutoff"]);
            memberCreator.CreateFloatMember("Alpha Cutoff", mat, alphaCutoff, rangeCutoff.x, rangeCutoff.y, group, "")
                .OnValueChanged.AddListener(value => shaderModifier.SetAlphaCutoff(mat, value));

            var receiveShadows = mat.GetInt("_ReceiveShadows");
            memberCreator.CreateBoolMember("Receive Shadows", mat, receiveShadows == 1 ? true : false, group, "")
                .OnValueChanged.AddListener(value => shaderModifier.SetReceiveShadows(mat, value));

            //var mainTex = mat.GetTexture("_BaseMap");
            //memberCreator.CreateTexturePropertyMember("Base Map", mat, mainTex, group, "")
            //    .OnValueChanged.AddListener(value => shaderModifier.SetTextureMap(mat, eShaderTextureMap.BaseMap, value));

            //var metallicTex = mat.GetTexture("_MetallicGlossMap");
            //memberCreator.CreateTexturePropertyMember("Metallic Map", mat, metallicTex, group, "_MetallicGlossMap");

            var metallicDegree = mat.GetFloat("_Metallic");
            Vector2 rangeMetallic = mat.shader.GetPropertyRangeLimits(shaderPropIdx["_Metallic"]);
            memberCreator.CreateFloatMember("Metallic", mat, metallicDegree, rangeMetallic.x, rangeMetallic.y, group, "_Metallic");

            var smoothness = mat.GetFloat("_Smoothness");
            Vector2 rangeSmoothness = mat.shader.GetPropertyRangeLimits(shaderPropIdx["_Smoothness"]);
            memberCreator.CreateFloatMember("Smoothness", mat, smoothness, rangeSmoothness.x, rangeSmoothness.y, group, "_Smoothness");

            //var bumpText = mat.GetTexture("_BumpMap");
            //memberCreator.CreateTexturePropertyMember("Normal Map", mat, bumpText, group, "")
            //    .OnValueChanged.AddListener(value => shaderModifier.SetTextureMap(mat, eShaderTextureMap.NormalMap, value));

            var mainColor = mat.GetColor("_BaseColor");
            memberCreator.CreateColorMember("Base Color", mat, mainColor, true, false, group, "_BaseColor");

            //var maskTex = mat.GetTexture("_DetailMask");
            //memberCreator.CreateTexturePropertyMember("Mask", mat, maskTex, group, "")
            //    .OnValueChanged.AddListener(value => shaderModifier.SetTextureMap(mat, eShaderTextureMap.DetailMaskMap, value));

            var emsEnable = mat.globalIlluminationFlags != (MaterialGlobalIlluminationFlags)4;
            var emsEnableMember = memberCreator.CreateBoolMember("Emission", mat, emsEnable, group, "");
            emsEnableMember.OnValueChanged.AddListener(value => shaderModifier.SetEmission(mat, value));

            var emsGroup = memberCreator.CreateSubGroup(emsEnableMember, value => value, emsEnable);

            var emsGIFlag = mat.globalIlluminationFlags;
            memberCreator.CreateEnumMember("Emission Global Illumination", mat, typeof(eEmissionGlobalIllumination), (int)emsGIFlag, emsGroup, "")
                .OnValueChanged.AddListener(value => shaderModifier.SetEmissionMode(mat, (eEmissionGlobalIllumination)value));

            //var emsTex = mat.GetTexture("_EmissionMap");
            //memberCreator.CreateTexturePropertyMember("Emission Map", mat, emsTex, emsGroup, "")
            //    .OnValueChanged.AddListener(value => shaderModifier.SetTextureMap(mat, eShaderTextureMap.EmissionMap, value));

            var emsColor = mat.GetColor("_EmissionColor");
            memberCreator.CreateColorMember("Emission Color", mat, mainColor, false, true, emsGroup, "_EmissionColor");
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

            //var textureProps = mat.GetTexturePropertyNames();
            //foreach (string prop in textureProps)
            //{
            //    var tex = mat.GetTexture(prop);
            //    memberCreator.CreateTexturePropertyMember(prop, mat, tex, group, prop);
            //}

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
                    memberCreator.CreateFloatMember(prop, mat, value, min, max, group, prop);
                }
                else
                {
                    memberCreator.CreateFloatMember(prop, mat, value, group, prop);
                }
            }

            var intProps = mat.GetPropertyNames(MaterialPropertyType.Int);
            foreach (string prop in intProps)
            {
                var value = mat.GetInteger(prop);
                memberCreator.CreateIntMember(prop, mat, value, group, prop);
            }

            var vecProps = mat.GetPropertyNames(MaterialPropertyType.Vector);
            foreach (string prop in vecProps)
            {
                var value = mat.GetVector(prop);
                if (shaderPropIdx.ContainsKey(prop) &&
                    mat.shader.GetPropertyType(shaderPropIdx[prop]) == UnityEngine.Rendering.ShaderPropertyType.Color)
                {
                    bool isHDR = mat.shader.GetPropertyFlags(shaderPropIdx[prop]) == UnityEngine.Rendering.ShaderPropertyFlags.HDR;
                    memberCreator.CreateColorMember(prop, mat, value, true, isHDR, group, prop);
                }
                else
                {
                    memberCreator.CreateVectorMember(prop, mat, value, group, prop);
                }
            }

            var matrixProps = mat.GetPropertyNames(MaterialPropertyType.Matrix);
            foreach (string prop in matrixProps)
            {
                var value = mat.GetMatrix(prop);
                memberCreator.CreateMatrixMember(prop, mat, value, group, prop);
            }
        }

        public void SaveAsset()
        {
            // API를 사용하여 서버로 전송
            modifier?.Save();
        }

        public void ResetAsset()
        {
            modifier?.Reset();
            materialProperties?.ToList().ForEach(prop => prop.UpdateUI());
        }
    }
}