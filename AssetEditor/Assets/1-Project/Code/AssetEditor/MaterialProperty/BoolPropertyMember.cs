using UnityEngine;
using UnityEngine.UI;

namespace Merlin
{
    public class BoolPropertyMember : MaterialPropertyMember<bool>
    {
        [SerializeField] private Button checkButton;
        [SerializeField] private GameObject checkMark;

        public new void Initialize(Material mat, string name, bool value)
        {
            base.Initialize(mat, name, value);

            checkMark.SetActive(value);
            checkButton.onClick.AddListener(OnValueChanged);
        }

        private void OnValueChanged()
        {
            checkMark.SetActive(!checkMark.activeSelf);
            currentValue = checkMark.activeSelf;
            mat.SetInt(propertyName, currentValue ? 1 : 0);
        }
    }
}