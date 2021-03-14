using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Rendering;

namespace CY.Rendering
{
    public sealed class ShadowParams
    {

        public readonly CullingResults cullingResults;

        public readonly ShadowSettings settings;
        public ShadowParams(CullingResults cullingResults, ShadowSettings settings)
        {
            this.cullingResults = cullingResults;
            this.settings = settings;
        }
    }
    public class Shadows
    {
        static int dirShadowAtlasId = Shader.PropertyToID("_DirectionalShadowAtlas"),
            dirShadowMatricesId = Shader.PropertyToID("_DirectionalShadowMatrices"),
            cascadeCountId = Shader.PropertyToID("_CascadeCount"),
            cascadeCullingSpheresId = Shader.PropertyToID("_CascadeCullingSpheres"),
            shadowDistanceFadeId = Shader.PropertyToID("_ShadowDistanceFade");
        static public Vector4[] cascadeSpheres => cascadeCullingSpheres;
        static Vector4[] cascadeCullingSpheres = new Vector4[maxCascades];
        static Matrix4x4[] dirShadowMatrices = new Matrix4x4[maxShadowDirectionalLightCount * maxCascades];
        const string bufferName = "Shadows";
        const int maxShadowDirectionalLightCount = 4, maxCascades = 4;
        int ShadowDirectionalLightCount;
        struct ShadowDirectionalLight
        {
            public int visibleLightIndex;
        }
        ShadowDirectionalLight[] shadowDirectionalLights = new ShadowDirectionalLight[maxShadowDirectionalLightCount];

        CommandBuffer buffer = new CommandBuffer
        {
            name = bufferName
        };
        ShadowParams shadowParams;
        public void Setup(ShadowParams param)
        {
            shadowParams = param;
            ShadowDirectionalLightCount = 0;
        }
        public void RenderDirectionalShadows()
        {
            int atlasSize = (int)shadowParams.settings.directional.atlasSize;
            //申请渲染纹理缓冲
            buffer.GetTemporaryRT(dirShadowAtlasId, atlasSize, atlasSize, 32, FilterMode.Bilinear, RenderTextureFormat.Shadowmap);
            //设置_DirectionalShadowAtlas纹理为相机渲染目标
            buffer.SetRenderTarget(dirShadowAtlasId, RenderBufferLoadAction.DontCare, RenderBufferStoreAction.Store);
            //此时相机渲染的或许是shadowmap，为保证后续渲染真实的世界需要clear depth buffer,但是color缓冲会被后续的渲染清空，所以此处不必清空
            buffer.ClearRenderTarget(true, false, Color.clear);
            buffer.BeginSample(bufferName);
            ExecuteBuffer();
            int tiles = ShadowDirectionalLightCount * shadowParams.settings.directional.cascadeCount;
            int split = tiles <= 1 ? 1 : tiles <= 4 ? 2 : 4;//在x，y方向各切分了split个块
            int tileSize = atlasSize / split;
            for (int i = 0; i < ShadowDirectionalLightCount; ++i)
            {
                RenderDirectionalShadows(i, split, tileSize);
            }
            //(1-d/m)/f,d->深度depth,m->最大距离maxDistance,f->distanceFade
            float f = 1f - shadowParams.settings.directional.cascadeFade;
            buffer.SetGlobalVector(shadowDistanceFadeId, new Vector4(1f / shadowParams.settings.maxDistance, 1f / shadowParams.settings.distanceFade, 1f / (1f - f * f)));
            buffer.SetGlobalInt(cascadeCountId, shadowParams.settings.directional.cascadeCount);
            buffer.SetGlobalVectorArray(
                cascadeCullingSpheresId, cascadeCullingSpheres
            );
            buffer.SetGlobalMatrixArray(dirShadowMatricesId, dirShadowMatrices);
            buffer.EndSample(bufferName);
            ExecuteBuffer();
        }
        Matrix4x4 ConvertToAtlasMatrix(Matrix4x4 m, Vector2 offset, int split)
        {
            //第三行为z轴，矩阵行列本身没有意义，当我们把它们当作行矩阵时，它的行便有意义（把它们当作列矩阵时，它的列便有意义）
            //统一标准，所有地方都用行矩阵
            if (SystemInfo.usesReversedZBuffer)
            {
                m.m20 = -m.m20;
                m.m21 = -m.m21;
                m.m22 = -m.m22;
                m.m23 = -m.m23;
            }
            //传入的矩阵m，是将物体从世界空间变换到裁剪空间，裁剪空间是带第四维变量的，是NDC经历透视变换之前的状态空间
            //所以这里需要用第四维向量w，即(m.m30,m.m31,m.m32,m.m33)，去做位移，实际(-1，1)对应的(-w,w),位移完了之后，在
            //与裁剪空间xoy平面平行的各平面其x,y坐标的区间在(0,2w),所以还要乘以0.5
            //此处还要把shadowmap图集考虑在内，需要进一步做缩放和位移
            float scale = 1f / split;
            m.m00 = (0.5f * (m.m00 + m.m30) + offset.x * m.m30) * scale;
            m.m01 = (0.5f * (m.m01 + m.m31) + offset.x * m.m31) * scale;
            m.m02 = (0.5f * (m.m02 + m.m32) + offset.x * m.m32) * scale;
            m.m03 = (0.5f * (m.m03 + m.m33) + offset.x * m.m33) * scale;
            m.m10 = (0.5f * (m.m10 + m.m30) + offset.y * m.m30) * scale;
            m.m11 = (0.5f * (m.m11 + m.m31) + offset.y * m.m31) * scale;
            m.m12 = (0.5f * (m.m12 + m.m32) + offset.y * m.m32) * scale;
            m.m13 = (0.5f * (m.m13 + m.m33) + offset.y * m.m33) * scale;
            return m;
        }
        void RenderDirectionalShadows(int index, int split, int tileSize)
        {
            ShadowDirectionalLight light = shadowDirectionalLights[index];
            var shadowSettings = new ShadowDrawingSettings(shadowParams.cullingResults, light.visibleLightIndex);
            int cascadeCount = shadowParams.settings.directional.cascadeCount;
            int tileOffset = index * cascadeCount;
            Vector3 ratios = shadowParams.settings.directional.CasadeRatios;
            for (int i = 0; i < cascadeCount; ++i)
            {
                shadowParams.cullingResults.ComputeDirectionalShadowMatricesAndCullingPrimitives(light.visibleLightIndex, i, cascadeCount, ratios, tileSize, 0f,
                    out Matrix4x4 viewMatrix, out Matrix4x4 projectionMatrix, out ShadowSplitData splitData);
                shadowSettings.splitData = splitData;
                if (index == 0)
                {
                    //cullingSphere是以摄像机位置为球心的
                    Vector4 cullingSphere = splitData.cullingSphere;
                    cullingSphere.w *= cullingSphere.w;
                    cascadeCullingSpheres[i] = cullingSphere;
                }
                int tileIndex = tileOffset + i;
                dirShadowMatrices[tileIndex] = ConvertToAtlasMatrix(projectionMatrix * viewMatrix, SetTileViewPort(tileIndex, split, tileSize), split);
                //绘制指定区域的
                buffer.SetViewProjectionMatrices(viewMatrix, projectionMatrix);
                buffer.SetGlobalDepthBias(50000f, 0f);
                ExecuteBuffer();
                GlobalUniqueParamsForRP.context.DrawShadows(ref shadowSettings);
                buffer.SetGlobalDepthBias(0f, 0f);
            }
        }
        Vector2 SetTileViewPort(int index, int split, float tileSize)
        {
            Vector2 offset = new Vector2(index % split, index / split);
            buffer.SetViewport(new Rect(offset.x * tileSize, offset.y * tileSize, tileSize, tileSize));
            return offset;
        }
        public void Render()
        {
            if (ShadowDirectionalLightCount > 0)
            {
                RenderDirectionalShadows();
            }
            else
            {
                buffer.GetTemporaryRT(dirShadowAtlasId, 1, 1, 32, FilterMode.Bilinear, RenderTextureFormat.Shadowmap);
                buffer.SetRenderTarget(dirShadowAtlasId, RenderBufferLoadAction.DontCare, RenderBufferStoreAction.Store);
            }
        }
        public void Cleanup()
        {
            //不能通过判断ShadowDirectionalLightCount
            buffer.ReleaseTemporaryRT(dirShadowAtlasId);
            ExecuteBuffer();
        }
        public Vector2 ReserveDirecionalShadows(Light light, int visibleLightIndex)
        {
            //目前只接受Light列表里的第一个直线光
            if (ShadowDirectionalLightCount < maxShadowDirectionalLightCount &&//可生产阴影的直线光数量不超过上限
                light.shadows != LightShadows.None && light.shadowStrength > 0f &&//灯光的设置里开启阴影，且阴影强度不为0
                shadowParams.cullingResults.GetShadowCasterBounds(visibleLightIndex, out Bounds b)//查看该灯光的影响范围是否在相机可见视锥体内
                )
            {
                shadowDirectionalLights[ShadowDirectionalLightCount] = new ShadowDirectionalLight { visibleLightIndex = visibleLightIndex };
                return new Vector2(light.shadowStrength, shadowParams.settings.directional.cascadeCount * ShadowDirectionalLightCount++);
            }
            return Vector2.zero;
        }
        void ExecuteBuffer()
        {
            GlobalUniqueParamsForRP.context.ExecuteCommandBuffer(buffer);
            buffer.Clear();
        }
    }
}
