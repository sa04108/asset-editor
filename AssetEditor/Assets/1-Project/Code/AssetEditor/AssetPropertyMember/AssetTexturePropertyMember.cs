using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace Merlin
{
    public class AssetTexturePropertyMember : MonoBehaviour
    {
        [SerializeField]
        private Button button;

        public Button Button => button;

        [SerializeField]
        private Image icon;

        [SerializeField]
        private TMP_Text desc;

        private Material mat;
        private AssetGridMember textureGrid;
        private string propertyName;
        private Texture currentValue;

        public void Initialize(Material mat, AssetGridMember textureGrid, string name, Texture tex)
        {
            this.mat = mat;
            this.textureGrid = textureGrid;
            propertyName = name;
            currentValue = tex;
            icon.sprite = TextureToSprite(tex);
            desc.text = $"Type: Texture\n" +
                        $"{name}";

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