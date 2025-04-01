using System;
using TMPro;
using UnityEngine;

namespace Merlin
{
    public class EnumPropertyMember : MaterialPropertyMember<int>
    {
        [SerializeField] private TMP_Dropdown dropDown;

        private void Start()
        {
            dropDown.onValueChanged.AddListener(OnSelected);
        }

        public void Initialize(string label, Material mat, Type enumType, int value, string propName)
        {
            base.Initialize(label, mat, value, propName);

            dropDown.options.Clear();
            var values = Enum.GetNames(enumType);
            foreach (string v in values)
            {
                dropDown.options.Add(new TMP_Dropdown.OptionData(v));
            }

            dropDown.SetValueWithoutNotify(value);
        }

        private void OnSelected(int value)
        {
            CurrentValue = value;
            mat.SetInt(propertyName, CurrentValue);
        }

        public override void UpdateUI()
        {
            base.UpdateUI();

            dropDown.SetValueWithoutNotify(CurrentValue);
        }
    }
}