using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Rendering;

namespace CY.Rendering
{
    [CreateAssetMenu(menuName = "Rendering/CYRenderPipeline")]
    public class CYRenderPipelineAsset : RenderPipelineAsset
    {
        public bool SRPBatcherEnabled = false;
        public bool SRPDynamicBatching = false;
        public bool GPUInstanceing = true;
        protected override RenderPipeline CreatePipeline()
        {
            return new CYRenderPipeline(this);
        }
    }
}
