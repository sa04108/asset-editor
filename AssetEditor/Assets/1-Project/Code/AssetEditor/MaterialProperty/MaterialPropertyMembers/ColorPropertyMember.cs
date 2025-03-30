using GravityBox.ColorPicker;
using UnityEngine;
using UnityEngine.Events;

namespace Merlin
{
    public class ColorPropertyMember : MaterialPropertyMember<Color>
    {
        [SerializeField] private ColorButton colorButton;

        public void Initialize(string label, Material mat, Color value, bool hasAlpha, bool isHDR, string propName, UnityAction<Color> onValueChanged)
        {
            base.Initialize(label, mat, value, propName, onValueChanged);

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

        public override void ResetProperty()
        {
            base.ResetProperty();

            colorButton.onColorUpdated.RemoveAllListeners();
            colorButton.color = CurrentValue;
            colorButton.onColorUpdated.AddListener(SetColor);
        }
    }
}