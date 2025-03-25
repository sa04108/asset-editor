using UnityEngine;
using UnityEngine.UI;

namespace Merlin
{
    public class ColorPropertyMember : MaterialPropertyMember<Color>
    {
        [SerializeField] private Button colorIconButton;

        private FlexibleColorPicker colorPicker;

        public void Initialize(Material mat, FlexibleColorPicker fcp, string name, Color value)
        {
            base.Initialize(mat, MaterialPropertyType.Vector, name, value);

            SetColorEditor(fcp, value);
        }

        private void SetColorEditor(FlexibleColorPicker fcp, Vector4 value)
        {
            colorPicker = fcp;
            colorPicker.color = value;
            colorPicker.onColorChange.AddListener(OnColorPick);
            colorIconButton.onClick.AddListener(ToggleColorPick);

            SetColor(value);
        }

        private void SetColor(Color color)
        {
            currentValue = color;
            colorIconButton.image.color = color;

            mat.SetColor(propertyName, color);
        }

        private void OnColorPick(Color color)
        {
            SetColor(color);
        }

        private void ToggleColorPick()
        {
            colorPicker.gameObject.SetActive(!colorPicker.gameObject.activeSelf);
            LayoutRebuilder.ForceRebuildLayoutImmediate(colorPicker.transform.parent.GetComponent<RectTransform>());
        }
    }
}