Shader "Hidden/InvertColor"
{
	Properties
	{
		_MainTex ("Texture", 2D) = "white" {}
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

			uniform sampler2D _MainTex;
			
			fixed4 frag(v2f_img i) : COLOR
			{
				fixed4 c = tex2D(_MainTex, i.uv);
				return 1 - c;
			}
			
			ENDCG
		}
	}
}
