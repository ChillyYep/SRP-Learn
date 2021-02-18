using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Rendering;
namespace CY.Rendering
{
    public class GlobalUniqueParamsForRP
    {
        public static ScriptableRenderContext context;
    }
    public class CYRenderPipeline : RenderPipeline
    {
        CameraRender render = new CameraRender();
        CYRenderPipelineAsset rpAsset;
        ShadowSettings shadowSettings;
        public CYRenderPipeline(RenderPipelineAsset asset)
        {
            rpAsset = asset as CYRenderPipelineAsset;
            this.shadowSettings = rpAsset.shadowSettings;
            GraphicsSettings.useScriptableRenderPipelineBatching = rpAsset.SRPBatcherEnabled;
            GraphicsSettings.lightsUseLinearIntensity = true;
        }
        protected override void Render(ScriptableRenderContext context, Camera[] cameras)
        {
            GlobalUniqueParamsForRP.context = context;
            foreach (var camera in cameras)
            {
                render.Render(new CameraRenderParams(camera, rpAsset.SRPDynamicBatching, rpAsset.GPUInstanceing, rpAsset.shadowSettings));
            }
        }
    }
}
