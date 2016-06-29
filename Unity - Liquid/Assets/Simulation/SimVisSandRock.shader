Shader "Custom/SimVisSandRock" {
	Properties {
		_Color ("Color", Color) = (1,1,1,1)
		_Glossiness ("Smoothness", Range(0,1)) = 0.5
		_Metallic ("Metallic", Range(0,1)) = 0.0

		_SandColor("Sand Color", Color) = (.3,.7,.35,1)
		_RockColor("Rock Color", Color) = (0,0,0,1)

		_WaterSandRockTex("Water Sand Rock", 2D) = "white" {}
	}
	SubShader {
		Tags { "RenderType"="Opaque" }
		LOD 200
		
		CGPROGRAM
		// Physically based Standard lighting model, and enable shadows on all light types
		#pragma surface surf Standard fullforwardshadows vertex:vert

		// Use shader model 3.0 target, to get nicer looking lighting
		#pragma target 3.0

		struct Input {
			float2 uv_WaterSandRockTex;
		};

		fixed4 _Color;
		half _Glossiness;
		half _Metallic;

		fixed4 _SandColor;
		fixed4 _RockColor;

		sampler2D_float _WaterSandRockTex;
		float4 _WaterSandRockTex_TexelSize;

		void vert(inout appdata_full v) {
			fixed4 wsr = tex2Dlod(_WaterSandRockTex, v.texcoord);
			v.vertex.y = wsr.g + wsr.b;
		}

		void surf (Input IN, inout SurfaceOutputStandard o) {
			// Color based on what's on top
			float4 wsr = tex2D(_WaterSandRockTex, IN.uv_WaterSandRockTex);
			float4 c = lerp(_RockColor, _SandColor, clamp(wsr.g * 5, 0, 1));
			o.Albedo = c.rgb;

			// Normal based on heights
			float4 hR = tex2D(_WaterSandRockTex, IN.uv_WaterSandRockTex + fixed2(_WaterSandRockTex_TexelSize.x, 0));
			float4 hL = tex2D(_WaterSandRockTex, IN.uv_WaterSandRockTex - fixed2(_WaterSandRockTex_TexelSize.x, 0));
			float4 hB = tex2D(_WaterSandRockTex, IN.uv_WaterSandRockTex + fixed2(0, _WaterSandRockTex_TexelSize.y));
			float4 hT = tex2D(_WaterSandRockTex, IN.uv_WaterSandRockTex - fixed2(0, _WaterSandRockTex_TexelSize.y));
			float4 hTotal = float4(
				hR.r + hR.g + hR.b,
				hL.r + hL.g + hL.b,
				hB.r + hB.g + hB.b,
				hT.r + hT.g + hT.b
				);

			o.Normal = normalize(float3(
				hTotal.g - hTotal.r,
				hTotal.a - hTotal.b,
				1
				));

			// Metallic and smoothness come from slider variables
			o.Metallic = _Metallic;
			o.Smoothness = _Glossiness;
			o.Alpha = c.a;
		}
		ENDCG
	}
	FallBack "Diffuse"
}
