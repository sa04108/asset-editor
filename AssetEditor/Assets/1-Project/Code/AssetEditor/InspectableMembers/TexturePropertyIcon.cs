using UnityEngine;
using UnityEngine.Events;
using UnityEngine.EventSystems;
using UnityEngine.UI;

namespace Merlin
{
    public class TexturePropertyIcon : MonoBehaviour, IPointerDownHandler, IDeselectHandler
    {
        [SerializeField] private RawImage icon;
        [SerializeField] private GameObject selectionEffect;

        [HideInInspector]
        public UnityEvent OnClick = new();

        public void Initialize(Texture tex)
        {
            SetTextureIcon(tex);
        }

        public void SetTextureIcon(Texture tex)
        {
            icon.texture = tex;
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