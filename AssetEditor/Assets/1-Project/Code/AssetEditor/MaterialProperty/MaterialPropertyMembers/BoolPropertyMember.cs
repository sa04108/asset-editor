using UnityEngine;
using UnityEngine.UI;

namespace Merlin
{
    public class BoolPropertyMember : MaterialPropertyMember<bool>
    {
        [SerializeField] private Button checkButton;
        [SerializeField] private GameObject checkMark;

        public new void Initialize(string label, Material mat, bool value, string propName)
        {
            base.Initialize(label, mat, value, propName);

            checkMark.SetActive(value);
            checkButton.onClick.AddListener(OnClick);
        }

        private void OnClick()
        {
            checkMark.SetActive(!checkMark.activeSelf);
            CurrentValue = checkMark.activeSelf;
            mat.SetInt(propertyName, CurrentValue ? 1 : 0);
        }

        public override void UpdateUI()
        {
            base.UpdateUI();

            checkMark.SetActive(CurrentValue);
        }
    }
}