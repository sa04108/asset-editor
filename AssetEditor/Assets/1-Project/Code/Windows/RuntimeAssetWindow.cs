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
            var window = (RuntimeAssetWindow)instance;
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
                button.GetComponent<RawImage>().texture = tex;
                button.gameObject.SetActive(true);

                button.onClick.AddListener(() =>
                {
                    window.onValueChanged.Invoke(tex);
                });
            }

            return window;
        }
    }
}