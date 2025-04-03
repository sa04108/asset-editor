using System;
using UnityEngine;
using UnityEngine.UI;

namespace Merlin
{
    public class PropertyMemberCreator : MonoBehaviour
    {
        [SerializeField] private MaterialMember materialMemberPreset;
        [SerializeField] private TexturePropertyMember textureMemberPreset;
        [SerializeField] private NumberPropertyMember numMemberPreset;
        [SerializeField] private ColorPropertyMember colorMemberPreset;
        [SerializeField] private VectorPropertyMember vectorMemberPreset;
        [SerializeField] private BoolPropertyMember boolMemberPreset;
        [SerializeField] private EnumPropertyMember enumMemberPreset;
        [SerializeField] private PropertyGroupMember groupMemberPreset;
        [SerializeField] private Transform memberGroupPreset;

        private Transform pointer;
        private Transform lastPointerParent;

        // 인스펙터 바인딩을 위한 포인터
        public void SetBindingPointer(Transform parent)
        {
            pointer = GetChild(parent, 0);
            lastPointerParent = parent;
        }

        private T BindOrInstantiate<T>(T prefab, Transform parent) where T : Component
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

            var aliveMember = pointer?.GetComponent<T>();
            if (aliveMember == null)
            {
                aliveMember = Instantiate(prefab, parent);
            }

            pointer = GetChild(parent, aliveMember.transform.GetSiblingIndex() + 1);
            return aliveMember;
        }

        public Transform CreateGroupMember(string title, Transform parent, bool unfoldOnStart = true)
        {
            var member = BindOrInstantiate(groupMemberPreset, parent);
            member.Initialize(title);

            var memberGroup = BindOrInstantiate(memberGroupPreset, parent);
            member.OnClick.AddListener(() => memberGroup.gameObject.SetActive(!memberGroup.gameObject.activeSelf));
            memberGroup.gameObject.SetActive(unfoldOnStart);

            LayoutRebuilder.ForceRebuildLayoutImmediate(parent.GetComponent<RectTransform>());

            return memberGroup;
        }

        public Transform CreateSubGroup<T>(MaterialPropertyMember<T> baseMember, Func<T, bool> setActive, Transform parent, bool unfoldOnStart = true)
        {
            var memberGroup = BindOrInstantiate(memberGroupPreset, parent).gameObject;
            memberGroup.SetActive(unfoldOnStart);
            baseMember.OnValueChanged.AddListener(value =>
            {
                memberGroup.SetActive(setActive(value));
                LayoutRebuilder.ForceRebuildLayoutImmediate(parent.GetComponent<RectTransform>());
            });

            LayoutRebuilder.ForceRebuildLayoutImmediate(parent.GetComponent<RectTransform>());

            return memberGroup.transform;
        }

        public MaterialMember CreateMaterialMember(string label, Material mat, Transform parent)
        {
            var member = Instantiate(materialMemberPreset, parent);
            member.Initialize(label, mat);

            return member;
        }

        public TexturePropertyMember CreateTexturePropertyMember(string label, Material mat, Texture value, Transform parent, string propName)
        {
            var member = BindOrInstantiate(textureMemberPreset, parent);
            member.Initialize(label, mat, value, propName);
            member.gameObject.SetActive(true);

            return member;
        }

        private NumberPropertyMember CreateNumberMember(string label, Material mat, MaterialPropertyType type, float value, float min, float max, Transform parent, string propName)
        {
            var member = BindOrInstantiate(numMemberPreset, parent);
            member.Initialize(label, mat, type, value, min, max, propName);

            return member;
        }

        private NumberPropertyMember CreateNumberMember(string label, Material mat, MaterialPropertyType type, float value, Transform parent, string propName)
        {
            var member = BindOrInstantiate(numMemberPreset, parent);
            member.Initialize(label, mat, type, value, propName);

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
            var member = BindOrInstantiate(colorMemberPreset, parent);
            member.Initialize(label, mat, value, hasAlpha, isHDR, propName);

            return member;
        }

        public VectorPropertyMember CreateVectorMember(string label, Material mat, Vector2 value, Transform parent, string propName)
        {
            var member = BindOrInstantiate(vectorMemberPreset, parent);
            member.Initialize(label, mat, value, propName);

            return member;
        }

        public BoolPropertyMember CreateBoolMember(string label, Material mat, bool value, Transform parent, string propName)
        {
            var member = BindOrInstantiate(boolMemberPreset, parent);
            member.Initialize(label, mat, value, propName);

            return member;
        }

        public EnumPropertyMember CreateEnumMember(string label, Material mat, Type enumType, int value, Transform parent, string propName)
        {
            var member = BindOrInstantiate(enumMemberPreset, parent);
            member.Initialize(label, mat, enumType, value, propName);

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