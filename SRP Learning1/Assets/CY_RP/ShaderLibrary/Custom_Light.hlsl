#ifndef CUSTOM_LIGHT_INCLUDED
#define CUSTOM_LIGHT_INCLUDED
#define MAX_DIRECTIONAL_LIGHT_COUNT 4
struct Light {
	float3 color;
	float3 direction;
	float attenuation;
};
CBUFFER_START(_CustomLight)
	int _DirectionalLightCount;
	float4 _DirectionalLightColors[MAX_DIRECTIONAL_LIGHT_COUNT];
	float4 _DirectionalLightDirections[MAX_DIRECTIONAL_LIGHT_COUNT];
	float4 _DirectionalLightShadowData[MAX_DIRECTIONAL_LIGHT_COUNT];
CBUFFER_END
int GetDirectionalLightCount()
{
	return _DirectionalLightCount;
}
DirectionalShadowData GetDirectionalShadowData (int lightIndex,ShadowData shadowData) {
	DirectionalShadowData data;
	data.strength = _DirectionalLightShadowData[lightIndex].x*shadowData.strength;//强度
	data.tileIndex = _DirectionalLightShadowData[lightIndex].y+shadowData.cascadeIndex;//索引
	return data;
}
Light GetDirectionalLight (int index,Surface surfaceWS,ShadowData shadowData) {
	Light light;
	light.color = _DirectionalLightColors[index].rgb;
	light.direction = _DirectionalLightDirections[index].xyz;
	DirectionalShadowData dirShadowData = GetDirectionalShadowData(index,shadowData);
	light.attenuation = GetDirectionalShadowAttenuation(dirShadowData, surfaceWS);
	// light.attenuation = shadowData.cascadeIndex * 0.25;
	return light;
}
float3 GetSaturateIncomingLightColor (Surface surface, Light light) {
	return saturate(dot(surface.normal, light.direction)*light.attenuation) * light.color;
}
float Square (float v) {
	return v * v;
}
float SpecularStrength (Surface surface, BRDF brdf, Light light) {
	float3 h = SafeNormalize(light.direction + surface.viewDirection);
	float nh2 = Square(saturate(dot(surface.normal, h)));
	float lh2 = Square(saturate(dot(light.direction, h)));
	float r2 = Square(brdf.roughness);
	float d2 = Square(nh2 * (r2 - 1.0) + 1.00001);
	float normalization = brdf.roughness * 4.0 + 2.0;
	return r2 / (d2 * max(0.1, lh2) * normalization);
}
float3 DirectBRDF (Surface surface, BRDF brdf, Light light) {
	return SpecularStrength(surface, brdf, light) * brdf.specular + brdf.diffuse;
}
float3 GetDiffuse (Surface surface,BRDF brdf, Light light) {
	return GetSaturateIncomingLightColor(surface, light) * DirectBRDF(surface,brdf,light);
}

#endif