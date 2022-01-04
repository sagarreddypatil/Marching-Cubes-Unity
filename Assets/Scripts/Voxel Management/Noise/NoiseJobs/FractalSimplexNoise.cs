using UnityEngine;
using Unity.Jobs;
using Unity.Collections;
using Unity.Burst;
using Unity.Mathematics;

public class FractalSimplexNoise : VoxelJob
{
    public float noiseScale = 0.15f;
    [Range(1, 16)]
    public int octaves = 8;
    public float dimension = 2f;
    public float lacunarity = 2f;
    public float noiseIntensity = 0.2f;

    public override JobHandle GenerateVoxels()
    {
        int resolution = this.resolution + 3;

        var job = new FractalSimplexNoiseJob {
            position = position,
            voxelScale = voxelScale,
            noiseScale = noiseScale,
            resolution = resolution,
            octaves = octaves,
            dimension = dimension,
            lacunarity = lacunarity,
            noiseIntensity = noiseIntensity,
            noiseValues = voxelData
        };

        return job.Schedule(voxelData.Length, resolution * resolution * resolution);
    }
}

[BurstCompile]
struct FractalSimplexNoiseJob : IJobParallelFor
{
    public float3 position;
    public float voxelScale;
    public float noiseScale;

    public int resolution;
    public int octaves;
    public float dimension;
    public float lacunarity;
    public float noiseIntensity;

    [NativeDisableParallelForRestriction]
    [WriteOnly]
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
        float3 pos = (position + (float3)intPos * voxelScale) * noiseScale;

        float output = 0;
        for (int i = 0; i < octaves; i++)
        {
            output += noise.snoise(pos * math.exp2(math.max(0f, lacunarity) * i)) / math.exp2(math.max(0f, dimension) * i);
        }

        noiseValues[idx] = NoisePostProcess.RidgedHorizontalLandscape(pos, output * noiseIntensity);
    }
}