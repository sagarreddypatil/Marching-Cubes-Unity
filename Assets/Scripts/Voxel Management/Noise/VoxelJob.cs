using UnityEngine;
using Unity.Collections;
using Unity.Jobs;
using Unity.Mathematics;

public abstract class VoxelJob : MonoBehaviour
{
    public NativeArray<float> voxelData { get; set; }
    public float3 position { get; set; }
    public int resolution { get; set; }
    public float voxelSize { get; set; }
    public float noiseScale { get; set; }

    public abstract JobHandle GenerateVoxels(JobHandle dependsOn = default);
}