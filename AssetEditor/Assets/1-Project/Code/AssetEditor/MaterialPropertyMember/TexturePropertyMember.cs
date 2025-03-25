using UnityEngine;
using UnityEngine.UI;

namespace Merlin
{
    public class TexturePropertyMember : MaterialPropertyMember<Texture>
    {
        [SerializeField]
        private Button button;

        public Button Button => button;

        [SerializeField]
        private RawImage icon;

        public override void Initialize(Material mat, MaterialPropertyType type, string name, Texture tex)
        {
            base.Initialize(mat, type, name, tex);

            icon.texture = tex;

            button.onClick.AddListener(() =>
            {
                RuntimeAssetWindow.SetOwner(transform, texture =>
                {
                    currentValue = texture;
                    icon.texture = texture;
                    mat.SetTexture(name, texture);
                });
            });
        }
    }
}