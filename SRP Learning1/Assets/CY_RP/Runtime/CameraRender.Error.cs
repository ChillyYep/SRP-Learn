using System.Collections;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;
using UnityEngine.Profiling;
using UnityEngine.Rendering;

namespace CY.Rendering
{
    partial class CameraRender
    {
#if UNITY_EDITOR
        static ShaderTagId[] legacyShaderTagIds = {
            new ShaderTagId("Always"),
            new ShaderTagId("ForwardBase"),
            new ShaderTagId("PrepassBase"),
            new ShaderTagId("Vertex"),
            new ShaderTagId("VertexLMRGBM"),
            new ShaderTagId("VertexLM")
        };
        static Material errorMaterial;
        string SampleName { get; set; }
        partial void DrawUnsupportedShaders()
        {
            if (errorMaterial == null)
            {
                errorMaterial = new Material(Shader.Find("Hidden/InternalErrorShader"));
            }
            var drawingSettings = new DrawingSettings(legacyShaderTagIds[0], new SortingSettings(Params.camera)) { overrideMaterial = errorMaterial };
            for (int i = 0; i < legacyShaderTagIds.Length; ++i)
            {
                drawingSettings.SetShaderPassName(i, legacyShaderTagIds[i]);
            }
            var filteringSettings = FilteringSettings.defaultValue;
            GlobalUniqueParamsForRP.context.DrawRenderers(cullingResults, ref drawingSettings, ref filteringSettings);
        }
        partial void PrepareBuffer()
        {
            Profiler.BeginSample("Editor Only");
            cmdBuffer.name = SampleName = Params.camera.name;
            Profiler.EndSample();
        }
        partial void PrepareForSceneWindow()
        {
            if (Params.camera.cameraType == CameraType.SceneView)
            {
                ScriptableRenderContext.EmitWorldGeometryForSceneView(Params.camera);
            }
        }
        partial void DrawGizmos()
        {
            if (Handles.ShouldRenderGizmos())
            {
                GlobalUniqueParamsForRP.context.DrawGizmos(Params.camera, GizmoSubset.PreImageEffects);
                GlobalUniqueParamsForRP.context.DrawGizmos(Params.camera, GizmoSubset.PostImageEffects);
            }
        }
#else
    const string SampleName = bufferName;
#endif
    }
}
