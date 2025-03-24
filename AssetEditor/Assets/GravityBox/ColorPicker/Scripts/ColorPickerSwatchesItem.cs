using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

namespace GravityBox.ColorPicker
{
    /// <summary>
    /// Color preset behaviour
    /// </summary>
    public class ColorPickerSwatchesItem : MonoBehaviour, IPointerClickHandler
    {
        public System.Action<ColorPickerSwatchesItem> onClick;
        public System.Action<ColorPickerSwatchesItem> onContext;

        [SerializeField]
        private Image foreground;
        [SerializeField]
        private Material alphaMaterial;
        [SerializeField]
        private GameObject hdrText;

        private Color _color;
        private float _intensity;

        public Color color
        {
            get => _color;
            set
            {
                SetColor(value, intensity);
                _color = value;
            }
        }

        public float intensity
        {
            get => _intensity;
            set
            {
                SetColor(color, value);
                _intensity = value;
            }
        }

        public void OnPointerClick(PointerEventData eventData)
        {
            switch (eventData.button)
            {
                case PointerEventData.InputButton.Left:
                    onClick?.Invoke(this);
                    break;
                case PointerEventData.InputButton.Right:
                    onContext?.Invoke(this);
                    break;
            }
        }

        private void SetColor(Color color, float value)
        {
            Color c = color;
            if (value != 0)
                c = color.ToHDRColor(1f); //this will make difference but will not make color completely white in case intensity is high
            foreground.color = c;
            foreground.material = c.a < 1f ? alphaMaterial : null;
            hdrText.gameObject.SetActive(value > 0);
        }
    }
}