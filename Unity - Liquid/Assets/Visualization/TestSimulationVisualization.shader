Shader "Custom/TestSimulationVisualization" {
	Properties {
		_Color ("Color", Color) = (1,1,1,1)
		//_MainTex ("Albedo (RGB)", 2D) = "white" {}
		_Glossiness ("Smoothness", Range(0,1)) = 0.5
		_Metallic ("Metallic", Range(0,1)) = 0.0

		_WaterColor("Water Color", Color) = (0,0,1,1)
		_SandColor("Sand Color", Color) = (.3,.7,.35,1)
		_RockColor("Rock Color", Color) = (0,0,0,1)
		_SedimentColor("Rock Color", Color) = (1,0,0,1)

		_WaterSandRockSediment("Water Sand Rock Sediment", 2D) = "white" {}
		_Flux("Flux", 2D) = "white" {}
		_VelocityXY("Velocity XY", 2D) = "white" {}

		_DT("Delta Time", Float) = 0.2
		_L("Pipe Length", Float) = 0.2
		_Kc("Sediment capacity constant", Float) = 0.2

		_ErosionMinimumAngleThresshold("Erosion minimum angle thresshold", Float) = 0.001

		_HeightScale("Height scale", Range(0.1,10)) = 1

		_DebugPerc("Debug shine-through percentage", Range(0,1)) = 0.5
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
			float2 uv_WaterSandRockSediment;
		};

		//sampler2D _MainTex;
		sampler2D _WaterSandRockSediment;
		sampler2D _Flux;
		sampler2D _VelocityXY;
		float4 _WaterSandRockSediment_TexelSize;

		half _Glossiness;
		half _Metallic;
		fixed4 _Color;
		float _Amount;

		fixed4 _WaterColor;
		fixed4 _SandColor;
		fixed4 _RockColor;
		fixed4 _SedimentColor;

		float _HeightScale;

		float _DT;
		float _L;
		float _Kc;

		float _ErosionMinimumAngleThresshold;

		float _DebugPerc;

		void vert(inout appdata_full v) {
			fixed4 wsr = tex2Dlod(_WaterSandRockSediment, v.texcoord);
			v.vertex.y += (wsr.r + wsr.g + wsr.b) * _HeightScale;
		}


		float3 angleToHue(float angle) //asumed to be between -pi and pi, as atan2 gives them
		{
			angle = (1 + angle / 3.141592) / 2; //map -pi,pi to 0,1
			float R = abs(angle * 6 - 3) - 1;
			float G = 2 - abs(angle * 6 - 2);
			float B = 2 - abs(angle * 6 - 4);
			return saturate(float3(R, G, B));
		}

		void surf (Input IN, inout SurfaceOutputStandard o) {
			
			// Color based on what's on top
			float4 wsr = tex2D(_WaterSandRockSediment, IN.uv_WaterSandRockSediment);
			float4 wsrColor = lerp(_RockColor, _SandColor, clamp(wsr.g * 5, 0, 1));
			wsrColor = lerp(wsrColor, _WaterColor, clamp(wsr.r * 3, 0, 1));
			wsrColor = lerp(wsrColor, _SedimentColor, clamp(wsr.a * 50000, 0, 1));
			//wsrColor = _SedimentColor * wsr.a * 100000;

			// Color based on flux
			float4 flux = tex2D(_Flux, IN.uv_WaterSandRockSediment); //assume same uv
			float4 fluxColor = float4(flux.r - flux.g, flux.b - flux.a, 0, 1);

			// Color based on velocity
			float4 vel = tex2D(_VelocityXY, IN.uv_WaterSandRockSediment);
			float velDir = atan2(vel.g, vel.r);
			float4 velColor = float4(angleToHue(velDir), 1);

			// Color based on sediment capacity
			float4 heights = tex2D(_WaterSandRockSediment, IN.uv_WaterSandRockSediment);
			float4 velocity = tex2D(_VelocityXY, IN.uv_WaterSandRockSediment);
			float4 hR = tex2D(_WaterSandRockSediment, IN.uv_WaterSandRockSediment + fixed2(_WaterSandRockSediment_TexelSize.x, 0));
			float4 hL = tex2D(_WaterSandRockSediment, IN.uv_WaterSandRockSediment - fixed2(_WaterSandRockSediment_TexelSize.x, 0));
			float4 hB = tex2D(_WaterSandRockSediment, IN.uv_WaterSandRockSediment + fixed2(0, _WaterSandRockSediment_TexelSize.y));
			float4 hT = tex2D(_WaterSandRockSediment, IN.uv_WaterSandRockSediment - fixed2(0, _WaterSandRockSediment_TexelSize.y));

			float dhx = (hR.g + hR.b - hL.g - hL.b) / (2 * _L);
			float dhy = (hB.g + hB.b - hT.g - hT.b) / (2 * _L);
			float sinAlpha = max(_ErosionMinimumAngleThresshold, sqrt(1 - 1 / (1 + dhx * dhx + dhy * dhy)));
			float C = _Kc * sinAlpha * sqrt(velocity.r * velocity.r + velocity.g * velocity.g) * _L * _L * heights.r; //transport capacity
			float4 capColor = float4((float3)(C / _Kc * 1000), 1);

			// total color
			float4 c = lerp(wsrColor, saturate(capColor), _DebugPerc);
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
