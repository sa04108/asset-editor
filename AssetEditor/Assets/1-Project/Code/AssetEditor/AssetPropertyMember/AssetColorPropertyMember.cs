using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace Merlin
{
    public class AssetColorPropertyMember : MonoBehaviour
    {
        [SerializeField] private TMP_Text title;
        [SerializeField] private Button colorIconButton;

        private Material mat;
        private string propertyName;
        private Color currentValue;
        private FlexibleColorPicker colorPicker;

        private void Start()
        {
        }

        public void Initialize(Material mat, FlexibleColorPicker fcp, string name, Color value)
        {
            this.mat = mat;

            propertyName = name;
            title.text = $"{name}";
            currentValue = value;

            SetColorEditor(fcp, value);
        }

        private void SetColorEditor(FlexibleColorPicker fcp, Vector4 value)
        {
            colorPicker = fcp;
            colorPicker.color = value;
            colorPicker.onColorChange.AddListener(OnColorPick);
            colorIconButton.onClick.AddListener(ToggleColorPick);

            SetColor(value);
        }

        private void SetColor(Color color)
        {
            currentValue = color;
            colorIconButton.image.color = color;

            mat.SetColor(propertyName, color);
        }

        private void OnColorPick(Color color)
        {
            SetColor(color);
        }

        private void ToggleColorPick()
        {
            colorPicker.gameObject.SetActive(!colorPicker.gameObject.activeSelf);
            LayoutRebuilder.ForceRebuildLayoutImmediate(colorPicker.transform.parent.GetComponent<RectTransform>());
        }
    }
}