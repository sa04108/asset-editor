using System;
using TMPro;
using UnityEngine;

namespace Merlin
{
    public class EnumPropertyMember : MaterialPropertyMember<int>
    {
        [SerializeField] private TMP_Dropdown dropDown;

        public void Initialize(Material mat, string name, Type enumType, int value)
        {
            base.Initialize(mat, name, value);

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