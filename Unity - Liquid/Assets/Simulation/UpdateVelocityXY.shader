Shader "Hidden/UpdateOutflowFlux"
{
	Properties
	{
		_MainTex("Velocity XY", 2D) = "white" {} //main assumed to be the current outflow flux field
		_OutflowFluxRLBT("Flux RLBT", 2D) = "white" {}
		_WaterSandRockSedimentTex("Water, Sand, Rock, Sediment heights", 2D) = "white" {}
		_PreviousWaterSandRockSedimentTex("Previous Water, Sand, Rock, Sediment heights", 2D) = "white" {}

		_L("Pipe Length", Float) = 0.2
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
			sampler2D_float _OutflowFluxRLBT;
			float4 _OutflowFluxRLBT_TexelSize;
			sampler2D_float _WaterSandRockSedimentTex;
			sampler2D_float _PreviousWaterSandRockSedimentTex;

			float _L;

			float4 frag(v2f_img i) : SV_Target
			{
				float4 flux = tex2D(_OutflowFluxRLBT, i.uv);
				float4 heights = tex2D(_WaterSandRockSedimentTex, i.uv);
				float4 previousHeights = tex2D(_PreviousWaterSandRockSedimentTex, i.uv);

				// neightbouring flux
				float4 fR = tex2D(_OutflowFluxRLBT, i.uv + fixed2(_OutflowFluxRLBT_TexelSize.x, 0));
				float4 fL = tex2D(_OutflowFluxRLBT, i.uv - fixed2(_OutflowFluxRLBT_TexelSize.x, 0));
				float4 fB = tex2D(_OutflowFluxRLBT, i.uv + fixed2(0, _OutflowFluxRLBT_TexelSize.y));
				float4 fT = tex2D(_OutflowFluxRLBT, i.uv - fixed2(0, _OutflowFluxRLBT_TexelSize.y));

				// 3.2.2 (Water Surface and) Velocity Field Update
				float dAv = max(0.000001f, (heights.r + previousHeights.r) * 0.5);
				float4 dW = float4(
					(fL.r - flux.g + flux.r - fR.g) * 0.5,
					(fB.a - flux.b + flux.a - fT.b) * 0.5,
					0, 0);
				
				return dW / dAv / _L;
			}
			ENDCG
		}
	}
}
