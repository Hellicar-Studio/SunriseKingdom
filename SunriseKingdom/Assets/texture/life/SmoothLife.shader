// Thanks to Alan Zucconi: http://www.alanzucconi.com/?p=4643
// James Bentley 27-4-17, mostly appropriated from Alan so all credit to him!

// i.uv is ALREADY NORMALIZED!!!
Shader "Custom/SmoothLife"
{
	Properties
	{
		_MainTex("Base (RGB)", 2D) = "white" {}

		_ColorTex("Color Texture (RGB)", 2D) = "white" {}

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
			uniform float2 _Mouse;

			float2 normz(float2 x) {
				return x == float2(0.0, 0.0) ? float2(0.0, 0.0) : normalize(x);
			}

			// reverse advection
			float3 advect(float2 ab, float2 vUv, float2 step, float sc) {

				float2 aUv = vUv - ab * sc * step;

				const float _G0 = 0.25; // center weight
				const float _G1 = 0.125; // edge-neighbors
				const float _G2 = 0.0625; // vertex-neighbors

										  // 3x3 neighborhood coordinates
				float step_x = step.x;
				float step_y = step.y;
				float2 n = float2(0.0, step_y);
				float2 ne = float2(step_x, step_y);
				float2 e = float2(step_x, 0.0);
				float2 se = float2(step_x, -step_y);
				float2 s = float2(0.0, -step_y);
				float2 sw = float2(-step_x, -step_y);
				float2 w = float2(-step_x, 0.0);
				float2 nw = float2(-step_x, step_y);

				float3 uv = tex2D(_MainTex, frac(aUv)).xyz;
				float3 uv_n = tex2D(_MainTex, frac(aUv + n)).xyz;
				float3 uv_e = tex2D(_MainTex, frac(aUv + e)).xyz;
				float3 uv_s = tex2D(_MainTex, frac(aUv + s)).xyz;
				float3 uv_w = tex2D(_MainTex, frac(aUv + w)).xyz;
				float3 uv_nw = tex2D(_MainTex, frac(aUv + nw)).xyz;
				float3 uv_sw = tex2D(_MainTex, frac(aUv + sw)).xyz;
				float3 uv_ne = tex2D(_MainTex, frac(aUv + ne)).xyz;
				float3 uv_se = tex2D(_MainTex, frac(aUv + se)).xyz;

				return _G0*uv + _G1*(uv_n + uv_e + uv_w + uv_s) + _G2*(uv_nw + uv_sw + uv_ne + uv_se);
			}

			float4 frag(v2f_img i) : COLOR
			{

				float2 mouse = _Mouse.xy / _ScreenParams.xy;

				const float _K0 = -20.0 / 6.0; // center weight
				const float _K1 = 4.0 / 6.0;   // edge-neighbors
				const float _K2 = 1.0 / 6.0;   // vertex-neighbors
				const float cs = -0.6;  // curl scale
				const float ls = 0.05;  // laplacian scale
				const float ps = -0.8;  // laplacian of divergence scale
				const float ds = -0.05; // divergence scale
				const float dp = -0.04; // divergence update scale
				const float pl = 0.3;   // divergence smoothing
				const float ad = 6.0;   // advection distance scale
				const float pwr = 1.0;  // power when deriving rotation angle from curl
				const float amp = 1.0;  // self-amplification
				const float upd = 0.8;  // update smoothing
				const float sq2 = 0.6;  // diagonal weight

				float2 vUv = i.uv.xy;
				float2 texel = 1. / _ScreenParams.xy;

				float step_x = texel.x;
				float step_y = texel.y;
				float2 n = float2(0.0, step_y);
				float2 ne = float2(step_x, step_y);
				float2 e = float2(step_x, 0.0);
				float2 se = float2(step_x, -step_y);
				float2 s = float2(0.0, -step_y);
				float2 sw = float2(-step_x, -step_y);
				float2 w = float2(-step_x, 0.0);
				float2 nw = float2(-step_x, step_y);

				float3 uv = tex2D(_MainTex, frac(vUv)).xyz;
				float3 uv_n = tex2D(_MainTex, frac(vUv + n)).xyz;
				float3 uv_e = tex2D(_MainTex, frac(vUv + e)).xyz;
				float3 uv_s = tex2D(_MainTex, frac(vUv + s)).xyz;
				float3 uv_w = tex2D(_MainTex, frac(vUv + w)).xyz;
				float3 uv_nw = tex2D(_MainTex, frac(vUv + nw)).xyz;
				float3 uv_sw = tex2D(_MainTex, frac(vUv + sw)).xyz;
				float3 uv_ne = tex2D(_MainTex, frac(vUv + ne)).xyz;
				float3 uv_se = tex2D(_MainTex, frac(vUv + se)).xyz;

				float3 lapl = _K0*uv + _K1*(uv_n + uv_e + uv_w + uv_s) + _K2*(uv_nw + uv_sw + uv_ne + uv_se);
				float sp = ps * lapl.z;

				// Danger here, reference to a "center point" need to look at this is we start getting errors
				float curl = uv_n.x - uv_s.x - uv_e.y + uv_w.y + sq2 * (uv_nw.x + uv_nw.y + uv_ne.x - uv_ne.y + uv_sw.y - uv_sw.x - uv_se.y - uv_se.x);

				// Build in functions here could return different values
				float sc = cs * sign(curl) * pow(abs(curl), pwr);

				float div = uv_s.y - uv_n.y - uv_e.x + uv_w.x + sq2 * (uv_nw.x - uv_nw.y - uv_ne.x - uv_ne.y + uv_sw.x + uv_sw.y + uv_se.y - uv_se.x);
				float sd = uv.z + dp * div + pl * lapl.z;

				float2 norm = normz(uv.xy);

				float3 ab = advect(float2(uv.x, uv.y), vUv, texel, ad);

				// temp values for the update rule
				float ta = amp * ab.x + ls * lapl.x + norm.x * sp + uv.x * ds * sd;
				float tb = amp * ab.y + ls * lapl.y + norm.y * sp + uv.y * ds * sd;

				// rotate CAREFUL HERE!
				float a = ta * cos(sc) - tb * sin(sc);
				float b = ta * sin(sc) + tb * cos(sc);

				float3 abd = upd * uv + (1.0 - upd) * float3(a, b, sd);

				//float2 d = length(vUv.xy - mouse.xy);
				//float m = exp(-length(d) *100);
				//abd.xy -= max(m * normz(d), 0.0);

				abd.z = clamp(abd.z, -1.0, 1.0);
				abd.xy = clamp(length(abd.xy) > 1.0 ? normz(abd.xy) : abd.xy, -1.0, 1.0);

				return float4(abd, 0.0);
			}
			ENDCG
		}
	}
}