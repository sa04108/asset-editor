using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

namespace GravityBox.UI
{
    /// <summary>
    /// Compound control grouping Slider and editbox into a single element
    /// has two input modes "Normalized" and "Real Values". First value goes in 0..1 range, 
    /// comes out in same range BUT on the outside (for user) can be of any range: 0..255, 0..1, 0..100 or 0.360
    /// Second value goes in and comes out in raw range, for example -10..10, so it looks and works the same 
    /// Text output and text input is always in real range in both modes
    /// </summary>
    public class SliderValue : MonoBehaviour
    {
        public enum ValueType { Integer, Float }

        public ValueType valueType;
        public Slider slider;
        public InputField inputField;
        public Slider.SliderEvent onValueChanged = new Slider.SliderEvent();

        [SerializeField]
        private float _minValue;
        [SerializeField]
        private float _maxValue;
        [SerializeField]
        private bool _isNormalized;

        private float lastValidValue;

        public float value 
        {
            get => slider.value; 
            set => slider.value = Clamp(value);
        }

        public bool isNormalized 
        {
            get => _isNormalized; 
            set 
            {
                _isNormalized = value;
                slider.minValue = value ? 0 : minValue;
                slider.maxValue = value ? 1 : maxValue;
            } 
        }

        public float minValue { get => _minValue; set => _minValue = value; }
        public float maxValue { get => _maxValue; set => _maxValue = value; }
        public int characterLimit { get => inputField.characterLimit; set => inputField.characterLimit = value; }

        private void OnEnable()
        {
            lastValidValue = slider.value;
            slider.onValueChanged.AddListener(OnSliderValueChanged);
            inputField.SetTextWithoutNotify(ValueToString(slider.value));
            inputField.onValueChanged.AddListener(OnInputTextChanged);
            inputField.onEndEdit.AddListener(OnInputTextDone);
            inputField.onValidateInput += ValidateInput;
        }

        private void OnDisable()
        {
            slider.onValueChanged.RemoveListener(OnSliderValueChanged);
            inputField.onValueChanged.RemoveListener(OnInputTextChanged);
            inputField.onEndEdit.RemoveListener(OnInputTextDone);
            inputField.onValidateInput -= ValidateInput;
        }

        public void SetValueWithoutNotify(float value)
        {
            value = Clamp(value);
            slider.SetValueWithoutNotify(value);
            inputField.SetTextWithoutNotify(ValueToString(value));
        }

        private void OnSliderValueChanged(float value) => SetValueFromSlider(value);
        private void OnInputTextChanged(string value) => SetValueFromText(value);
        private void OnInputTextDone(string value) => AutocorrectInputText(value);

        //call this when value really changed
        private void ValueChangedEvent() => onValueChanged.Invoke(value);

        //do not allow non numerical input, also let "-" and "." only. all we need for correct input.
        //also decimal input filter does not work in slider so it is kind of a workaround
        private char ValidateInput(string text, int charIndex, char addedChar)
        {
            bool IsChar(char validChar)
            {
                return (addedChar.Equals(validChar) && text.IndexOf(addedChar) < 0);
            }

            if (valueType == ValueType.Integer)
                return char.IsDigit(addedChar) || IsChar('-') ? addedChar : '\0';
            else
            {
                if (addedChar.Equals(','))
                    addedChar = '.';

                return char.IsDigit(addedChar) || IsChar('-') || IsChar('.') ? addedChar : '\0';
            }
        }

        //update text from slider value both for normalized and real values
        private void SetValueFromSlider(float value)
        {
            lastValidValue = value;
            inputField.SetTextWithoutNotify(ValueToString(value));
            ValueChangedEvent();
        }

        //parse text to value, also auto handle "0." input, when parsing fails really
        //but in our case it will fallback to "0"
        private void SetValueFromText(string value)
        {
            float result = StringToValue(value);
            if (result != slider.value)
            {
                slider.SetValueWithoutNotify(result);
                ValueChangedEvent();
            }
        }

        //if text input is done and value gets out of bounds, this kicks in and fixes things for us
        private void AutocorrectInputText(string value)
        {
            float result = StringToValue(value);
            inputField.SetTextWithoutNotify(ValueToString(result));
        }

        //number to string correction according to current range and output
        private string ValueToString(float value)
        {
            lastValidValue = value;

            if (isNormalized)
                value = Mathf.Lerp(minValue, maxValue, value);

            return (valueType == ValueType.Integer) ? Mathf.RoundToInt(value).ToString() : value.ToString();
        }

        //parsing float value text to value
        private float StringToValue(string value)
        {
            if (float.TryParse(value, out float result))
            {
                if (isNormalized)
                    result = Mathf.InverseLerp(minValue, maxValue, result);
                
                result = Clamp(result);

                lastValidValue = result;
                return result;
            }

            return lastValidValue;
        }

        private float Clamp(float value) 
        {
            return isNormalized ? Mathf.Clamp01(value) : Mathf.Clamp(value, minValue, maxValue);
        }
    }
}