using GravityBox.UI;
using TMPro;
using UnityEngine;

namespace GravityBox.ColorPicker
{
    public class ColorPickerSlider : MonoBehaviour
    {
        public enum ColorMode { RGBA, RGBA01, HSV }
        public ColorMode colorMode = ColorMode.HSV;

        [SerializeField]
        private int colorChannel;

        [SerializeField]
        private ColorObject colorObject;
        [SerializeField]
        private SliderValue slider;
        [SerializeField]
        private TMP_Text label;
        [SerializeField]
        private Material material;

        private const string RGBlabels = "RGBA";
        private const string HSVlabels = "HSVA";
        private string[] RGBkeywords = new string[] { "_RGB", "_RGB", "_RGB" };
        private string[] HSVkeywords = new string[] { "_LINE", "_SAT", "_" };

        private void OnEnable()
        {
            UpdateControls(force:true);
            SwitchMode(colorMode);

            colorObject.onColorChanged += OnColorUpdated;
            slider.onValueChanged.AddListener(OnSliderValueChanged);
        }

        private void OnDisable()
        {
            colorObject.onColorChanged -= OnColorUpdated;
            slider.onValueChanged.RemoveListener(OnSliderValueChanged);
        }

        private void OnColorUpdated() => UpdateControls();

        /// <summary>
        /// Updating controls with HSV values (packed in Color variable for convinient conversions)
        /// Convertions only happen if we use RGB 0-255 or RGB 0-1 modes of color picker
        /// </summary>
        /// <param name="force">update slider value and position</param>
        /// <returns></returns>
        private Color UpdateControls(bool force = false)
        {
            Color color = colorObject.GetHSV();
            if (material != null)
                material.SetVector("_HSV", color);

            if (colorMode != ColorMode.HSV)
                color = colorObject.GetRGBA32();
            float value = color[colorChannel];

            if (force || slider.value != value)
                slider.SetValueWithoutNotify(value);

            return color;
        }

        /// <summary>
        /// Sliders here are always 0..1 but text input is converted to required value according to color mode
        /// </summary>
        /// <param name="mode"></param>
        private void SwitchMode(ColorMode mode)
        {
            colorMode = mode;
            switch (mode)
            {
                case ColorMode.RGBA:
                    slider.characterLimit = 3;
                    slider.valueType = SliderValue.ValueType.Integer;
                    slider.maxValue = 255;
                    label.text = RGBlabels[colorChannel].ToString();
                    break;
                case ColorMode.RGBA01:
                    slider.characterLimit = 4;
                    slider.valueType = SliderValue.ValueType.Float;
                    slider.maxValue = 1;
                    label.text = RGBlabels[colorChannel].ToString();
                    break;
                case ColorMode.HSV:
                    slider.characterLimit = 3;
                    slider.valueType = SliderValue.ValueType.Integer;
                    slider.maxValue = colorChannel == 0 ? 360 : 100;
                    label.text = HSVlabels[colorChannel].ToString();
                    break;
            }
            SwitchMaterialMode(mode);
            UpdateControls(force:true);
        }

        //slider background uses material with shader to display and update
        //correct color mode gradients
        private void SwitchMaterialMode(ColorMode mode)
        {
            if (colorChannel > 2) return;

            switch (mode)
            {
                case ColorMode.RGBA:
                    material.DisableKeyword(HSVkeywords[colorChannel]);
                    material.EnableKeyword(RGBkeywords[colorChannel]);
                    break;
                case ColorMode.RGBA01:
                    material.DisableKeyword(HSVkeywords[colorChannel]);
                    material.EnableKeyword(RGBkeywords[colorChannel]);
                    break;
                case ColorMode.HSV:
                    material.DisableKeyword(RGBkeywords[colorChannel]);
                    material.EnableKeyword(HSVkeywords[colorChannel]);
                    break;
            }
        }

        public void OnColorModeChanged(int mode) => SwitchMode((ColorMode)mode);
        private void OnSliderValueChanged(float value) => SetColorChannelValue(value);

        private void SetColorChannelValue(float value)
        {
            Color currentColor = colorMode == ColorMode.HSV ? colorObject.GetHSV() : (Color)colorObject.GetRGBA32();
            currentColor[colorChannel] = value;
            colorObject.SetHSV(colorMode == ColorMode.HSV ? currentColor : currentColor.ToHSV());
        }
    }
}