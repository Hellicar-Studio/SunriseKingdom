Shader "Custom/SunLife"
{
	Properties
	{
		_MainTex ("Texture", 2D) = "white" {}
		_InitialTex("Texture", 2D) = "white" {}
	}
	SubShader
	{
		ZTest Always Cull Off ZWrite Off
		Fog{ Mode off }

		Pass
		{
			CGPROGRAM
			#pragma vertex vert_img
			#pragma fragment frag
			
			#include "UnityCG.cginc"

			uniform sampler2D _MainTex;
			uniform sampler2D _InitialTex;
			uniform sampler2D _RandomTex;
			float4 _MainTex_ST;
			uniform float3 _Mouse;
			const float pi = 3.14159265358979323846264338327950288419716939937510582097494459230781640;
			const float pi2 = 3.14159265358979323846264338327950288419716939937510582097494459230781640 / 2.0;

			float random(float2 uv)
			{
				return frac(sin(dot(uv, float2(12.9898, 78.233))) * 43758.5453);
			}

			float4 get_pixel(float2 uv, float x_offset, float y_offset) //  Remember to normalize x_offset and y_offset!
			{
				return tex2D(_MainTex, (uv.xy) + (float2(x_offset, y_offset) / _ScreenParams.xy));
			}

			float step_simulation(float2 uv)
			{
				float val = get_pixel(uv, 0.0, 0.0).r;

				val += random(uv)*val*0.15; // errosion

				val = get_pixel(
					uv, 
					sin(get_pixel(uv, val, 0.0).r - get_pixel(uv, -val, 0.0) + pi).r  * val * 0.4,
					cos(get_pixel(uv, 0.0, -val).r - get_pixel(uv, 0.0, val) - pi2).r * val * 0.4
				).r;

				val *= 1.0001;

				return val;
			}
			
			fixed4 frag (v2f_img i) : SV_Target
			{
				fixed4 fragColor;// = //tex2D(_MainTex, i.uv.xy);
				//fragColor.r = float4(random(i.uv.xy), 0.0, 0.0, 1.0);
				float val = step_simulation(i.uv.xy);

				//if (iMouse.z > 0.0)
				//val += smoothstep(1.0 / 10.0, 0.5, length(float2(0.5, 0.5) - i.uv.xy));

				float4 color = pow(float4(cos(val), tan(val), sin(val), 1.0) * 0.5 + 0.5, float4(0.5, 0.5, 0.5, 0.5));
				fragColor = color/2;


				return fragColor;//float4(val, 0.0, 0.0, 1.0);//fragColor;
			}
			ENDCG
		}
	}
}
