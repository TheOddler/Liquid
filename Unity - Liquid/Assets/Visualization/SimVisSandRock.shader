Shader "Custom/SimVisSandRock" {
	Properties {
		_Color ("Color", Color) = (1,1,1,1)
		_Glossiness ("Smoothness", Range(0,1)) = 0.5
		_Metallic ("Metallic", Range(0,1)) = 0.0

		_SandColor("Sand Color", Color) = (.3,.7,.35,1)
		_RockColor("Rock Color", Color) = (0,0,0,1)

		_L("Pipe Length", Float) = 0.2
		_WaterSandRockSediment("Water Sand Rock Sediment", 2D) = "white" {}

		_Indicator("Mouse indicator", Vector) = (0,0,0,0)
		_IndicatorColor("Indicator color", Color) = (1,0,0,1)
		_IndicatorSize("Indicator size", Range(0,1)) = 0.05
	}
	SubShader {
		Tags { "RenderType"="Opaque" }
		LOD 200
		
		CGPROGRAM
		#include "SimVisInclude.cginc"

		// Physically based Standard lighting model, and enable shadows on all light types
		#pragma surface surf Standard fullforwardshadows vertex:vert

		// Use shader model 3.0 target, to get nicer looking lighting
		#pragma target 3.0

		struct Input {
			float2 uv_WaterSandRockSediment;
		};

		fixed4 _Color;
		half _Glossiness;
		half _Metallic;

		fixed4 _SandColor;
		fixed4 _RockColor;

		float _L;
		sampler2D_float _WaterSandRockSediment;
		float4 _WaterSandRockSediment_TexelSize;

		float4 _Indicator;
		float4 _IndicatorColor;
		float _IndicatorSize;

		void vert(inout appdata_full v) {
			v.vertex.y = SampleHeightSand(_WaterSandRockSediment, v.texcoord);
		}

		void surf (Input IN, inout SurfaceOutputStandard o) {
			// Color based on what's on top
			float4 wsr = tex2D(_WaterSandRockSediment, IN.uv_WaterSandRockSediment);
			float4 c = lerp(_RockColor, _SandColor, clamp(wsr.g * 5, 0, 1));

			// Indicator
			float amountInv = min(1, distance(IN.uv_WaterSandRockSediment, _Indicator.xy) / _IndicatorSize);
			float amount = 1 - amountInv * amountInv;
			c.rgb = lerp(c.rgb, _IndicatorColor, amount);

			// Combine color
			o.Albedo = c.rgb;

			// Normal based on heights
			o.Normal = CalculateSandNormal(_WaterSandRockSediment, IN.uv_WaterSandRockSediment, _WaterSandRockSediment_TexelSize, _L);

			// Metallic and smoothness come from slider variables
			o.Metallic = _Metallic;
			o.Smoothness = _Glossiness;
			o.Alpha = c.a;
		}
		ENDCG
	}
	FallBack "Diffuse"
}
