using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Rendering;

namespace CY.Rendering
{
    public sealed class CameraRenderParams
    {
        public readonly Camera camera;
        public readonly bool useDynamicBatching;
        public readonly bool useGPUInstancing;
        public readonly ShadowSettings shadowSettings;
        public CameraRenderParams(Camera camera, bool useDynamicBatching, bool useGPUInstancing, ShadowSettings shadowSettings)
        {
            this.camera = camera;
            this.useDynamicBatching = useDynamicBatching;
            this.useGPUInstancing = useGPUInstancing;
            this.shadowSettings = shadowSettings;
        }
    }
    public partial class CameraRender
    {
        /// <summary>
        /// unity SRP管线中有一个默认标签叫做SRPDefaultUnlit，如果不在Shader的Tags显示指定LightMode，
        /// 就会默认使用"LightMode"="SRPDefaultUnlit"，但要想该标签生效，还是需要在DrawSetting中设置tagId
        /// </summary>
        static ShaderTagId unlitShaderTagId = new ShaderTagId("SRPDefaultUnlit"),
            litShaderTagId = new ShaderTagId("CustomLit"),
            shadowCasterTagId = new ShaderTagId("ShadowCaster");
        Lighting lighting = new Lighting();
        const string bufferName = "Render Camera";
        CommandBuffer cmdBuffer = new CommandBuffer()
        {
            name = bufferName
        };
        CullingResults cullingResults;
        CameraRenderParams Params;
        /// <summary>
        /// 渲染流程，创建、资源回收/释放都在这一个流程里
        /// </summary>
        /// <param name="param"></param>
        public void Render(CameraRenderParams param)
        {
            Params = param;
            PrepareBuffer();
            PrepareForSceneWindow();
            if (!Cull())
            {
                return;
            }
            cmdBuffer.BeginSample(SampleName);
            ExcuteBuffer();
            lighting.Setup(new LightingParams(cullingResults, Params.shadowSettings));
            cmdBuffer.EndSample(SampleName);
            Setup();
            DrawVisibleGeometry();
            DrawUnsupportedShaders();
            DrawGizmos();
            lighting.Cleanup();
            Submit();
        }
        void Setup()
        {
            CameraClearFlags flags = Params.camera.clearFlags;
            cmdBuffer.ClearRenderTarget(flags <= CameraClearFlags.Depth, flags == CameraClearFlags.Color, flags == CameraClearFlags.Color ? Params.camera.backgroundColor.linear : Color.clear);
            //将相机的属性关联进来，包括VP（view-projection）矩阵
            GlobalUniqueParamsForRP.context.SetupCameraProperties(Params.camera);
            cmdBuffer.BeginSample(SampleName);
            ExcuteBuffer();
        }
        bool Cull()
        {
            if (Params.camera.TryGetCullingParameters(out ScriptableCullingParameters p))
            {
                //shadowDistance会影响_CascadeCullingSpheres
                p.shadowDistance = Mathf.Min(Params.shadowSettings.maxDistance, Params.camera.farClipPlane);
                cullingResults = GlobalUniqueParamsForRP.context.Cull(ref p);
                return true;
            }
            return false;
        }
        void DrawVisibleGeometry()
        {
            var sortingSettings = new SortingSettings(Params.camera)
            {
                criteria = SortingCriteria.CommonOpaque
            };
            var drawingSettings = new DrawingSettings(unlitShaderTagId, sortingSettings)
            {
                enableDynamicBatching = Params.useDynamicBatching,
                enableInstancing = Params.useGPUInstancing
            };
            //增加shader标签
            drawingSettings.SetShaderPassName(1, litShaderTagId);
            //drawingSettings.SetShaderPassName(2, shadowCasterTagId);
            var filterSettings = new FilteringSettings(RenderQueueRange.opaque);
            GlobalUniqueParamsForRP.context.DrawRenderers(cullingResults, ref drawingSettings, ref filterSettings);
            //绘制天空盒，由Camera的ClearFlag决定是否渲染天空盒,在不透明队列后面渲染，避免天空盒overdraw
            GlobalUniqueParamsForRP.context.DrawSkybox(Params.camera);
            sortingSettings.criteria = SortingCriteria.CommonTransparent;
            drawingSettings.sortingSettings = sortingSettings;
            filterSettings.renderQueueRange = RenderQueueRange.transparent;
            GlobalUniqueParamsForRP.context.DrawRenderers(cullingResults, ref drawingSettings, ref filterSettings);
        }
        partial void DrawUnsupportedShaders();
        partial void PrepareBuffer();
        partial void DrawGizmos();
        partial void PrepareForSceneWindow();
        void ExcuteBuffer()
        {
            GlobalUniqueParamsForRP.context.ExecuteCommandBuffer(cmdBuffer);
            cmdBuffer.Clear();
        }
        void Submit()
        {
            cmdBuffer.EndSample(SampleName);
            ExcuteBuffer();
            //Submit提交给GPU执行
            GlobalUniqueParamsForRP.context.Submit();
        }
    }
}
