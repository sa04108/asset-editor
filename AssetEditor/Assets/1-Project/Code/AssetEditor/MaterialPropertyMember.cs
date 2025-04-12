using GravityBox.ColorPicker;
using Sirenix.OdinInspector;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace Merlin
{
    public enum eMaterialPropertyType
    {
        Bool,
        Color,
        Enum,
        Float,
        Vector
    }

    /// <summary>
    /// 인스펙터 창에 표시되는 Material Property 항목
    /// 이 클래스는 UI 링크만 제공하며 실제 UI의 변경이나 조작은 AssetInspector에서 수행
    /// </summary>
    public class MaterialPropertyMember : MonoBehaviour
    {
        public eMaterialPropertyType Type;

        // ShowIf Attribute를 통해 지정된 타입에 대해서만 유니티 인스펙터에 노출시킴
        [ShowIf("Type", eMaterialPropertyType.Bool)]
        public Button CheckButton;

        [ShowIf("Type", eMaterialPropertyType.Bool)]
        public GameObject CheckMark;

        [ShowIf("Type", eMaterialPropertyType.Color)]
        public ColorButton ColorButton;

        [ShowIf("Type", eMaterialPropertyType.Enum)]
        public TMP_Dropdown DropDown;

        [ShowIf("Type", eMaterialPropertyType.Float)]
        public TMP_InputField FloatInputField;

        [ShowIf("Type", eMaterialPropertyType.Float)]
        public Slider Slider;

        [ShowIf("Type", eMaterialPropertyType.Vector)]
        public TMP_InputField[] VectorInputFields;

        [ShowIf("Type", eMaterialPropertyType.Vector)]
        public Vector2 VectorValue;
    }
}