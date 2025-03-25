using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

namespace Merlin
{
    public class RuntimeAssetWindow : RuntimeWindow<Texture>
    {
        [SerializeField]
        private Button elementPreset;

        [SerializeField]
        private Transform elementParent;

        private List<Button> elements = new();

        public static RuntimeAssetWindow GetTextureWindow(Texture[] texArr)
        {
            var window = instance as RuntimeAssetWindow;
            if (window.elements.Count > 0)
            {
                foreach (var element in window.elements)
                {
                    Destroy(element.gameObject);
                }
            }

            foreach (Texture tex in texArr)
            {
                var button = Instantiate(window.elementPreset, window.elementParent);
                window.elements.Add(button);
                button.image.sprite = window.TextureToSprite(tex);
                button.gameObject.SetActive(true);

                button.onClick.AddListener(() =>
                {
                    window.onValueChanged.Invoke(tex);
                });
            }

            return window;
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