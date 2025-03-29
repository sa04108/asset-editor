using UnityEngine;

namespace Merlin
{
    public enum eShaderWorkflowMode
    {
        Specular,
        Metallic
    }

    public enum eShaderSurfaceType
    {
        Opaque,
        Transparent
    }

    public enum eShaderRenderFace
    {
        Both,
        Back,
        Front
    }

    public enum eShaderTextureMap
    {
        BaseMap,
        MetallicMap,
        NormalMap,
        OcclusionMap,
        EmissionMap,
        DetailMaskMap
    }

    public enum eEmissionGlobalIllumination
    {
        NoEmission,
        Realtime,
        Baked,
        None
    }

    public class MaterialPropertyState
    {
        public MaterialPropertyState()
        {
        }

        public void SetWorkflowMode(Material mat, eShaderWorkflowMode mode)
        {
            mat.SetInt("_WorkflowMode", (int)mode);

            if (mode == eShaderWorkflowMode.Specular)
            {
                mat.EnableKeyword("_SPECULAR_SETUP");
            }
            else if (mode == eShaderWorkflowMode.Metallic)
            {
                mat.DisableKeyword("_SPECULAR_SETUP");
            }
        }

        public void SetSurfaceType(Material mat, eShaderSurfaceType mode)
        {
            mat.SetInt("_Surface", (int)mode);

            if (mode == eShaderSurfaceType.Opaque)
            {
                mat.SetOverrideTag("RenderType", "Opaque");
                mat.DisableKeyword("_ALPHAPREMULTIPLY_ON");
                mat.DisableKeyword("_SURFACE_TYPE_TRANSPARENT");
                mat.renderQueue = 2000;

                mat.SetShaderPassEnabled("DepthOnly", true);
                mat.SetShaderPassEnabled("SHADOWCASTER", true);

                mat.SetInt("_DstBlend", 0);
                mat.SetInt("_DstBlendAlpha", 0);
                mat.SetInt("_Surface", 0);
                mat.SetInt("_ZWrite", 1);
            }
            else if (mode == eShaderSurfaceType.Transparent)
            {
                mat.SetOverrideTag("RenderType", "Transparent");
                mat.EnableKeyword("_ALPHAPREMULTIPLY_ON");
                mat.EnableKeyword("_SURFACE_TYPE_TRANSPARENT");
                mat.renderQueue = 3000;

                mat.SetShaderPassEnabled("DepthOnly", false);
                mat.SetShaderPassEnabled("SHADOWCASTER", false);

                mat.SetInt("_DstBlend", 10);
                mat.SetInt("_DstBlendAlpha", 10);
                mat.SetInt("_Surface", 1);
                mat.SetInt("_ZWrite", 0);
            }
        }

        public void SetRenderFace(Material mat, eShaderRenderFace mode)
        {
            mat.SetInt("_Cull", (int)mode);

            if (mode == eShaderRenderFace.Both ||
                mode == eShaderRenderFace.Back)
            {
                mat.doubleSidedGI = true;
            }
            else if (mode == eShaderRenderFace.Front)
            {
                mat.doubleSidedGI = false;
            }
        }

        public void SetAlphaClipping(Material mat, bool alphaClipping)
        {
            if (alphaClipping)
            {
                // is surface type Opaque?
                if (mat.GetInt("_Surface") == 0)
                {
                    mat.SetOverrideTag("RenderType", "TransparentCutout");
                    mat.SetInt("_AlphaToMask", 1);
                }

                mat.EnableKeyword("_ALPHATEST_ON");
                mat.renderQueue = 2450;
                mat.SetInt("_AlphaClip", 1);
            }
            else
            {
                if (mat.GetInt("_Surface") == 0)
                {
                    mat.SetOverrideTag("RenderType", "Opaque");
                    mat.SetInt("_AlphaToMask", 0);
                }

                mat.DisableKeyword("_ALPHATEST_ON");
                mat.renderQueue = 2000;
                mat.SetInt("_AlphaClip", 0);
            }
        }

        public void SetAlphaCutoff(Material mat, float threshold = 0.5f)
        {
            mat.SetFloat("_Cutoff", threshold);
        }

        public void SetReceiveShadows(Material mat, bool receiveShadows)
        {
            mat.SetInt("_ReceiveShadows", receiveShadows ? 1 : 0);

            if (receiveShadows)
            {
                mat.DisableKeyword("_RECEIVE_SHADOWS_OFF");
            }
            else
            {
                mat.EnableKeyword("_RECEIVE_SHADOWS_OFF");
            }
        }

        public void SetTextureMap(Material mat, eShaderTextureMap map, Texture tex = null)
        {
            switch (map)
            {
                case eShaderTextureMap.BaseMap:
                    mat.SetTexture("_BaseMap", tex);
                    mat.SetTexture("_MainTex", tex);
                    break;

                case eShaderTextureMap.MetallicMap:
                    mat.SetTexture("_MetallicGlossMap", tex);

                    if (tex != null)
                        mat.EnableKeyword("_METALLICSPECGLOSSMAP");
                    else
                        mat.DisableKeyword("_METALLICSPECGLOSSMAP");
                    break;

                case eShaderTextureMap.NormalMap:
                    mat.SetTexture("_BumpMap", tex);
                    break;

                case eShaderTextureMap.OcclusionMap:
                    mat.SetTexture("_OcclusionMap", tex);

                    if (tex != null)
                        mat.EnableKeyword("_OCCLUSIONMAP");
                    else
                        mat.DisableKeyword("_OCCLUSIONMAP");
                    break;

                case eShaderTextureMap.EmissionMap:
                    mat.SetTexture("_EmissionMap", tex);
                    break;

                case eShaderTextureMap.DetailMaskMap:
                    mat.SetTexture("_DetailMask", tex);
                    break;
            }
        }

        public void SetEmission(Material mat, eEmissionGlobalIllumination mode)
        {
            switch (mode)
            {
                case eEmissionGlobalIllumination.NoEmission:
                    mat.DisableKeyword("_EMISSION");
                    mat.globalIlluminationFlags = (MaterialGlobalIlluminationFlags)4;
                    break;
                case eEmissionGlobalIllumination.Realtime:
                    mat.EnableKeyword("_EMISSION");
                    mat.globalIlluminationFlags = (MaterialGlobalIlluminationFlags)1;
                    break;
                case eEmissionGlobalIllumination.Baked:
                    mat.DisableKeyword("_EMISSION");
                    mat.globalIlluminationFlags = (MaterialGlobalIlluminationFlags)6;
                    break;
                case eEmissionGlobalIllumination.None:
                    mat.EnableKeyword("_EMISSION");
                    mat.globalIlluminationFlags = 0;
                    break;
            }
        }
    }
}