Shader "Custom/SunGrowth"
{
	Properties
	{
		_MainTex ("Texture", 2D) = "white" {}
		_RandomTex("Noise Seed", 2D) = "white" {}
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

			uniform sampler2D _MainTex;
			uniform sampler2D _RandomTex;
			float4 _MainTex_ST;
			
			v2f vert (appdata v)
			{
				v2f o;
				o.vertex = UnityObjectToClipPos(v.vertex);
				o.uv = TRANSFORM_TEX(v.uv, _MainTex);
				UNITY_TRANSFER_FOG(o,o.vertex);
				return o;
			}

			const float2 Diffusion = float2(0.08, 0.03);
			//const float F = 0.04;
			//const float k = 0.06;
			const float dt = 2.;

			float rand(float2 co) {
				// implementation found at: lumina.sourceforge.net/Tutorials/Noise.html
				return frac(sin(dot(co.xy, float2(12.9898, 78.233))) * 43758.5453);
			}

			// nine point stencil
			float2 laplacian1(float2 position) {
				float2 pixelSize = 1. / _ScreenParams.xy;
				float4 P = float4(pixelSize, 0.0, -pixelSize.x);
				return
					0.5* tex2D(_MainTex, position - P.xy).xy // first row
					+ tex2D(_MainTex, position - P.zy).xy
					+ 0.5* tex2D(_MainTex, position - P.wy).xy
					+ tex2D(_MainTex, position - P.xz).xy // seond row
					- 6.0* tex2D(_MainTex, position).xy
					+ tex2D(_MainTex, position + P.xz).xy
					+ 0.5*tex2D(_MainTex, position + P.wy).xy  // third row
					+ tex2D(_MainTex, position + P.zy).xy
					+ 0.5*tex2D(_MainTex, position + P.xy).xy;
			}
			// nine point stencil
			float2 laplacian2(float2 position) {
				float2 pixelSize = 5. / _ScreenParams.xy;
				float4 P = float4(pixelSize, 0.0, -pixelSize.x);
				return
					0.5* tex2D(_MainTex, position - P.xy).xy // first row
					+ tex2D(_MainTex, position - P.zy).xy
					+ 0.5* tex2D(_MainTex, position - P.wy).xy
					+ tex2D(_MainTex, position - P.xz).xy // seond row
					- 6.0* tex2D(_MainTex, position).xy
					+ tex2D(_MainTex, position + P.xz).xy
					+ 0.5*tex2D(_MainTex, position + P.wy).xy  // third row
					+ tex2D(_MainTex, position + P.zy).xy
					+ 0.5*tex2D(_MainTex, position + P.xy).xy;
			}
			// nine point stencil
			float2 laplacian3(float2 position) {
				float2 pixelSize = 7. / _ScreenParams.xy;
				float4 P = float4(pixelSize, 0.0, -pixelSize.x);
				return
					0.5* tex2D(_MainTex, position - P.xy).xy // first row
					+ tex2D(_MainTex, position - P.zy).xy
					+ 0.5* tex2D(_MainTex, position - P.wy).xy
					+ tex2D(_MainTex, position - P.xz).xy // seond row
					- 6.0* tex2D(_MainTex, position).xy
					+ tex2D(_MainTex, position + P.xz).xy
					+ 0.5*tex2D(_MainTex, position + P.wy).xy  // third row
					+ tex2D(_MainTex, position + P.zy).xy
					+ 0.5*tex2D(_MainTex, position + P.xy).xy;
			}

			float3 normal(float2 uv) {
				float3 delta = float3(1. / _ScreenParams.xy, 0.);
				float du = tex2D(_MainTex, uv + delta.xz).x - tex2D(_MainTex, uv - delta.xz).x;
				float dv = tex2D(_MainTex, uv + delta.zy).x - tex2D(_MainTex, uv - delta.zy).x;
				return normalize(float3(du, dv, 1.));
			}
			float3 getColor(float2 uv) {
				return 0.5 + 0.5*sin(float3(uv, uv.x - uv.y)*float3(12.2, 6.8, 1.25) + float3(1., .0, 1.25));
			}
			
			fixed4 frag (v2f i) : SV_Target
			{
				fixed4 fragColor;
				float2 uv = i.uv.xy;

				fragColor = float4(0.0, 0.0, 0.0, 1.0);
				if (_Time.y < 1) {
					float rnd = rand(uv) + (sin(50.*uv.x) + sin(50.*uv.y))*0.;
					if (rnd>0.5) fragColor.x = .5;
					else fragColor.y = .5;
				}
				else {
					float F = uv.y*0.05 + 0.0;
					float k = uv.x*0.05 + 0.025;
					float4 data = tex2D(_MainTex, uv);
					float u = data.x;
					float v = data.y;
					float2 Duv = (1.*laplacian1(uv) + 0.*laplacian2(uv) + 0.*laplacian3(uv))*Diffusion;
					float du = Duv.x - u*v*v + F*(1. - u);
					float dv = Duv.y + u*v*v - (F + k)*v;
					fragColor.xy = clamp(float2(u + du*dt, v + dv*dt), 0., 1.);

					fragColor.x = F;//float4(uv, 0.0, 1.0);


					//return float4(col, 1.0);
				}

				float3 col = getColor(fragColor.xy)*(1.5*fragColor.y + 0.25);

				fragColor = float4(col, 1.0);
				return fragColor;
			}
			ENDCG
		}
	}
}
