using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.UI;

namespace Merlin
{
    public class AssetInspector : MonoBehaviour
    {
        [Header("Links")]
        [SerializeField] private Transform materialParent;
        [SerializeField] private MaterialMember materialMemberPreset;
        [SerializeField] private Button resetButton;

        [Header("Inspectable Members")]
        [SerializeField] private InspectableMember workflowModeMember;
        [SerializeField] private InspectableMember surfaceMember;
        [SerializeField] private InspectableMember blendMember;
        [SerializeField] private InspectableMember renderFaceMember;
        [SerializeField] private InspectableMember alphaClippingMember;
        [SerializeField] private InspectableMember alphaCutoffMember;
        [SerializeField] private InspectableMember receiveShadowsMember;
        [SerializeField] private InspectableMember metallicMember;
        [SerializeField] private InspectableMember smoothnessMember;
        [SerializeField] private InspectableMember baseColorMember;
        [SerializeField] private InspectableMember normalScaleMember;
        [SerializeField] private InspectableMember emissionMember;
        [SerializeField] private InspectableMember emissionColorMember;
        [SerializeField] private InspectableMember emissionFlagMember;
        [SerializeField] private InspectableMember baseTilingMember;
        [SerializeField] private InspectableMember baseOffsetMember;

        private AssetModifier modifier;

        private void Start()
        {
            ClearMembers(materialParent);

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
            ClearMembers(materialParent);

            modifier = new(go);
            var materials = modifier.GetSharedMaterials();
            foreach (var material in materials)
            {
                var member = Instantiate(materialMemberPreset, materialParent);
                member.Initialize(material.name, material);
                member.OnClick.AddListener(InspectMaterial);
            }
        }

        private void InspectMaterial(Material mat)
        {
            if (mat.shader.name == "Universal Render Pipeline/Lit")
            {
                InspectLitMaterialProperties(mat);
            }
            else
            {
                Debug.LogWarning($"Shader {mat.shader.name} for current material is not supported");
            }
        }

        private void InspectLitMaterialProperties(Material mat)
        {
            Dictionary<string, int> shaderPropIdx = new();
            for (int i = 0; i < mat.shader.GetPropertyCount(); i++)
            {
                shaderPropIdx.Add(mat.shader.GetPropertyName(i), i);
            }

            var workflowMode = mat.GetInt("_WorkflowMode");
            metallicMember.gameObject.SetActive(workflowMode == (int)eShaderWorkflowMode.Metallic);
            BindEnum(workflowModeMember, workflowMode, value =>
            {
                URPLitShaderModifier.SetWorkflowMode(mat, (eShaderWorkflowMode)value);
                metallicMember.gameObject.SetActive(value == (int)eShaderWorkflowMode.Metallic);
            });

            var surfaceType = mat.GetInt("_Surface");
            blendMember.transform.parent.gameObject.SetActive(surfaceType == (int)eShaderSurfaceType.Transparent);
            BindEnum(surfaceMember, surfaceType, value =>
            {
                URPLitShaderModifier.SetSurfaceType(mat, (eShaderSurfaceType)value);
                blendMember.transform.parent.gameObject.SetActive(value == (int)eShaderSurfaceType.Transparent);
            });

            var blend = mat.GetInt("_Blend");
            BindEnum(blendMember, blend, value => URPLitShaderModifier.SetBlendMode(mat, (eBlendMode)value));

            var renderFace = mat.GetInt("_Cull");
            BindEnum(renderFaceMember, renderFace, value => URPLitShaderModifier.SetRenderFace(mat, (eShaderRenderFace)value));

            var alphaClipping = mat.GetInt("_AlphaClip");
            alphaCutoffMember.transform.parent.gameObject.SetActive(alphaClipping == 1);
            BindBool(alphaClippingMember, alphaClipping == 1, value =>
            {
                URPLitShaderModifier.SetAlphaClipping(mat, value);
                alphaCutoffMember.transform.parent.gameObject.SetActive(value);
            });

            var alphaCutoff = mat.GetFloat("_Cutoff");
            BindFloat(alphaCutoffMember, alphaCutoff, value => mat.SetFloat("_Cutoff", value));

            var receiveShadows = mat.GetInt("_ReceiveShadows");
            BindBool(receiveShadowsMember, receiveShadows == 1, value => URPLitShaderModifier.SetReceiveShadows(mat, value));

            var metallic = mat.GetFloat("_Metallic");
            BindFloat(metallicMember, metallic, value => mat.SetFloat("_Metallic", value));

            var smoothness = mat.GetFloat("_Smoothness");
            BindFloat(smoothnessMember, smoothness, value => mat.SetFloat("_Smoothness", value));

            var baseColor = mat.GetColor("_BaseColor");
            BindColor(baseColorMember, baseColor, value => URPLitShaderModifier.SetBaseColor(mat, value));

            var normalScale = mat.GetFloat("_BumpScale");
            BindFloat(normalScaleMember, normalScale, value => mat.SetFloat("_BumpScale", value));

            var emsEnable = mat.IsKeywordEnabled("_EMISSION");
            emissionColorMember.transform.parent.gameObject.SetActive(emsEnable);
            BindBool(emissionMember, emsEnable, value =>
            {
                URPLitShaderModifier.SetEmission(mat, value);
                emissionColorMember.transform.parent.gameObject.SetActive(value);
            });

            var emsColor = mat.GetColor("_EmissionColor");
            BindColor(emissionColorMember, emsColor, value => mat.SetColor("_EmissionColor", value));

            var emsGIFlag = mat.globalIlluminationFlags;
            BindEnum(emissionFlagMember, (int)emsGIFlag, value => URPLitShaderModifier.SetEmissionMode(mat, (eEmissionGlobalIllumination)value));

            var baseTiling = mat.mainTextureScale;
            BindVector(baseTilingMember, baseTiling, value => URPLitShaderModifier.SetBaseTiling(mat, value));

            var baseOffset = mat.mainTextureOffset;
            BindVector(baseOffsetMember, baseOffset, value => URPLitShaderModifier.SetBaseOffset(mat, value));
        }

        public void BindBool(InspectableMember member, bool value, UnityAction<bool> onValueChanged)
        {
            member.CheckButton.onClick.RemoveAllListeners();
            member.CheckButton.onClick.AddListener(() =>
            {
                member.CheckMark.SetActive(!member.CheckMark.activeSelf);
                onValueChanged.Invoke(member.CheckMark.activeSelf);
            });
            member.CheckMark.SetActive(value);
        }

        public void BindColor(InspectableMember member, Color value, UnityAction<Color> onValueChanged)
        {
            member.ColorButton.onColorUpdated.RemoveAllListeners();
            member.ColorButton.color = value;
            member.ColorButton.onColorUpdated.AddListener(onValueChanged);
        }

        public void BindEnum(InspectableMember member, int value, UnityAction<int> onValueChanged)
        {
            member.DropDown.onValueChanged.RemoveAllListeners();
            member.DropDown.value = value;
            member.DropDown.onValueChanged.AddListener(onValueChanged);
        }

        public void BindFloat(InspectableMember member, float value, UnityAction<float> onValueChanged)
        {
            member.NumberInputField.onEndEdit.RemoveAllListeners();
            member.NumberInputField.text = value.ToString();
            member.NumberInputField.onEndEdit.AddListener(input =>
            {
                float value = float.Parse(input);
                if (member.Slider.gameObject.activeSelf)
                {
                    value = Mathf.Clamp(value, member.Slider.minValue, member.Slider.maxValue);
                }

                member.NumberInputField.SetTextWithoutNotify(value.ToString());
                onValueChanged.Invoke(value);
            });

            if (member.Slider.gameObject.activeSelf)
            {
                member.Slider.onValueChanged.RemoveAllListeners();
                member.Slider.value = value;
                member.Slider.onValueChanged.AddListener(value =>
                {
                    member.NumberInputField.SetTextWithoutNotify(value.ToString());
                    onValueChanged.Invoke(value);
                });
            }
        }

        public void BindVector(InspectableMember member, Vector2 value, UnityAction<Vector2> onValueChanged)
        {
            member.VectorValue = value;

            for (int i = 0; i < 2; i++)
            {
                int captured = i;
                member.VectorInputFields[i].onEndEdit.RemoveAllListeners();
                member.VectorInputFields[i].text = value[i].ToString();
                member.VectorInputFields[i].onEndEdit.AddListener(input =>
                {
                    float value = float.Parse(input.ToString());
                    member.VectorValue[captured] = value;

                    member.VectorInputFields[captured].SetTextWithoutNotify(value.ToString());
                    onValueChanged.Invoke(member.VectorValue);
                });
            }
        }

        public void ResetAsset()
        {
            modifier?.Reset();
        }
    }
}