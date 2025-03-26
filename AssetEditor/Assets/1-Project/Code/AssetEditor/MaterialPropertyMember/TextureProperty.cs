using UnityEngine;
using UnityEngine.Events;
using UnityEngine.UI;

namespace Merlin
{
    public class TextureProperty : MonoBehaviour
    {
        [SerializeField]
        private Button button;

        public Button Button => button;

        [SerializeField]
        private RawImage icon;

        public void Initialize(Material mat, string name, Texture tex)
        {
            icon.texture = tex;

            button.onClick.AddListener(() =>
            {
                RuntimeAssetWindow.SetOwner(transform, texture =>
                {
                    icon.texture = texture;
                    mat.SetTexture(name, texture);
                });
            });
        }
    }
}