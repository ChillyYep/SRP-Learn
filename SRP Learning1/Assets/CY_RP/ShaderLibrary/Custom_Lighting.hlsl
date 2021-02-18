#ifndef CUSTOM_LIGHTING_INCLUDED
#define CUSTOM_LIGHTING_INCLUDED
float3 GetMainDiffuse (Surface surfaceWS,BRDF brdf) {
	ShadowData shadowData=GetShadowData(surfaceWS);
	float3 color=0.0;
	[unroll(3)]
	for (int i = 0; i < GetDirectionalLightCount(); i++) {
		Light light = GetDirectionalLight(i,surfaceWS,shadowData);
		color += GetDiffuse(surfaceWS,brdf,light);
		// return light.attenuation;
	}
	return color;
}
#endif