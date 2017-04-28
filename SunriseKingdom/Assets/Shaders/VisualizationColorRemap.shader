Shader "Custom/VisualizationColorRemap"
{
	Properties
	{
		_MainTex ("Texture", 2D) = "white" {}
		
		_ColorRemap("Color Remap", 2D) = "white" {}
	}
		SubShader
	{
		Pass
		{
			CGPROGRAM
			#pragma vertex vert_img
			#pragma fragment frag
			// make fog work
			#pragma multi_compile_fog

			#include "UnityCG.cginc"

			uniform sampler2D _MainTex;
			fixed4 frag (v2f_img i) : COLOR
			{
				// sample the texture
				fixed4 col = tex2D(_MainTex, i.uv);

				float3 norm = normalize(col);

				float3 div = float3(0.1, 0.1, 0.1) * norm.z;
				float3 rbcol = 0.5 + 0.6 * cross(norm.xyz, float3(0.5, -0.4, 0.5));

				return float4(div + rbcol, 0.0);
			}
			ENDCG
		}
	}
}
