Shader "Custom/SunPaint"
{
	Properties
	{
		_MainTex("Texture", 2D) = "white" {}
		_NoiseTex("Texture", 2D) = "white" {}
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

			sampler2D _MainTex;
			uniform sampler2D _NoiseTex;
			uniform float2 _Mouse;


			float2 hash2(float n) { return frac(sin(float2(n, n + 1.0))*float2(43758.5453123, 22578.1459123)); }

			// smoothstep interpolation of texture
			float4 ssamp(float2 uv, float oct)
			{
				uv /= oct;

				//return tex2D(_MainTex, uv);

				float texSize = 8.;

				float2 x = uv * texSize - .5;
				float2 f = frac(x);

				// remove fractional part
				x -= f;

				// apply smoothstep to fractional part
				f = f*f*(3.0 - 2.0*f);

				// reapply fractional part
				x += f;

				uv = (x + .5) / texSize;

				return tex2D(_NoiseTex, uv);
			}


			float2 e = float2(1. / 256., 0.);
			float4 dx(float2 uv, float oct) { return (ssamp(uv + e.xy, oct) - ssamp(uv - e.xy, oct)) / (2.*e.x); }
			float4 dy(float2 uv, float oct) { return (ssamp(uv + e.yx, oct) - ssamp(uv - e.yx, oct)) / (2.*e.x); }
			
			fixed4 frag (v2f_img i) : SV_Target
			{
				i.uv *= _ScreenParams.xy;

				float2 uv = i.uv / _ScreenParams.xy;
				float4 res = float4(0., 0., 0., 0.);
				float scl = _ScreenParams.x / 640.;

				if (length(i.uv - _Mouse.xy) < 5.*scl)
				{
					// added sin gives pulsing feel
					res += .7*float4(uv, 0.5 + 0.5*sin(_Time.y), 1.0)*(.6 + .5*-sin(_Time.y*10.5));
				}

				// lookup offset
				float2 off = 0.* (float2(128., 128.) / _ScreenParams.xy) * unity_DeltaTime;

				float oct = .25;
				float2 curl1 = .001*float2(dy(uv, oct).x, -dx(uv, oct).x)*oct;
				oct = 5.; float sp = 0.1;
				curl1 += .0002*float2(dy(uv + sp*_Time.y, oct).x, -dx(uv + sp*_Time.y, oct).x)*oct;

				off += curl1;
				off *= .4;

				res -= .999*tex2D(_MainTex, uv - off);

				res *= 2.;

				res = smoothstep(0., 1., res);

				fixed4 col = res;

				//fixed4 col = ssamp(uv, 0.25);// (_Mouse.x, 0., 0., 1.);//res;//tex2D(_MainTex, i.uv);

				return col;
			}
			ENDCG
		}
	}
}
