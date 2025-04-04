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

        private void Start()
        {
            button.onClick.AddListener(() =>
            {
                foreach (Transform member in transform.parent)
                {
                    if (member == transform)
                        Select();
                    else
                        member.GetComponent<MaterialMember>()?.Deselect();
                }

                OnClick.Invoke(mat);
            });
        }

        public void Initialize(string label, Material mat)
        {
            this.label.text = $"{label}";
            this.mat = mat;
        }

        private void Select()
        {
            var color = button.image.color;
            color.a = 1f;
            button.image.color = color;
        }

        private void Deselect()
        {
            var color = button.image.color;
            color.a = 0f;
            button.image.color = color;
        }
    }
}