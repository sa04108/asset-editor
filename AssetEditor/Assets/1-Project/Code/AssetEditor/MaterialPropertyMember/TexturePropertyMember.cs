using TMPro;
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
        private Image icon;

        [SerializeField]
        private TMP_Text desc;

        private GridMember textureGrid;

        public void Initialize(Material mat, GridMember textureGrid, string name, Texture tex)
        {
            base.Initialize(mat, MaterialPropertyType.Texture, name, tex);

            this.textureGrid = textureGrid;
            icon.sprite = TextureToSprite(tex);
            desc.text = $"{name}";

            button.onClick.AddListener(() =>
            {
                textureGrid.SetOwner(transform, texture =>
                {
                    currentValue = texture;
                    icon.sprite = TextureToSprite(texture);
                    mat.SetTexture(name, texture);
                });
            });
        }

        private Sprite TextureToSprite(Texture texture)
        {
            if (texture == null)
                return null;

            Texture2D tex2D = texture as Texture2D;
            Sprite sprite = Sprite.Create(
                tex2D,
                new Rect(0, 0, tex2D.width, tex2D.height),
                new Vector2(0.5f, 0.5f)
            );

            return sprite;
        }
    }
}