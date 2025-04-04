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
        private ShaderSelectionMember shaderSelector;

        [SerializeField]
        private Transform materialParent;

        [SerializeField]
        private Transform memberParent;

        [SerializeField]
        private PropertyMemberCreator memberCreator;

        [SerializeField]
        private Button resetButton;

        private AssetModifier modifier;
        private IMaterialPropertyMember[] materialProperties;

        private string[] shaderOptions =
        {
            "Universal Render Pipeline/Lit",
            "Universal Render Pipeline/Unlit"
        };

        private void Start()
        {
            ClearMembers(materialParent);
            ClearMembers(memberParent);

            shaderSelector.Initialize(shaderOptions);
            shaderSelector.OnShaderChanged.AddListener(InspectMaterial);

            resetButton.onClick.AddListener(ResetAsset);
        }

        private void ClearMembers(Transform parent)
        {
            for (int i = parent.childCount - 1; i >= 0; i--)
            {
                Destroy(parent.GetChild(i).gameObject);
            }
        }

        public void LoadModel(GameObject go)
        {
            shaderSelector.gameObject.SetActive(false);
            ClearMembers(materialParent);
            ClearMembers(memberParent);

            modifier = new(go);
            var materials = modifier.GetSharedMaterials();
            foreach (var material in materials)
            {
                memberCreator.CreateMaterialMember(material.name, material, materialParent)
                    .OnClick.AddListener(InspectMaterial);
            }
        }

        private void InspectMaterial(Material mat)
        {
            shaderSelector.SetMaterial(mat);
            ClearMembers(memberParent);

            if (mat.shader.name == shaderOptions[0])
                InspectLitMaterialProperties(mat);
            else if (mat.shader.name == shaderOptions[1])
                InspectUnlitMaterialProperties(mat);
            else
                InspectAllMaterialProperties(mat);
        }

        private void InspectLitMaterialProperties(Material mat)
        {
            ShaderPropertyIndex shaderPropIdx = new();
            for (int i = 0; i < mat.shader.GetPropertyCount(); i++)
            {
                shaderPropIdx.Add(mat.shader.GetPropertyName(i), i);
            }

            var optionGroup = memberCreator.CreateGroupMember("Surface Options", memberParent);
            var matModifier = new URPLitShaderModifier();

            var workflowMode = mat.GetInt("_WorkflowMode");
            var workflowMember = memberCreator.CreateEnumMember("Workflow Mode", mat, typeof(eShaderWorkflowMode), workflowMode, optionGroup, null);
            workflowMember.OnValueChanged.AddListener(value => matModifier.SetWorkflowMode(mat, (eShaderWorkflowMode)value));

            var surfaceType = mat.GetInt("_Surface");
            var surfaceMember = memberCreator.CreateEnumMember("Surface Type", mat, typeof(eShaderSurfaceType), surfaceType, optionGroup, null);
            surfaceMember.OnValueChanged.AddListener(value => matModifier.SetSurfaceType(mat, (eShaderSurfaceType)value));

            var blendGroup = memberCreator.CreateSubGroup(surfaceMember,
                value => value == (int)eShaderSurfaceType.Transparent,
                optionGroup,
                surfaceType == (int)eShaderSurfaceType.Transparent);

            var blend = mat.GetInt("_Blend");
            memberCreator.CreateEnumMember("Blending Mode", mat, typeof(eBlendMode), blend, blendGroup, null)
                .OnValueChanged.AddListener(value => matModifier.SetBlendMode(mat, (eBlendMode)value));

            var renderFace = mat.GetInt("_Cull");
            memberCreator.CreateEnumMember("Render Face", mat, typeof(eShaderRenderFace), renderFace, optionGroup, null)
                .OnValueChanged.AddListener(value => matModifier.SetRenderFace(mat, (eShaderRenderFace)value));

            var alphaCliping = mat.GetInt("_AlphaClip");
            var alphaClipMember = memberCreator.CreateBoolMember("Alpha Clipping", mat, alphaCliping == 1, optionGroup, null);
            alphaClipMember.OnValueChanged.AddListener(value => matModifier.SetAlphaClipping(mat, value));

            var alphaClipGroup = memberCreator.CreateSubGroup(alphaClipMember, value => value, optionGroup, alphaCliping == 1);

            var alphaCutoff = mat.GetFloat("_Cutoff");
            Vector2 rangeCutoff = mat.shader.GetPropertyRangeLimits(shaderPropIdx["_Cutoff"]);
            memberCreator.CreateFloatMember("Threshold", mat, alphaCutoff, rangeCutoff.x, rangeCutoff.y, alphaClipGroup, "_Cutoff");

            var receiveShadows = mat.GetInt("_ReceiveShadows");
            memberCreator.CreateBoolMember("Receive Shadows", mat, receiveShadows == 1, optionGroup, null)
                .OnValueChanged.AddListener(value => matModifier.SetReceiveShadows(mat, value));

            //var mainTex = mat.GetTexture("_BaseMap");
            //memberCreator.CreateTexturePropertyMember("Base Map", mat, mainTex, group, "")
            //    .OnValueChanged.AddListener(value => shaderModifier.SetTextureMap(mat, eShaderTextureMap.BaseMap, value));

            //var metallicTex = mat.GetTexture("_MetallicGlossMap");
            //memberCreator.CreateTexturePropertyMember("Metallic Map", mat, metallicTex, group, "_MetallicGlossMap");

            var inputGroup = memberCreator.CreateGroupMember("Surface Inputs", memberParent);

            var metallicGroup = memberCreator.CreateSubGroup(workflowMember,
                value => value == (int)eShaderWorkflowMode.Metallic,
                inputGroup,
                workflowMode == (int)eShaderWorkflowMode.Metallic, true);

            var metallicDegree = mat.GetFloat("_Metallic");
            Vector2 rangeMetallic = mat.shader.GetPropertyRangeLimits(shaderPropIdx["_Metallic"]);
            memberCreator.CreateFloatMember("Metallic", mat, metallicDegree, rangeMetallic.x, rangeMetallic.y, metallicGroup, "_Metallic");

            var smoothness = mat.GetFloat("_Smoothness");
            Vector2 rangeSmoothness = mat.shader.GetPropertyRangeLimits(shaderPropIdx["_Smoothness"]);
            memberCreator.CreateFloatMember("Smoothness", mat, smoothness, rangeSmoothness.x, rangeSmoothness.y, inputGroup, "_Smoothness");

            //var bumpText = mat.GetTexture("_BumpMap");
            //memberCreator.CreateTexturePropertyMember("Normal Map", mat, bumpText, group, "")
            //    .OnValueChanged.AddListener(value => shaderModifier.SetTextureMap(mat, eShaderTextureMap.NormalMap, value));

            var baseColor = mat.GetColor("_BaseColor");
            memberCreator.CreateColorMember("Base Color", mat, baseColor, true, false, inputGroup, null)
                .OnValueChanged.AddListener(value => matModifier.SetBaseColor(mat, value));

            var bumpScale = mat.GetFloat("_BumpScale");
            memberCreator.CreateFloatMember("Normal Scale", mat, bumpScale, inputGroup, "_BumpScale");

            //var maskTex = mat.GetTexture("_DetailMask");
            //memberCreator.CreateTexturePropertyMember("Mask", mat, maskTex, group, "")
            //    .OnValueChanged.AddListener(value => shaderModifier.SetTextureMap(mat, eShaderTextureMap.DetailMaskMap, value));

            var emsEnable = mat.globalIlluminationFlags != (MaterialGlobalIlluminationFlags)4;
            var emsEnableMember = memberCreator.CreateBoolMember("Emission", mat, emsEnable, inputGroup, null);
            emsEnableMember.OnValueChanged.AddListener(value => matModifier.SetEmission(mat, value));

            var emsGroup = memberCreator.CreateSubGroup(emsEnableMember, value => value, inputGroup, emsEnable);

            var emsColor = mat.GetColor("_EmissionColor");
            memberCreator.CreateColorMember("Emission Color", mat, baseColor, false, true, emsGroup, "_EmissionColor");

            var emsGIFlag = mat.globalIlluminationFlags;
            memberCreator.CreateEnumMember("Emission Global Illumination", mat, typeof(eEmissionGlobalIllumination), (int)emsGIFlag, emsGroup, null)
                .OnValueChanged.AddListener(value => matModifier.SetEmissionMode(mat, (eEmissionGlobalIllumination)value));

            //var emsTex = mat.GetTexture("_EmissionMap");
            //memberCreator.CreateTexturePropertyMember("Emission Map", mat, emsTex, emsGroup, "")
            //    .OnValueChanged.AddListener(value => shaderModifier.SetTextureMap(mat, eShaderTextureMap.EmissionMap, value));

            var baseTiling = mat.mainTextureScale;
            memberCreator.CreateVectorMember("Tiling", mat, baseTiling, inputGroup, null)
                .OnValueChanged.AddListener(value => matModifier.SetBaseTiling(mat, value));

            var baseOffset = mat.mainTextureOffset;
            memberCreator.CreateVectorMember("Offset", mat, baseOffset, inputGroup, null)
                .OnValueChanged.AddListener(value => matModifier.SetBaseOffset(mat, value));
        }

        private void InspectUnlitMaterialProperties(Material mat)
        {
            ShaderPropertyIndex shaderPropIdx = new();
            for (int i = 0; i < mat.shader.GetPropertyCount(); i++)
            {
                shaderPropIdx.Add(mat.shader.GetPropertyName(i), i);
            }

            var optionGroup = memberCreator.CreateGroupMember("Surface Options", memberParent);
            var matModifier = new URPUnlitShaderModifier();

            var surfaceType = mat.GetInt("_Surface");
            var surfaceMember = memberCreator.CreateEnumMember("Surface Type", mat, typeof(eShaderSurfaceType), surfaceType, optionGroup, null);
            surfaceMember.OnValueChanged.AddListener(value => matModifier.SetSurfaceType(mat, (eShaderSurfaceType)value));

            var blendGroup = memberCreator.CreateSubGroup(surfaceMember,
                value => value == (int)eShaderSurfaceType.Transparent,
                optionGroup,
                surfaceType == (int)eShaderSurfaceType.Transparent);

            var blend = mat.GetInt("_Blend");
            memberCreator.CreateEnumMember("Blending Mode", mat, typeof(eBlendMode), blend, blendGroup, null)
                .OnValueChanged.AddListener(value => matModifier.SetBlendMode(mat, (eBlendMode)value));

            var renderFace = mat.GetInt("_Cull");
            memberCreator.CreateEnumMember("Render Face", mat, typeof(eShaderRenderFace), renderFace, optionGroup, null)
                .OnValueChanged.AddListener(value => matModifier.SetRenderFace(mat, (eShaderRenderFace)value));

            var alphaCliping = mat.GetInt("_AlphaClip");
            var alphaClipMember = memberCreator.CreateBoolMember("Alpha Clipping", mat, alphaCliping == 1, optionGroup, null);
            alphaClipMember.OnValueChanged.AddListener(value => matModifier.SetAlphaClipping(mat, value));

            var alphaClipGroup = memberCreator.CreateSubGroup(alphaClipMember, value => value, optionGroup, alphaCliping == 1);

            var alphaCutoff = mat.GetFloat("_Cutoff");
            Vector2 rangeCutoff = mat.shader.GetPropertyRangeLimits(shaderPropIdx["_Cutoff"]);
            memberCreator.CreateFloatMember("Threshold", mat, alphaCutoff, rangeCutoff.x, rangeCutoff.y, alphaClipGroup, "_Cutoff");

            var inputGroup = memberCreator.CreateGroupMember("Surface Inputs", memberParent);

            var baseColor = mat.GetColor("_BaseColor");
            memberCreator.CreateColorMember("Base Color", mat, baseColor, true, false, inputGroup, null)
                .OnValueChanged.AddListener(value => matModifier.SetBaseColor(mat, value));

            var baseTiling = mat.mainTextureScale;
            memberCreator.CreateVectorMember("Tiling", mat, baseTiling, inputGroup, null)
                .OnValueChanged.AddListener(value => matModifier.SetBaseTiling(mat, value));

            var baseOffset = mat.mainTextureOffset;
            memberCreator.CreateVectorMember("Offset", mat, baseOffset, inputGroup, null)
                .OnValueChanged.AddListener(value => matModifier.SetBaseOffset(mat, value));
        }

        private void InspectAllMaterialProperties(Material mat)
        {
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

            //var matrixProps = mat.GetPropertyNames(MaterialPropertyType.Matrix);
            //foreach (string prop in matrixProps)
            //{
            //    var value = mat.GetMatrix(prop);
            //    memberCreator.CreateMatrixMember(prop, mat, value, group, prop);
            //}
        }

        public void ResetAsset()
        {
            modifier?.Reset();

            materialProperties = memberParent.GetComponentsInChildren<IMaterialPropertyMember>();
            materialProperties?.ToList().ForEach(prop => prop.UpdateUI());
        }
    }
}