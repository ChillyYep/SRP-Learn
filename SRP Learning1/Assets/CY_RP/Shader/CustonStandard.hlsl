#ifndef CUSTONSTANDARD_INCLUDED
#define CUSTONSTANDARD_INCLUDED
#include "Packages/com.unity.render-pipelines.core/ShaderLibrary/Common.hlsl"
#include "Packages/com.unity.render-pipelines.core/ShaderLibrary/CommonMaterial.hlsl"
#include "../ShaderLibrary/UnityInput.hlsl"
#include "../ShaderLibrary/Custom_Surface.hlsl"
#include "../ShaderLibrary/CustomBRDF.hlsl"
#include "../ShaderLibrary/CustomShadows.hlsl"
#include "../ShaderLibrary/Custom_Light.hlsl"
#include "../ShaderLibrary/Custom_Lighting.hlsl"

#include "Packages/com.unity.render-pipelines.core/ShaderLibrary/UnityInstancing.hlsl"
#include "Packages/com.unity.render-pipelines.core/ShaderLibrary/SpaceTransforms.hlsl"

TEXTURE2D(_BaseMap);
SAMPLER(sampler_BaseMap);

UNITY_INSTANCING_BUFFER_START(UnityPerMaterial)
    UNITY_DEFINE_INSTANCED_PROP(float4,_BaseMap_ST)
    UNITY_DEFINE_INSTANCED_PROP(float4,_BaseColor)
    UNITY_DEFINE_INSTANCED_PROP(float,_Metallic)
    UNITY_DEFINE_INSTANCED_PROP(float,_Smoothness)
    UNITY_DEFINE_INSTANCED_PROP(float,_Cutoff)
UNITY_INSTANCING_BUFFER_END(UnityPerMaterial)

struct Attributes{
    float3 positionOS:POSITION;
    float2 baseUV:TEXCOORD0;
    float3 normalOS:NORMAL;
    UNITY_VERTEX_INPUT_INSTANCE_ID
};
struct Varyings{
    float4 positionCS:SV_POSITION;
    float2 baseUV:VAR_BASE_UV;
    float3 normalWS:VAR_NORMAL;
    float3 positionWS : VAR_POSITION;
    UNITY_VERTEX_INPUT_INSTANCE_ID
};

Varyings LitPassVertex(Attributes input)
{
    Varyings output;
    UNITY_SETUP_INSTANCE_ID(input)
    UNITY_TRANSFER_INSTANCE_ID(input,output);
    float4 baseST=UNITY_ACCESS_INSTANCED_PROP(UnityPerMaterial,_BaseMap_ST);
    output.baseUV=input.baseUV*baseST.xy+baseST.zw;
    output.positionWS=TransformObjectToWorld(input.positionOS);
    output.positionCS=TransformWorldToHClip(output.positionWS);
    output.normalWS=TransformObjectToWorldNormal(input.normalOS);
    return output;
}
float4 LitPassFragment(Varyings input):SV_TARGET
{
    UNITY_SETUP_INSTANCE_ID(input)
    float4 baseMap=SAMPLE_TEXTURE2D(_BaseMap,sampler_BaseMap,input.baseUV);
    float4 baseColor=UNITY_ACCESS_INSTANCED_PROP(UnityPerMaterial,_BaseColor);
    float4 base=baseMap*baseColor;
    #if defined(_CLIPPING)
        clip(base.a-UNITY_ACCESS_INSTANCED_PROP(UnityPerMaterial,_Cutoff));
    #endif
    Surface surface;
    surface.position=input.positionWS;
    surface.normal=normalize(input.normalWS);
    surface.color=base.rgb;
    surface.alpha=base.a;
    surface.metallic=UNITY_ACCESS_INSTANCED_PROP(UnityPerMaterial,_Metallic);
    surface.smoothness=UNITY_ACCESS_INSTANCED_PROP(UnityPerMaterial,_Smoothness);
    surface.viewDirection=normalize(_WorldSpaceCameraPos-input.positionWS);
    surface.depth=-TransformWorldToView(input.positionWS).z;
    #if defined(_PREMULTIPLY_ALPHA)
        BRDF brdf=GetBRDF(surface,true);
    #else
        BRDF brdf=GetBRDF(surface);
    #endif
	return float4(GetMainDiffuse(surface,brdf), surface.alpha);
}
#endif