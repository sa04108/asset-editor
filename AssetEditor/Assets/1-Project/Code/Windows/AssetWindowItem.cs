using TMPro;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.EventSystems;
using UnityEngine.UI;

namespace Merlin
{
    public class AssetWindowItem : MonoBehaviour, IDragHandler, IPointerDownHandler, IPointerUpHandler, ISelectHandler, IDeselectHandler
    {
        [SerializeField] private ScrollRect scrollRect;
        [SerializeField] private RawImage icon;
        [SerializeField] private TMP_Text label;
        [SerializeField] private GameObject selectionEffect;

        public RawImage Icon => icon;
        public TMP_Text Label => label;

        [HideInInspector]
        public UnityEvent OnClick;

        public void OnDrag(PointerEventData eventData)
        {
        }

        public void OnPointerDown(PointerEventData eventData)
        {
            EventSystem.current.SetSelectedGameObject(gameObject);

            scrollRect.enabled = false;
            OnClick.Invoke();
        }

        public void OnPointerUp(PointerEventData eventData)
        {
            scrollRect.enabled = true;
        }

        public void OnSelect(BaseEventData eventData)
        {
            selectionEffect.SetActive(true);
        }

        public void OnDeselect(BaseEventData eventData)
        {
            selectionEffect.SetActive(false);
        }
    }
}