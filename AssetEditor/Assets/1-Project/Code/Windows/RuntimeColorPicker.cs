using UnityEngine;
using GB = GravityBox.ColorPicker;

namespace Merlin
{
    public class RuntimeColorPicker : RuntimeWindow<Color>
    {
        private GB.ColorPickerWindow cpInstance;

        public static Color Color
        {
            get => ((RuntimeColorPicker)instance).cpInstance.Color;
            set => ((RuntimeColorPicker)instance).cpInstance.Color = value;
        }

        [SerializeField] private GB.ColorPickerWindow colorPicker;

        private void Awake()
        {
            cpInstance = colorPicker;
        }

        private void OnEnable()
        {
            // ColorPicker 내부 코드에 의해 Enable마다 새로 Action을 넣어줘야 한다.
            cpInstance.onColorUpdated += color => onValueChanged.Invoke(color);
        }
    }
}