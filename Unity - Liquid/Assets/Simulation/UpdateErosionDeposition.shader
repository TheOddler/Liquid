Shader "Hidden/UpdateErosionDeposition"
{
	Properties
	{
		_MainTex ("Water, Sand, Rock, Sediment heights", 2D) = "white" {} // assumed to be the Water, Sand, Rock heights
		_VelocityXY("Velocity XY", 2D) = "white" {}

		_DT("Delta Time", Float) = 0.2
		_L("Pipe Length", Float) = 0.2
		_Kc("Sediment capacity constant", Float) = 0.2
		_Ks("Dissolving constant", Float) = 0.2
		_Kd("Deposition constant", Float) = 0.2

		_ErosionMinimumAngleThresshold("Erosion minimum angle thresshold", Float) = 0.001
	}
	SubShader
	{
		// No culling or depth
		Cull Off ZWrite Off ZTest Always

		Pass
		{
			CGPROGRAM
			#pragma vertex vert_img
			#pragma fragment frag
			
			#include "UnityCG.cginc"
			
			sampler2D_float _MainTex;
			sampler2D_float _VelocityXY;
			float4 _MainTex_TexelSize;

			float _DT;
			float _L;
			float _Kc;
			float _Ks;
			float _Kd;

			float _ErosionMinimumAngleThresshold;

			float4 frag (v2f_img i) : SV_Target
			{
				float4 heights = tex2D(_MainTex, i.uv);
				float4 velocity = tex2D(_VelocityXY, i.uv);

				// neightbouring heights
				float4 hR = tex2D(_MainTex, i.uv + fixed2(_MainTex_TexelSize.x, 0));
				float4 hL = tex2D(_MainTex, i.uv - fixed2(_MainTex_TexelSize.x, 0));
				float4 hB = tex2D(_MainTex, i.uv + fixed2(0, _MainTex_TexelSize.y));
				float4 hT = tex2D(_MainTex, i.uv - fixed2(0, _MainTex_TexelSize.y));

				//
				// 3.3 Erosion and Deposition
				// ---
				// calculations for tilt with help from: http://math.stackexchange.com/questions/1044044/local-tilt-angle-based-on-height-field#1044080
				float dhx = (hR.g + hR.b - hL.g - hL.b) / (2 * _L);
				float dhy = (hB.g + hB.b - hT.g - hT.b) / (2 * _L);
				float sinAlpha = max(_ErosionMinimumAngleThresshold, sqrt(1 - 1 / (1 + dhx * dhx + dhy * dhy)));
				float C = _Kc * sinAlpha * sqrt(velocity.r * velocity.r + velocity.g * velocity.g) * _L * _L * heights.r; //transport capacity

				if (C > heights.a) { //more capacity than sediment = erode
					float erode = min(C - heights.a, _Ks * (C - heights.a));
					heights.g -= erode;
					heights.a += erode;
				}
				else { //less capacity than sediment = deposit
					float deposit = max(heights.a, _Kd * (heights.a - C));
					heights.g += deposit;
					heights.a -= deposit;
				}
				return heights;
			}
			ENDCG
		}
	}
}
