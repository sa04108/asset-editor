using System;
using TMPro;
using UnityEngine;
using UnityEngine.Events;

namespace Merlin
{
    public class EnumPropertyMember : MaterialPropertyMember<int>
    {
        [SerializeField] private TMP_Dropdown dropDown;

        public void Initialize(string label, Material mat, Type enumType, int value, string propName, UnityAction<int> onValueChanged)
        {
            base.Initialize(label, mat, value, propName, onValueChanged);

            dropDown.options = new();
            var values = Enum.GetNames(enumType);
            foreach (string v in values)
            {
                dropDown.options.Add(new TMP_Dropdown.OptionData(v));
            }

            dropDown.value = value;
            dropDown.onValueChanged.AddListener(OnValueChanged);
        }

        private void OnValueChanged(int value)
        {
            CurrentValue = value;
            mat.SetInt(propertyName, CurrentValue);
        }

        public override void ResetProperty()
        {
            base.ResetProperty();

            dropDown.SetValueWithoutNotify(CurrentValue);
        }
    }
}