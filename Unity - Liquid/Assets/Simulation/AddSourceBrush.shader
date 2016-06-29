Shader "Hidden/AddSourceBrush"
{
	Properties
	{
		_MainTex ("Texture", 2D) = "white" {}
		_Brush("Brush", 2D) = "white" {}
		_Scale("Scaling factor, include delta time if wanted here", Float) = 1.0
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

			Blend Add One

			sampler2D _MainTex;
			sampler2D _Brush;

			float _Scale;

			float4 frag (v2f_img i) : SV_Target
			{
				float4 col = tex2D(_MainTex, i.uv);
				return col * _Scale;
			}
			ENDCG
		}
	}
}
