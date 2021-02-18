#ifndef CUSTOMBRDF_INCLUDED
#define CUSTOMBRDF_INCLUDED
struct BRDF{
    float3 diffuse;
    float3 specular;
    float roughness;
};
#define MIN_REFLECTIVITY 0.04
//金属度越高，材质对光的吸收率越高，漫反射颜色越暗
float OneMinusReflectivity (float metallic) {
	float range = 1.0 - MIN_REFLECTIVITY;
	return range - metallic * range;
}
BRDF GetBRDF(Surface surface,bool applyAlphaToDiffuse = false)
{
    BRDF brdf;
    float oneMinusReflectivity=OneMinusReflectivity(surface.metallic);
    brdf.diffuse=surface.color*oneMinusReflectivity;   
    if(applyAlphaToDiffuse)
    {
        brdf.diffuse*=surface.alpha;
    }
    //金属度越高，镜面反射越强
    brdf.specular=lerp(MIN_REFLECTIVITY,surface.color,surface.metallic);
    float perceptualRoughness = PerceptualSmoothnessToPerceptualRoughness(surface.smoothness);
	brdf.roughness = PerceptualRoughnessToRoughness(perceptualRoughness);
    return brdf;
}
#endif