using TMPro;
using UnityEngine;
using UnityEngine.AddressableAssets;
using UnityEngine.EventSystems;
using UnityEngine.UI;

namespace Merlin
{
    public class AssetEditor : MonoBehaviour
    {
        private AssetInspector inspector;

        private Vector3 assetPivot;

        [Header("Links")]
        [SerializeField]
        private Transform assetParent;

        [SerializeField]
        private TMP_InputField assetKeyInputField;

        [SerializeField]
        private Button loadButton;

        [SerializeField]
        private GameObject loading;

        [Header("Options")]
        [SerializeField]
        private float rotationSpeed = 5.0f;

        [SerializeField]
        private float zoomSpeed = 10.0f;

        private void Start()
        {
            inspector = GetComponent<AssetInspector>();
            loadButton.onClick.AddListener(() => LoadModel());
        }

        private void Update()
        {
            OnMouseInput();
        }

        private void OnMouseInput()
        {
            var cameraTransform = Camera.main.transform;

            // 우클릭(마우스 오른쪽 버튼)이 눌린 상태에서 처리
            if (Input.GetMouseButton(1))
            {
                // 마우스 이동 값을 가져옵니다.
                float horizontal = Input.GetAxis("Mouse X") * rotationSpeed;
                float vertical = Input.GetAxis("Mouse Y") * rotationSpeed;

                // 모델을 기준으로 Y축을 따라 수평 회전
                cameraTransform.RotateAround(assetPivot, Vector3.up, horizontal);

                // 모델을 기준으로 카메라의 오른쪽 축을 따라 수직 회전
                // 음수를 곱해 위/아래 방향이 자연스럽게 움직이도록 함
                cameraTransform.RotateAround(assetPivot, cameraTransform.right, -vertical);
            }

            if (EventSystem.current?.IsPointerOverGameObject() ?? true)
                return;

            // 마우스 휠 입력을 통한 확대/축소 처리
            float scroll = Input.GetAxis("Mouse ScrollWheel");
            if (Mathf.Abs(scroll) > 0.01f)
            {
                // 방향 벡터 계산
                Vector3 direction = (cameraTransform.position - assetPivot).normalized;
                // 거리
                float distance = Vector3.Distance(cameraTransform.position, assetPivot);
                // 스크롤 값에 따라 거리를 변경
                distance -= scroll * zoomSpeed;
                // 최소 거리가 1m 이하로 내려가지 않도록 보정
                distance = Mathf.Clamp(distance, 1f, 20f);
                // 변경된 거리로 카메라 위치 갱신
                cameraTransform.position = assetPivot + direction * distance;
            }
        }

        private void LoadModel()
        {
            loading.SetActive(true);
            Addressables.InitializeAsync()
            .Completed += _ =>
            {
                Addressables.LoadAssetAsync<GameObject>(assetKeyInputField.text)
                .Completed += _ =>
                {
                    var go = _.Result;
                    for (int i = assetParent.childCount - 1; i >= 0; i--)
                    {
                        Destroy(assetParent.GetChild(i).gameObject);
                    }

                    var instance = Instantiate(go, assetParent);
                    assetPivot = instance.transform.position;
                    Camera.main.transform.LookAt(assetPivot);

                    inspector.LoadModel(go);
                    loading.SetActive(false);
                };
            };
        }
    }
}