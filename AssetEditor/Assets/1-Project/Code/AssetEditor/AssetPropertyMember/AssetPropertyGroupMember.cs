using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace Merlin
{
    public class AssetPropertyGroupMember : MonoBehaviour
    {
        [SerializeField]
        private Button button;

        public Button Button => button;

        [SerializeField]
        private TMP_Text desc;

        public void Initialize(Texture tex, string type, string name)
        {
            desc.text = $"{type}";
        }
    }
}