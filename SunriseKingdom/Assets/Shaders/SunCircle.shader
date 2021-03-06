﻿Shader "Custom/SunCircle"
{
	Properties
	{
		XOffset("X Offset", Range(0.0, 6.5)) = 0.0
		YOffset("X Offset", Range(0.0, 1.0)) = 0.0
		Speed("Speed", Range(1.0, 100.0)) = 1.0
		MinSize("Minimum Size", Range(0.0, 6.5)) = 0.0
		MaxSize("Maximum Size", Range(0.0, 6.5)) = 6.5
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

			uniform float Speed;
			uniform float MinSize;
			uniform float MaxSize;

			uniform float4 Colors[365];
			// Min 240 for longest day - Max 375 for shortest Day 
			uniform float Times[365];

			uniform int days;
			uniform int shotsPerDay;

			v2f vert (appdata v)
			{
				v2f o;
				o.vertex = UnityObjectToClipPos(v.vertex);
				o.uv = v.uv;
				return o;
			}

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

				// Mix 4 coorners percentages
				return lerp(a, b, u.x) +
					(c - a)* u.y * (1.0 - u.x) +
					(d - b) * u.x * u.y;
			}

			float map(float value, float low1, float high1, float low2, float high2) 
			{
				return low2 + (value - low1) * (high2 - low2) / (high1 - low1);
			}

			float map(float value, float low1, float high1, float low2, float high2, bool isClamped)
			{
				float val = low2 + (value - low1) * (high2 - low2) / (high1 - low1);
				if (isClamped) {
					if (val < low2)
						return low2;
					if (val > high2)
						return high2;
				}
				return val;
			}
			
			fixed4 frag (v2f input) : SV_Target
			{
				float4 col = float4(0, 0, 0, 1);
				float2 uv = input.uv.xy;
				uv.x *= _ScreenParams.x / _ScreenParams.y;
				float backgroundHue = 1.0;//0.7+0.4*uv.y;
				float3 color = float3(backgroundHue, backgroundHue, backgroundHue);

				float2 uvV;
				uvV.y = uv.y *(1.1 - (uv.y + 0.1));   // vec2(1.0)- uv.yx; -> 1.-u.yx; Thanks FabriceNeyret !
				uvV.x = uv.x *(6.4 - uv.x);   // vec2(1.0)- uv.yx; -> 1.-u.yx; Thanks FabriceNeyret !

				float vig = uvV.x*uvV.y * 15.0; // multiply with sth for intensity

				vig = pow(vig, 0.7); // change pow for modifying the extend of the vignette

				bool done = false;
				// bubbles
				for (int i = 0; i < 365; i++)
				{
					// bubble seeds
					float rad = map(_Time.y * Speed - float(i), 0.0, 3600, 0.0, 6.5);//siz;
					float pox = map(rad, 0, 6.5, 6.0, 0.0);

					float poy = 0.5;//map(sin(float(i)*321.22 + 4.1), -1, 1, 0.2, 0.9);

					// bubble size, position and color
					float2  pos = float2(pox, poy);//-1.0 - rad + (2.0 + 2.0*rad)*fmod(pha + 0.1*_Time.y*(0.2 + 0.8*siz), 1.0));
					float dis = length(uv - pos);
					float3  col = Colors[i];
					//col -= map(pox, 0.0, 6.5, 0.5, 0.0);

					// render
					float f = length(uv - pos) / rad;
					float x = fmod(uv.x, 0.2);
					float y = fmod(uv.y, 0.2);

					//f += noise(uv - (6.5 - uv) / (size*0.4) + _Time.y);
					f = sqrt(clamp(1.0 - f*f, 0.0, 1.0));
					if (col.x != 1.0) {
						float3 newCol = col.xyz * (1.0 - smoothstep(rad*-0.5, rad, dis)) * f;
						color = lerp(color, newCol, 0.5);
					}

					//int numR = color.r;
					//int numG = color.g;
					//int numB = color.b;

					//float percentR = frac(color.r);
					//float percentG = frac(color.g);
					//float percentB = frac(color.b);

					//if (fmod(-numR, 2) == 1) {
					//	color.r = 1.0 - percentR;
					//}
					//if (fmod(-numG, 2) == 1) {
					//	color.g = 1.0 - percentG;
					//}
					//if (fmod(-numB, 2) == 1) {
					//	color.b = 1.0 - percentB;
					//}
				}

				//float2 test = input.uv;
				//vig = max(vig, 0.0);
				//vig = 1.0 - vig;
				//if (vig < 0.0)
				//	vig = 0.0;
				col = float4(color, 1);
				//col += float4( vig, vig, vig, 0);
				return col;
			}
			ENDCG
		}
	}
}
