// Alan Zucconi: http://www.alanzucconi.com/?p=4643
Shader "Hidden/Life"
{
	Properties
	{
		_MainTex("Base (RGB)", 2D) = "white" {}

		_Pixels("Pixels in a quad", Float) = 128
	}
	
	SubShader
	{
		// Required to work
		ZTest Always Cull Off ZWrite Off
		Fog{ Mode off }

		Pass
		{
			CGPROGRAM
			
			#pragma vertex vert_img
			#pragma fragment frag

			#include "UnityCG.cginc"

			uniform sampler2D _MainTex;
			uniform half _Pixels;

			static int2 rule[9] =
			{
				int2(0,0),
				int2(0,0),
				int2(1,0),	// 2 neighours = survive
				int2(0,1),	// 3 neighours = born
				int2(0,0),
				int2(0,0),
				int2(0,0),
				int2(0,0),
				int2(0,0),
			};

			float4 frag(v2f_img i) : COLOR
			{
				// Cell centre
				fixed2 uv = round(i.uv * _Pixels) / _Pixels;

				half s = 1 / _Pixels;

				float tl = tex2D(_MainTex, uv + fixed2(-s, -s)).r;	// Top Left
				float cl = tex2D(_MainTex, uv + fixed2(-s, 0)).r;		// Centre Left
				float bl = tex2D(_MainTex, uv + fixed2(-s, +s)).r;	// Bottom Left

				float tc = tex2D(_MainTex, uv + fixed2(-0, -s)).r;	// Top Centre
				//float cc = tex2D(_MainTex, uv + fixed2(0, 0)).r;		// Centre Centre
				float3 cc = tex2D(_MainTex, uv + fixed2(0, 0));		// Centre Centre
				float bc = tex2D(_MainTex, uv + fixed2(0, +s)).r;	// Bottom Centre

				float tr = tex2D(_MainTex, uv + fixed2(+s, -s)).r;	// Top Right
				float cr = tex2D(_MainTex, uv + fixed2(+s, 0)).r;		// Centre Right
				float br = tex2D(_MainTex, uv + fixed2(+s, +s)).r;	// Bottom Right

				int count = tl + cl + bl + tc + bc + tr + cr + br;

				/*
				// Death
				if (count < 2 || count > 3)
					return float4(0, 0, 0, 1);
				// Life
				if (count == 3)
					return float4(1, 1, 1, 1);
				// Stay
				return cc;
				*/

				//int status = cc * death[count] + life[count];
				int2 r = rule[count];
				int status = cc.r * r.x + r.y;
				//return float4(status, status, status, 1);


				
				if (cc.r == 1 && status == 1)
					return float4(status, cc.g*0.9, cc.b*0.9, 1);
				else
					return float4(status, status, status, 1);
				

				
			}
			ENDCG
		}
	}
}