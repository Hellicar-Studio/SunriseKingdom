﻿Shader "Custom/SunSpots"
{
	Properties
	{
		XOffset("X Offset", Range(0.0, 6.5)) = 0.0
		YOffset("X Offset", Range(0.0, 1.0)) = 0.0
		Speed("Speed", Range(0.001, 1.0)) = 1.0
		WobbleSpeed("Wobble Speed", Range(0.0, 1.0)) = 0.0
		MinSize("Minimum Size", Range(0.0, 6.5)) = 0.0
		MaxSize("Maximum Size", Range(0.0, 6.5)) = 6.5
	}
	SubShader
	{
		Pass
		{
			CGPROGRAM
			#pragma vertex vert_img
			#pragma fragment frag
			#pragma target 3.0
			
			#include "UnityCG.cginc"

			uniform float Speed;
			uniform float WobbleSpeed;
			uniform float Time;
			// Min 240 for longest day - Max 375 for shortest Day 
			uniform float MinSize;
			uniform float MaxSize;

			uniform float4 Colors[365];
			uniform float NumColors;
			//uniform float Times[365];

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

			float3 calculateColor(float3 color, float2 uv) {
				for (int i = 0; i < NumColors; i++)
				{
					// bubble seeds
					float size = map(Colors[i].w, 375, 240, MinSize, MaxSize, true);
					float rad = smoothstep(i / NumColors, (i + 1) / NumColors, Time);
					rad = map(rad, 0.0, 1.0, 0.0, size);
					rad = max(rad, 0.0);
					float pox = map(sin(float(i)*546.13 + 7.5), -1, 1, 0.2, 6.2);
					float poy = map(sin(float(i)*321.22 + 4.1), -1, 1, 0.2, 0.9);

					// bubble size, position and color
					float2 pos = float2(pox, poy);//-1.0 - rad + (2.0 + 2.0*rad)*fmod(pha + 0.1*Time*(0.2 + 0.8*siz), 1.0));
					float dis = length(uv - pos) / 2;
					float3 col = float3(1.0, 1.0, 1.0) - Colors[i].xyz;

					// render
					float f = length(uv - pos) / rad;

					f += noise(uv - (6.5 - uv) / (size*0.4) + Time * WobbleSpeed);
					f = sqrt(clamp(1.0 - f*f, 0.0, 1.0));
					//if (col.x != 1.0) {
					color -= col.xyz * (1.0 - smoothstep(rad*-0.5, rad, dis)) * f;
					//} 

					//int numR = color.r;
					//int numG = color.g;
					//int numB = color.b;

					//float percentR = frac(color.r);
					//float percentG = frac(color.g);
					//float percentB = frac(color.b);

					//if (fmod(-numR, 2) == 1) {
					//	color.r = 0.0 + percentR;
					//}
					//if (fmod(-numG, 2) == 1) {
					//	color.g = 0.0 + percentG;
					//}
					//if (fmod(-numB, 2) == 1) {
					//	color.b = 0.0 + percentB;
					//}
				}
				return color;
			}
			
			fixed4 frag (v2f_img input) : SV_Target
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

				// bubbles
				color = calculateColor(color, uv);

				col = float4(color, 1);
				return col;
			}
			ENDCG
		}
	}
}
