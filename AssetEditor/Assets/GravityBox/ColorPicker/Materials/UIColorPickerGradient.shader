///
/// Shader for various gradients in picker window
/// including HSV ring and box selection gradients
///
Shader "UI/ColorPicker/Gradient"
{
	Properties
	{
		_HSV("Value", Vector) = (0,0,0,0)
		_Thickness ("RingSize", Range(0.005,0.5)) = 0.25
		_Checker ("Checker Size", Vector) = (16, 4, 0, 0)
		_Mask("RGBMask", Vector) = (0,0,0,0)

		_Radius("Radius", Float) = 0.1
		_Intensity("Intensity", Float) = 0

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
			#pragma multi_compile _ _RING _BOX _LINE _SAT _ALPHA _RGB
			#pragma multi_compile __ UNITY_UI_ALPHACLIP 			
			#pragma multi_compile _GRADIENT _SWATCH _SOLID
			#include "UnityCG.cginc"
			#include "UnityUI.cginc"

			/*
			#define UNITY_PI            3.14159265359f
			#define UNITY_TWO_PI        6.28318530718f
			#define UNITY_FOUR_PI       12.56637061436f
			#define UNITY_INV_PI        0.31830988618f
			#define UNITY_INV_TWO_PI    0.15915494309f
			#define UNITY_INV_FOUR_PI   0.07957747155f
			#define UNITY_HALF_PI       1.57079632679f
			#define UNITY_INV_HALF_PI   0.636619772367f
			*/

			half3 HSVtoRGB(half3 arg1)
			{
				half4 K = half4(1.0, 2.0 / 3.0, 1.0 / 3.0, 3.0);
				half3 P = abs(frac(arg1.xxx + K.xyz) * 6.0 - K.www);
				return arg1.z * lerp(K.xxx, saturate(P - K.xxx), arg1.y);
			}

			float ring(float2 uv, float2 pos, float radius, float thickness) 
			{
				float a = 0;
				a = smoothstep(radius, radius - 0.005, distance(uv, pos));
				a *= smoothstep(radius - 0.005 - thickness, radius - thickness, distance(uv, pos));
				return a;
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
				fixed4 color : COLOR;
			};

			float _Thickness;
			float _Radius;
			float4 _HSV;
			float4 _Mask;
			float4 _Checker;
			float _Intensity;
			
			v2f vert (appdata v)
			{
				v2f o;
				o.vertex = UnityObjectToClipPos(v.vertex);
				o.uv = v.uv;
				o.color = v.color;
				return o;
			}
			
			fixed4 frag(v2f i) : SV_Target
			{
				fixed4 col = fixed4(1, 1, 1, 1);
				col.rgb *= i.uv.x;
#ifdef _RING
				float2 coord = i.uv - float2(0.5, 0.5);
				float angle = atan2(coord.y, coord.x) * UNITY_INV_TWO_PI;
				col.rgb = HSVtoRGB(float3(angle, 1, 1));

				float r2 = _Thickness / 2.0;
				float2 pos = float2(cos(_HSV.x * UNITY_TWO_PI), sin(_HSV.x * UNITY_TWO_PI)) * (0.5 - r2) + 0.5;
				
				col.rgb += fixed3(1, 1, 1) * ring(i.uv, pos, r2, 0.01);
				
				col.a = ring(i.uv, float2(0.5, 0.5), 0.5, _Thickness);
#endif
#ifdef _BOX
				float3 a = float3(1,1,1) * i.uv.y;
				float3 b = HSVtoRGB(float3(_HSV.x, 1, 1)) * i.uv.y;
				col.rgb = lerp(a, b, i.uv.x);
				float r2 = _Thickness / 2.0;
				
				col.a = smoothstep(1 - r2, 1 - r2 - 0.005, i.uv.x);
				col.a *= smoothstep(r2, r2 + 0.005, i.uv.x);
				col.a *= smoothstep(1 - r2, 1 - r2 - 0.005, i.uv.y);
				col.a *= smoothstep(r2, r2 + 0.005, i.uv.y);

				col += ring(i.uv, float2(_HSV.y * (1 - r2 * 2) + r2, _HSV.z * (1 - r2 * 2) + r2), r2, 0.02);
#endif
#ifdef _ALPHA
				float t = floor(i.uv.x * _Checker.x) + floor(i.uv.y * _Checker.y);
				col.rgb = lerp(fixed3(1, 1, 1), fixed3(0.5, 0.5, 0.5), t % 2.0);
	#ifdef _GRADIENT			
				col.rgb = lerp(col.rgb, HSVtoRGB(_HSV.xyz), i.uv.x);
	#elif _SWATCH
				col.rgb = lerp(col.rgb, i.color.rgb, saturate(step(i.uv.x + i.uv.y, 1.0) + i.color.a));
				return col;
	#else
				col.rgb = lerp(col.rgb, i.color.rgb, i.color.a);
				return col;
	#endif
#endif
#ifdef _LINE
				col.rgb = HSVtoRGB(float3(i.uv.x, 1 ,1));
#endif
#ifdef _SAT
				col.rgb = lerp(float3(1, 1, 1), HSVtoRGB(float3(_HSV.x, 1, 1)), i.uv.x);
#endif
#ifdef _RGB
				col.rgb = HSVtoRGB(_HSV.xyz) * _Mask.rgb;
				col.rgb += (1 - _Mask.rgb) * i.uv.x;
#endif
#ifdef UNITY_COLORSPACE_GAMMA
				return col;
#endif
				return fixed4(GammaToLinearSpace(col.rgb), col.a);
			}

			ENDCG
		}
	}

	CustomEditor "GravityBox.ColorPicker.UIColorPickerGradientEditor"
}
