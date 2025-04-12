using System.Collections.Generic;
using UnityEngine;

namespace Merlin
{
    /// <summary>
    /// 에셋의 변경점을 보관하고 초기화 기능을 수행
    /// </summary>
    public class AssetModifier
    {
        // 변경점이 기록되는 material 목록
        private List<Material> sharedMaterials = new();
        // 초기 상태의 material 목록
        private List<Material> originalMaterials = new();

        public AssetModifier(GameObject go)
        {
            Renderer[] renderers = go.GetComponentsInChildren<Renderer>();
            HashSet<int> matHashes = new();

            foreach (Renderer renderer in renderers)
            {
                var mat = renderer.sharedMaterial;
                var matHash = mat.GetHashCode();

                // material hash를 통해 같은 material을 이미 등록했다면 무시
                if (mat == null || matHashes.Contains(matHash))
                    continue;

                matHashes.Add(matHash);
                var originalMat = new Material(mat);
                sharedMaterials.Add(mat);
                originalMaterials.Add(originalMat);
            }
        }

        public Material[] GetSharedMaterials()
        {
            return sharedMaterials.ToArray();
        }

        public void Reset()
        {
            for (int i = 0; i < sharedMaterials.Count; i++)
            {
                // 현재 sharedMaterial로 초기 material 값을 복사
                sharedMaterials[i].CopyPropertiesFromMaterial(originalMaterials[i]);
            }
        }
    }
}