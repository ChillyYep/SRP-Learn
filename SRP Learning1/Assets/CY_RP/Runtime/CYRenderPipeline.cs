using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Rendering;
namespace CY.Rendering
{
    public class CYRenderPipeline : RenderPipeline
    {
        CameraRender render = new CameraRender();
        CYRenderPipelineAsset rpAsset;
        public CYRenderPipeline(RenderPipelineAsset asset)
        {
            rpAsset = asset as CYRenderPipelineAsset;
            GraphicsSettings.useScriptableRenderPipelineBatching = rpAsset.SRPBatcherEnabled;
        }
        protected override void Render(ScriptableRenderContext context, Camera[] cameras)
        {
            foreach (var camera in cameras)
            {
                render.SetDrawField(rpAsset.SRPDynamicBatching, rpAsset.GPUInstanceing);
                render.Render(context, camera);
            }
        }
    }
}
