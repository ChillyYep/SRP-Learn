#ifndef CUSTOM_LIGHT_INCLUDED
#define CUSTOM_LIGHT_INCLUDED
struct Light {
	float3 color;
	float3 direction;
};
CBUFFER_START(_CustomLight)
	float3 _DirectionalLightColor;
	float3 _DirectionalLightDirection;
CBUFFER_END
Light GetDirectionalLight () {
	Light light;
	light.color = _DirectionalLightColor;
	light.direction = _DirectionalLightDirection;
	return light;
}

float3 GetIncomingLight (Surface surface, Light light) {
	return dot(surface.normal, light.direction) * light.color;
}

float3 IncomingLight (Surface surface, Light light) {
	return saturate(dot(surface.normal, light.direction)) * light.color;
}

float3 GetLighting (Surface surface, Light light) {
	return IncomingLight(surface, light) * surface.color;
}


#endif