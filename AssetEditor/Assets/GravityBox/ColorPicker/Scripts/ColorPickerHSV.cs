using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

namespace GravityBox.ColorPicker
{
    /// <summary>
    /// Handling HSV Ring and Box selection (clicking and dragging)
    /// commented out lastValidPosition is good to fix out of bounds clicks and drags
    /// but for now dragging mouse outside of control's visuals doesn't feels uncomfortable
    /// </summary>
    public class ColorPickerHSV : MonoBehaviour, IPointerDownHandler, IDragHandler
    {
        public enum Mode { ValueH, ValueSV }

        [SerializeField]
        private Mode mode = Mode.ValueH;
        [SerializeField]
        private ColorObject colorObject;

        private RectTransform _rectTransform;
        private RawImage _rawImage;

        //when mouse leaves ring
        //it will stuck to last valid cursor position
        //uncomment here if need this behaviour (also uncomment code in GetRelativeRingPosition)
        //private Vector2 _lastValidPosition;

        public RectTransform rectTransform
        {
            get
            {
                if (_rectTransform == null)
                    _rectTransform = GetComponent<RectTransform>();

                return _rectTransform;
            }
        }

        private RawImage rawImage
        {
            get
            {
                if (_rawImage == null)
                    _rawImage = GetComponent<RawImage>();

                return _rawImage;
            }
        }

        private void OnEnable()
        {
            OnColorUpdated();
            colorObject.onColorChanged += OnColorUpdated;
        }

        private void OnDisable()
        {
            colorObject.onColorChanged -= OnColorUpdated;
        }

        /// <summary>
        /// Calculating HSV value based on pointer position
        /// </summary>
        /// <param name="eventData"></param>
        public void OnDrag(PointerEventData eventData)
        {
            Vector2 position;
            Vector4 hsv = rawImage.material.GetVector("_HSV");
            switch (mode)
            {
                case Mode.ValueH:
                    position = GetRelativeRingPosition(eventData.position);
                    float value = Mathf.Atan2(position.y, position.x) * (1 / (Mathf.PI * 2));
                    if (value < 0)
                        value += 1f;
                    hsv.x = value;
                    break;
                case Mode.ValueSV:
                    position = GetRelativeBoxPosition(eventData.position);
                    hsv.y = position.x;
                    hsv.z = position.y;
                    break;
            }
            colorObject.SetHSV(hsv);
        }

        public void OnPointerDown(PointerEventData eventData)
        {
            OnDrag(eventData);
        }

        /// <summary>
        /// calculating quad relative coordinates based on pointer position for circular shape
        /// </summary>
        /// <param name="position">Pointer position</param>
        /// <returns>Relative coordinates (x,y)</returns>
        private Vector2 GetRelativeRingPosition(Vector2 position)
        {
            RectTransformUtility.ScreenPointToLocalPointInRectangle(rectTransform, position, null, out Vector2 result);
            result.x /= rectTransform.rect.width;
            result.y /= rectTransform.rect.height;

            //if (result.magnitude > 1.2f || result.magnitude < 1f - 0.15f) //rawImage.material.GetFloat("_Thickness"))
            //    result = _lastValidPosition;
            //else
            //    _lastValidPosition = result;

            return result.normalized;
        }

        /// <summary>
        /// calculating quad relative coordinates based on pointer position for box shape
        /// </summary>
        /// <param name="position">Pointer position</param>
        /// <returns>Relative coordinates (x,y)</returns>
        private Vector2 GetRelativeBoxPosition(Vector2 position)
        {
            RectTransformUtility.ScreenPointToLocalPointInRectangle(rectTransform, position, null, out Vector2 result);
            result.x /= rectTransform.rect.width;
            result.y /= rectTransform.rect.height;
            result += new Vector2(0.5f, 0.5f);
            float t = rawImage.material.GetFloat("_Thickness");
            result.x = Mathf.InverseLerp(t, 1f - t, result.x);
            result.y = Mathf.InverseLerp(t, 1f - t, result.y);

            return result;
        }

        //colorObject does auto convertion of current color to HSV RGBA etc
        //alpha channel which is shared between color modes
        void OnColorUpdated() 
        {
            rawImage.material.SetVector("_HSV", colorObject.GetHSV());
        }
    }
}
