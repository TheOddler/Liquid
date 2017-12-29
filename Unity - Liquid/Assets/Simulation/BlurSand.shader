Shader "Hidden/UpdateHeights"
{
	Properties
	{
		_MainTex ("Water, Sand, Rock, Sediment heights", 2D) = "white" {} // assumed to be the Water, Sand, Rock heights
		_Sigma("Gausian sigma", Float) = 7
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
			float4 _MainTex_TexelSize;

			float _SandBlur;
			float _Sigma;

			float normpdf(float x, float sigma)
			{
				return 0.39894*exp(-0.5*x*x / (sigma*sigma)) / sigma;
			}

			float4 frag (v2f_img i) : SV_Target
			{
				half4 col = tex2D(_MainTex, i.uv);
				const int mSize = 15; //must be const for loop unrolling to work
				const int iter = (mSize - 1) / 2;
				for (int x = -iter; x <= iter; ++x) {
					for (int y = -iter; y <= iter; ++y) {
						col += tex2D(_MainTex, i.uv + float2(x * _MainTex_TexelSize.x, y * _MainTex_TexelSize.y)) * normpdf(float(x), _Sigma);
					}
				}
				return col / mSize;

				// float4 heights = tex2D(_MainTex, i.uv);

				// // neightbouring heights
				// float4 hR = tex2D(_MainTex, i.uv + fixed2(_MainTex_TexelSize.x, 0));
				// float4 hL = tex2D(_MainTex, i.uv - fixed2(_MainTex_TexelSize.x, 0));
				// float4 hB = tex2D(_MainTex, i.uv + fixed2(0, _MainTex_TexelSize.y));
				// float4 hT = tex2D(_MainTex, i.uv - fixed2(0, _MainTex_TexelSize.y));
				
				// // Blur sand
				// return float4(
				// 	heights.r, 
				// 	(heights.g + (hR.g + hL.g + hB.g + hT.g) * _SandBlur) / (1 + 4 * _SandBlur),
				// 	heights.b, 
				// 	heights.a
				// );
			}
			ENDCG
		}
	}
}
