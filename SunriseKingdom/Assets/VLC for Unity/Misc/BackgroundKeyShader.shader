Shader "Custom/BackgroundKeyShader" {
	Properties {
		_MainTex ("Base (RGB)", 2D) = "white" {}
		_ColorKey ("Color Key", Color) = (0,0,0,1)
		_ColorSensitivity ("Color Sensitivity", Float) = 0.6 
	}
	SubShader {
		Pass {
			Tags { "RenderType"="Opaque" }
			LOD 200
		
			CGPROGRAM

			#pragma vertex VertexShaderFunction
			#pragma fragment FragmentShaderFunction
		
			#include "UnityCG.cginc"

			struct VertexData{
				float4 position : POSITION;
				float2 uvs : TEXCOORD0;
			};

			struct VertexToFragment	{
				float4 position : SV_POSITION;
				float2 uvs : TEXCOORD0;
			};

			VertexToFragment VertexShaderFunction(VertexData input)	{
				VertexToFragment result;
				result.position = mul (UNITY_MATRIX_MVP, input.position);
				result.uvs = input.uvs;
				return result;
			}
		
			sampler2D _MainTex;
			float3 _ColorKey;
			float _ColorSensitivity;

			float4 FragmentShaderFunction(VertexToFragment input) : SV_Target	{
				float4 color = tex2D(_MainTex, input.uvs);
			
				float R = abs(color.r - _ColorKey.r);
				float G = abs(color.g - _ColorKey.g);
				float B = abs(color.b - _ColorKey.b);

				if (R < _ColorSensitivity && G < _ColorSensitivity && B < _ColorSensitivity){
					return float4(0.0f, 0.0f, 0.0f, 0.0f);
				}

				return color;
			}
			ENDCG
		}
	}
}
