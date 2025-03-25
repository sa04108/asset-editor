using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace GravityBox.ColorPicker
{
    /// <summary>
    /// Main Color Picker Window
    /// </summary>
    public class ColorPickerWindow : MonoBehaviour
    {
        public System.Action<Color> onColorUpdated;

        [SerializeField]
        private TMP_InputField hexInputField;
        [SerializeField]
        private Image newColor;
        [SerializeField]
        private Image oldColor;
        [SerializeField]
        private RawImage captureContainer;

        [SerializeField]
        private ColorObject colorObject;
        [SerializeField]
        private GameObject alpha;
        [SerializeField]
        private GameObject hdrSettings;

        private bool _showHDR;
        private bool _showAlpha;

        public bool showHDR 
        {
            get => _showHDR; 
            set 
            {
                _showHDR = value; 
                colorObject.isHDR = value; 
                hdrSettings.SetActive(value); 
            }
        }

        public bool showAlpha 
        {
            get => _showAlpha; 
            set 
            {
                _showAlpha = value;
                colorObject.hasAlpha = value;
                alpha.SetActive(value); 
            }
        }

        /// <summary>
        /// RGB color is a little slower because of conversion, 
        /// so if you need HSV value actually, get it directly from property below
        /// </summary>
        public Color Color
        {
            get => colorObject.GetRGBA();
            set => colorObject.SetRGBA(value);
        }

        public Color HSV
        {
            get => colorObject.GetHSV();
            set => colorObject.SetHSV(value);
        }

        private void OnEnable()
        {
            UpdateColorsAndText(true);

            colorObject.onColorChanged += OnColorChanged;
            hexInputField.onEndEdit.AddListener(OnColorTextChanged);
        }

        private void OnDisable()
        {
            colorObject.onColorChanged -= OnColorChanged;
            hexInputField.onEndEdit.RemoveListener(OnColorTextChanged);

            onColorUpdated = null;
        }
       
        public void Show(Color rgb, bool hasAlpha, bool isHDR)
        {
            showAlpha = hasAlpha;
            showHDR = isHDR;

            colorObject.SetRGBA(rgb);
            UpdateColorsAndText(true);
        }

        public void PickColor()
        {
            ScreenPixelCaptureTool captureTool = new ScreenPixelCaptureTool();
            captureTool.PickColor(this, captureContainer, colorObject.GetRGBA(), (result) => colorObject.SetRGBA(result));
        }

        void OnColorChanged()
        {
            UpdateColorsAndText(false);
            onColorUpdated?.Invoke(colorObject.GetRGBA());
        }

        void OnColorTextChanged(string hex)
        {
            hex = hex.StartsWith("#") ? hex : "#" + hex;
            if (ColorUtility.TryParseHtmlString(hex, out Color c))
            {
                //assigning Color directly will reset intensity value,
                //since Color is considered source of intensity value by ColorObject
                //casting to Color32 (which can't have HDR value) considered as changing
                //underlying color, so intensity is not shanged,
                //if you wish to reset intensity when hex text is changed just remove casting
                colorObject.SetRGBA((Color32)c);
            }
            else
            {
                //if color is not parsed correctly it will restore to
                //last color available and should not call events here
                UpdateColorsAndText(false);
            }
        }

        //applying color to previews here
        void SetColor(Image image, Color rgb)
        {
            image.color = showAlpha ? rgb : new Color(rgb.r, rgb.g, rgb.b, 1);
        }

        //hex text shows only non-exposured color value hence RGBA32
        //color previews dislay exposured color which is RGBA value of colorObject
        void UpdateColorsAndText(bool updateOld)
        {
            Color rgb = colorObject.GetRGBA32(); //color value without exposure
            hexInputField.SetTextWithoutNotify(ColorUtility.ToHtmlStringRGB(rgb));

            SetColor(newColor, colorObject.GetRGBA()); //color WITH exposure
            if (updateOld)
                SetColor(oldColor, colorObject.GetRGBA()); //color WITH exposure
        }
    }
}