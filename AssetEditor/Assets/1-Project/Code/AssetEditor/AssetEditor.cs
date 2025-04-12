using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.AddressableAssets;
using UnityEngine.EventSystems;
using UnityEngine.UI;

namespace Merlin
{
    /// <summary>
    /// 웹 API를 통해 에셋을 가져오고 씬에서의 UI 및 사용자 조작을 구성합니다.
    /// </summary>
    public class AssetEditor : MonoBehaviour
    {
        private AssetInspector inspector;

        private Vector3 assetPivot;

        [Header("Links")]
        [SerializeField]
        private Transform assetParent;

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

        // 마우스 입력을 받아 처리
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

            // 커서가 UI위에 있는 경우에는 무시
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

        // 모델(에셋)을 불러옵니다.
        private void LoadModel()
        {
            // 로딩 아이콘 활성화(화면 중앙 표시)
            loading.SetActive(true);
            // 런타임에 카탈로그가 업데이트되어 있는지 확인합니다.
            // 업데이트 되었다면 새로운 버전의 에셋을 다운로드 받습니다.
            Addressables.CheckForCatalogUpdates()
            .Completed += _ =>
            {
                if (_.Result.Count > 0)
                {
                    // 카탈로그를 업데이트합니다.
                    Addressables.UpdateCatalogs(_.Result)
                    .Completed += _ =>
                    {
                        var keys = _.Result.SelectMany(locator => locator.Keys);
                        LoadAsset(keys);
                    };
                }
                else
                {
                    // 카탈로그가 변경되지 않은 경우 기존 카탈로그를 통해 에셋을 불러옵니다.
                    var keys = Addressables.ResourceLocators.SelectMany(locator => locator.Keys);
                    LoadAsset(keys);
                }
            };
        }

        private void LoadAsset(IEnumerable<object> keys)
        {
            // Addressable로 prefab을 하나 찾습니다.
            string assetKey = null;
            foreach (var key in keys)
            {
                string keyStr = key as string;
                if (keyStr == null)
                    continue;

                if (keyStr.EndsWith(".prefab"))
                {
                    assetKey = keyStr;
                }
            }

            // prefab을 찾지 못한 경우 종료
            if (string.IsNullOrEmpty(assetKey))
            {
                loading.SetActive(false);
                Debug.Log("Could not find prefab asset.");
                return;
            }

            Addressables.LoadAssetAsync<GameObject>(assetKey)
            .Completed += _ =>
            {
                var go = _.Result;
                // 기존에 로드된 에셋이 있다면 제거합니다.
                for (int i = assetParent.childCount - 1; i >= 0; i--)
                {
                    Destroy(assetParent.GetChild(i).gameObject);
                }

                var instance = Instantiate(go, assetParent);
                // 카메라가 바라보기 위한 에셋의 중심 피벗 설정
                assetPivot = instance.transform.position;
                Camera.main.transform.LookAt(assetPivot);

                // 에셋의 material을 inspect하도록 요청
                inspector.LoadModel(go);
                // 로딩 표시 해제
                loading.SetActive(false);
            };
        }
    }
}