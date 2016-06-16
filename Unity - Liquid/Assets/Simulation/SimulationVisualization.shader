Shader "Custom/SimulationVisualization" {
	Properties {
		_Color ("Color", Color) = (1,1,1,1)
		//_MainTex ("Albedo (RGB)", 2D) = "white" {}
		_Glossiness ("Smoothness", Range(0,1)) = 0.5
		_Metallic ("Metallic", Range(0,1)) = 0.0

		_WaterColor("Water Color", Color) = (0,0,1,1)
		_SandColor("Sand Color", Color) = (.3,.7,.35,1)
		_RockColor("Rock Color", Color) = (0,0,0,1)

		_WaterSandRock("Water Sand Rock", 2D) = "white" {}
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
			float2 uv_WaterSandRock;
		};

		//sampler2D _MainTex;
		sampler2D _WaterSandRock;

		half _Glossiness;
		half _Metallic;
		fixed4 _Color;
		float _Amount;

		fixed4 _WaterColor;
		fixed4 _SandColor;
		fixed4 _RockColor;

		void vert(inout appdata_full v) {
			fixed4 wsr = tex2Dlod(_WaterSandRock, v.texcoord);
			v.vertex.y += wsr.r + wsr.g + wsr.b;
		}

		void surf (Input IN, inout SurfaceOutputStandard o) {
			float4 wsr = tex2D(_WaterSandRock, IN.uv_WaterSandRock);
			
			// Color based on what's on top
			float4 c = lerp(_RockColor, _SandColor, clamp(wsr.g * 5, 0, 1));
			c = lerp(c, _WaterColor, clamp(wsr.r * 5, 0, .8));
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
