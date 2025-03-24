using System.Collections.Generic;
using UnityEngine;

namespace Merlin
{
    // Shader Name -> Property Name -> Property Type -> Property Value
    // using ShaderProperty = Dictionary<string, Dictionary<string, Dictionary<string, object>>>;

    public class AssetModifier : MonoBehaviour
    {
        [SerializeField]
        private Transform memberParent;

        [SerializeField]
        private AssetPropertyMemberCreator memberCreator;

        private int presetCount;
        private GameObject assetInstance;

        [SerializeField]
        private TextAsset shaderPropJson;

        //private ShaderProperty shaderProps;

        private void Start()
        {
            presetCount = memberParent.transform.childCount;
            //shaderProps = JsonConvert.DeserializeObject<ShaderProperty>(shaderPropJson.text);
        }

        public void SetFbxInstance(GameObject go)
        {
            assetInstance = go;

            ClearMembers();
            LoadMembers();
        }

        private AssetGridMember LoadTexturesGrid()
        {
            var textures = GetComponent<AssetEditor>().GetTextures();
            var gridMember = memberCreator.CreateGridMember(textures.ToArray(), memberParent);

            return gridMember;
        }

        private void LoadMembers()
        {
            Renderer[] renderers = assetInstance.GetComponentsInChildren<Renderer>();
            HashSet<int> materialSet = new();
            AssetGridMember texturesGrid = LoadTexturesGrid();

            foreach (Renderer renderer in renderers)
            {
                foreach (Material mat in renderer.sharedMaterials)
                {
                    var matHash = mat.GetHashCode();

                    if (materialSet.Contains(matHash))
                        continue;
                    materialSet.Add(matHash);

                    Dictionary<string, int> shaderPropIdx = new();
                    for (int i = 0; i < mat.shader.GetPropertyCount(); i++)
                    {
                        shaderPropIdx.Add(mat.shader.GetPropertyName(i), i);
                    }

                    var group = memberCreator.CreateGroupMember(mat.mainTexture, "Material", mat.name, memberParent);

                    var textureProps = mat.GetTexturePropertyNames();
                    foreach (string prop in textureProps)
                    {
                        var tex = mat.GetTexture(prop);
                        memberCreator.CreateTexturePropertyMember(mat, texturesGrid, prop, tex, group);
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
                            memberCreator.CreateVectorMember(mat, prop, value, true, group);
                        }
                        else
                        {
                            memberCreator.CreateVectorMember(mat, prop, value, false, group);
                        }
                    }

                    var matrixProps = mat.GetPropertyNames(MaterialPropertyType.Matrix);
                    foreach (string prop in matrixProps)
                    {
                        var value = mat.GetMatrix(prop);
                        memberCreator.CreateMatrixMember(mat, prop, value, group);
                    }
                }
            }
        }

        private void ClearMembers()
        {
            for (int i = memberParent.childCount - 1; i >= presetCount; i--)
            {
                Destroy(memberParent.GetChild(i).gameObject);
            }
        }

        //private Dictionary<string, T> GetShaderPropertyValue<T>(string shaderName, string propName, string propType)
        //{
        //    return ((JObject)shaderProps[shaderName][propName][propType]).ToObject<Dictionary<string, T>>();
        //}
    }
}