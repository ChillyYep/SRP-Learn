#ifndef CUSTOM_LIT_PASS_INCLUDED
#define CUSTOM_LIT_PASS_INCLUDED

#include "Packages/com.unity.render-pipelines.core/ShaderLibrary/Common.hlsl"
#include "UnityInput.hlsl"
#include "../ShaderLibrary/Custom_Surface.hlsl"
#include "../ShaderLibrary/Custom_Light.hlsl"
#include "../ShaderLibrary/Custom_Lighting.hlsl"

#define UNITY_MATRIX_M unity_ObjectToWorld
#define UNITY_MATRIX_I_M unity_WorldToObject
#define UNITY_MATRIX_V unity_MatrixV
#define UNITY_MATRIX_VP unity_MatrixVP
#define UNITY_MATRIX_P glstate_matrix_projection
#include "Packages/com.unity.render-pipelines.core/ShaderLibrary/UnityInstancing.hlsl"
#include "Packages/com.unity.render-pipelines.core/ShaderLibrary/SpaceTransforms.hlsl"

TEXTURE2D(_BaseMap);
SAMPLER(sampler_BaseMap);

UNITY_INSTANCING_BUFFER_START(UnityPerMaterial)
    UNITY_DEFINE_INSTANCED_PROP(float4,_BaseMap_ST)
    UNITY_DEFINE_INSTANCED_PROP(float4,_BaseColor)
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
    UNITY_VERTEX_INPUT_INSTANCE_ID
};

Varyings LitPassVertex(Attributes input)
{
    Varyings output;
    UNITY_SETUP_INSTANCE_ID(input)
    UNITY_TRANSFER_INSTANCE_ID(input,output)
    float4 baseST=UNITY_ACCESS_INSTANCED_PROP(UnityPerMaterial,_BaseMap_ST);
    output.baseUV=input.baseUV*baseST.xy+baseST.zw;
    float3 positionWS=TransformObjectToWorld(input.positionOS);
    output.positionCS=TransformWorldToHClip(positionWS);
    output.normalWS=TransformObjectToWorldNormal(input.normalOS);
    return output;
}
float4 LitPassFragment(Varyings input):SV_TARGET
{
    UNITY_SETUP_INSTANCE_ID(input)
    float4 baseMap=SAMPLE_TEXTURE2D(_BaseMap,sampler_BaseMap,input.baseUV);
    float4 baseColor=UNITY_ACCESS_INSTANCED_PROP(UnityPerMaterial,_BaseColor);
    float4 base=baseMap*baseColor;
    Surface surface;
    surface.normal=normalize(input.normalWS);
    surface.color=base.rgb;
    surface.alpha=base.a;
	return float4(GetLighting(surface), surface.alpha);
}
#endif