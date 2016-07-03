#ifndef SIMVIS_CG_INCLUDED
#define SIMVIS_CG_INCLUDED

inline float CalculateHeightSand(float4 wsr) //water sand rock
{
	return wsr.g + wsr.b;
}

inline float SampleHeightSand(sampler2D heights, float4 coords)
{
	float4 wsr = tex2Dlod(heights, coords);
	return CalculateHeightSand(wsr);
}

inline float CalculateHeightWater(float4 wsr) //water sand rock
{
	return wsr.r + wsr.g + wsr.b;
}

inline float SampleHeightWater(sampler2D heights, float4 coords)
{
	float4 wsr = tex2Dlod(heights, coords);
	return CalculateHeightWater(wsr);
}

inline float4 SampleRight(sampler2D tex, float2 coords, float4 texelSize)
{
	return tex2D(tex, coords + fixed2(texelSize.x, 0));
}

inline float4 SampleLeft(sampler2D tex, float2 coords, float4 texelSize)
{
	return tex2D(tex, coords - fixed2(texelSize.x, 0));
}

inline float4 SampleBottom(sampler2D tex, float2 coords, float4 texelSize)
{
	return tex2D(tex, coords + fixed2(0, texelSize.y));
}

inline float4 SampleTop(sampler2D tex, float2 coords, float4 texelSize)
{
	return tex2D(tex, coords - fixed2(0, texelSize.y));
}

inline float3 CalculateNormal(float4 hRLBT, float _L)
{
	return normalize(float3(
		hRLBT.g - hRLBT.r,
		hRLBT.a - hRLBT.b,
		_L
		));
}

float3 CalculateSandNormal(sampler2D wsr, float2 coords, float4 texelSize, float _L)
{
	float4 hR = SampleRight(wsr, coords, texelSize);
	float4 hL = SampleLeft(wsr, coords, texelSize);
	float4 hB = SampleBottom(wsr, coords, texelSize);
	float4 hT = SampleTop(wsr, coords, texelSize);
	float4 hTotal = float4(
		CalculateHeightSand(hR),
		CalculateHeightSand(hL),
		CalculateHeightSand(hB),
		CalculateHeightSand(hT)
		);
	
	return CalculateNormal(hTotal, _L);
}

float3 CalculateWaterNormal(sampler2D wsr, float2 coords, float4 texelSize, float _L)
{
	float4 hR = SampleRight(wsr, coords, texelSize);
	float4 hL = SampleLeft(wsr, coords, texelSize);
	float4 hB = SampleBottom(wsr, coords, texelSize);
	float4 hT = SampleTop(wsr, coords, texelSize);
	float4 hTotal = float4(
		CalculateHeightWater(hR),
		CalculateHeightWater(hL),
		CalculateHeightWater(hB),
		CalculateHeightWater(hT)
		);

	return CalculateNormal(hTotal, _L);
}

#endif
