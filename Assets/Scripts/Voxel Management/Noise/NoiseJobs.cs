using Unity.Jobs;
using Unity.Mathematics;
using Unity.Burst;
using Unity.Collections;

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