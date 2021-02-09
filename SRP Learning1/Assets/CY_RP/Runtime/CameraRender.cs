using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Rendering;

namespace CY.Rendering
{
    public partial class CameraRender
    {
        /// <summary>
        /// unity SRP管线中有一个默认标签叫做SRPDefaultUnlit，如果不在Shader的Tags显示指定LightMode，
        /// 就会默认使用"LightMode"="SRPDefaultUnlit"，但要想该标签生效，还是需要在DrawSetting中设置tagId
        /// </summary>
        static ShaderTagId unlitShaderTagId = new ShaderTagId("SRPDefaultUnlit"),
            litShaderTagId = new ShaderTagId("CustomLit");
        Lighting lighting = new Lighting();
        ScriptableRenderContext context;
        Camera camera;
        const string bufferName = "Render Camera";
        CommandBuffer cmdBuffer = new CommandBuffer()
        {
            name = bufferName
        };
        CullingResults cullingResults;
        bool useDynamicBatching = false;
        bool useGPUInstancing = false;
        public void SetDrawField(bool useDynamicBatching, bool useGPUInstancing)
        {
            this.useDynamicBatching = useDynamicBatching;
            this.useGPUInstancing = useGPUInstancing;
        }
        public void Render(ScriptableRenderContext context, Camera camera)
        {
            this.context = context;
            this.camera = camera;
            PrepareBuffer();
            PrepareForSceneWindow();
            if (!Cull())
            {
                return;
            }
            Setup();
            lighting.Setup(context,ref cullingResults);
            DrawVisibleGeometry();
            DrawUnsupportedShaders();
            DrawGizmos();
            Submit();
        }
        void Setup()
        {
            CameraClearFlags flags = camera.clearFlags;
            cmdBuffer.ClearRenderTarget(flags <= CameraClearFlags.Depth, flags == CameraClearFlags.Color, flags == CameraClearFlags.Color ? camera.backgroundColor.linear : Color.clear);
            //将相机的属性关联进来，包括VP（view-projection）矩阵
            context.SetupCameraProperties(camera);
            cmdBuffer.BeginSample(SampleName);
            ExcuteBuffer();
        }
        bool Cull()
        {
            if (camera.TryGetCullingParameters(out ScriptableCullingParameters p))
            {
                cullingResults = context.Cull(ref p);
                return true;
            }
            return false;
        }
        void DrawVisibleGeometry()
        {
            var sortingSettings = new SortingSettings(camera)
            {
                criteria = SortingCriteria.CommonOpaque
            };
            var drawingSettings = new DrawingSettings(unlitShaderTagId, sortingSettings)
            {
                enableDynamicBatching = useDynamicBatching,
                enableInstancing = useGPUInstancing
            };
            //增加shader标签
            drawingSettings.SetShaderPassName(1, litShaderTagId);
            var filterSettings = new FilteringSettings(RenderQueueRange.opaque);
            context.DrawRenderers(cullingResults, ref drawingSettings, ref filterSettings);
            //绘制天空盒，由Camera的ClearFlag决定是否渲染天空盒
            context.DrawSkybox(camera);
            sortingSettings.criteria = SortingCriteria.CommonTransparent;
            drawingSettings.sortingSettings = sortingSettings;
            filterSettings.renderQueueRange = RenderQueueRange.transparent;
            context.DrawRenderers(cullingResults, ref drawingSettings, ref filterSettings);
        }
        partial void DrawUnsupportedShaders();
        partial void PrepareBuffer();
        partial void DrawGizmos();
        partial void PrepareForSceneWindow();
        void ExcuteBuffer()
        {
            context.ExecuteCommandBuffer(cmdBuffer);
            cmdBuffer.Clear();
        }
        void Submit()
        {
            cmdBuffer.EndSample(SampleName);
            ExcuteBuffer();
            context.Submit();
        }
    }
}
