using System;
using System.IO;
using System.Linq;
using System.Text;
using UnityEditor;
using UnityEditor.AddressableAssets;
using UnityEditor.AddressableAssets.Build;
using UnityEditor.AddressableAssets.Settings;
using UnityEngine;
using UnityEngine.AddressableAssets;

namespace Merlin
{
    public enum eAssetBuildExitCode
    {
        Others = -1,
        Success = 0,
        NoAsset = 1,
        CompileError = 2,
        UnsupportedFileType = 3,
        PathNotFound = 4
    }

    public class AddressableMenu : AssetPostprocessor
    {
        private static string BuildPath
        {
            get
            {
                var settings = AddressableAssetSettingsDefaultObject.Settings;
                var path = settings.profileSettings.GetValueById(settings.activeProfileId, settings.RemoteCatalogBuildPath.Id).Replace("[BuildTarget]", "");

                return path;
            }
        }

        [MenuItem("Addressables/Build", false, 0)]
        public static int Build()
        {
            Debug.Log("[Addressables] Build Start");
            AddressableAssetSettings settings = AddressableAssetSettingsDefaultObject.Settings;
            AddressablesPlayerBuildResult buildResult;
            eAssetBuildExitCode exitCode = eAssetBuildExitCode.Success;

            try
            {
                if (CanUpdateBuild(settings, out var contentPath))
                {
                    buildResult = ContentUpdateScript.BuildContentUpdate(settings, contentPath);
                }
                else
                {
                    AddressableAssetSettings.BuildPlayerContent(out buildResult);
                }

                if (buildResult.LocationCount == 0)
                {
                    Debug.LogWarning($"[Addressables] Build Error: {buildResult.Error}");
                    exitCode = eAssetBuildExitCode.Others;

                    if (string.IsNullOrEmpty(buildResult.Error))
                    {
                        exitCode = eAssetBuildExitCode.NoAsset;
                    }
                    else if (buildResult.Error.Contains("SBP ErrorError"))
                    {
                        exitCode = eAssetBuildExitCode.CompileError;
                    }
                    else if (buildResult.Error.Contains("unsupported file type"))
                    {
                        exitCode = eAssetBuildExitCode.UnsupportedFileType;
                    }
                    else if (buildResult.Error.Contains("not find a part of the path"))
                    {
                        exitCode = eAssetBuildExitCode.PathNotFound;
                    }

                    throw new InvalidOperationException();
                }

                Debug.Log("[Addressables] Assets build completed");
                Debug.Log($"[Addressables] Build duration: {buildResult.Duration}s");
                Debug.Log($"[Addressables] Location count: {buildResult.LocationCount}");
                StringBuilder sb = new("[Addressables] Bundle List: ");
                foreach (var bundleResult in buildResult.AssetBundleBuildResults)
                {
                    sb.AppendLine(bundleResult.FilePath);
                }
                Debug.Log(sb);
#if PLATFORM_STANDALONE_WIN
                var projectPath = Directory.GetParent(Application.dataPath).FullName;
                var path = Path.Combine(projectPath, BuildPath);
                System.Diagnostics.Process.Start("explorer.exe", path);
                Debug.Log($"[Addressables] Load File Path {path}");
#endif
            }
            catch (InvalidOperationException)
            {
                Debug.LogError($"[Addressables] Exception of type '{exitCode}' was thrown.");
            }
            catch (Exception e)
            {
                Debug.LogError($"[Addressables] {e}: {e?.Message}");
                exitCode = eAssetBuildExitCode.Others;
            }

            Debug.Log($"[Addressables] exit code: {(int)exitCode}");
            return (int)exitCode;
        }

        private static bool CanUpdateBuild(AddressableAssetSettings settings, out string contentFilePath)
        {
            contentFilePath = Path.Combine(settings.ContentStateBuildPath, "addressables_content_state.bin");
            return File.Exists(contentFilePath);
        }

        [MenuItem("Addressables/Clean Build", false, 0)]
        public static void CleanBuild()
        {
            // Remote Build Path 제거
            if (Directory.Exists(BuildPath))
            {
                Directory.Delete(BuildPath, true);
            }

            AddressableAssetSettings.CleanPlayerContent();
            AddressableAssetSettings.BuildPlayerContent();
        }

        [MenuItem("Addressables/Clear Bundle Cache", false, 100)]
        public static void ClearCache()
        {
            // 다운로드 받은 모든 번들 캐시를 제거합니다.
            Caching.ClearCache();
            Addressables.CleanBundleCache();

            Debug.Log("All bundle cache removed");
        }

        [MenuItem("Addressables/Open Build File Path", false, 100)]
        public static void OpenFilePath()
        {
            Debug.Log($"Open path {BuildPath}");
            EditorUtility.RevealInFinder(BuildPath);
        }

        private const string autoAssignBundleMenu = "Addressables/Assign Bundle Automatically";
        private const string autoAssignBundlePref = "AutoBundleAssignmentEnabled";

        [MenuItem(autoAssignBundleMenu, true, 200)]
        private static bool InitAutoAssignBundle()
        {
            bool currentState = EditorPrefs.GetBool(autoAssignBundlePref, true);
            Menu.SetChecked(autoAssignBundleMenu, currentState);

            return true;
        }

        [MenuItem(autoAssignBundleMenu, false, 200)]
        public static void ToggleAutoAssignBundle()
        {
            bool currentState = EditorPrefs.GetBool(autoAssignBundlePref, true);
            currentState = !currentState;
            EditorPrefs.SetBool(autoAssignBundlePref, currentState);
            Menu.SetChecked(autoAssignBundleMenu, currentState);
        }

        public override int GetPostprocessOrder()
        {
            // 0번은 FBX Import Process에서 사용
            return 1;
        }

        // AssetImport시 번들 자동 지정
        static void OnPostprocessAllAssets(string[] importedAssets, string[] deletedAssets, string[] movedAssets, string[] movedFromAssetPaths)
        {
            bool autoAssign = EditorPrefs.GetBool(autoAssignBundlePref, true);
            if (!autoAssign)
                return;

            if (importedAssets.Length == 0)
                return;

            // Addressable Asset Settings 가져오기
            AddressableAssetSettings settings = AddressableAssetSettingsDefaultObject.Settings;
            if (settings == null)
            {
                Debug.LogError("Couldn't find [Addressable Asset Settings]. Initialize it first.");
                return;
            }

            AddressableAssetGroup assetGroup = settings.DefaultGroup;
            if (assetGroup == null)
            {
                assetGroup = settings.CreateGroup("AddressableAssets", true, false, false, null, typeof(UnityEditor.AddressableAssets.Settings.GroupSchemas.BundledAssetGroupSchema));
            }

            // 임포트된 에셋 중 폴더 이름이 "__"로 시작하는 경우 자동 지정
            foreach (string assetPath in importedAssets)
            {
                // Addressable 경로인 경우에 한하여 적용
                if (!assetPath.StartsWith("Assets/Addressable/"))
                    continue;

                // 에셋 경로의 상위 폴더 이름 확인
                string assetFolder = Path.GetDirectoryName(assetPath);
                if (string.IsNullOrEmpty(assetFolder))
                    continue;

                // 마지막 폴더 이름 추출
                string folderName = new DirectoryInfo(assetFolder).Name;
                if (folderName.StartsWith("__"))
                {
                    SetDirectoryAsAddressable(assetFolder, assetGroup);

                    string materialForder = Directory.GetDirectories(assetFolder, "Materials", SearchOption.TopDirectoryOnly).FirstOrDefault();
                    if (!string.IsNullOrEmpty(materialForder))
                    {
                        SetDirectoryAsAddressable(materialForder, assetGroup);
                    }
                }
            }
        }

        private static void SetDirectoryAsAddressable(string dir, AddressableAssetGroup group)
        {
            AddressableAssetSettings settings = AddressableAssetSettingsDefaultObject.Settings;
            string guid = AssetDatabase.AssetPathToGUID(dir);
            // 이미 Addressable로 지정된 폴더인지 확인
            var entry = settings.FindAssetEntry(guid);
            if (entry == null)
            {
                settings.CreateOrMoveEntry(guid, group);
                Debug.Log($"'{dir}'is automatically assgined as Addressable Asset.");
            }
        }
    }
}