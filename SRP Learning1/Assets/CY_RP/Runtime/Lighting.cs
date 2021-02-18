using System.Collections;
using System.Collections.Generic;
using Unity.Collections;
using UnityEngine;
using UnityEngine.Rendering;
namespace CY.Rendering
{
    public sealed class LightingParams
    {
        public readonly CullingResults cullingResults;
        public readonly ShadowSettings shadowSettings;
        public LightingParams(CullingResults cullingResults, ShadowSettings shadowSettings)
        {
            this.cullingResults = cullingResults;
            this.shadowSettings = shadowSettings;
        }
    }
    public class Lighting
    {
        const string bufferName = "Lighting";
        const int maxDirLightCount = 4;
        static int dirLightDirColorId = Shader.PropertyToID("_DirectionalLightColors"),
            dirLightDirectionId = Shader.PropertyToID("_DirectionalLightDirections"),
            dirLightCountId = Shader.PropertyToID("_DirectionalLightCount"),
            dirLightShadowDataId = Shader.PropertyToID("_DirectionalLightShadowData");
        static Vector4[] dirLightColors = new Vector4[maxDirLightCount],
            dirLightDirections = new Vector4[maxDirLightCount],
            dirLightShadowData = new Vector4[maxDirLightCount];
        CommandBuffer buffer = new CommandBuffer()
        {
            name = bufferName
        };
        LightingParams lightingParams;
        Shadows shadows = new Shadows();
        public void Setup(LightingParams lightingParams)
        {
            this.lightingParams = lightingParams;
            buffer.BeginSample(bufferName);
            shadows.Setup(new ShadowParams(lightingParams.cullingResults, lightingParams.shadowSettings));
            SetupLights();
            shadows.Render();
            buffer.EndSample(bufferName);
            GlobalUniqueParamsForRP.context.ExecuteCommandBuffer(buffer);
            buffer.Clear();
        }
        void SetupDirectionalLight(int index, ref VisibleLight visibleLight)
        {
            dirLightColors[index] = visibleLight.finalColor;
            //对于灯光坐标系来说，其z轴就是光照方向，相反数则是物体到光源的方向
            //visibleLight.light.transform.forward;
            dirLightDirections[index] = -visibleLight.localToWorldMatrix.GetColumn(2);
            dirLightShadowData[index] = shadows.ReserveDirecionalShadows(visibleLight.light, index);
        }
        public void Cleanup()
        {
            shadows.Cleanup();
        }
        void SetupLights()
        {
            //在视锥体范围内有影响力的灯光,当然RenderSettings.sun必然包含其中
            NativeArray<VisibleLight> visibleLights = lightingParams.cullingResults.visibleLights;
            int dirLightCount = 0;
            for (int i = 0; i < visibleLights.Length; ++i)
            {
                var visibleLight = visibleLights[i];
                if (visibleLight.lightType == LightType.Directional)
                {
                    SetupDirectionalLight(i, ref visibleLight);
                    ++dirLightCount;
                    if (dirLightCount >= maxDirLightCount)
                    {
                        break;
                    }
                }

            }
            buffer.SetGlobalInt(dirLightCountId, dirLightCount);
            buffer.SetGlobalVectorArray(dirLightDirColorId, dirLightColors);
            buffer.SetGlobalVectorArray(dirLightDirectionId, dirLightDirections);
            buffer.SetGlobalVectorArray(dirLightShadowDataId, dirLightShadowData);
        }
    }

}
