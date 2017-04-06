Shader "Custom/SunVisualisation"
{
	Properties
	{
		_Color1 ("Color1", Color) = (1, 1, 1, 1)
		_Color2 ("Color2", Color) = (0, 0, 0, 0)
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

			uniform float4 _Color1;
			uniform float4 _Color2;
			uniform float4 Colors[365];
			uniform int days;

			v2f vert (appdata v)
			{
				v2f o;
				o.vertex = UnityObjectToClipPos(v.vertex);
				o.uv = v.uv;
				return o;
			}
			
			fixed4 frag (v2f i) : SV_Target
			{
				fixed4 col;
				float partitionWidth = 1.0 / days;
				int myPartition = i.uv.x / partitionWidth;
				// sample the texture
				col = Colors[myPartition] * i.uv.y;//tex2D(_MainTex, i.uv);
				return col;
			}
			ENDCG
		}
	}
}
