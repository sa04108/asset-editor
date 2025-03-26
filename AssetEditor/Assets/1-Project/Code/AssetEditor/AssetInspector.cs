using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace Merlin
{
    // Shader Name -> Property Name -> Property Type -> Property Value
    // using ShaderProperty = Dictionary<string, Dictionary<string, Dictionary<string, object>>>;

    public class AssetInspector : MonoBehaviour
    {
        [SerializeField]
        private Transform memberParent;

        [SerializeField]
        private PropertyMemberCreator memberCreator;

        private int presetCount;

        //[SerializeField]
        //private TextAsset shaderPropJson;

        //private ShaderProperty shaderProps;

        private void Start()
        {
            presetCount = memberParent.transform.childCount;
            //shaderProps = JsonConvert.DeserializeObject<ShaderProperty>(shaderPropJson.text);
        }

        public void SetFbxInstance(GameObject go)
        {
            ClearMembers();
            LoadMembers(go);
        }

        private void ClearMembers()
        {
            for (int i = memberParent.childCount - 1; i >= presetCount; i--)
            {
                Destroy(memberParent.GetChild(i).gameObject);
            }
        }

        private void LoadMembers(GameObject go)
        {
            Renderer[] renderers = go.GetComponentsInChildren<Renderer>();
            HashSet<Material> materialSet = new();

            foreach (Renderer renderer in renderers)
            {
                foreach (Material mat in renderer.sharedMaterials)
                {
                    var matHash = mat.GetHashCode();

                    if (materialSet.Contains(mat))
                        continue;
                    materialSet.Add(mat);
                }
            }

            RuntimeAssetWindow.Get(go.transform, materialSet.ToArray(), InspectMaterial);
        }

        private void InspectMaterial(Material mat)
        {
            ClearMembers();

            Dictionary<string, int> shaderPropIdx = new();
            for (int i = 0; i < mat.shader.GetPropertyCount(); i++)
            {
                shaderPropIdx.Add(mat.shader.GetPropertyName(i), i);
            }

            var group = memberCreator.CreateGroupMember("Options", memberParent);

            var textureProps = mat.GetTexturePropertyNames();
            foreach (string prop in textureProps)
            {
                var tex = mat.GetTexture(prop);
                memberCreator.CreateTexturePropertyMember(mat, prop, tex, group);
            }

            var floatProps = mat.GetPropertyNames(MaterialPropertyType.Float);
            foreach (string prop in floatProps)
            {
                var value = mat.GetFloat(prop);
                if (shaderPropIdx.ContainsKey(prop) &&
                    mat.shader.GetPropertyType(shaderPropIdx[prop]) == UnityEngine.Rendering.ShaderPropertyType.Range)
                {
                    Vector2 rangeVec = mat.shader.GetPropertyRangeLimits(shaderPropIdx[prop]);
                    float min = rangeVec.x;
                    float max = rangeVec.y;
                    memberCreator.CreateFloatMember(mat, prop, value, min, max, group);
                }
                else
                {
                    memberCreator.CreateFloatMember(mat, prop, value, group);
                }
            }

            var intProps = mat.GetPropertyNames(MaterialPropertyType.Int);
            foreach (string prop in intProps)
            {
                var value = mat.GetInteger(prop);
                memberCreator.CreateIntMember(mat, prop, value, group);
            }

            var vecProps = mat.GetPropertyNames(MaterialPropertyType.Vector);
            foreach (string prop in vecProps)
            {
                var value = mat.GetVector(prop);
                if (shaderPropIdx.ContainsKey(prop) &&
                    mat.shader.GetPropertyType(shaderPropIdx[prop]) == UnityEngine.Rendering.ShaderPropertyType.Color)
                {
                    bool isHDR = mat.shader.GetPropertyFlags(shaderPropIdx[prop]) == UnityEngine.Rendering.ShaderPropertyFlags.HDR;
                    memberCreator.CreateColorMember(mat, prop, value, isHDR, group);
                }
                else
                {
                    memberCreator.CreateVectorMember(mat, prop, value, group);
                }
            }

            var matrixProps = mat.GetPropertyNames(MaterialPropertyType.Matrix);
            foreach (string prop in matrixProps)
            {
                var value = mat.GetMatrix(prop);
                memberCreator.CreateMatrixMember(mat, prop, value, group);
            }
        }

        //private Dictionary<string, T> GetShaderPropertyValue<T>(string shaderName, string propName, string propType)
        //{
        //    return ((JObject)shaderProps[shaderName][propName][propType]).ToObject<Dictionary<string, T>>();
        //}
    }
}