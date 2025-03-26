using GravityBox.ColorPicker;
using UnityEngine;

namespace Merlin
{
    public class ColorPropertyMember : MaterialPropertyMember<Color>
    {
        [SerializeField] private ColorButton colorButton;

        public void Initialize(Material mat, MaterialPropertyType type, string name, Color value, bool hasAlpha, bool isHDR)
        {
            base.Initialize(mat, MaterialPropertyType.Vector, name, value);

            colorButton.colorImage.hasAlpha = hasAlpha;
            colorButton.colorImage.isHDR = isHDR;
            colorButton.color = value;
            colorButton.onColorUpdated.AddListener(SetColor);

            SetColor(value);
        }

        private void SetColor(Color color)
        {
            currentValue = color;

            mat.SetColor(propertyName, color);
        }
    }
}