﻿#ifndef CUSTOM_LIGHT_INCLUDED
#define CUSTOM_LIGHT_INCLUDED
#define MAX_DIRECTIONAL_LIGHT_COUNT 4
struct Light {
	float3 color;
	float3 direction;
};
CBUFFER_START(_CustomLight)
	int _DirectionalLightCount;
	float4 _DirectionalLightColors[MAX_DIRECTIONAL_LIGHT_COUNT];
	float4 _DirectionalLightDirections[MAX_DIRECTIONAL_LIGHT_COUNT];
CBUFFER_END
int GetDirectionalLightCount()
{
	return _DirectionalLightCount;
}
Light GetDirectionalLight (int index) {
	Light light;
	light.color = _DirectionalLightColors[index].rgb;
	light.direction = _DirectionalLightDirections[index].xyz;
	return light;
}
float3 GetSaturateIncomingLightColor (Surface surface, Light light) {
	return saturate(dot(surface.normal, light.direction)) * light.color;
}

float3 GetDiffuse (Surface surface, Light light) {
	return GetSaturateIncomingLightColor(surface, light) * surface.color;
}


#endif