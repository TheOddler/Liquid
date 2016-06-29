Shader "Hidden/UpdateWaterHeight"
{
	Properties
	{
		_MainTex ("Water, Sand, Rock heights", 2D) = "white" {} // assumed to be the Water, Sand, Rock heights
		_OutflowFluxRLBT("Flux RLBT", 2D) = "white" {}

		_DT("Delta Time", Float) = 0.2
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

			float _DT;
			float _L;

			float4 frag (v2f_img i) : SV_Target
			{
				float4 heights = tex2D(_MainTex, i.uv);
				float4 flux = tex2D(_OutflowFluxRLBT, i.uv);

				// neightbouring flux
				float4 fR = tex2D(_OutflowFluxRLBT, i.uv + fixed2(_OutflowFluxRLBT_TexelSize.x, 0));
				float4 fL = tex2D(_OutflowFluxRLBT, i.uv - fixed2(_OutflowFluxRLBT_TexelSize.x, 0));
				float4 fB = tex2D(_OutflowFluxRLBT, i.uv + fixed2(0, _OutflowFluxRLBT_TexelSize.y));
				float4 fT = tex2D(_OutflowFluxRLBT, i.uv - fixed2(0, _OutflowFluxRLBT_TexelSize.y));

				// Formula 6
				float deltaV = _DT * (fR.g + fL.r + fB.a + fT.b - flux.r - flux.g - flux.b - flux.a);
				
				// Formula 7
				return heights + float4(deltaV / (_L * _L), 0, 0, 0);
				//return heights + float4(deltaV / 0.2, 0, 0, 0);
			}
			ENDCG
		}
	}
}
