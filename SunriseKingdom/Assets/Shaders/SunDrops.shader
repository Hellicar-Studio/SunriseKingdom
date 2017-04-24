Shader "Custom/SunDrops"
{
	Properties
	{
		XOffset("X Offset", Range(0.0, 6.5)) = 0.0
		YOffset("X Offset", Range(0.0, 1.0)) = 0.0
		size("Size", Range(0.0, 1.0)) = 0.01
		Speed("Speed", Range(0.001, 1.0)) = 1.0
		WobbleSpeed("Wobble Speed", Range(0.0, 1.0)) = 0.0

	}
	SubShader
	{
		Tags{ "RenderType" = "Opaque" }
		LOD 100

		Pass
		{
			CGPROGRAM
			#pragma vertex vert
			#pragma fragment frag
					// make fog work
			#pragma multi_compile_fog

			#include "UnityCG.cginc"

			struct appdata
			{
				float4 vertex : POSITION;
				float2 uv : TEXCOORD0;
			};

			struct v2f
			{
				float2 uv : TEXCOORD0;
				UNITY_FOG_COORDS(1)
					float4 vertex : SV_POSITION;
			};

			uniform float size;
			uniform float Speed;
			uniform float WobbleSpeed;

			//uniform float4 Colors[3][365];
			uniform float4 Colors1[365];
			uniform float4 Colors2[365];
			uniform float4 Colors3[365];
			uniform float4 Colors4[365];
			uniform float4 Colors5[365];
			// uniform float4 Colors6[365];
			// uniform float4 Colors7[365];
			// uniform float4 Colors8[365];
			// uniform float4 Colors9[365];
			// uniform float4 Colors10[365];
			// uniform float4 Colors11[365];
			// uniform float4 Colors12[365];

			uniform int days;
			uniform int shotsPerDay;

			v2f vert(appdata v)
			{
				v2f o;
				o.vertex = UnityObjectToClipPos(v.vertex);
				o.uv = v.uv;
				return o;
			}

			float hash(float n)
			{
				return frac(sin(n)*43758.5453);
			}

			float noise(in float2 x)
			{
				float2 p = floor(x);
				float2 f = frac(x);
				f = f*f*(3.0 - 2.0*f);
				float n = p.x + p.y*57.0;
				return lerp(lerp(hash(n + 0.0), hash(n + 1.0), f.x),
					lerp(hash(n + 57.0), hash(n + 58.0), f.x), f.y);
			}

			float map(float value, float low1, float high1, float low2, float high2)
			{
				return low2 + (value - low1) * (high2 - low2) / (high1 - low1);
			}

			fixed4 frag(v2f i) : SV_Target
			{
				float4 col = float4(0, 0, 0, 1);
				float2 uv = i.uv.xy;
				uv.x *= _ScreenParams.x / _ScreenParams.y;
				float backgroundHue = 0.0 + 0.2*uv.y;
				float3 color = float3(backgroundHue, backgroundHue, backgroundHue);

				// bubbles
				for (int i = 0; i<days; i++)
				{
					// bubble seeds
					float rad = min(_Time.y * Speed - (float)i / 10.0, size);//siz;
					if (rad < 0) rad = 0;
					float pox = map(sin(float(i)*546.13 + 7.5), -1, 1, 0.2, 6.2);
					float poy = map(sin(float(i)*321.22 + 4.1), -1, 1, 0.1, 0.9);

					// buble size, position and color
					float2  pos = float2(pox, poy);//-1.0 - rad + (2.0 + 2.0*rad)*fmod(pha + 0.1*_Time.y*(0.2 + 0.8*siz), 1.0));
					float dis = length(uv - pos);
					float3  col = Colors1[i] * 1.0 / days * days;
																	// render
					float f = length(uv - pos) / rad;
					float x = fmod(uv.x, 0.2);
					float y = fmod(uv.y, 0.2);

					f += noise((uv - (6.5 - uv)*f + _Time.y * WobbleSpeed)*3.0);
					f = sqrt(clamp(1.0 - f*f, 0.0, 1.0));
					color += f * f * f * f * f * f * f * Colors1[i];//col.xyz * (1.0 - smoothstep(rad*0.55, rad, dis)) * f;
				}

				col = float4(color, 1);
				return col;
			}
			ENDCG
		}
	}
}
