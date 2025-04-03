using TMPro;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.UI;

namespace Merlin
{
    public class PropertyGroupMember : MonoBehaviour
    {
        [SerializeField]
        private Button button;

        [HideInInspector]
        public UnityEvent OnClick => button.onClick;

        [SerializeField]
        private TMP_Text title;

        public void Initialize(string title)
        {
            this.title.text = $"{title}";

            button.onClick.RemoveAllListeners();
        }
    }
}