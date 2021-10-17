using System;
using Unity.Jobs;
using Unity.Mathematics;
using Unity.Burst;
using System.Collections.Generic;
using Unity.Collections;
using System.Collections;

[BurstCompile]
public struct MarchingCubesJob : IJobParallelFor
{
    public float surfaceLevel;
    public int size;
    public float scale;
    public int3 position;

    [NativeDisableParallelForRestriction]
    [WriteOnly]
    public NativeArray<Triangle> triangles;

    [ReadOnly]
    public NativeArray<float> voxels;

    public void Execute(int idx)
    {
        int tmpIdx = idx;

        int x = tmpIdx % size;
        tmpIdx /= size;
        int y = tmpIdx % size;
        tmpIdx /= size;
        int z = tmpIdx % size;

        var cube = new Cube();
        int cubeIdx = 0;

        for (int i = 0; i < 8; i++)
        {
            int3 intCoords = new int3(x, y, z) + LUT.cornerCoords[i];
            float3 realCoords = (float3)(intCoords)*scale;
            cube[i] = new float4(realCoords, getNoise(intCoords));
            if (cube[i].w < surfaceLevel)
            {
                cubeIdx |= 1 << i;
            }
        }

        int counter = 0;

        for (int i = 0; triTableValue(cubeIdx, i) != -1; i += 3)
        {
            int a0 = LUT.cornerIndexAFromEdge[triTableValue(cubeIdx, i)];
            int b0 = LUT.cornerIndexBFromEdge[triTableValue(cubeIdx, i)];

            int a1 = LUT.cornerIndexAFromEdge[triTableValue(cubeIdx, i + 1)];
            int b1 = LUT.cornerIndexBFromEdge[triTableValue(cubeIdx, i + 1)];

            int a2 = LUT.cornerIndexAFromEdge[triTableValue(cubeIdx, i + 2)];
            int b2 = LUT.cornerIndexBFromEdge[triTableValue(cubeIdx, i + 2)];

            var newTri = new Triangle {
                created = true,
                c = interpolateVerts(cube[a0], cube[b0]),
                b = interpolateVerts(cube[a1], cube[b1]),
                a = interpolateVerts(cube[a2], cube[b2]),
            };
            triangles[idx * 5 + counter] = newTri;
            counter++;
        }
        for (int i = counter; i < 5; i++)
        {
            triangles[idx * 5 + i] = new Triangle {
                created = false,
                c = float3.zero,
                b = float3.zero,
                a = float3.zero
            };
        }
    }

    float3 interpolateVerts(float4 a, float4 b)
    {
        float t = (surfaceLevel - a.w) / (b.w - a.w);
        return a.xyz + t * (b.xyz - a.xyz);
    }

    int triTableValue(int a, int b)
    {
        return LUT.triTable[a * 16 + b];
    }

    float getNoise(int3 pos) // TODO: use noise from outside source, passed as parameter to this job
    {
        int voxelSize = size + 1;
        return voxels[pos.x + pos.y * voxelSize + pos.z * voxelSize * voxelSize];

        // return -(float)pos.y + noise.snoise((float3)pos);
    }
}