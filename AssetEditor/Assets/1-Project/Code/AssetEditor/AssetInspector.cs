using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.UI;

namespace Merlin
{
    /// <summary>
    /// 에셋의 material property를 나열하고 사용자가 수정할 수 있도록 UI를 구성합니다.
    /// </summary>
    public class AssetInspector : MonoBehaviour
    {
        [Header("Links")]
        [SerializeField] private Transform materialParent;
        [SerializeField] private MaterialMember materialMemberPreset;
        [SerializeField] private GameObject content;
        [SerializeField] private Button resetButton;

        [Header("Inspectable Members")]
        [SerializeField] private MaterialPropertyMember workflowModeMember;
        [SerializeField] private MaterialPropertyMember surfaceMember;
        [SerializeField] private MaterialPropertyMember blendMember;
        [SerializeField] private MaterialPropertyMember renderFaceMember;
        [SerializeField] private MaterialPropertyMember alphaClippingMember;
        [SerializeField] private MaterialPropertyMember alphaCutoffMember;
        [SerializeField] private MaterialPropertyMember receiveShadowsMember;
        [SerializeField] private MaterialPropertyMember metallicMember;
        [SerializeField] private MaterialPropertyMember smoothnessMember;
        [SerializeField] private MaterialPropertyMember baseColorMember;
        [SerializeField] private MaterialPropertyMember normalScaleMember;
        [SerializeField] private MaterialPropertyMember emissionMember;
        [SerializeField] private MaterialPropertyMember emissionColorMember;
        [SerializeField] private MaterialPropertyMember emissionFlagMember;
        [SerializeField] private MaterialPropertyMember baseTilingMember;
        [SerializeField] private MaterialPropertyMember baseOffsetMember;

        private AssetModifier modifier;
        private Material selectedMaterial;

        private void Start()
        {
            ClearMembers(materialParent);
            content.SetActive(false);

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

            // asset modifier를 만듭니다.
            // asset modifier는 material 변경점을 저장하고 초기화 기능을 가지는 객체입니다.
            modifier = new(go);
            var materials = modifier.GetSharedMaterials();
            foreach (var material in materials)
            {
                // 각각의 shared material을 인스펙터에 표시합니다.
                var member = Instantiate(materialMemberPreset, materialParent);
                member.Initialize(material.name, material);
                member.OnClick.AddListener(InspectMaterial);
            }
        }

        private void InspectMaterial(Material mat)
        {
            selectedMaterial = mat;

            // 현재 Inspect 가능한 shader는 URP Lit 뿐입니다.
            if (mat.shader.name == "Universal Render Pipeline/Lit")
            {
                InspectLitMaterialProperties(mat);
            }
            else
            {
                // 이외의 shader는 표시하지 않습니다.
                Debug.LogWarning($"Shader {mat.shader.name} for current material is not supported");
            }
        }

        /// <summary>
        /// URP Lit Shader에 대해 각 material property들을 인스펙터에 표시합니다.
        /// 각각의 Bind함수를 통해 이미 scene에 배치된 member에 값을 바인딩합니다.
        /// </summary>
        /// <param name="mat"></param>
        private void InspectLitMaterialProperties(Material mat)
        {
            content.SetActive(true);

            var workflowMode = mat.GetInt("_WorkflowMode");
            // Metallic은 Workflow Mode가 metallic일 때 활성화
            metallicMember.gameObject.SetActive(workflowMode == (int)eShaderWorkflowMode.Metallic);
            BindEnum(workflowModeMember, workflowMode, value =>
            {
                URPLitShaderModifier.SetWorkflowMode(mat, (eShaderWorkflowMode)value);
                metallicMember.gameObject.SetActive(value == (int)eShaderWorkflowMode.Metallic);
            });

            var surfaceType = mat.GetInt("_Surface");
            // Blending Mode는 Surface Type이 Transparent일 때 활성화
            blendMember.gameObject.SetActive(surfaceType == (int)eShaderSurfaceType.Transparent);
            BindEnum(surfaceMember, surfaceType, value =>
            {
                URPLitShaderModifier.SetSurfaceType(mat, (eShaderSurfaceType)value);
                blendMember.gameObject.SetActive(value == (int)eShaderSurfaceType.Transparent);
            });

            var blend = mat.GetInt("_Blend");
            BindEnum(blendMember, blend, value => URPLitShaderModifier.SetBlendMode(mat, (eBlendMode)value));

            var renderFace = mat.GetInt("_Cull");
            BindEnum(renderFaceMember, renderFace, value => URPLitShaderModifier.SetRenderFace(mat, (eShaderRenderFace)value));

            var alphaClipping = mat.GetInt("_AlphaClip");
            // Threshold는 Alpha Clip이 켜질 때만 활성화
            alphaCutoffMember.gameObject.SetActive(alphaClipping == 1);
            BindBool(alphaClippingMember, alphaClipping == 1, value =>
            {
                URPLitShaderModifier.SetAlphaClipping(mat, value);
                alphaCutoffMember.gameObject.SetActive(value);
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
            // Emission Color 및 GI Flag는 Emission이 켜질 때만 활성화
            emissionColorMember.gameObject.SetActive(emsEnable);
            emissionFlagMember.gameObject.SetActive(emsEnable);
            BindBool(emissionMember, emsEnable, value =>
            {
                URPLitShaderModifier.SetEmission(mat, value);
                emissionColorMember.gameObject.SetActive(value);
                emissionFlagMember.gameObject.SetActive(value);
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

        public void BindBool(MaterialPropertyMember member, bool value, UnityAction<bool> onValueChanged)
        {
            member.CheckButton.onClick.RemoveAllListeners();
            member.CheckButton.onClick.AddListener(() =>
            {
                member.CheckMark.SetActive(!member.CheckMark.activeSelf);
                onValueChanged.Invoke(member.CheckMark.activeSelf);
            });
            member.CheckMark.SetActive(value);
        }

        public void BindColor(MaterialPropertyMember member, Color value, UnityAction<Color> onValueChanged)
        {
            member.ColorButton.onColorUpdated.RemoveAllListeners();
            member.ColorButton.color = value;
            member.ColorButton.onColorUpdated.AddListener(onValueChanged);
        }

        public void BindEnum(MaterialPropertyMember member, int value, UnityAction<int> onValueChanged)
        {
            member.DropDown.onValueChanged.RemoveAllListeners();
            member.DropDown.value = value;
            member.DropDown.onValueChanged.AddListener(onValueChanged);
        }

        public void BindFloat(MaterialPropertyMember member, float value, UnityAction<float> onValueChanged)
        {
            member.FloatInputField.onEndEdit.RemoveAllListeners();
            member.FloatInputField.text = value.ToString();
            member.FloatInputField.onEndEdit.AddListener(input =>
            {
                float value = float.Parse(input);
                if (member.Slider.gameObject.activeSelf)
                {
                    value = Mathf.Clamp(value, member.Slider.minValue, member.Slider.maxValue);
                }

                member.FloatInputField.SetTextWithoutNotify(value.ToString());
                onValueChanged.Invoke(value);
            });

            if (member.Slider.gameObject.activeSelf)
            {
                member.Slider.onValueChanged.RemoveAllListeners();
                member.Slider.value = value;
                member.Slider.onValueChanged.AddListener(value =>
                {
                    member.FloatInputField.SetTextWithoutNotify(value.ToString());
                    onValueChanged.Invoke(value);
                });
            }
        }

        public void BindVector(MaterialPropertyMember member, Vector2 value, UnityAction<Vector2> onValueChanged)
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

        // asset modifier를 통해 에셋을 초기상태로 되돌립니다.
        // 현재는 material만 되돌립니다.
        public void ResetAsset()
        {
            modifier?.Reset();

            // 현재 선택된 material이 있다면 초기화된 material로 다시 inspect
            if (selectedMaterial != null)
                InspectMaterial(selectedMaterial);
        }
    }
}