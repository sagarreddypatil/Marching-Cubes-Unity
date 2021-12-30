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

    [NativeDisableParallelForRestriction]
    [WriteOnly]
    public NativeArray<Triangle> triangles;

    [NativeDisableParallelForRestriction]
    [WriteOnly]
    public NativeArray<Triangle> normals;

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

        var cubeCornerValues = new Cube();
        var cubeCornerNorms = new Cube();
        int cubeIdx = 0;

        for (int i = 0; i < 8; i++)
        {
            int3 intCoords = new int3(x, y, z) + LUT.cornerCoords[i];
            float3 realCoords = (float3)(intCoords)*scale;

            cubeCornerValues[i] = new float4(realCoords, getVoxelValue(intCoords));
            cubeCornerNorms[i] = new float4(calculateNorm(intCoords), getVoxelValue(intCoords));

            if (cubeCornerValues[i].w < surfaceLevel)
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
                c = interpolateVerts(cubeCornerValues[a0], cubeCornerValues[b0]),
                b = interpolateVerts(cubeCornerValues[a1], cubeCornerValues[b1]),
                a = interpolateVerts(cubeCornerValues[a2], cubeCornerValues[b2]),
            };

            var newNorms = new Triangle {
                created = true,
                c = interpolateVerts(cubeCornerNorms[a0], cubeCornerNorms[b0]),
                b = interpolateVerts(cubeCornerNorms[a1], cubeCornerNorms[b1]),
                a = interpolateVerts(cubeCornerNorms[a2], cubeCornerNorms[b2]),
            };

            triangles[idx * 5 + counter] = newTri;
            normals[idx * 5 + counter] = newNorms;

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
        // return (a.xyz + b.xyz) / 2f;
    }

    float3 calculateNorm(int3 pos)
    {
        float3 norm = new float3(getVoxelValue(pos + new int3(-1, 0, 0)) - getVoxelValue(pos + new int3(1, 0, 0)),
                                 getVoxelValue(pos + new int3(0, -1, 0)) - getVoxelValue(pos + new int3(0, 1, 0)),
                                 getVoxelValue(pos + new int3(0, 0, -1)) - getVoxelValue(pos + new int3(0, 0, 1)));

        return math.normalize(norm);
    }

    int triTableValue(int a, int b)
    {
        return LUT.triTable[a * 16 + b];
    }

    float getVoxelValue(int3 pos)
    {
        int voxelSize = size + 3;
        return voxels[(pos.x + 1) + (pos.y + 1) * voxelSize + (pos.z + 1) * voxelSize * voxelSize];
    }
}