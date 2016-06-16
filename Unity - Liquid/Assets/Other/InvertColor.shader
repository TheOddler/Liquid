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
			#pragma vertex vert_img //buildin vertex shader for image effects
			#pragma fragment frag

			#include "UnityCG.cginc"

			/*uniform*/ sampler2D _MainTex; //maybe with "uniform", but don't know what that is
			
			fixed4 frag(v2f_img i) : COLOR 
				// v2f_img is buildin, contains:
				//		float4 pos : SV_POSITION;
				//		half2 uv : TEXCOORD0;
			{
				fixed4 c = tex2D(_MainTex, i.uv);
				return 1 - c;
			}
			
			ENDCG
		}
	}
}
