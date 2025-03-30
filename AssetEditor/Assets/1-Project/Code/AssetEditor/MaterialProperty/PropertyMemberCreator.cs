using System;
using UnityEngine;
using UnityEngine.Events;

namespace Merlin
{
    public class PropertyMemberCreator : MonoBehaviour
    {
        [SerializeField] private TexturePropertyMember textureMemberPreset;
        [SerializeField] private NumberPropertyMember numMemberPreset;

        [SerializeField] private ColorPropertyMember colorMemberPreset;
        [SerializeField] private VectorPropertyMember vectorMemberPreset;
        [SerializeField] private MatrixPropertyMember matrixMemberPreset;
        [SerializeField] private BoolPropertyMember boolMemberPreset;
        [SerializeField] private EnumPropertyMember enumMemberPreset;

        [SerializeField] private PropertyGroupMember groupMemberPreset;
        [SerializeField] private Transform memberGroupPreset;

        private void Start()
        {
            textureMemberPreset.gameObject.SetActive(false);
            numMemberPreset.gameObject.SetActive(false);
            colorMemberPreset.gameObject.SetActive(false);
            vectorMemberPreset.gameObject.SetActive(false);
            matrixMemberPreset.gameObject.SetActive(false);
            boolMemberPreset.gameObject.SetActive(false);
            enumMemberPreset.gameObject.SetActive(false);

            groupMemberPreset.gameObject.SetActive(false);
            memberGroupPreset.gameObject.SetActive(false);
        }

        public Transform CreateGroupMember(string title, Transform parent, bool foldOnStart = false)
        {
            var member = Instantiate(groupMemberPreset, parent);
            member.Initialize(title);
            member.gameObject.SetActive(true);

            var memberGroup = Instantiate(memberGroupPreset, parent);
            member.Button.onClick.AddListener(() => memberGroup.gameObject.SetActive(!memberGroup.gameObject.activeSelf));
            memberGroup.gameObject.SetActive(!foldOnStart);

            return memberGroup;
        }

        public TexturePropertyMember CreateTexturePropertyMember(string label, Material mat, Texture value, Transform parent, string propName, UnityAction<Texture> onValueChanged = null)
        {
            var member = Instantiate(textureMemberPreset, parent);
            member.Initialize(label, mat, value, propName, onValueChanged);
            member.gameObject.SetActive(true);

            return member;
        }

        private NumberPropertyMember CreateNumberMember(string label, Material mat, MaterialPropertyType type, float value, float min, float max, Transform parent, string propName, UnityAction<float> onValueChanged)
        {
            var member = Instantiate(numMemberPreset, parent);
            member.Initialize(label, mat, type, value, min, max, propName, onValueChanged);
            member.gameObject.SetActive(true);

            return member;
        }

        private NumberPropertyMember CreateNumberMember(string label, Material mat, MaterialPropertyType type, float value, Transform parent, string propName, UnityAction<float> onValueChanged)
        {
            var member = Instantiate(numMemberPreset, parent);
            member.Initialize(label, mat, type, value, propName, onValueChanged);
            member.gameObject.SetActive(true);

            return member;
        }

        public NumberPropertyMember CreateFloatMember(string label, Material mat, float value, float min, float max, Transform parent, string propName, UnityAction<float> onValueChanged = null)
        {
            return CreateNumberMember(label, mat, MaterialPropertyType.Float, value, min, max, parent, propName, onValueChanged);
        }

        public NumberPropertyMember CreateFloatMember(string label, Material mat, float value, Transform parent, string propName, UnityAction<float> onValueChanged = null)
        {
            return CreateNumberMember(label, mat, MaterialPropertyType.Float, value, parent, propName, onValueChanged);
        }

        public NumberPropertyMember CreateIntMember(string label, Material mat, int value, int min, int max, Transform parent, string propName, UnityAction<float> onValueChanged = null)
        {
            return CreateNumberMember(label, mat, MaterialPropertyType.Int, value, min, max, parent, propName, onValueChanged);
        }

        public NumberPropertyMember CreateIntMember(string label, Material mat, int value, Transform parent, string propName, UnityAction<float> onValueChanged = null)
        {
            return CreateNumberMember(label, mat, MaterialPropertyType.Int, value, parent, propName, onValueChanged);
        }

        public ColorPropertyMember CreateColorMember(string label, Material mat, Color value, bool hasAlpha, bool isHDR, Transform parent, string propName, UnityAction<Color> onValueChanged = null)
        {
            var member = Instantiate(colorMemberPreset, parent);
            member.Initialize(label, mat, value, hasAlpha, isHDR, propName, onValueChanged);
            member.gameObject.SetActive(true);

            return member;
        }

        public VectorPropertyMember CreateVectorMember(string label, Material mat, Vector4 value, Transform parent, string propName, UnityAction<Vector4> onValueChanged = null)
        {
            var member = Instantiate(vectorMemberPreset, parent);
            member.Initialize(label, mat, value, propName, onValueChanged);
            member.gameObject.SetActive(true);

            return member;
        }

        public MatrixPropertyMember CreateMatrixMember(string label, Material mat, Matrix4x4 value, Transform parent, string propName)
        {
            var member = Instantiate(matrixMemberPreset, parent);
            member.Initialize(mat, propName, value);
            member.gameObject.SetActive(true);

            return member;
        }

        public BoolPropertyMember CreateBoolMember(string label, Material mat, bool value, Transform parent, string propName, UnityAction<bool> onValueChanged = null)
        {
            var member = Instantiate(boolMemberPreset, parent);
            member.Initialize(label, mat, value, propName, onValueChanged);
            member.gameObject.SetActive(true);

            return member;
        }

        public EnumPropertyMember CreateEnumMember(string label, Material mat, Type enumType, int value, Transform parent, string propName, UnityAction<int> onValueChanged = null)
        {
            var member = Instantiate(enumMemberPreset, parent);
            member.Initialize(label, mat, enumType, value, propName, onValueChanged);
            member.gameObject.SetActive(true);

            return member;
        }
    }
}