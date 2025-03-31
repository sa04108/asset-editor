using UnityEngine;

namespace Merlin
{
    public enum eShaderSurfaceType
    {
        Opaque,
        Transparent
    }

    public enum eBlendMode
    {
        Alpha,
        Premultiply,
        Additive
    }

    public enum eShaderRenderFace
    {
        Front,
        Back
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

    public class HDRPLitShaderModifier
    {
        public void SetSurfaceType(Material mat, eShaderSurfaceType mode)
        {
            mat.SetInt("_Surface", (int)mode);

            if (mode == eShaderSurfaceType.Opaque)
            {
                mat.SetOverrideTag("RenderType", null);
                mat.DisableKeyword("_ENABLE_FOG_ON_TRANSPARENT");
                mat.DisableKeyword("_SURFACE_TYPE_TRANSPARENT");
                mat.renderQueue = 2225;

                mat.SetInt("_AlphaDstBlend", 0);
                mat.SetInt("_DstBlend", 0);
                mat.SetInt("_DstBlend2", 0);
                mat.SetInt("_StencilRefDepth", 8);
                mat.SetInt("_StencilRefGBuffer", 10);
                mat.SetInt("_StencilRefMV", 40);
                mat.SetInt("_ZTestDepthEqualForOpaque", 3);
                mat.SetInt("_ZWrite", 1);
            }
            else if (mode == eShaderSurfaceType.Transparent)
            {
                mat.SetOverrideTag("RenderType", "Transparent");
                mat.EnableKeyword("_ENABLE_FOG_ON_TRANSPARENT");
                mat.EnableKeyword("_SURFACE_TYPE_TRANSPARENT");
                mat.renderQueue = 3000;

                mat.SetInt("_DstBlend2", 10);
                mat.SetInt("_StencilRefDepth", 0);
                mat.SetInt("_StencilRefGBuffer", 2);
                mat.SetInt("_StencilRefMV", 32);
                mat.SetInt("_ZTestDepthEqualForOpaque", 4);
                mat.SetInt("_ZWrite", 0);

                SetBlendMode(mat, (eBlendMode)mat.GetInt("_BlendMode"));
            }
        }

        public void SetBlendMode(Material mat, eBlendMode mode)
        {
            mat.SetInt("_BlendMode", (int)mode);

            if (mode == eBlendMode.Alpha)
            {
                mat.SetInt("_BlendMode", 0);
                mat.SetInt("_AlphaDstBlend", 10);
                mat.SetInt("_DstBlend", 10);
            }
            else if (mode == eBlendMode.Premultiply)
            {
                mat.SetInt("_BlendMode", 1);
                mat.SetInt("_AlphaDstBlend", 1);
                mat.SetInt("_DstBlend", 1);
            }
            else if (mode == eBlendMode.Additive)
            {
                mat.SetInt("_BlendMode", 4);
                mat.SetInt("_AlphaDstBlend", 10);
                mat.SetInt("_DstBlend", 10);
            }
        }

        public void SetRenderFace(Material mat, eShaderRenderFace mode)
        {
            if (mode == eShaderRenderFace.Front)
            {
                mat.SetInt("_CullMode", 1);
                mat.SetInt("_CullModeForward", 1);
                mat.SetInt("_OpaqueCullMode", 1);
                mat.SetInt("_TransparentCullMode", 1);
            }
            else if (mode == eShaderRenderFace.Back)
            {
                mat.SetInt("_CullMode", 2);
                mat.SetInt("_CullModeForward", 2);
                mat.SetInt("_OpaqueCullMode", 2);
                mat.SetInt("_TransparentCullMode", 2);
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