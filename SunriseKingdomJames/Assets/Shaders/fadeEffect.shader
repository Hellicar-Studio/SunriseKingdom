Shader "Custom/fade" {
	Properties{
		_MainTex("Base (RGB)", 2D) = "white" {}
		_Percentage("Percentage", Range(0, 1)) = 0.1
		_FadeDuration("Fade Duration", Range(0, 0.5)) = 0.01
	}
	SubShader{
		Pass{
			CGPROGRAM
			#pragma vertex vert_img
			#pragma fragment frag

			#include "UnityCG.cginc"

			uniform sampler2D _MainTex;
			uniform float _Percentage;
			uniform float _FadeDuration;

			float4 frag(v2f_img i) : COLOR{
				float4 result = tex2D(_MainTex, i.uv);
				//Fade Out
				result *= 1.0 - smoothstep(1.0 - _FadeDuration, 1.0, _Percentage);
				//Fade In
				result *= smoothstep(0.0, _FadeDuration, _Percentage);

				return result;


			}
			ENDCG
		}
	}
}