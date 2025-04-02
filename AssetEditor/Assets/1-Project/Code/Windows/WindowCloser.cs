using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.InputSystem;
using UnityEngine.UI;

namespace Merlin
{
    public class WindowCloser : MonoBehaviour
    {
        [SerializeField]
        private bool autoHide = true;

        [SerializeField]
        private Button closeButton;

        [SerializeField]
        private GraphicRaycaster graphicRaycaster;

        [SerializeField]
        private GameObject windowObject;

        private void Awake()
        {
            if (windowObject == null)
                windowObject = gameObject;

            closeButton.onClick.AddListener(Close);

            var clickAction = new InputAction(type: InputActionType.Button, binding: "<Mouse>/leftButton");
            clickAction.performed += OnClick;
            clickAction.Enable();
        }

        private void OnClick(InputAction.CallbackContext context)
        {
            if (!autoHide)
                return;

            Vector2 clickPosition = Mouse.current.position.ReadValue();

            // 클릭 위치를 기준으로 PointerEventData 생성
            PointerEventData pointerData = new PointerEventData(EventSystem.current)
            {
                position = clickPosition
            };

            // GraphicRaycaster를 이용하여 클릭된 UI 요소들을 찾습니다.
            List<RaycastResult> results = new List<RaycastResult>();
            graphicRaycaster.Raycast(pointerData, results);

            bool clickedOnTarget = false;
            foreach (var result in results)
            {
                if (result.gameObject == windowObject || result.gameObject.transform.IsChildOf(windowObject.transform))
                {
                    clickedOnTarget = true;
                    break;
                }
            }

            // 외부에서 클릭되었다면 UI 비활성화
            if (!clickedOnTarget)
            {
                Close();
            }
        }

        private void Close()
        {
            windowObject.SetActive(false);
        }
    }
}