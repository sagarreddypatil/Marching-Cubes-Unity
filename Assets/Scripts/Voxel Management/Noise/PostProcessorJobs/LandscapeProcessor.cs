using UnityEngine;
using Unity.Jobs;
using Unity.Collections;
using Unity.Burst;
using Unity.Mathematics;

public class LandscapeProcessor : VoxelJob
{
    public override JobHandle GenerateVoxels(JobHandle dependsOn = default)
    {
        var job = new LandscapeProcessorJob {
            position = position,
            voxelSize = voxelSize,
            noiseScale = noiseScale,
            resolution = resolution,
            noiseValues = voxelData,
        };

        return job.Schedule(resolution * resolution * resolution, 8, dependsOn);
    }
}

[BurstCompile]
struct LandscapeProcessorJob : IJobParallelFor
{
    public float3 position;
    public float voxelSize;
    public float noiseScale;
    public int resolution;

    [NativeDisableParallelForRestriction]
    public NativeArray<float> noiseValues;

    public void Execute(int idx)
    {
        int tmpIdx = idx;

        int x = tmpIdx % resolution;
        tmpIdx /= resolution;
        int y = tmpIdx % resolution;
        tmpIdx /= resolution;
        int z = tmpIdx % resolution;

        var intPos = new int3(x - 1, y - 1, z - 1);
        float3 pos = (position + (float3)intPos * voxelSize) * noiseScale;

        noiseValues[idx] = RidgedHorizontalLandscape(pos, noiseValues[idx]);
    }

    private float HorizontalLandscape(float3 pos, float val)
    {
        return -pos.y + val;
    }

    private float RidgedHorizontalLandscape(float3 pos, float val)
    {
        return HorizontalLandscape(pos, math.abs(val));
    }
}