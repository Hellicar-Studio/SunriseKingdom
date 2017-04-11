Shader "Custom/SunVisualisationGradient"
{
	Properties
	{

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

			//uniform float4 Colors[3][365];
			uniform float4 Colors1[365];
			uniform float4 Colors2[365];
			uniform float4 Colors3[365];
			uniform float4 Colors4[365];
			uniform float4 Colors5[365];
			//uniform float4 Colors6[365];
			//uniform float4 Colors7[365];
			//uniform float4 Colors8[365];
			//uniform float4 Colors9[365];
			// uniform float4 Colors10[365];
			// uniform float4 Colors11[365];
			// uniform float4 Colors12[365];

			uniform int days;
			uniform int shotsPerDay;

			v2f vert (appdata v)
			{
				v2f o;
				o.vertex = UnityObjectToClipPos(v.vertex);
				o.uv = v.uv;
				return o;
			}

			int getPartition(float pos, float paritionSize) {
				return pos / paritionSize;
			}

			float getPercentage(float pos, float partitionSize) {
					return (pos%partitionSize) / partitionSize;
			}

			float4 getColorPercentage(float4 col1, float4 col2, int partition, float pos, float partitionSize) {
				float pe = getPercentage(pos, partitionSize);
				int pa = getPartition(pos, partitionSize);
				if (pa == partition)
					return col2 * pe + col1 * (1.0f - pe);
				return 0.0f;
			}

			float4 getColorPercentage(float4 col1, float4 col2, float pos, float partitionSize) {
				float pe = getPercentage(pos, partitionSize);
				int pa = getPartition(pos, partitionSize);
				return col2 * pe + col1 * (1.0f - pe);
			}

			float4 getFinalColor(float4 col1, float4 col2, float4 nextCol1, float4 nextCol2, int partition, float2 pos, float2 partitionSize) {
				float4 col = float4(0, 0, 0, 1);
				float4 nextCol = float4(0, 0, 0, 1);
				col += getColorPercentage(col1, col2, partition, pos.y, partitionSize.y);
				nextCol += getColorPercentage(nextCol1, nextCol2, partition, pos.y, partitionSize.y);
				return getColorPercentage(col, nextCol, pos.x, partitionSize.x);
			}
			
			fixed4 frag (v2f i) : SV_Target
			{
				float4 col = float4(0, 0, 0, 1);
				float2 partitionSize;
				partitionSize.x = 1.0 / days;
				partitionSize.y = 1.0 / shotsPerDay;

				int partitionX = getPartition(i.uv.x, partitionSize.x);

				// sample the texture
				float p = getPercentage(i.uv.y, partitionSize.y);
				float4 col1 = Colors1[partitionX];
				float4 col2 = Colors2[partitionX];
				float4 col3 = Colors3[partitionX];
				float4 col4 = Colors4[partitionX];
				float4 col5 = Colors5[partitionX];

				float4 nextCol1 = Colors1[partitionX + 1];
				float4 nextCol2 = Colors2[partitionX + 1];
				float4 nextCol3= Colors3[partitionX + 1];
				float4 nextCol4 = Colors4[partitionX + 1];
				float4 nextCol5 = Colors5[partitionX + 1];

				col += getFinalColor(col1, col2, nextCol1, nextCol2, 0, i.uv.xy, partitionSize);
				col += getFinalColor(col2, col3, nextCol2, nextCol3, 1, i.uv.xy, partitionSize);
				col += getFinalColor(col3, col4, nextCol3, nextCol4, 2, i.uv.xy, partitionSize);
				col += getFinalColor(col4, col5, nextCol4, nextCol5, 3, i.uv.xy, partitionSize);

				//col += getColorPercentage(col1, col2, 0, i.uv.y, partitionSize.y);
				//col += getColorPercentage(col2, col3, 1, i.uv.y, partitionSize.y);
				//col += getColorPercentage(col3, col4, 2, i.uv.y, partitionSize.y);
				//col += getColorPercentage(col4, col5, 3, i.uv.y, partitionSize.y);

				return col;
			}
			ENDCG
		}
	}
}
