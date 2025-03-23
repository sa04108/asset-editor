using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.UI;

namespace Merlin
{
    public class AssetGridMember : MonoBehaviour
    {
        [SerializeField]
        private Button elementPreset;

        private List<Button> elements;

        private Transform owner;

        private UnityEvent<Texture> onSelectedEvent = new();

        public void Initialize(Texture[] texArr)
        {
            foreach (Texture tex in texArr)
            {
                var button = Instantiate(elementPreset, transform);
                button.image.sprite = TextureToSprite(tex);
                button.gameObject.SetActive(true);

                button.onClick.AddListener(() =>
                {
                    onSelectedEvent.Invoke(tex);
                });
            }
        }

        public void SetOwner(Transform owner, UnityAction<Texture> onSelected)
        {
            onSelectedEvent.RemoveAllListeners();
            onSelectedEvent.AddListener(onSelected);

            if (this.owner == owner)
            {
                gameObject.SetActive(!gameObject.activeSelf);
            }
            else
            {
                this.owner = owner;

                transform.SetParent(owner.transform.parent);
                transform.SetSiblingIndex(transform.parent.childCount - 1); // 이 transform이 owner transform에 영향을 주므로 맨 끝으로 먼저 보냄
                transform.SetSiblingIndex(owner.transform.GetSiblingIndex() + 1);
                gameObject.SetActive(true);
            }

            LayoutRebuilder.ForceRebuildLayoutImmediate(transform.parent.GetComponent<RectTransform>());
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