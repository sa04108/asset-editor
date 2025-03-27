using System.Collections.Generic;
using System.Linq;
using TMPro;
using UnityEngine;
using UnityEngine.AddressableAssets;
using UnityEngine.EventSystems;
using UnityEngine.UI;

namespace Merlin
{
    public class AssetEditor : MonoBehaviour
    {
        private AssetInspector modifier;

        [Header("Links")]
        [SerializeField]
        private Transform assetParent;

        [SerializeField]
        private Transform buttonParent;

        [SerializeField]
        private Button buttonPreset;

        [Header("Options")]
        [SerializeField]
        private float rotationSpeed = 5.0f;

        [SerializeField]
        private float zoomSpeed = 10.0f;

        [SerializeField]
        private List<string> keys = new();

        private List<Texture> textures = new();

        private void Start()
        {
            modifier = GetComponent<AssetInspector>();
            Addressables.InitializeAsync().Completed += handle =>
            {
                foreach (var key in handle.Result.Keys)
                {
                    string keyStr = key as string;
                    if (keyStr == null)
                        continue;

                    if (keyStr.EndsWith(".fbx") ||
                    keyStr.EndsWith(".3ds"))
                    {
                        keys.Add(keyStr);
                    }
                }

                Addressables.LoadAssetsAsync<Texture>(handle.Result.Keys, texture =>
                {
                    textures.Add(texture);
                }, Addressables.MergeMode.Union).Completed += texHandle =>
                {
                    AssetWindow.Get(transform, textures.ToArray(), null);
                };

                for (int i = 0; i < keys.Count; i++)
                {
                    int c_i = i;
                    CreateButton($"Instantiate {keys[i].Split('/').LastOrDefault()}")
                        .onClick.AddListener(() => DownloadAndInstantiate(keys[c_i]));
                }

                CreateButton($"Check For Update")
                    .onClick.AddListener(() => CheckForUpdate());

                CreateButton($"Check For Download")
                    .onClick.AddListener(() => CheckForDownload());
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

                // 원점을 기준으로 Y축을 따라 수평 회전
                cameraTransform.RotateAround(Vector3.zero, Vector3.up, horizontal);

                // 원점을 기준으로 카메라의 오른쪽 축을 따라 수직 회전
                // 음수를 곱해 위/아래 방향이 자연스럽게 움직이도록 함
                cameraTransform.RotateAround(Vector3.zero, cameraTransform.right, -vertical);
            }

            if (EventSystem.current?.IsPointerOverGameObject() ?? true)
                return;

            // 마우스 휠 입력을 통한 확대/축소 처리
            float scroll = Input.GetAxis("Mouse ScrollWheel");
            if (Mathf.Abs(scroll) > 0.01f)
            {
                // 카메라와 원점 사이의 방향 벡터 계산
                Vector3 direction = (cameraTransform.position - Vector3.zero).normalized;
                // 현재 카메라와 원점 사이의 거리
                float distance = cameraTransform.position.magnitude;
                // 스크롤 값에 따라 거리를 변경
                distance -= scroll * zoomSpeed;
                // 최소 거리가 1m 이하로 내려가지 않도록 보정
                distance = Mathf.Clamp(distance, 1f, 20f);
                // 변경된 거리로 카메라 위치 갱신
                cameraTransform.position = direction * distance;
            }
        }

        private void DownloadAndInstantiate(string key)
        {
            for (int i = assetParent.childCount - 1; i >= 0; i--)
            {
                Destroy(assetParent.GetChild(i).gameObject);
            }

            Addressables.GetDownloadSizeAsync(key)
                .Completed += handle =>
                {
                    if (handle.Result > 0)
                    {
#if NO_XDT
                        Debug.Log($"Download size for this: {handle.Result}");
                        Addressables.DownloadDependenciesAsync(key)
                            .Completed += _ => Addressables.InstantiateAsync(key, assetParent)
                            .Completed += handle => modifier.SetFbxInstance(handle.Result);
#else
                        MessageBox.Show(
                            $"Total Download Size: {handle.Result}\n" +
                            "Do you want to download?",
                            eMessageBoxButtons.YesNo)
                        .Subscribe(result =>
                        {
                            if (result.Code == eMessageBoxResult.Yes)
                            {
                                Addressables.DownloadDependenciesAsync(asset)
                                .Completed += _ => Addressables.InstantiateAsync(asset, assetParent)
                                .Completed += handle => modifier.SetFbxInstance(handle.Result);
                            }
                        });
#endif
                    }
                    else
                    {
                        Addressables.InstantiateAsync(key, assetParent)
                        .Completed += handle => modifier.SetFbxInstance(handle.Result);
                    }
                };
        }

        private void CheckForUpdate()
        {
            Addressables.CheckForCatalogUpdates()
            .Completed += handle =>
            {
#if NO_XDT
                if (handle.Result.Count > 0)
                {
                    Debug.Log("Catalog Updated");
                    Addressables.UpdateCatalogs(handle.Result)
                    .Completed += _ => CheckForDownload();
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
                            .Completed += _ => CheckForDownload();
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

        private void CheckForDownload()
        {
            Addressables.GetDownloadSizeAsync(keys)
            .Completed += handle =>
            {
#if NO_XDT
                if (handle.Result > 0)
                {
                    Debug.Log($"Total Download Size: {handle.Result}");
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
                            Addressables.DownloadDependenciesAsync(assets, Addressables.MergeMode.Union);
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

        public List<Texture> GetTextures()
        {
            return textures;
        }
    }
}