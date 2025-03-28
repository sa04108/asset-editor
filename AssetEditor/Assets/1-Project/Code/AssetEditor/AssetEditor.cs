using System.Collections.Generic;
using System.Linq;
using TMPro;
using UnityEngine;
using UnityEngine.AddressableAssets;
using UnityEngine.AddressableAssets.ResourceLocators;
using UnityEngine.EventSystems;
using UnityEngine.UI;

namespace Merlin
{
    public class AssetEditor : MonoBehaviour
    {
        private AssetInspector inspector;

        [Header("Links")]
        [SerializeField]
        private Transform assetParent;

        [SerializeField]
        private Transform buttonParent;

        [SerializeField]
        private Button buttonPreset;

        [Header("Options")]
        [SerializeField]
        private Vector3 assetPivot = Vector3.zero;

        [SerializeField]
        private float rotationSpeed = 5.0f;

        [SerializeField]
        private float zoomSpeed = 10.0f;

        private void Start()
        {
            inspector = GetComponent<AssetInspector>();
            Addressables.InitializeAsync().Completed += handle =>
            {
                IResourceLocator locator = handle.Result;

                CreateButton($"Models")
                    .onClick.AddListener(() => LoadModels(locator.Keys));

                CreateButton($"Check For Update")
                    .onClick.AddListener(() => CheckForUpdate(locator.Keys));

                CreateButton($"Check For Download")
                    .onClick.AddListener(() => CheckForDownload(locator.Keys));
            };
        }

        void Update()
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

        private void LoadModels(IEnumerable<object> keys)
        {
            List<string> modelKeys = new();
            List<List<string>> texKeys = new();

            foreach (var key in keys)
            {
                string keyStr = key as string;
                if (keyStr == null)
                    continue;

                if (keyStr.EndsWith(".fbx") ||
                keyStr.EndsWith(".3ds"))
                {
                    modelKeys.Add(keyStr);
                }
            }

            foreach (var model in modelKeys)
            {
                string bundleName = GetBundleNameFromPath(model);
                if (bundleName == null)
                    continue;

                List<string> texList = new();

                foreach (var key in keys)
                {
                    string keyStr = key as string;
                    if (keyStr == null)
                        continue;

                    if (!keyStr.Contains(bundleName))
                        continue;

                    if (keyStr.ToLower().EndsWith(".jpg") ||
                        keyStr.ToLower().EndsWith(".png"))
                        texList.Add(keyStr);
                }

                texKeys.Add(texList);
            }

            Addressables.LoadAssetsAsync<GameObject>(modelKeys, null, Addressables.MergeMode.Union)
            .Completed += _ =>
            {
                var goList = _.Result.ToList();
                AssetWindow.Show(transform, _.Result.ToArray(), go =>
                {
                    for (int i = assetParent.childCount - 1; i >= 0; i--)
                    {
                        Destroy(assetParent.GetChild(i).gameObject);
                    }

                    var instance = Instantiate(go, assetParent);
                    assetPivot = instance.transform.position;
                    Camera.main.transform.LookAt(assetPivot);

                    Addressables.LoadAssetsAsync<Texture>(texKeys[goList.IndexOf(go)], null, Addressables.MergeMode.Union)
                    .Completed += _ =>
                    {
                        var textures = _.Result.ToArray();
                        AssetWindow.Show(go.transform, textures, null);
                        inspector.SetModelInstance(go, textures);
                    };
                });
            };
        }

        private string GetBundleNameFromPath(string key)
        {
            var split = key.Split('/');

            foreach (var part in split)
            {
                if (part.StartsWith("__"))
                    return part;
            }

            return null;
        }

        private void CheckForUpdate(IEnumerable<object> keys)
        {
            Addressables.CheckForCatalogUpdates()
            .Completed += _ =>
            {
#if NO_XDT
                if (_.Result.Count > 0)
                {
                    Debug.Log("Catalog Updated");
                    Addressables.UpdateCatalogs(_.Result)
                    .Completed += _ => CheckForDownload(keys);
                }
                else
                {
                    Debug.Log("Catalog is the latest version");
                }
#else
                if (handle.Result.Count > 0)
                {
                    MessageBox.Show(
                        "Asset Update Available.\n" +
                        "Do you want to check for download?",
                        eMessageBoxButtons.YesNo)
                    .Subscribe(result =>
                    {
                        if (result.Code == eMessageBoxResult.Yes)
                        {
                            Addressables.UpdateCatalogs(handle.Result)
                            .Completed += _ => CheckForDownload(keys);
                        }
                    });
                }
                else
                {
                    MessageBox.Show(
                        "Asset is the latest version.",
                        eMessageBoxButtons.OK);
                }
#endif
            };
        }

        private void CheckForDownload(IEnumerable<object> keys)
        {
            Addressables.GetDownloadSizeAsync(keys)
            .Completed += _ =>
            {
#if NO_XDT
                if (_.Result > 0)
                {
                    Debug.Log($"Total Download Size: {_.Result}");
                    Addressables.DownloadDependenciesAsync(keys, Addressables.MergeMode.Union);
                }
                else
                {
                    Debug.Log("All asset is the latest version");
                }
#else
                if (handle.Result > 0)
                {
                    MessageBox.Show(
                        $"Total Download Size: {handle.Result}\n" +
                        "Do you want to download?",
                        eMessageBoxButtons.YesNo)
                    .Subscribe(result =>
                    {
                        if (result.Code == eMessageBoxResult.Yes)
                        {
                            Addressables.DownloadDependenciesAsync(keys, Addressables.MergeMode.Union);
                        }
                    });
                }
                else
                {
                    MessageBox.Show(
                        "All asset downloaded for current version",
                        eMessageBoxButtons.OK);
                }
#endif
            };
        }

        private Button CreateButton(string text)
        {
            var button = Instantiate(buttonPreset, buttonParent);
            button.gameObject.SetActive(true);
            button.GetComponentInChildren<TMP_Text>().text = text;

            return button;
        }
    }
}