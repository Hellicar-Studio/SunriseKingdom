Shader "Custom/SunSpots"
{
	Properties
	{
		XOffset("X Offset", Range(0.0, 6.5)) = 0.0
		YOffset("X Offset", Range(0.0, 1.0)) = 0.0
		size("Size", Range(0.0, 1.0)) = 0.01
		Speed("Speed", Range(0.001, 1.0)) = 1.0
		WobbleSpeed("Wobble Speed", Range(0.0, 1.0)) = 0.0
<<<<<<< HEAD

=======
>>>>>>> a65c0369eb9d7203440bc798a2d63f4f1b063aed
	}
	SubShader
	{
		Tags { "RenderType"="Opaque" }
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
<<<<<<< HEAD
			//uniform float4 Colors6[365];
			//uniform float4 Colors7[365];
			//uniform float4 Colors8[365];
			//uniform float4 Colors9[365];
=======
			uniform float4 Colors6[365];
			// uniform float4 Colors7[365];
			// uniform float4 Colors8[365];
			// uniform float4 Colors9[365];
>>>>>>> a65c0369eb9d7203440bc798a2d63f4f1b063aed
			// uniform float4 Colors10[365];
			// uniform float4 Colors11[365];
			// uniform float4 Colors12[365];

<<<<<<< HEAD
			uniform int days;
			uniform int shotsPerDay;
=======
			uniform int days = 10.0;
			uniform int shotsPerDay = 1.0;
>>>>>>> a65c0369eb9d7203440bc798a2d63f4f1b063aed

			v2f vert (appdata v)
			{
				v2f o;
				o.vertex = UnityObjectToClipPos(v.vertex);
				o.uv = v.uv;
				return o;
			}

<<<<<<< HEAD
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
=======
			// 2D Random
			float random(in float2 st) {
				return frac(sin(dot(st.xy,
					float2(12.9898, 78.233)))
					* 43758.5453123);
			}

			// 2D Noise based on Morgan McGuire @morgan3d
			// https://www.shadertoy.com/view/4dS3Wd
			float noise(in float2 st) {
				float2 i = floor(st);
				float2 f = frac(st);

				// Four corners in 2D of a tile
				float a = random(i);
				float b = random(i + float2(1.0, 0.0));
				float c = random(i + float2(0.0, 1.0));
				float d = random(i + float2(1.0, 1.0));

				// Smooth Interpolation

				// Cubic Hermine Curve.  Same as SmoothStep()
				float2 u = f*f*(3.0 - 2.0*f);
				// u = smoothstep(0.,1.,f);

				// Mix 4 coorners porcentages
				return lerp(a, b, u.x) +
					(c - a)* u.y * (1.0 - u.x) +
					(d - b) * u.x * u.y;
>>>>>>> a65c0369eb9d7203440bc798a2d63f4f1b063aed
			}

			float map(float value, float low1, float high1, float low2, float high2) 
			{
				return low2 + (value - low1) * (high2 - low2) / (high1 - low1);
			}
			
			fixed4 frag (v2f i) : SV_Target
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
					float rad = min(_Time.y * Speed - (float)i/10.0, size);//siz;
					if (rad < 0) rad = 0;
					float pox = map(sin(float(i)*546.13 + 7.5), -1, 1, 0.2, 6.2);
					float poy = map(sin(float(i)*321.22 + 4.1), -1, 1, 0.1, 0.9);

<<<<<<< HEAD
					// buble size, position and color
					float2  pos = float2(pox, poy);//-1.0 - rad + (2.0 + 2.0*rad)*fmod(pha + 0.1*_Time.y*(0.2 + 0.8*siz), 1.0));
					float dis = length(uv - pos);
					float3  col = Colors1[i] * 1.0 / days * days;//lerp(float3(0.94, 0.3, 0.0), float3(0.1, 0.4, 0.8), 0.5 + 0.5*sin(float(i)*1.2 + 1.9));
=======
					// bubble size, position and color
					float2  pos = float2(pox, poy);//-1.0 - rad + (2.0 + 2.0*rad)*fmod(pha + 0.1*_Time.y*(0.2 + 0.8*siz), 1.0));
					float dis = length(uv - pos);
					float3  col = Colors1[i];//lerp(float3(0.94, 0.3, 0.0), float3(0.1, 0.4, 0.8), 0.5 + 0.5*sin(float(i)*1.2 + 1.9));
>>>>>>> a65c0369eb9d7203440bc798a2d63f4f1b063aed
					// col+= 8.0*smoothstep( rad*0.95, rad, dis );

					// render
					float f = length(uv - pos) / rad;
					float x = fmod(uv.x, 0.2);
					float y = fmod(uv.y, 0.2);

<<<<<<< HEAD
					f += noise(uv - (6.5 - uv) + _Time.y * WobbleSpeed);
=======
					f += noise(uv - (6.5 - uv) / (size*0.8) + _Time.y * WobbleSpeed);
>>>>>>> a65c0369eb9d7203440bc798a2d63f4f1b063aed
					f = sqrt(clamp(1.0 - f*f, 0.0, 1.0));
					color += col.xyz *(1.0 - smoothstep(rad*0.55, rad, dis)) * f;
				}

				col = float4(color, 1);
				return col;
			}
			ENDCG
		}
	}
}
