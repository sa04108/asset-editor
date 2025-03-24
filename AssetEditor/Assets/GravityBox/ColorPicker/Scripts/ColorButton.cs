using UnityEngine;
using UnityEngine.Events;
using UnityEngine.EventSystems;

namespace GravityBox.ColorPicker
{
    /// <summary>
    /// Color Picker call button can be RGBA, noAlpha (RGB) color, HDR color (alpha and no)
    /// handles converting HDR color back to color + exposure values
    /// uses and manipulates special ColorImage Graphic component to show alpha value similar to how Unity Editor does it,
    /// same for HDR colors
    /// </summary>
    public class ColorButton : MonoBehaviour, IPointerClickHandler
    {
        [System.Serializable]
        public class ColorEvent : UnityEvent<Color> { }
        public ColorEvent onColorUpdated = new ColorEvent();

        [SerializeField]
        private ColorPickerManager colorPicker;
        [SerializeField]
        private ColorImage _colorImage;

        private GameObject _pickColorButton;

        public Color color
        {
            get => colorImage == null ? Color.black : colorImage.colorValue;
            set
            {
                if (!hasAlpha)
                    colorImage.colorValue = new Color(value.r, value.g, value.b, 1);
                else
                    colorImage.colorValue = value;

                onColorUpdated.Invoke(value);
            }
        }

        public bool hasAlpha => colorImage == null ? false : colorImage.hasAlpha;
        public bool isHDR => colorImage == null ? false : colorImage.isHDR;

        public ColorImage colorImage 
        {
            get 
            {
                if (_colorImage == null)
                    _colorImage = transform.GetChild(0).GetComponent<ColorImage>();

                return _colorImage;
            }
        }

        public GameObject pickColorButton
        {
            get 
            {
                if(_pickColorButton == null)
                    _pickColorButton = transform.GetChild(1).gameObject;

                return _pickColorButton;
            }
        }

        public void OnPointerClick(PointerEventData eventData)
        {
            GameObject clicked = eventData.pointerCurrentRaycast.gameObject;
            if (clicked == colorImage.gameObject)
                colorPicker.Show(this);
            else if (clicked == pickColorButton)
                StartPixelCapture();
        }

        void StartPixelCapture()
        {
            ScreenPixelCaptureTool captureTool = new ScreenPixelCaptureTool();
            captureTool.PickColor(this, colorImage, color, SetColor);
            colorImage.StartCapture();
        }

        void SetColor(Color color) { this.color = color; colorImage.StopCapture(); } 
    }
}