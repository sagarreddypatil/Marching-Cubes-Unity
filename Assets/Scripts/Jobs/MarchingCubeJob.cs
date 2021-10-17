using System;
using Unity.Jobs;
using Unity.Mathematics;
using Unity.Burst;
using System.Collections.Generic;
using Unity.Collections;
using System.Collections;

public struct Triangle
{
    public bool created;
    public float3 c;
    public float3 b;
    public float3 a;
}

struct Cube : IEnumerable<float4>
{
    float4 _0;
    float4 _1;
    float4 _2;
    float4 _3;
    float4 _4;
    float4 _5;
    float4 _6;
    float4 _7;

    public float4 this[int index]
    {
        get
        {
            switch (index)
            {
            case 0:
                return _0;
            case 1:
                return _1;
            case 2:
                return _2;
            case 3:
                return _3;
            case 4:
                return _4;
            case 5:
                return _5;
            case 6:
                return _6;
            case 7:
                return _7;
            default:
                throw new ArgumentOutOfRangeException($"Cube only has 8 edges. You tried to access edge ${index}");
            }
        }
        set
        {
            switch (index)
            {
            case 0:
                _0 = value;
                break;

            case 1:
                _1 = value;
                break;

            case 2:
                _2 = value;
                break;

            case 3:
                _3 = value;
                break;

            case 4:
                _4 = value;
                break;

            case 5:
                _5 = value;
                break;

            case 6:
                _6 = value;
                break;

            case 7:
                _7 = value;
                break;
            }
        }
    }

    public IEnumerator<float4> GetEnumerator()
    {
        for (int i = 0; i < 8; i++)
        {
            yield return this[i];
        }
    }

    IEnumerator IEnumerable.GetEnumerator()
    {
        return GetEnumerator();
    }
}

[BurstCompile]
public struct MarchingCubesJob : IJobParallelFor
{
    public float surfaceLevel;
    public int size;
    public float scale;
    public int3 position;

    [NativeDisableParallelForRestriction]
    public NativeArray<Triangle> triangles;

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
            cube[i] = new float4(realCoords, noise(intCoords));
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

    float noise(int3 pos) // TODO: use noise from outside source, passed as parameter to this job
    {
        return -(float)pos.y;
    }
}