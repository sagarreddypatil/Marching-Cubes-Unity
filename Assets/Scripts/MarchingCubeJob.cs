using UnityEngine;
using Unity.Jobs;
using Unity.Mathematics;
using Unity.Burst;
using Unity.Collections;

public struct Triangle
{
    public float3 c;
    public float3 b;
    public float3 a;
}

[BurstCompile]
public struct MarchingCubesJob : IJobParallelFor
{
    public float surfaceLevel;
    public int size;
    public int metersPerCube;
    public float3 position;
    public float scale;

    public NativeList<Triangle> triangles;

    public void Execute(int idx)
    {
        int x = idx % size;
        idx /= size;
        int y = idx % size;
        idx /= size;
        int z = idx;

        int3[] coords = {
            new int3(x, y, z),
            new int3(x + 1, y, z),
            new int3(x + 1, y, z + 1),
            new int3(x, y, z + 1),
            new int3(x, y + 1, z),
            new int3(x + 1, y + 1, z),
            new int3(x + 1, y + 1, z + 1),
            new int3(x, y + 1, z + 1),
        };

        float4[] cube = new float4[8];
        int cubeIdx = 0;

        for (int i = 0; i < 8; i++)
        {
            float3 realCoords = (float3)coords[i] * metersPerCube;
            cube[i] = new float4(realCoords, noise(realCoords));
            if (cube[i].w < surfaceLevel)
            {
                cubeIdx |= 1 << i;
            }
        }

        for (int i = 0; triTableValue(cubeIdx, i) != -1; i += 3)
        {
            int a0 = LUT.cornerIndexAFromEdge[triTableValue(cubeIdx, i)];
            int b0 = LUT.cornerIndexBFromEdge[triTableValue(cubeIdx, i)];

            int a1 = LUT.cornerIndexAFromEdge[triTableValue(cubeIdx, i + 1)];
            int b1 = LUT.cornerIndexBFromEdge[triTableValue(cubeIdx, i + 1)];

            int a2 = LUT.cornerIndexAFromEdge[triTableValue(cubeIdx, i + 2)];
            int b2 = LUT.cornerIndexBFromEdge[triTableValue(cubeIdx, i + 2)];

            var newTri = new Triangle {
                a = interpolateVerts(cube[a0], cube[b0]),
                b = interpolateVerts(cube[a1], cube[b1]),
                c = interpolateVerts(cube[a2], cube[b2]),
            };

            triangles.Add(newTri);
        }
    }

    float3 interpolateVerts(float4 a, float4 b)
    {
        float t = (surfaceLevel - a.w) / (b.w - a.w);
        return a.xyz + t * (b.xyz - a.xyz);
    }

    int triTableValue(int a, int b)
    {
        return LUT.triTable[16 * a + b];
    }

    float noise(float3 pos) // TODO: use noise from outside source, passed as parameter to this job
    {
        return -pos.y;
    }
}