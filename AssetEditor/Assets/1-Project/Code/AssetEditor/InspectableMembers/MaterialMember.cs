using TMPro;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.UI;

namespace Merlin
{
    public class MaterialMember : MonoBehaviour
    {
        [SerializeField] private TMP_Text label;
        [SerializeField] private Button button;
        private Material mat;

        [HideInInspector]
        public UnityEvent<Material> OnClick;

        public void Initialize(string label, Material mat)
        {
            this.label.text = $"{label}";
            this.mat = mat;

            button.onClick.RemoveAllListeners();
            button.onClick.AddListener(() => OnClick.Invoke(mat));
            OnClick.RemoveAllListeners();
        }
    }
}