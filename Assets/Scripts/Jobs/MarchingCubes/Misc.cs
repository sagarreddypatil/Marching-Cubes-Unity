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