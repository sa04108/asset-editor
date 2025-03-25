using UnityEngine;
using GB = GravityBox.ColorPicker;

namespace Merlin
{
    public class RuntimeColorPicker : RuntimeWindow<Color>
    {
        private static GB.ColorPickerWindow cpInstance;

        public static Color Color
        {
            get => cpInstance.Color;
            set => cpInstance.Color = value;
        }

        [SerializeField] private GB.ColorPickerWindow colorPicker;

        protected override void Start()
        {
            base.Start();

            cpInstance = colorPicker;
            cpInstance.onColorUpdated += color => onValueChanged.Invoke(color);
        }
    }
}