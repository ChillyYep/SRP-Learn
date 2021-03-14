Shader "Custom/Standard"
{
    Properties
    {
        _BaseMap("Texture",2D)="white"{}
        _BaseColor("Color",Color)=(0.5,0.5,0.5,1.0)
        _Metallic("Metallic",Range(0,1)) = 0
        _Smoothness ("Smoothness", Range(0, 1)) = 0.5
        [Toggle(_CLIPPING)] _Clipping ("Alpha Clipping", Float) = 0
        _Cutoff("Alpha Cutoff",Range(0.0,1.0))=0.5
        [Enum(UnityEngine.Rendering.BlendMode)]_SrcBlend("SrcBlend",Float)=1
        [Enum(UnityEngine.Rendering.BlendMode)]_DstBlend("DstBlend",Float)=0
        [Toggle(_PREMULTIPLY_ALPHA)] _PremulAlpha("Premultiply",Float) = 0
        [Enum(Off,0,On,1)]_ZWrite("Z Write",Float)=1
    }
    CustomEditor "CustomShaderGUI"
    SubShader
    {
        Pass
        {
            Blend [_SrcBlend] [_DstBlend]
            ZWrite [_ZWrite]
            Cull Off
            Tags{
                "LightMode"="CustomLit"
            }
            HLSLPROGRAM
            #pragma target 3.5
            #pragma shader_feature _CLIPPING
            #pragma shader_feature _PREMULTIPLY_ALPHA
            #pragma multi_compile_instancing
            #pragma vertex LitPassVertex
            #pragma fragment LitPassFragment
            #include "CustonStandard.hlsl"
            ENDHLSL
        }
        Pass{
            //URP自带ShadowCaster标签，在渲染其他对象前渲染深度图
            Tags {
				"LightMode" = "ShadowCaster"
			}

			ColorMask 0 //ColorMask,表示只输出某种颜色通道（ColorMask RGB | A | 0 | 其他R,G,B,A的组合）

			HLSLPROGRAM
			#pragma target 3.5
			#pragma shader_feature _CLIPPING
			#pragma multi_compile_instancing//gpu instancing
			#pragma vertex ShadowCasterPassVertex
			#pragma fragment ShadowCasterPassFragment
			#include "ShadowCasterPass.hlsl"
			ENDHLSL
        }
    }
}
