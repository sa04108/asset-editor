using Newtonsoft.Json;
using System.IO;
using UnityEngine;

namespace Merlin
{
    public enum eMaterialWorkflowMode
    {
        Specular = 0,
        Metallic = 1
    }

    public enum eMaterialSurfaceType
    {
        Opaque = 0,
        Transparent = 1
    }

    public class MaterialPropertyConfig
    {
        public eMaterialWorkflowMode WorkFlowMode;
        public eMaterialSurfaceType SurfaceType;
    }

    public class MaterialPropertyState
    {
        private MaterialPropertyConfig config;

        public MaterialPropertyState()
        {
            config = new();
        }

        public void SetWorkflowMode(Material mat, eMaterialWorkflowMode mode)
        {
            config.WorkFlowMode = mode;
            mat.SetFloat("_WorkflowMode", (float)mode);

            if (mode == eMaterialWorkflowMode.Specular)
            {
                mat.EnableKeyword("_SPECULAR_SETUP");
            }
            else if (mode == eMaterialWorkflowMode.Metallic)
            {
                mat.DisableKeyword("_SPECULAR_SETUP");
            }
        }

        public void SetSurfaceType(Material mat, eMaterialSurfaceType mode)
        {
            config.SurfaceType = mode;
            mat.SetFloat("_Surface", (float)mode);

            if (mode == eMaterialSurfaceType.Opaque)
            {
                mat.SetOverrideTag("RenderType", "Opaque");
                mat.DisableKeyword("_ALPHAPREMULTIPLY_ON");
                mat.DisableKeyword("_SURFACE_TYPE_TRANSPARENT");
                mat.renderQueue = 2000;

                mat.SetShaderPassEnabled("DepthOnly", true);
                mat.SetShaderPassEnabled("SHADOWCASTER", true);

                mat.SetFloat("_DstBlend", 0);
                mat.SetFloat("_DstBlendAlpha", 0);
                mat.SetFloat("_ZWrite", 1);
            }
            else if (mode == eMaterialSurfaceType.Transparent)
            {
                mat.SetOverrideTag("RenderType", "Transparent");
                mat.EnableKeyword("_ALPHAPREMULTIPLY_ON");
                mat.EnableKeyword("_SURFACE_TYPE_TRANSPARENT");
                mat.renderQueue = 3000;

                mat.SetShaderPassEnabled("DepthOnly", false);
                mat.SetShaderPassEnabled("SHADOWCASTER", false);

                mat.SetFloat("_DstBlend", 10);
                mat.SetFloat("_DstBlendAlpha", 10);
                mat.SetFloat("_ZWrite", 0);
            }
        }

        public void ExtractJson(string path)
        {
            string json = JsonConvert.SerializeObject(config, Formatting.Indented);
            File.WriteAllText(path, json);
        }
    }
}