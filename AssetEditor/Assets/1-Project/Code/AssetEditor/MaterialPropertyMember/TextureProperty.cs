using UnityEngine;
using UnityEngine.Events;
using UnityEngine.EventSystems;
using UnityEngine.UI;

namespace Merlin
{
    public class TextureProperty : MonoBehaviour, IPointerDownHandler, ISelectHandler
    {
        [SerializeField] private RawImage icon;
        [SerializeField] private GameObject selectionEffect;

        [HideInInspector]
        private UnityEvent OnClick = new();

        public void Initialize(Material mat, string name, Texture tex)
        {
            icon.texture = tex;

            OnClick.AddListener(() =>
            {
                AssetWindow.Get<Texture>(transform, texture =>
                {
                    icon.texture = texture;
                    mat.SetTexture(name, texture);
                },
                OnUnsubscribe);
            });
        }

        public void OnPointerDown(PointerEventData eventData)
        {
            EventSystem.current.SetSelectedGameObject(gameObject);

            OnClick.Invoke();
        }

        public void OnSelect(BaseEventData eventData)
        {
            selectionEffect.SetActive(true);
        }

        private void OnUnsubscribe()
        {
            if (gameObject != null)
            {
                selectionEffect.SetActive(false);
            }
        }
    }
}