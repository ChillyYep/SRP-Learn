#ifndef CUSTOMSHADOWS_INCLUDED
#define CUSTOMSHADOWS_INCLUDED

#define MAX_SHADOWED_DIRECTIONAL_LIGHT_COUNT 4
#define MAX_CASCADE_COUNT 4

TEXTURE2D_SHADOW(_DirectionalShadowAtlas);
#define SHADOW_SAMPLER sampler_linear_clamp_compare
SAMPLER_CMP(SHADOW_SAMPLER);

CBUFFER_START(_CustomShadows)
	int _CascadeCount;
	float4 _CascadeCullingSpheres[MAX_CASCADE_COUNT];
	float4x4 _DirectionalShadowMatrices[MAX_SHADOWED_DIRECTIONAL_LIGHT_COUNT*MAX_CASCADE_COUNT];
	float4 _ShadowDistanceFade;;
CBUFFER_END

struct DirectionalShadowData {
	float strength;
	int tileIndex;
};
struct ShadowData{
	int cascadeIndex;
	float strength;
};
float DistanceSquared(float3 pA, float3 pB) {
	return dot(pA - pB, pA - pB);
}
float FadedShadowStrength (float distance, float scale, float fade) {
	return saturate((1.0 - distance * scale) * fade);//(1-d/m)/f,d->深度depth,m->最大距离maxDistance,f->distanceFade
}
ShadowData GetShadowData(Surface surfaceWS)
{
	ShadowData data;
	data.strength=FadedShadowStrength(surfaceWS.depth,_ShadowDistanceFade.x,_ShadowDistanceFade.y);//原始的强度由Light的Shadow中的strength控制，现在加上深度的影响
	int i;
	[unroll(4)]
	for (i = 0; i < _CascadeCount; i++) {
		float4 sphere = _CascadeCullingSpheres[i];
		float distanceSqr = DistanceSquared(surfaceWS.position, sphere.xyz);//shpere.xyz球中心坐标，sphere.w为半径的平方
		if (distanceSqr < sphere.w) {
			if (i == _CascadeCount - 1) {
				data.strength *= FadedShadowStrength(
					distanceSqr, 1.0 / sphere.w, _ShadowDistanceFade.z
				);
			}
			break;
		}
		if(i==_CascadeCount){
			data.strength=0.0;//只是优化
		}
	}
	data.cascadeIndex = i;
	return data;
}
float SampleDirectionalShadowAtlas (float3 positionSTS) {
	return SAMPLE_TEXTURE2D_SHADOW(
		_DirectionalShadowAtlas, SHADOW_SAMPLER, positionSTS
	);
}
float GetDirectionalShadowAttenuation (DirectionalShadowData data, Surface surfaceWS) {
    if (data.strength <= 0.0) {
		return 1.0;
	}
	float4 positionSTS = mul(
		_DirectionalShadowMatrices[data.tileIndex],
		float4(surfaceWS.position, 1.0)
	);
	positionSTS.xyz/=positionSTS.w;
	float shadow = SampleDirectionalShadowAtlas(positionSTS.xyz);
	return lerp(1.0, shadow, data.strength);//stength越小，阴影越暗
}
#endif