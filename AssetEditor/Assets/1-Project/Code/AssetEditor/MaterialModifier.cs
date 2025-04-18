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

    public enum eBlendMode
    {
        Alpha,
        Premultiply,
        Additive,
        Multiply
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
        None,
        Realtime,
        Baked
    }

    public class URPLitShaderModifier
    {
        public URPLitShaderModifier()
        {
        }

        public void SetWorkflowMode(Material mat, eShaderWorkflowMode mode)
        {
            mat.SetInt("_WorkflowMode", (int)mode);

            if (mode == eShaderWorkflowMode.Specular)
            {
                mat.EnableKeyword("_SPECULAR_SETUP");
                mat.EnableKeyword("_METALLICSPECGLOSSMAP");
            }
            else if (mode == eShaderWorkflowMode.Metallic)
            {
                mat.DisableKeyword("_SPECULAR_SETUP");
                mat.DisableKeyword("_METALLICSPECGLOSSMAP");
            }
        }

        public void SetSurfaceType(Material mat, eShaderSurfaceType mode)
        {
            mat.SetInt("_Surface", (int)mode);

            if (mode == eShaderSurfaceType.Opaque)
            {
                mat.SetOverrideTag("RenderType", "Opaque");
                mat.DisableKeyword("_SURFACE_TYPE_TRANSPARENT");
                mat.DisableKeyword("_ALPHAPREMULTIPLY_ON");
                mat.DisableKeyword("_ALPHAMODULATE_ON");
                mat.renderQueue = 2000;

                mat.SetInt("_ZWrite", 1);
                mat.SetInt("_DstBlend", 0);
                mat.SetInt("_DstBlendAlpha", 0);
                mat.SetInt("_SrcBlend", 1);
                mat.SetInt("_SrcBlendAlpha", 1);

                mat.SetShaderPassEnabled("DepthOnly", true);
                mat.SetShaderPassEnabled("SHADOWCASTER", true);
            }
            else if (mode == eShaderSurfaceType.Transparent)
            {
                mat.SetOverrideTag("RenderType", "Transparent");
                mat.EnableKeyword("_SURFACE_TYPE_TRANSPARENT");
                mat.renderQueue = 3000;

                mat.SetInt("_ZWrite", 0);

                mat.SetShaderPassEnabled("DepthOnly", false);
                mat.SetShaderPassEnabled("SHADOWCASTER", false);

                SetBlendMode(mat, (eBlendMode)mat.GetInt("_Blend"));
            }

            SetAlphaClipping(mat, mat.GetInt("_AlphaClip") == 1);
        }

        public void SetBlendMode(Material mat, eBlendMode mode)
        {
            mat.SetInt("_Blend", (int)mode);

            if (mode == eBlendMode.Alpha)
            {
                mat.EnableKeyword("_ALPHAPREMULTIPLY_ON");
                mat.DisableKeyword("_ALPHAMODULATE_ON");
                mat.SetInt("_DstBlend", 10);
                mat.SetInt("_DstBlendAlpha", 10);
                mat.SetInt("_SrcBlend", 1);
                mat.SetInt("_SrcBlendAlpha", 1);
            }
            else if (mode == eBlendMode.Premultiply)
            {
                mat.DisableKeyword("_ALPHAPREMULTIPLY_ON");
                mat.DisableKeyword("_ALPHAMODULATE_ON");
                mat.SetInt("_DstBlend", 10);
                mat.SetInt("_DstBlendAlpha", 10);
                mat.SetInt("_SrcBlend", 1);
                mat.SetInt("_SrcBlendAlpha", 1);
            }
            else if (mode == eBlendMode.Additive)
            {
                mat.EnableKeyword("_ALPHAPREMULTIPLY_ON");
                mat.DisableKeyword("_ALPHAMODULATE_ON");
                mat.SetInt("_DstBlend", 1);
                mat.SetInt("_DstBlendAlpha", 1);
                mat.SetInt("_SrcBlend", 1);
                mat.SetInt("_SrcBlendAlpha", 1);
            }
            else if (mode == eBlendMode.Multiply)
            {
                mat.DisableKeyword("_ALPHAPREMULTIPLY_ON");
                mat.EnableKeyword("_ALPHAMODULATE_ON");
                mat.SetInt("_DstBlend", 0);
                mat.SetInt("_DstBlendAlpha", 1);
                mat.SetInt("_SrcBlend", 2);
                mat.SetInt("_SrcBlendAlpha", 0);
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
                if (mat.GetInt("_Surface") == (int)eShaderSurfaceType.Opaque)
                {
                    mat.SetOverrideTag("RenderType", "TransparentCutout");
                    mat.SetInt("_AlphaToMask", 1);
                    mat.renderQueue = 2450;
                }

                mat.EnableKeyword("_ALPHATEST_ON");
                mat.SetInt("_AlphaClip", 1);
            }
            else
            {
                if (mat.GetInt("_Surface") == (int)eShaderSurfaceType.Opaque)
                {
                    mat.SetOverrideTag("RenderType", "Opaque");
                    mat.renderQueue = 2000;
                }

                mat.DisableKeyword("_ALPHATEST_ON");
                mat.SetInt("_AlphaClip", 0);
                mat.SetInt("_AlphaToMask", 0);
            }
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

        public void SetEmission(Material mat, bool enable)
        {
            if (enable)
            {
                mat.EnableKeyword("_EMISSION");
            }
            else
            {
                mat.DisableKeyword("_EMISSION");
            }
        }

        public void SetEmissionMode(Material mat, eEmissionGlobalIllumination mode)
        {
            mat.globalIlluminationFlags = (MaterialGlobalIlluminationFlags)mode;
        }

        public void SetBaseColor(Material mat, Color color)
        {
            mat.SetColor("_BaseColor", color);
            mat.SetColor("_Color", color);
        }

        public void SetBaseTiling(Material mat, Vector2 tiling)
        {
            mat.SetTextureScale("_BaseMap", tiling);
            mat.SetTextureScale("_MainTex", tiling);
        }

        public void SetBaseOffset(Material mat, Vector2 offset)
        {
            mat.SetTextureOffset("_BaseMap", offset);
            mat.SetTextureOffset("_MainTex", offset);
        }
    }

    public class URPUnlitShaderModifier
    {
        public void SetSurfaceType(Material mat, eShaderSurfaceType mode)
        {
            mat.SetInt("_Surface", (int)mode);

            if (mode == eShaderSurfaceType.Opaque)
            {
                mat.SetOverrideTag("RenderType", "Opaque");
                mat.DisableKeyword("_SURFACE_TYPE_TRANSPARENT");
                mat.DisableKeyword("_ALPHAMODULATE_ON");
                mat.renderQueue = 2000;

                mat.SetInt("_ZWrite", 1);
                mat.SetInt("_DstBlend", 0);
                mat.SetInt("_DstBlendAlpha", 0);
                mat.SetInt("_SrcBlend", 1);
                mat.SetInt("_SrcBlendAlpha", 1);

                mat.SetShaderPassEnabled("DepthOnly", true);
                mat.SetShaderPassEnabled("SHADOWCASTER", true);
            }
            else if (mode == eShaderSurfaceType.Transparent)
            {
                mat.SetOverrideTag("RenderType", "Transparent");
                mat.EnableKeyword("_SURFACE_TYPE_TRANSPARENT");
                mat.renderQueue = 3000;

                mat.SetInt("_ZWrite", 0);

                mat.SetShaderPassEnabled("DepthOnly", false);
                mat.SetShaderPassEnabled("SHADOWCASTER", false);

                SetBlendMode(mat, (eBlendMode)mat.GetInt("_Blend"));
            }

            SetAlphaClipping(mat, mat.GetInt("_AlphaClip") == 1);
        }

        public void SetBlendMode(Material mat, eBlendMode mode)
        {
            mat.SetInt("_Blend", (int)mode);

            if (mode == eBlendMode.Alpha)
            {
                mat.DisableKeyword("_ALPHAMODULATE_ON");
                mat.SetInt("_DstBlend", 10);
                mat.SetInt("_DstBlendAlpha", 10);
                mat.SetInt("_SrcBlend", 5);
                mat.SetInt("_SrcBlendAlpha", 1);
            }
            else if (mode == eBlendMode.Premultiply)
            {
                mat.DisableKeyword("_ALPHAMODULATE_ON");
                mat.SetInt("_DstBlend", 10);
                mat.SetInt("_DstBlendAlpha", 10);
                mat.SetInt("_SrcBlend", 1);
                mat.SetInt("_SrcBlendAlpha", 1);
            }
            else if (mode == eBlendMode.Additive)
            {
                mat.DisableKeyword("_ALPHAMODULATE_ON");
                mat.SetInt("_DstBlend", 1);
                mat.SetInt("_DstBlendAlpha", 1);
                mat.SetInt("_SrcBlend", 5);
                mat.SetInt("_SrcBlendAlpha", 1);
            }
            else if (mode == eBlendMode.Multiply)
            {
                mat.EnableKeyword("_ALPHAMODULATE_ON");
                mat.SetInt("_DstBlend", 0);
                mat.SetInt("_DstBlendAlpha", 1);
                mat.SetInt("_SrcBlend", 2);
                mat.SetInt("_SrcBlendAlpha", 0);
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
                if (mat.GetInt("_Surface") == (int)eShaderSurfaceType.Opaque)
                {
                    mat.SetOverrideTag("RenderType", "TransparentCutout");
                    mat.SetInt("_AlphaToMask", 1);
                    mat.renderQueue = 2450;
                }

                mat.EnableKeyword("_ALPHATEST_ON");
                mat.SetInt("_AlphaClip", 1);
            }
            else
            {
                if (mat.GetInt("_Surface") == (int)eShaderSurfaceType.Opaque)
                {
                    mat.SetOverrideTag("RenderType", "Opaque");
                    mat.renderQueue = 2000;
                }

                mat.DisableKeyword("_ALPHATEST_ON");
                mat.SetInt("_AlphaClip", 0);
                mat.SetInt("_AlphaToMask", 0);
            }
        }

        public void SetBaseColor(Material mat, Color color)
        {
            mat.SetColor("_BaseColor", color);
            mat.SetColor("_Color", color);
        }

        public void SetBaseTiling(Material mat, Vector2 tiling)
        {
            mat.SetTextureScale("_BaseMap", tiling);
            mat.SetTextureScale("_MainTex", tiling);
        }

        public void SetBaseOffset(Material mat, Vector2 offset)
        {
            mat.SetTextureOffset("_BaseMap", offset);
            mat.SetTextureOffset("_MainTex", offset);
        }
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
    }
}