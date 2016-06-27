Shader "Hidden/UpdateOutflowFlux"
{
	Properties
	{
		_MainTex("Flux RLBT", 2D) = "white" {} //main assumed to be the current outflow flux field
		_WaterSandRockTex("Water, Sand, Rock heights", 2D) = "white" {}

		_DT("Delta Time", Float) = 0.2
		_L("Pipe Length", Float) = 0.2
		_A("Pipe Cross Section Area", Float) = 1.0
		_G("Gravity Constant", Float) = 9.81
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

			sampler2D _MainTex;
			sampler2D _WaterSandRockTex;
			float4 _WaterSandRockTex_TexelSize;

			float _DT;
			float _L;
			float _A;
			float _G;

			float4 frag(v2f_img i) : SV_Target
			{
				float4 flux = tex2D(_MainTex, i.uv);
				float4 heights = tex2D(_WaterSandRockTex, i.uv);

				float4 hR = tex2D(_WaterSandRockTex, i.uv + fixed2(_WaterSandRockTex_TexelSize.x, 0));
				float4 hL = tex2D(_WaterSandRockTex, i.uv - fixed2(_WaterSandRockTex_TexelSize.x, 0));
				float4 hB = tex2D(_WaterSandRockTex, i.uv + fixed2(0, _WaterSandRockTex_TexelSize.y));
				float4 hT = tex2D(_WaterSandRockTex, i.uv - fixed2(0, _WaterSandRockTex_TexelSize.y));

				//float otherHeight = heights.g + heights.b; //only sand and rock
				//float totalHeight = heights.r + heights.g + heights.b; //with water
				//float4 totalHeight4 = float4(totalHeight, totalHeight, totalHeight, totalHeight);
				float4 totalHeight4 = heights.r + heights.g + heights.b;
				float4 totalH_RLBT = float4(
					hR.r + hR.g + hR.b,
					hL.r + hL.g + hL.b,
					hB.r + hB.g + hB.b,
					hT.r + hT.g + hT.b);

				// Formula 3
				float4 dh = totalHeight4 - totalH_RLBT;

				// Formula 2
				float4 fn = max(0, flux + _DT * _A * _G * dh / _L); // fn = flux next; add fluxdamp?

				// Formula 4
				float K = min(1, (heights.r * _L * _L) / (fn.r + fn.g + fn.b + fn.a + 0.001) / _DT);

				// Formula 5
				return K * fn;
			}
			ENDCG
		}
	}
}
