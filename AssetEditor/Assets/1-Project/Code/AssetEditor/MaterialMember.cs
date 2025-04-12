using TMPro;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.UI;

namespace Merlin
{
    /// <summary>
    /// Inspect 되는 Material 버튼
    /// </summary>
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
                // 부모 transform을 순회하면서 자기 자신인지 확인하며 선택 효과 출력
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

        // 선택 시 효과
        private void Select()
        {
            var color = button.image.color;
            color.a = 1f;
            button.image.color = color;
        }

        // 다른 material 선택 시 효과
        private void Deselect()
        {
            var color = button.image.color;
            color.a = 0f;
            button.image.color = color;
        }
    }
}