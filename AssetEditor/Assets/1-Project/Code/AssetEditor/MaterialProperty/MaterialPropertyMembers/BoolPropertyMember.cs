using UnityEngine;
using UnityEngine.Events;
using UnityEngine.UI;

namespace Merlin
{
    public class BoolPropertyMember : MaterialPropertyMember<bool>
    {
        [SerializeField] private Button checkButton;
        [SerializeField] private GameObject checkMark;

        public new void Initialize(string label, Material mat, bool value, string propName, UnityAction<bool> onValueChanged)
        {
            base.Initialize(label, mat, value, propName, onValueChanged);

            checkMark.SetActive(value);
            checkButton.onClick.AddListener(OnValueChanged);
        }

        private void OnValueChanged()
        {
            checkMark.SetActive(!checkMark.activeSelf);
            CurrentValue = checkMark.activeSelf;
            mat.SetInt(propertyName, CurrentValue ? 1 : 0);
        }

        public override void ResetProperty()
        {
            base.ResetProperty();

            checkMark.SetActive(CurrentValue);
        }
    }
}