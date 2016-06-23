Shader "Hidden/UpdateOutflowFlux"
{
	Properties
	{
		_MainTex("Height", 2D) = "white" {}
		_SourceTex("Source", 2D) = "white" {}
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
			sampler2D _SourceTex;

			float4 frag(v2f_img i) : SV_Target
			{
				float4 height = tex2D(_MainTex, i.uv);
				float4 source = tex2D(_SourceTex, i.uv);

				return height + source;
			}
			ENDCG
		}
	}
}
