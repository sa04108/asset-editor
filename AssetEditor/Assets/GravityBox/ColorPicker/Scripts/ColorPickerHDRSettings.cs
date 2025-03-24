using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using GravityBox.UI;

namespace GravityBox.ColorPicker
{
    /// <summary>
    /// HDR Controls for Color picker, similar to what Unity Editor version has
    /// </summary>
    public class ColorPickerHDRSettings : MonoBehaviour
    {
        [SerializeField]
        private Image[] pickers;
        [SerializeField]
        private ColorObject colorObject;
        [SerializeField]
        private SliderValue slider;

        private void OnEnable()
        {
            OnColorChanged();

            colorObject.onColorChanged += OnColorChanged;
            slider.onValueChanged.AddListener(OnSliderIntensityChanged);
        }

        private void OnDisable()
        {
            colorObject.onColorChanged -= OnColorChanged;
            slider.onValueChanged.RemoveListener(OnSliderIntensityChanged);
        }

        /// <summary>
        /// Function for +2 +1 -1 -2 button shortcuts
        /// </summary>
        /// <param name="value"></param>
        public void AddIntensity(float value)
        {
            if (value != 0)
            {
                float intensity = colorObject.GetIntensity() + value;
                if (intensity <= 10 && intensity >= -10)
                    colorObject.SetIntensity(intensity);
            }
        }

        private void OnColorChanged() 
        {
            slider.SetValueWithoutNotify(colorObject.GetIntensity());
            RecalculateIntensityColors();
        }

        private void OnSliderIntensityChanged(float value)
        {
            if (value != colorObject.GetIntensity())
                colorObject.SetIntensity(value);
        }

        /// <summary>
        /// Calculate colors of shortcut buttons
        /// </summary>
        private void RecalculateIntensityColors()
        {
            Color rgb = colorObject.GetRGBA32();

            for (int index = 0; index < pickers.Length; index++)
            {
                Color hdr = rgb.ToHDRColor(colorObject.GetIntensity() + (index - 2));
                pickers[index].color = hdr;

                Graphic text = pickers[index].transform.GetChild(0).GetComponent<Graphic>();
                if (text != null)
                    text.color = hdr.GetLuminance() > 0.6f ? Color.black : Color.white;
            }
        }
    }
}