using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;
[ExecuteInEditMode]
public class MeshBall : MonoBehaviour
{
    static int baseColorId = Shader.PropertyToID("_BaseColor");
    static int cutoffId = Shader.PropertyToID("_Cutoff");
    [SerializeField]
    Mesh mesh = default;
    [SerializeField]
    Material material = default;
    [SerializeField]
    [Range(0f, 1f)]
    float cutoff = 0f;
    const int ARRAY_LENGTH = 1023;
    Matrix4x4[] matrices = new Matrix4x4[ARRAY_LENGTH];
    Vector4[] baseColors = new Vector4[ARRAY_LENGTH];
    float[] cutoffs = new float[ARRAY_LENGTH];
    MaterialPropertyBlock block;
    private void Awake()
    {
        for (int i = 0; i < ARRAY_LENGTH; ++i)
        {
            matrices[i] = Matrix4x4.TRS(Random.insideUnitSphere * 10f, Quaternion.identity, Vector3.one);
            baseColors[i] = new Vector4(Random.value, Random.value, Random.value, 1f);
        }
    }

    private void Update()
    {
        if (block == null)
        {
            block = new MaterialPropertyBlock();
            block.SetVectorArray(baseColorId, baseColors);
        }
        block.SetFloatArray(cutoffId, cutoffs);
        for (int i = 0; i < ARRAY_LENGTH; ++i)
        {
            cutoffs[i] = cutoff;
        }
        Graphics.DrawMeshInstanced(mesh, 0, material, matrices, ARRAY_LENGTH, block);
    }
}
