using GravityBox.ColorPicker;
using UnityEngine;

namespace Merlin
{
    public class ColorPropertyMember : MaterialPropertyMember<Color>
    {
        [SerializeField] private ColorButton colorButton;

        public void Initialize(Material mat, string name, Color value, bool hasAlpha, bool isHDR)
        {
            base.Initialize(mat, name, value);

            colorButton.colorImage.hasAlpha = hasAlpha;
            colorButton.colorImage.isHDR = isHDR;
            colorButton.color = value;
            colorButton.onColorUpdated.AddListener(SetColor);
        }

        private void SetColor(Color color)
        {
            CurrentValue = color;

            mat.SetColor(propertyName, color);
        }
    }
}