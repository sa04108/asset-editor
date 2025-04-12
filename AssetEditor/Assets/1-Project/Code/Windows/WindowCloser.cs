using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.InputSystem;
using UnityEngine.UI;

namespace Merlin
{
    public class WindowCloser : MonoBehaviour
    {
        // 창의 외부를 클릭했을 때 자동으로 닫히게 할지 여부
        [SerializeField] private bool autoClose;

        // 닫기 버튼
        [SerializeField] private Button closeButton;

        // 이 창이 있는 캔버스의 Raycaster. 창의 외부를 클릭했는지 확인하기 위한 용도
        [SerializeField] private GraphicRaycaster graphicRaycaster;

        // 닫기를 통해 비활성화할 창 오브젝트
        [SerializeField] private GameObject windowObject;

        private void Awake()
        {
            // 별도로 지정되지 않은 경우 부모 오브젝트로부터 찾습니다.
            if (graphicRaycaster == null)
                graphicRaycaster = GetComponentInParent<GraphicRaycaster>();

            // 별도로 지정되지 않은 경우 자기 자신을 창 오브젝트로 합니다.
            if (windowObject == null)
                windowObject = gameObject;

            closeButton.onClick.AddListener(Close);

            if (autoClose)
            {
                // Unity InputAction을 통해 좌클릭 입력을 받습니다.
                var clickAction = new InputAction(type: InputActionType.Button, binding: "<Mouse>/leftButton");
                clickAction.performed += CloseOnClickOutside;
                clickAction.Enable();
            }
        }

        /// <summary>
        /// 창의 외부를 클릭하면 닫히게 합니다.
        /// </summary>
        /// <param name="context"></param>
        private void CloseOnClickOutside(InputAction.CallbackContext context)
        {
            // 창이 이미 비활성화된 경우 무시
            if (!windowObject.activeSelf)
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