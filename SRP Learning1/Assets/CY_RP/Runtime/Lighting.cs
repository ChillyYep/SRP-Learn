using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Rendering;

public class Lighting
{
    const string bufferName = "Lighting";
    static int dirLightDirColorId = Shader.PropertyToID("_DirectionalLightColor"),
        dirLightDirectionId = Shader.PropertyToID("_DirectionalLightDirection");
    CommandBuffer buffer = new CommandBuffer()
    {
        name = bufferName
    };
    public void Setup(ScriptableRenderContext context)
    {
        buffer.BeginSample(bufferName);
        SetupDirectionalLight();
        buffer.EndSample(bufferName);
        context.ExecuteCommandBuffer(buffer);
        buffer.Clear();
    }
    void SetupDirectionalLight()
    {
        Light light = RenderSettings.sun;
        //buffer.SetGlobalVector(dirLightDirColorId, light.color.linear);
        buffer.SetGlobalColor(dirLightDirColorId, light.color.linear * light.intensity);
        buffer.SetGlobalVector(dirLightDirectionId, -light.transform.forward);
    }
}
