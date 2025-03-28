using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace Merlin
{
    public class NumberPropertyMember : MaterialPropertyMember<float>
    {
        [SerializeField] private TMP_InputField inputField;
        [SerializeField] private Slider slider;

        [SerializeField] private Sprite numberBox;
        [SerializeField] private Sprite rangeBox;

        MaterialPropertyType type;

        private float CurrentValue
        {
            get => currentValue;
            set
            {
                currentValue = value;

                if (type == MaterialPropertyType.Float)
                {
                    mat.SetFloat(propertyName, value);
                }
                else if (type == MaterialPropertyType.Int)
                {
                    mat.SetInteger(propertyName, (int)value);
                }
            }
        }

        private void Start()
        {
            inputField.onEndEdit.AddListener(OnInputValueChanged);
            slider.onValueChanged.AddListener(OnSliderValueChanged);
        }

        public void Initialize(Material mat, MaterialPropertyType type, string name, float value, float min, float max)
        {
            base.Initialize(mat, name, value);

            this.type = type;
            inputField.SetTextWithoutNotify(value.ToString());
            inputField.image.sprite = rangeBox;

            if (type == MaterialPropertyType.Int)
                slider.wholeNumbers = true;

            slider.value = value;
            slider.minValue = min;
            slider.maxValue = max;
            slider.gameObject.SetActive(true);
        }

        public void Initialize(Material mat, MaterialPropertyType type, string name, float value)
        {
            base.Initialize(mat, name, value);

            this.type = type;
            inputField.SetTextWithoutNotify(value.ToString());
            inputField.image.sprite = numberBox;

            slider.gameObject.SetActive(false);
        }

        private void OnInputValueChanged(string value)
        {
            if (type == MaterialPropertyType.Float &&
                float.TryParse(value, out float fResult))
            {
                if (slider.gameObject.activeSelf)
                {
                    fResult = Mathf.Clamp(fResult, slider.minValue, slider.maxValue);
                    slider.SetValueWithoutNotify(fResult);
                }

                inputField.SetTextWithoutNotify(fResult.ToString());
                CurrentValue = fResult;
            }
            else if (type == MaterialPropertyType.Int &&
                int.TryParse(value, out int iResult))
            {
                if (slider.gameObject.activeSelf)
                {
                    iResult = Mathf.Clamp(iResult, (int)slider.minValue, (int)slider.maxValue);
                    slider.SetValueWithoutNotify(iResult);
                }

                inputField.SetTextWithoutNotify(iResult.ToString());
                CurrentValue = iResult;
            }
            else // 빈 값 입력 포함
            {
                inputField.SetTextWithoutNotify(CurrentValue.ToString());
            }
        }

        public void OnSliderValueChanged(float value)
        {
            CurrentValue = value;
            inputField.SetTextWithoutNotify(CurrentValue.ToString());
        }
    }
}