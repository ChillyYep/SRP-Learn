﻿Shader "Custom/GPUInstancingTest"
{
    Properties{
        _BaseColor("Color",Color)=(1.0,1.0,1.0,1.0)
        _BaseMap("Texture",2D)="white"{}
        [Toggle(_CLIPPING)] _Clipping ("Alpha Clipping", Float) = 0
        _Cutoff("Alpha Cutoff",Range(0.0,1.0))=0.5
        [Enum(UnityEngine.Rendering.BlendMode)]_SrcBlend("SrcBlend",Float)=1
        [Enum(UnityEngine.Rendering.BlendMode)]_DstBlend("DstBlend",Float)=0
        [Enum(Off,0,On,1)]_ZWrite("Z Write",Float)=1
    }
    SubShader
    {
        Pass
        {
            Blend [_SrcBlend] [_DstBlend]
            ZWrite [_ZWrite]
            HLSLPROGRAM
            #pragma shader_feature _CLIPPING
            #pragma multi_compile_instancing
            #pragma vertex UnlitPassVertex
            #pragma fragment UnlitPassFragment
            #include "UnlitPass.hlsl"
            ENDHLSL
        }
    }
}