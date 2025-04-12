using GravityBox.ColorPicker;
using Sirenix.OdinInspector;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace Merlin
{
    public enum eInspectableType
    {
        Bool,
        Color,
        Enum,
        Number,
        Vector
    }

    public class InspectableMember : MonoBehaviour
    {
        public eInspectableType InspectorableType;

        [ShowIf("InspectorableType", eInspectableType.Bool)]
        public Button CheckButton;

        [ShowIf("InspectorableType", eInspectableType.Bool)]
        public GameObject CheckMark;

        [ShowIf("InspectorableType", eInspectableType.Color)]
        public ColorButton ColorButton;

        [ShowIf("InspectorableType", eInspectableType.Enum)]
        public TMP_Dropdown DropDown;

        [ShowIf("InspectorableType", eInspectableType.Number)]
        public TMP_InputField NumberInputField;

        [ShowIf("InspectorableType", eInspectableType.Number)]
        public Slider Slider;

        [ShowIf("InspectorableType", eInspectableType.Vector)]
        public TMP_InputField[] VectorInputFields;

        [ShowIf("InspectorableType", eInspectableType.Vector)]
        public Vector2 VectorValue;
    }
}