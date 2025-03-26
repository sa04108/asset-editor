using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace Merlin
{
    public class PropertyGroupMember : MonoBehaviour
    {
        [SerializeField]
        private Button button;

        public Button Button => button;

        [SerializeField]
        private TMP_Text title;

        public void Initialize(string title)
        {
            this.title.text = $"{title}";
        }
    }
}