using GravityBox.ColorPicker;
using UnityEngine;

namespace Merlin
{
    public class ColorPropertyMember : MaterialPropertyMember<Color>
    {
        [SerializeField] private ColorButton colorButton;

        public void Initialize(string label, Material mat, Color value, bool hasAlpha, bool isHDR, string propName)
        {
            base.Initialize(label, mat, value, propName);

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

        public override void UpdateUI()
        {
            base.UpdateUI();

            colorButton.onColorUpdated.RemoveAllListeners();
            colorButton.color = CurrentValue;
            colorButton.onColorUpdated.AddListener(SetColor);
        }
    }
}