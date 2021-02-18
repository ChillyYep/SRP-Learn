#ifndef CUSTON_UNLIT_PASS_INCLUDED
#define CUSTON_UNLIT_PASS_INCLUDED

//real的定义在Common.hlsl中
#include "Packages/com.unity.render-pipelines.core/ShaderLibrary/Common.hlsl"
#include "../ShaderLibrary/UnityInput.hlsl"
#define UNITY_MATRIX_M unity_ObjectToWorld
#define UNITY_MATRIX_I_M unity_WorldToObject
#define UNITY_MATRIX_V unity_MatrixV
#define UNITY_MATRIX_VP unity_MatrixVP
#define UNITY_MATRIX_P glstate_matrix_projection
#include "Packages/com.unity.render-pipelines.core/ShaderLibrary/UnityInstancing.hlsl"
#include "Packages/com.unity.render-pipelines.core/ShaderLibrary/SpaceTransforms.hlsl"

TEXTURE2D(_BaseMap);
SAMPLER(sampler_BaseMap);
//material data,Material CBuffer
UNITY_INSTANCING_BUFFER_START(UnityPerMaterial1)
    UNITY_DEFINE_INSTANCED_PROP(float4,_BaseMap_ST)
    UNITY_DEFINE_INSTANCED_PROP(float4,_BaseColor)
    UNITY_DEFINE_INSTANCED_PROP(float,_Cutoff)
UNITY_INSTANCING_BUFFER_END(UnityPerMaterial1)

struct Attributes{
    float3 positionOS:POSITION;
    float2 baseUV:TEXCOORD0;
    //定义一个instanceId,一般是uint类型的
    UNITY_VERTEX_INPUT_INSTANCE_ID
};
struct Varyings{
    float4 positionCS:SV_POSITION;
    float2 baseUV:VAR_BASE_UV;
    //定义一个instanceId,一般是uint类型的
    UNITY_VERTEX_INPUT_INSTANCE_ID
};
Varyings UnlitPassVertex(Attributes input)
{
    Varyings output;
    //和instanced相关的一些变量的赋值
    UNITY_SETUP_INSTANCE_ID(input);
    //input中的instanceId拷贝到output中的instanceId
    UNITY_TRANSFER_INSTANCE_ID(input,output);
    float3 positionWS=TransformObjectToWorld(input.positionOS);
    output.positionCS=TransformWorldToHClip(positionWS);
    //取得对应的值
    float4 baseST=UNITY_ACCESS_INSTANCED_PROP(UnityPerMaterial1,_BaseMap_ST);
    output.baseUV=input.baseUV*baseST.xy+baseST.zw;
    return output;
}

float4 UnlitPassFragment(Varyings input):SV_TARGET
{
    UNITY_SETUP_INSTANCE_ID(input);
    float4 baseMap=SAMPLE_TEXTURE2D(_BaseMap,sampler_BaseMap,input.baseUV);
    float4 baseColor=UNITY_ACCESS_INSTANCED_PROP(UnityPerMaterial1,_BaseColor);
    float4 base =baseMap*baseColor;
    #if defined(_CLIPPING)
        clip(base.a-UNITY_ACCESS_INSTANCED_PROP(UnityPerMaterial1,_Cutoff));
    #endif
    return base;
    // return baseMap*baseColor;
}

#endif