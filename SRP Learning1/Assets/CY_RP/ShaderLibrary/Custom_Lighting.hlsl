#ifndef CUSTOM_LIGHTING_INCLUDED
#define CUSTOM_LIGHTING_INCLUDED
float3 GetMainDiffuse (Surface surface) {
	float3 color=0.0;
	[unroll(3)]
	for (int i = 0; i < GetDirectionalLightCount(); i++) {
		color += GetDiffuse(surface, GetDirectionalLight(i));
	}
	return color;
}
#endif