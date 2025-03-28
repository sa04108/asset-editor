using System;
using UnityEngine;

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

        public TexturePropertyMember CreateTexturePropertyMember(Material mat, string name, Texture value, Transform parent)
        {
            var member = Instantiate(textureMemberPreset, parent);
            member.Initialize(mat, name, value);
            member.gameObject.SetActive(true);

            return member;
        }

        public NumberPropertyMember CreateNumberMember(Material mat, MaterialPropertyType type, string name, float value, float min, float max, Transform parent)
        {
            var member = Instantiate(numMemberPreset, parent);
            member.Initialize(mat, type, name, value, min, max);
            member.gameObject.SetActive(true);

            return member;
        }

        public NumberPropertyMember CreateNumberMember(Material mat, MaterialPropertyType type, string name, float value, Transform parent)
        {
            var member = Instantiate(numMemberPreset, parent);
            member.Initialize(mat, type, name, value);
            member.gameObject.SetActive(true);

            return member;
        }

        public NumberPropertyMember CreateFloatMember(Material mat, string name, float value, float min, float max, Transform parent)
        {
            return CreateNumberMember(mat, MaterialPropertyType.Float, name, value, min, max, parent);
        }

        public NumberPropertyMember CreateFloatMember(Material mat, string name, float value, Transform parent)
        {
            return CreateNumberMember(mat, MaterialPropertyType.Float, name, value, parent);
        }

        public NumberPropertyMember CreateIntMember(Material mat, string name, int value, int min, int max, Transform parent)
        {
            return CreateNumberMember(mat, MaterialPropertyType.Int, name, value, min, max, parent);
        }

        public NumberPropertyMember CreateIntMember(Material mat, string name, int value, Transform parent)
        {
            return CreateNumberMember(mat, MaterialPropertyType.Int, name, value, parent);
        }

        public ColorPropertyMember CreateColorMember(Material mat, string name, Color value, bool hasAlpha, bool isHDR, Transform parent)
        {
            var member = Instantiate(colorMemberPreset, parent);
            member.Initialize(mat, name, value, hasAlpha, isHDR);
            member.gameObject.SetActive(true);

            return member;
        }

        public VectorPropertyMember CreateVectorMember(Material mat, string name, Vector4 value, Transform parent)
        {
            var member = Instantiate(vectorMemberPreset, parent);
            member.Initialize(mat, name, value);
            member.gameObject.SetActive(true);

            return member;
        }

        public MatrixPropertyMember CreateMatrixMember(Material mat, string name, Matrix4x4 value, Transform parent)
        {
            var member = Instantiate(matrixMemberPreset, parent);
            member.Initialize(mat, name, value);
            member.gameObject.SetActive(true);

            return member;
        }

        public BoolPropertyMember CreateBoolMember(Material mat, string name, bool value, Transform parent)
        {
            var member = Instantiate(boolMemberPreset, parent);
            member.Initialize(mat, name, value);
            member.gameObject.SetActive(true);

            return member;
        }

        public EnumPropertyMember CreateEnumMember(Material mat, string name, Type enumType, int value, Transform parent)
        {
            var member = Instantiate(enumMemberPreset, parent);
            member.Initialize(mat, name, enumType, value);
            member.gameObject.SetActive(true);

            return member;
        }
    }
}