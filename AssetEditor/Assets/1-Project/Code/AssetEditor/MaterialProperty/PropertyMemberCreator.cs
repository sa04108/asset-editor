using System;
using UnityEngine;
using UnityEngine.UI;

namespace Merlin
{
    public class PropertyMemberCreator : MonoBehaviour
    {
        //[SerializeField] private TexturePropertyMember textureMemberPreset;
        [SerializeField] private NumberPropertyMember numMemberPreset;

        [SerializeField] private ColorPropertyMember colorMemberPreset;
        [SerializeField] private VectorPropertyMember vectorMemberPreset;

        //[SerializeField] private MatrixPropertyMember matrixMemberPreset;
        [SerializeField] private BoolPropertyMember boolMemberPreset;

        [SerializeField] private EnumPropertyMember enumMemberPreset;

        [SerializeField] private PropertyGroupMember groupMemberPreset;
        [SerializeField] private Transform memberGroupPreset;

        private Transform pointer;
        private Transform lastPointerParent;

        private void Start()
        {
            //textureMemberPreset.gameObject.SetActive(false);
            numMemberPreset.gameObject.SetActive(false);
            colorMemberPreset.gameObject.SetActive(false);
            vectorMemberPreset.gameObject.SetActive(false);
            //matrixMemberPreset.gameObject.SetActive(false);
            boolMemberPreset.gameObject.SetActive(false);
            enumMemberPreset.gameObject.SetActive(false);

            groupMemberPreset.gameObject.SetActive(false);
            memberGroupPreset.gameObject.SetActive(false);
        }

        // 인스펙터 바인딩을 위한 포인터
        public void SetBindingPointer(Transform parent)
        {
            pointer = GetChild(parent, 0);
            lastPointerParent = parent;
        }

        private Transform BindOrInstantiate(Transform prefab, Transform parent)
        {
            if (lastPointerParent == parent.parent)
            {
                pointer = GetChild(parent, 0);
            }
            else if (parent == lastPointerParent.parent)
            {
                pointer = GetChild(parent, lastPointerParent.GetSiblingIndex() + 1);
            }

            lastPointerParent = parent;

            var aliveMember = pointer;
            if (aliveMember == null)
            {
                aliveMember = Instantiate(prefab, parent);
            }

            pointer = GetChild(parent, aliveMember.transform.GetSiblingIndex() + 1);
            return aliveMember;
        }

        public Transform CreateGroupMember(string title, Transform parent, bool unfoldOnStart = true)
        {
            var member = BindOrInstantiate(groupMemberPreset.transform, parent).GetComponent<PropertyGroupMember>();
            member.Initialize(title);
            member.gameObject.SetActive(true);

            var memberGroup = BindOrInstantiate(memberGroupPreset, parent);
            member.Button.onClick.AddListener(() => memberGroup.gameObject.SetActive(!memberGroup.gameObject.activeSelf));
            memberGroup.gameObject.SetActive(unfoldOnStart);

            LayoutRebuilder.ForceRebuildLayoutImmediate(parent.GetComponent<RectTransform>());

            return memberGroup;
        }

        public Transform CreateSubGroup<T>(MaterialPropertyMember<T> parentMember, Func<T, bool> setActive, bool unfoldOnStart = true)
        {
            var memberGroup = BindOrInstantiate(memberGroupPreset, parentMember.transform.parent).gameObject;
            memberGroup.SetActive(unfoldOnStart);
            parentMember.OnValueChanged.AddListener(value =>
            {
                memberGroup.SetActive(setActive(value));
                LayoutRebuilder.ForceRebuildLayoutImmediate(parentMember.transform.parent.GetComponent<RectTransform>());
            });

            LayoutRebuilder.ForceRebuildLayoutImmediate(parentMember.transform.parent.GetComponent<RectTransform>());

            return memberGroup.transform;
        }

        //public TexturePropertyMember CreateTexturePropertyMember(string label, Material mat, Texture value, Transform parent, string propName)
        //{
        //    var member = BindOrInstantiate(textureMemberPreset.transform, parent).GetComponent<TexturePropertyMember>();
        //    member.Initialize(label, mat, value, propName);
        //    member.gameObject.SetActive(true);

        //    return member;
        //}

        private NumberPropertyMember CreateNumberMember(string label, Material mat, MaterialPropertyType type, float value, float min, float max, Transform parent, string propName)
        {
            var member = BindOrInstantiate(numMemberPreset.transform, parent).GetComponent<NumberPropertyMember>();
            member.Initialize(label, mat, type, value, min, max, propName);
            member.gameObject.SetActive(true);

            return member;
        }

        private NumberPropertyMember CreateNumberMember(string label, Material mat, MaterialPropertyType type, float value, Transform parent, string propName)
        {
            var member = BindOrInstantiate(numMemberPreset.transform, parent).GetComponent<NumberPropertyMember>();
            member.Initialize(label, mat, type, value, propName);
            member.gameObject.SetActive(true);

            return member;
        }

        public NumberPropertyMember CreateFloatMember(string label, Material mat, float value, float min, float max, Transform parent, string propName)
        {
            return CreateNumberMember(label, mat, MaterialPropertyType.Float, value, min, max, parent, propName);
        }

        public NumberPropertyMember CreateFloatMember(string label, Material mat, float value, Transform parent, string propName)
        {
            return CreateNumberMember(label, mat, MaterialPropertyType.Float, value, parent, propName);
        }

        public NumberPropertyMember CreateIntMember(string label, Material mat, int value, int min, int max, Transform parent, string propName)
        {
            return CreateNumberMember(label, mat, MaterialPropertyType.Int, value, min, max, parent, propName);
        }

        public NumberPropertyMember CreateIntMember(string label, Material mat, int value, Transform parent, string propName)
        {
            return CreateNumberMember(label, mat, MaterialPropertyType.Int, value, parent, propName);
        }

        public ColorPropertyMember CreateColorMember(string label, Material mat, Color value, bool hasAlpha, bool isHDR, Transform parent, string propName)
        {
            var member = BindOrInstantiate(colorMemberPreset.transform, parent).GetComponent<ColorPropertyMember>();
            member.Initialize(label, mat, value, hasAlpha, isHDR, propName);
            member.gameObject.SetActive(true);

            return member;
        }

        public VectorPropertyMember CreateVectorMember(string label, Material mat, Vector2 value, Transform parent, string propName)
        {
            var member = BindOrInstantiate(vectorMemberPreset.transform, parent).GetComponent<VectorPropertyMember>();
            member.Initialize(label, mat, value, propName);
            member.gameObject.SetActive(true);

            return member;
        }

        //public MatrixPropertyMember CreateMatrixMember(string label, Material mat, Matrix4x4 value, Transform parent, string propName)
        //{
        //    var member = Instantiate(matrixMemberPreset, parent);
        //    member.Initialize(mat, propName, value);
        //    member.gameObject.SetActive(true);

        //    return member;
        //}

        public BoolPropertyMember CreateBoolMember(string label, Material mat, bool value, Transform parent, string propName)
        {
            var member = BindOrInstantiate(boolMemberPreset.transform, parent).GetComponent<BoolPropertyMember>();
            member.Initialize(label, mat, value, propName);
            member.gameObject.SetActive(true);

            return member;
        }

        public EnumPropertyMember CreateEnumMember(string label, Material mat, Type enumType, int value, Transform parent, string propName)
        {
            var member = BindOrInstantiate(enumMemberPreset.transform, parent).GetComponent<EnumPropertyMember>();
            member.Initialize(label, mat, enumType, value, propName);
            member.gameObject.SetActive(true);

            return member;
        }

        private Transform GetChild(Transform parent, int sibIndex)
        {
            if (sibIndex >= parent.childCount)
                return null;
            else
                return parent.GetChild(sibIndex);
        }
    }
}