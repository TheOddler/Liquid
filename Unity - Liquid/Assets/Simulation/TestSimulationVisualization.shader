Shader "Custom/TestSimulationVisualization" {
	Properties {
		_Color ("Color", Color) = (1,1,1,1)
		//_MainTex ("Albedo (RGB)", 2D) = "white" {}
		_Glossiness ("Smoothness", Range(0,1)) = 0.5
		_Metallic ("Metallic", Range(0,1)) = 0.0

		_WaterColor("Water Color", Color) = (0,0,1,1)
		_SandColor("Sand Color", Color) = (.3,.7,.35,1)
		_RockColor("Rock Color", Color) = (0,0,0,1)

		_WaterSandRockTex("Water Sand Rock", 2D) = "white" {}
		_FluxTex("Flux", 2D) = "white" {}

		_DebugPerc("Howmuch should the debug shine through", Range(0,1)) = 0.5
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

		//sampler2D _MainTex;
		sampler2D _WaterSandRockTex;
		sampler2D _FluxTex;

		half _Glossiness;
		half _Metallic;
		fixed4 _Color;
		float _Amount;

		fixed4 _WaterColor;
		fixed4 _SandColor;
		fixed4 _RockColor;

		float _DebugPerc;

		void vert(inout appdata_full v) {
			fixed4 wsr = tex2Dlod(_WaterSandRockTex, v.texcoord);
			v.vertex.y += wsr.r + wsr.g + wsr.b;
		}

		void surf (Input IN, inout SurfaceOutputStandard o) {
			
			// Color based on what's on top
			float4 wsr = tex2D(_WaterSandRockTex, IN.uv_WaterSandRockTex);
			float4 wsrColor = lerp(_RockColor, _SandColor, clamp(wsr.g * 5, 0, 1));
			wsrColor = lerp(wsrColor, _WaterColor, clamp(wsr.r * 5, 0, .8));

			// Color based on flux
			float4 fluxColor = tex2D(_FluxTex, IN.uv_WaterSandRockTex); //assume same uv

			// total color
			float4 c = lerp(wsrColor, fluxColor, _DebugPerc);
			o.Albedo = c.rgb;
			// Metallic and smoothness come from slider variables
			o.Metallic = _Metallic;
			o.Smoothness = _Glossiness;
			o.Alpha = c.a;
		}
		ENDCG
	}
	FallBack "Diffuse"
}
