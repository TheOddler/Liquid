Shader "Custom/SimVisSandRock" {
	Properties {
		_Color ("Color", Color) = (1,1,1,1)
		_Glossiness ("Smoothness", Range(0,1)) = 0.5
		_Metallic ("Metallic", Range(0,1)) = 0.0

		_SandColor("Sand Color", Color) = (.3,.7,.35,1)
		_RockColor("Rock Color", Color) = (0,0,0,1)

		_L("Pipe Length", Float) = 0.2
		_WaterSandRockSedimentTex("Water Sand Rock Sediment", 2D) = "white" {}
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
			float2 uv_WaterSandRockSedimentTex;
		};

		fixed4 _Color;
		half _Glossiness;
		half _Metallic;

		fixed4 _SandColor;
		fixed4 _RockColor;

		float _L;
		sampler2D_float _WaterSandRockSedimentTex;
		float4 _WaterSandRockSedimentTex_TexelSize;

		void vert(inout appdata_full v) {
			v.vertex.y = SampleHeightSand(_WaterSandRockSedimentTex, v.texcoord);
		}

		void surf (Input IN, inout SurfaceOutputStandard o) {
			// Color based on what's on top
			float4 wsr = tex2D(_WaterSandRockSedimentTex, IN.uv_WaterSandRockSedimentTex);
			float4 c = lerp(_RockColor, _SandColor, clamp(wsr.g * 5, 0, 1));
			o.Albedo = c.rgb;

			// Normal based on heights
			o.Normal = CalculateSandNormal(_WaterSandRockSedimentTex, IN.uv_WaterSandRockSedimentTex, _WaterSandRockSedimentTex_TexelSize, _L);

			// Metallic and smoothness come from slider variables
			o.Metallic = _Metallic;
			o.Smoothness = _Glossiness;
			o.Alpha = c.a;
		}
		ENDCG
	}
	FallBack "Diffuse"
}
