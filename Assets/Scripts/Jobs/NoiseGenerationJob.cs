using System;
using Unity.Jobs;
using Unity.Mathematics;
using Unity.Burst;
using System.Collections.Generic;
using Unity.Collections;
using System.Collections;

public static class NoisePostProcess
{
    public static float HorizontalLandscape(float3 pos, float val)
    {
        return -pos.y + val;
    }
}

[BurstCompile]
public struct fBMNoiseJob : IJobParallelFor
{
    float3 position;
    float scale;

    int size;
    int octaves;
    float dimension;
    float lacunarity;

    public NativeArray<float> noiseValues;

    public void Execute(int idx)
    {
        int tmpIdx = idx;

        int x = tmpIdx % size;
        tmpIdx /= size;
        int y = tmpIdx % size;
        tmpIdx /= size;
        int z = tmpIdx % size;

        var intPos = new int3(x, y, z);
        float3 pos = position + (float3)intPos * scale;

        float output = 0;
        for (int i = 0; i < octaves; i++)
        {
            output += noise.cnoise(pos / math.max(1f, lacunarity * i)) * (1 - dimension * i);
        }

        noiseValues[idx] = NoisePostProcess.HorizontalLandscape(pos, output);
    }
}