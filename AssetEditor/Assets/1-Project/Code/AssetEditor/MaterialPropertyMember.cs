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

    public class MaterialPropertyMember : MonoBehaviour
    {
        public eMaterialPropertyType Type;

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