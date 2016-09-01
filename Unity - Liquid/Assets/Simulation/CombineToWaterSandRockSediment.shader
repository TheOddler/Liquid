Shader "Hidden/CombineToWaterSandRockSediment"
{
	Properties
	{
		//_MainTex("Main", 2D) = "black" {}
		
		_Water ("Water", 2D) = "black" {}
		_WaterScale("Water scale", Float) = 1

		_Sand("Sand", 2D) = "black" {}
		_SandScale("Sand scale", Float) = 1

		_Rock("Rock", 2D) = "black" {}
		_RockScale("Rock scale", Float) = 1

		_Sediment("Sediment", 2D) = "black" {}
		_SedimentScale("Sediment scale", Float) = 0
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

			//sampler2D _MainTex;
			sampler2D_float _Water;
			sampler2D_float _Sand;
			sampler2D_float _Rock;
			sampler2D_float _Sediment;

			float _WaterScale;
			float _SandScale;
			float _RockScale;
			float _SedimentScale;

			float4 frag (v2f_img i) : SV_Target
			{
				float4 water = tex2D(_Water, i.uv);
				float4 sand = tex2D(_Sand, i.uv);
				float4 rock = tex2D(_Rock, i.uv);
				float4 sediment = tex2D(_Sediment, i.uv);
				
				return float4(water.r * _WaterScale, sand.g * _SandScale, rock.b * _RockScale, sediment.a * _SedimentScale);
			}
			ENDCG
		}
	}
}
