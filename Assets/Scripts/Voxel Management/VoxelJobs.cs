using System;
using Unity.Jobs;
using Unity.Mathematics;
using Unity.Burst;
using System.Collections.Generic;
using Unity.Collections;
using System.Collections;

[BurstCompatible]
public static class Util
{
    public static half quantize(float value)
    {
        return (half)(value);
    }

    public static float dequantize(half value)
    {
        return value;
    }
}

[BurstCompatible]
public static class NoisePostProcess
{
    public static float HorizontalLandscape(float3 pos, float val)
    {
        return -pos.y + val;
    }

    public static float RidgedHorizontalLandscape(float3 pos, float val)
    {
        return HorizontalLandscape(pos, 1 - math.abs(val));
    }

    public static float Planet(float3 pos, float val, float radius)
    {
        return radius - math.length(pos) + val * 0.1f;
    }
}

[BurstCompile]
public struct FractalNoiseJob : IJobParallelFor
{
    public float3 position;
    public float meshScale;
    public float scale;

    public int size;
    public int octaves;
    public float dimension;
    public float lacunarity;
    public float noiseIntensity;

    [NativeDisableParallelForRestriction]
    [WriteOnly]
    public NativeArray<half> noiseValues;

    public void Execute(int idx)
    {
        int tmpIdx = idx;

        int x = tmpIdx % size;
        tmpIdx /= size;
        int y = tmpIdx % size;
        tmpIdx /= size;
        int z = tmpIdx % size;

        var intPos = new int3(x, y, z);
        float3 pos = (position + (float3)intPos * meshScale) * scale;

        float output = 0;
        for (int i = 0; i < octaves; i++)
        {
            output += noise.snoise(pos * math.exp2(math.max(0f, lacunarity) * i)) / math.exp2(math.max(0f, dimension) * i);
        }

        noiseValues[idx] = Util.quantize(NoisePostProcess.HorizontalLandscape(pos, output * noiseIntensity));
    }
}