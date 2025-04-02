using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace Merlin
{
    public class AssetModifier
    {
        private Dictionary<Material, Material> matDict = new();

        public AssetModifier(GameObject go)
        {
            Renderer[] renderers = go.GetComponentsInChildren<Renderer>();

            foreach (Renderer renderer in renderers)
            {
                var mat = renderer.sharedMaterial;

                if (mat == null || matDict.ContainsKey(mat))
                    continue;

                var originalMat = new Material(mat);
                matDict.Add(mat, originalMat);
            }
        }

        public Material[] GetSharedMaterials()
        {
            return matDict.Keys.ToArray();
        }

        public void Save()
        {
        }

        public void Reset()
        {
            foreach (var matPair in matDict)
            {
                // 현재 sharedMaterial로 초기 material 값을 복사
                matPair.Key.CopyPropertiesFromMaterial(matPair.Value);
            }
        }
    }
}