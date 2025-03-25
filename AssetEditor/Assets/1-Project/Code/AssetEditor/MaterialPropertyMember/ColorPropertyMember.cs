using UnityEngine;
using UnityEngine.UI;

namespace Merlin
{
    public class ColorPropertyMember : MaterialPropertyMember<Color>
    {
        [SerializeField] private Button colorIconButton;

        public override void Initialize(Material mat, MaterialPropertyType type, string name, Color value)
        {
            base.Initialize(mat, MaterialPropertyType.Vector, name, value);

            colorIconButton.onClick.AddListener(() =>
            {
                RuntimeColorPicker.SetOwner(transform, SetColor);
                RuntimeColorPicker.Color = currentValue;
            });

            SetColor(value);
        }

        private void SetColor(Color color)
        {
            currentValue = color;
            colorIconButton.image.color = color;

            mat.SetColor(propertyName, color);
        }
    }
}