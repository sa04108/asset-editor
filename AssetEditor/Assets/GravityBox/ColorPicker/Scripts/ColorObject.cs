using UnityEngine;

namespace GravityBox.ColorPicker
{
    /// <summary>
    /// Object connecting various components of Color Picker without them knowing it
    /// Does hidden color conversions to avoid constantly reconvert colors from rgba to hsv and hdr etc 
    /// </summary>
    public class ColorObject : ScriptableObject
    {
        public event System.Action onColorChanged;

        public bool hasAlpha;
        public bool isHDR;

        [SerializeField]
        private Color32 color;
        [SerializeField]
        private float intensity;

        private Color currentHSV; // just a color in (Hue, Saturation, Value) format
        private Color32 currentRGBA32; // color with no exposure
        private Color currentRGBA; // is this hdr valid color value?
        private Color currentHDR; // then why it is here?
        
        public void SetRGBA(Color rgba)
        {
            if (((Color32)rgba).Equals(color))
                return;

            if (!hasAlpha)
                rgba.a = 1;

            if (isHDR)
            {
                rgba.DecomposeHDR(out color, out intensity);
                currentRGBA = color;
                currentRGBA32 = color;
                currentHSV = currentRGBA.ToHSV();
                currentHDR = rgba;
            }
            else
            {
                color = rgba;
                currentRGBA = rgba;
                currentRGBA32 = rgba;
                currentHSV = rgba.ToHSV();
                currentHDR = rgba;
            }

            ColorUpdatedEvent();
        }

        public void SetRGBA(Color32 rgba)
        {
            if (rgba.Equals(color))
                return;

            if (!hasAlpha)
                rgba.a = 255;

            //can't be HDR color
            color = rgba;
            currentRGBA = rgba;
            currentRGBA32 = rgba;
            currentHSV = currentRGBA.ToHSV();
            currentHDR = isHDR ? currentRGBA.ToHDRColor(intensity) : currentRGBA;

            ColorUpdatedEvent();
        }

        public void SetHSV(Color hsv)
        {
            if (hsv.Equals(currentHSV))
                return;

            if (!hasAlpha)
                hsv.a = 1;

            color = hsv.ToRGB();
            currentRGBA = color;
            currentRGBA32 = color;
            currentHSV = hsv;
            currentHDR = isHDR ? currentRGBA.ToHDRColor(intensity) : currentRGBA;

            ColorUpdatedEvent();
        }

        public void SetIntensity(float value)
        {
            if (value.Equals(intensity))
                return;

            intensity = isHDR ? value : 0;
            currentHDR = ((Color)color).ToHDRColor(value);

            ColorUpdatedEvent();
        }

        public float GetIntensity() => isHDR ? intensity : 0;

        public Color GetRGBA() => isHDR ? currentHDR : currentRGBA;
        public Color32 GetRGBA32() => currentRGBA32;
        public Color GetHSV() => currentHSV;
        public Color GetHDR() => currentHDR;

        void ColorUpdatedEvent() { onColorChanged?.Invoke(); }
    }
}