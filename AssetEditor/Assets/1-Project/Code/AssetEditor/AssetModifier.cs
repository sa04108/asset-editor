using System.Collections.Generic;
using UnityEngine;

namespace Merlin
{
    public class AssetModifier
    {
        private List<Material> sharedMaterials = new();
        private List<Material> originalMaterials = new();

        public AssetModifier(GameObject go)
        {
            Renderer[] renderers = go.GetComponentsInChildren<Renderer>();
            HashSet<int> matHashes = new();

            foreach (Renderer renderer in renderers)
            {
                var mat = renderer.sharedMaterial;
                var matHash = mat.GetHashCode();

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