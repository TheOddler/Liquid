Shader "Hidden/UpdateSedimentTransportation"
{
	Properties
	{
		_MainTex ("Water, Sand, Rock, Sediment heights", 2D) = "white" {} // assumed to be the Water, Sand, Rock heights
		_VelocityXY("Velocity XY", 2D) = "white" {}

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
			sampler2D_float _VelocityXY;
			float4 _MainTex_TexelSize;

			float _DT;
			float _L;

			float4 frag (v2f_img i) : SV_Target
			{
				float4 heights = tex2D(_MainTex, i.uv);
				float4 velocity = tex2D(_VelocityXY, i.uv);

				float2 uvMoved = i.uv - _DT * velocity.rg;
				float4 heightsMoved = tex2D(_MainTex, uvMoved);

				heights.a = heightsMoved.a;
				return heights;
			}
			ENDCG
		}
	}
}
