using UnityEngine;
using UnityEngine.EventSystems;

namespace Merlin
{
    /// <summary>
    /// 현재 창의 상단을 잡고 끌어 드래그해주는 기능
    /// </summary>
    public class WindowDragger : UIBehaviour, IPointerDownHandler, IDragHandler
    {
        [Header("Resources")]
        // 창을 드래그하면서 벗어나지 못하게 하는 공간
        [SerializeField] private RectTransform dragArea;
        // 드래그 되는 창 오브젝트
        [SerializeField] private RectTransform dragObject;

        // 창 드래그 시작시 가장 앞에 위치시킬 지 여부
        [Header("Settings")]
        [SerializeField] private bool topOnDrag = true;

        private Vector2 originalLocalPointerPosition;
        private Vector3 originalPanelLocalPosition;

        protected override void Awake()
        {
            base.Awake();

            if (dragArea == null)
            {
                try
                {
                    var canvas = GetComponentInParent<Canvas>();
                    dragArea = canvas.GetComponent<RectTransform>();
                }
                catch { Debug.LogError("<b>[Movable Window]</b> Drag Area has not been assigned."); }
            }
        }

        protected override void OnEnable()
        {
            base.OnEnable();

            ClampToArea();
        }

        private RectTransform DragObjectInternal
        {
            get
            {
                if (dragObject == null) { return (transform as RectTransform); }
                else { return dragObject; }
            }
        }

        private RectTransform DragAreaInternal
        {
            get
            {
                if (dragArea == null)
                {
                    RectTransform canvas = transform as RectTransform;
                    while (canvas.parent != null && canvas.parent is RectTransform) { canvas = canvas.parent as RectTransform; }
                    return canvas;
                }
                else { return dragArea; }
            }
        }

        public void OnPointerDown(PointerEventData data)
        {
            originalPanelLocalPosition = DragObjectInternal.localPosition;
            // 창 내부 클릭 시 현재 포인터 위치 저장 (창 상단 포함)
            RectTransformUtility.ScreenPointToLocalPointInRectangle(DragAreaInternal, data.position, data.pressEventCamera, out originalLocalPointerPosition);
            gameObject.transform.SetAsLastSibling();
            if (topOnDrag == true) { dragObject.transform.SetAsLastSibling(); }
        }

        public void OnDrag(PointerEventData data)
        {
            Vector2 localPointerPosition;

            if (RectTransformUtility.ScreenPointToLocalPointInRectangle(DragAreaInternal, data.position, data.pressEventCamera, out localPointerPosition))
            {
                // 처음 클릭한 지점과의 델타 벡터 저장
                Vector3 offsetToOriginal = localPointerPosition - originalLocalPointerPosition;
                // 델타 벡터만큼 창의 위치를 이동
                DragObjectInternal.localPosition = originalPanelLocalPosition + offsetToOriginal;
            }

            // 창이 영역을 벗어나지 않도록 제한
            ClampToArea();
        }

        private void ClampToArea()
        {
            Vector3 pos = DragObjectInternal.localPosition;

            Vector3 minPosition = DragAreaInternal.rect.min - DragObjectInternal.rect.min;
            Vector3 maxPosition = DragAreaInternal.rect.max - DragObjectInternal.rect.max;

            pos.x = Mathf.Clamp(DragObjectInternal.localPosition.x, minPosition.x, maxPosition.x);
            pos.y = Mathf.Clamp(DragObjectInternal.localPosition.y, minPosition.y, maxPosition.y);

            DragObjectInternal.localPosition = pos;
        }
    }
}