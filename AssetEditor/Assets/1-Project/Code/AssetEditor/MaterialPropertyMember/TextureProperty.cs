using UnityEngine;
using UnityEngine.Events;
using UnityEngine.EventSystems;
using UnityEngine.UI;

namespace Merlin
{
    public class TextureProperty : MonoBehaviour, IPointerDownHandler, IDeselectHandler
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
                });
            });
        }

        public void OnPointerDown(PointerEventData eventData)
        {
            EventSystem.current.SetSelectedGameObject(gameObject);
            selectionEffect.SetActive(true);

            OnClick.Invoke();
        }

        public void OnDeselect(BaseEventData eventData)
        {
            selectionEffect.SetActive(false);
        }
    }
}