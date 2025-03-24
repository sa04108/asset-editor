///
/// Shader for pixel selecting screen in color picking window 
/// 19x19 pixels in size, with single pixel frame in the middle 
///
Shader "UI/ColorPicker/Pixels"
{
	Properties
	{
		[HideInInspector] _MainTex("Screenshot", 2D) = "black" {}
		_Color1("Cell Color", Color) = (0.5,0.5,0.5,1)
		_Color2("Selector Color", Color) = (1,1,1,1)
		_Screen ("Screen Parameters", Vector) = (0, 0, 256, 256)
		
		[HideInInspector] _StencilComp("Stencil Comparison", Float) = 8
		[HideInInspector] _Stencil("Stencil ID", Float) = 0
		[HideInInspector] _StencilOp("Stencil Operation", Float) = 0
		[HideInInspector] _StencilWriteMask("Stencil Write Mask", Float) = 255
		[HideInInspector] _StencilReadMask("Stencil Read Mask", Float) = 255

		[HideInInspector] _ColorMask("Color Mask", Float) = 15
		[HideInInspector] [Toggle(UNITY_UI_ALPHACLIP)] _UseUIAlphaClip("Use Alpha Clip", Float) = 0
	}
	SubShader
	{
		Tags
		{
			"Queue" = "Transparent"
			"IgnoreProjector" = "True"
			"RenderType" = "Transparent"
			"PreviewType" = "Plane"
		}

		Cull Off
		Lighting Off
		ZWrite Off
		ZTest[unity_GUIZTestMode]
		Blend SrcAlpha OneMinusSrcAlpha
		ColorMask[_ColorMask]

		Stencil
		{
			Ref[_Stencil]
			Comp[_StencilComp]
			Pass[_StencilOp]
			ReadMask[_StencilReadMask]
			WriteMask[_StencilWriteMask]
		}

		Pass
		{
			CGPROGRAM
			#pragma vertex vert
			#pragma fragment frag
			#pragma target 2.0
			#pragma multi_compile __ UNITY_UI_ALPHACLIP 			
			#include "UnityCG.cginc"
			#include "UnityUI.cginc"

			#define PIXELS 19.0

			float draw_cell(float2 uv, float smooth) 
			{
				float coord = uv.x * PIXELS % 1.0;
				float result = 0;
				result += smoothstep(1.0 - smooth, 1.0, coord);
				result += smoothstep(0.0 + smooth, 0.0, coord);
				coord = uv.y * PIXELS % 1.0;
				result += smoothstep(1.0 - smooth, 1.0, coord);
				result += smoothstep(0.0 + smooth, 0.0, coord);
				return clamp(result, 0, 1);
			}

			float draw_rect(float2 uv, float rmin, float rmax, float smooth, float thickness)
			{
				float result = 0;
				float a = rmin - smooth;
				float b = rmin + smooth;
				float c = rmax + smooth;
				float d = rmax - smooth;
				result += smoothstep(a, b, uv.x) * smoothstep(c, d, uv.x);
				result *= smoothstep(a, b, uv.y) * smoothstep(c, d, uv.y);
				float temp = 0;
				temp += smoothstep(a + thickness, b + thickness, uv.x) * smoothstep(c - thickness, d - thickness, uv.x);
				temp *= smoothstep(a + thickness, b + thickness, uv.y) * smoothstep(c - thickness, d - thickness, uv.y);
				return result * (1.0 - temp);
			}

			struct appdata
			{
				float4 vertex : POSITION;
				float2 uv : TEXCOORD0;
				float4 color    : COLOR;
			};

			struct v2f
			{
				float4 vertex : SV_POSITION;
				float2 uv : TEXCOORD0;
				float2 uv2 : TEXCOORD1;
				fixed4 color : COLOR;
			};

			uniform sampler2D _MainTex;
			uniform float4 _Color1;
			uniform float4 _Color2;
			uniform float4 _Screen;
			
			v2f vert (appdata v)
			{
				v2f o;
				o.vertex = UnityObjectToClipPos(v.vertex);
				o.uv = v.uv;
				
				o.uv2 = (v.uv + float2(-0.475, -0.475)) * (PIXELS / _Screen.zw) + (_Screen.xy / _Screen.zw);
				
				o.color = v.color;
				return o;
			}
			
			fixed4 frag(v2f i) : SV_Target
			{
				fixed4 col = tex2D(_MainTex, i.uv2);
				fixed4 pix = _Color1;
				
				pix.a = draw_cell(i.uv, 0.1);
				float rect = draw_rect(i.uv, 0.47, 0.53, 0.0025, 0.005);

				pix.rgb = lerp(pix.rgb, _Color2.rgb, rect);
				pix.a = clamp(pix.a + rect, 0, 1);

				col.rgb = lerp(col.rgb, pix.rgb, pix.a);
				col.a = i.color.a;
				
#ifdef UNITY_COLORSPACE_GAMMA
				return col;
#endif
				return fixed4(GammaToLinearSpace(col.rgb), col.a);
			}

			ENDCG
		}
	}
}
