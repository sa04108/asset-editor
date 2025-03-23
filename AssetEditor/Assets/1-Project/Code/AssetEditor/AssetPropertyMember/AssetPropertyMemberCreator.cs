using UnityEngine;

namespace Merlin
{
    public class AssetPropertyMemberCreator : MonoBehaviour
    {
        [SerializeField] private AssetTexturePropertyMember textureMemberPreset;
        [SerializeField] private AssetNumberPropertyMember numMemberPreset;
        [SerializeField] private AssetVectorPropertyMember vectorMemberPreset;
        [SerializeField] private AssetMatrixPropertyMember matrixMemberPreset;

        [SerializeField] private AssetGridMember gridMemberPreset;
        [SerializeField] private AssetPropertyGroupMember groupMemberPreset;
        [SerializeField] private Transform memberGroupPreset;
        [SerializeField] private FlexibleColorPicker colorPickerPreset;

        private void Start()
        {
            textureMemberPreset.gameObject.SetActive(false);
            numMemberPreset.gameObject.SetActive(false);
            vectorMemberPreset.gameObject.SetActive(false);
            matrixMemberPreset.gameObject.SetActive(false);

            gridMemberPreset.gameObject.SetActive(false);
            groupMemberPreset.gameObject.SetActive(false);
            memberGroupPreset.gameObject.SetActive(false);
            colorPickerPreset.gameObject.SetActive(false);
        }

        public Transform CreateGroupMember(Texture tex, string type, string name, Transform parent)
        {
            var member = Instantiate(groupMemberPreset, parent);
            member.Initialize(tex, type, name);
            member.gameObject.SetActive(true);

            var memberGroup = Instantiate(memberGroupPreset, parent);
            member.Button.onClick.AddListener(() => memberGroup.gameObject.SetActive(!memberGroup.gameObject.activeSelf));

            return memberGroup;
        }

        public AssetGridMember CreateGridMember(Texture[] texArr, Transform parent)
        {
            var member = Instantiate(gridMemberPreset, parent);
            member.Initialize(texArr);

            return member;
        }

        public AssetTexturePropertyMember CreateTexturePropertyMember(Material mat, AssetGridMember textureGrid, string name, Texture value, Transform parent)
        {
            var member = Instantiate(textureMemberPreset, parent);
            member.Initialize(mat, textureGrid, name, value);
            member.gameObject.SetActive(true);

            return member;
        }

        public AssetNumberPropertyMember CreateNumberMember(Material mat, MaterialPropertyType type, string name, float value, float min, float max, Transform parent)
        {
            var member = Instantiate(numMemberPreset, parent);
            member.Initialize(mat, type, name, value, min, max);
            member.gameObject.SetActive(true);

            return member;
        }

        public AssetNumberPropertyMember CreateNumberMember(Material mat, MaterialPropertyType type, string name, float value, Transform parent)
        {
            var member = Instantiate(numMemberPreset, parent);
            member.Initialize(mat, type, name, value);
            member.gameObject.SetActive(true);

            return member;
        }

        public AssetNumberPropertyMember CreateFloatMember(Material mat, string name, float value, float min, float max, Transform parent)
        {
            return CreateNumberMember(mat, MaterialPropertyType.Float, name, value, min, max, parent);
        }

        public AssetNumberPropertyMember CreateFloatMember(Material mat, string name, float value, Transform parent)
        {
            return CreateNumberMember(mat, MaterialPropertyType.Float, name, value, parent);
        }

        public AssetNumberPropertyMember CreateIntMember(Material mat, string name, int value, int min, int max, Transform parent)
        {
            return CreateNumberMember(mat, MaterialPropertyType.Int, name, value, min, max, parent);
        }

        public AssetNumberPropertyMember CreateIntMember(Material mat, string name, int value, Transform parent)
        {
            return CreateNumberMember(mat, MaterialPropertyType.Int, name, value, parent);
        }

        public AssetVectorPropertyMember CreateVectorMember(Material mat, string name, Vector4 value, bool isColor, Transform parent)
        {
            var member = Instantiate(vectorMemberPreset, parent);
            var fcp = Instantiate(colorPickerPreset, parent);
            member.Initialize(mat, fcp, name, isColor, value);
            member.gameObject.SetActive(true);

            return member;
        }

        public AssetMatrixPropertyMember CreateMatrixMember(Material mat, string name, Matrix4x4 value, Transform parent)
        {
            var member = Instantiate(matrixMemberPreset, parent);
            member.Initialize(mat, name, value);
            member.gameObject.SetActive(true);

            return member;
        }
    }
}