using UnityEngine;
using UnityEngine.EventSystems;

namespace GravityBox.UI
{
	/// <summary>
	/// Simple component helping drag RectTransform around
	/// </summary>
	public class DragWindow : MonoBehaviour, IDragHandler, IPointerDownHandler
	{
		private RectTransform _transform;
		private RectTransform _window;
		private Canvas _canvas;

		private Vector3[] corners = new Vector3[4];
		
		void Start()
		{
			_transform = (RectTransform)transform;
			_window = (RectTransform)transform.parent;
			_canvas = transform.GetComponentInParent<Canvas>();
		}

		public void OnDrag(PointerEventData eventData)
		{
			_transform.GetWorldCorners(corners);
			_window.anchoredPosition += ClampDelta(eventData.delta / _canvas.scaleFactor);
		}

		//when clicked bring window on top of everything in canvas
		public void OnPointerDown(PointerEventData eventData)
		{
			if (_canvas.transform == _window.parent)
				_window.SetAsLastSibling();
			else
				_window.parent.SetAsLastSibling();
		}

		private Vector2 ClampDelta(Vector2 delta) 
		{
			Vector2 min = corners[0];
			Vector2 max = corners[2];
			
			//when dragging window left make sure it's right corner is more then 100 pixels
			//when dragging right same for left corner 
			if (delta.x < 0)
				delta.x = Mathf.Max(delta.x, 100 - max.x);
			else
				delta.x = Mathf.Min(delta.x, Screen.width - 100 - min.x);

			//when dragging up or down just make sure dragged rect is on a screen
			if (delta.y < 0)
				delta.y = Mathf.Max(delta.y, -min.y);
			else
				delta.y = Mathf.Min(delta.y, Screen.height - max.y);

			return delta;
		}
	}
}