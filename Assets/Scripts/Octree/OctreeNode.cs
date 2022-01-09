using System.Collections.Generic;
using UnityEngine;
using Unity.Mathematics;

public struct OctreeNode
{
    public byte[] idx;
    public int depth;
    public string name
    {
        get {
            string output = "0-";
            for (int i = 0; i < depth; i++)
            {
                output += idx[i] + "-";
            }
            return output.Substring(0, output.Length - 1);
        }
    }

    public static float getSizeAtDepth(float startSize, int depth)
    {
        return startSize / (1 << depth);
    }

    public float getSize(float startSize)
    {
        return getSizeAtDepth(startSize, depth);
    }

    public float3 calculatePosition(float startSize) // calculates the position the bottom left corner
    {
        float3 center = new float3(0, 0, 0);
        for (int i = 0; i < depth; i++)
        {
            center += new float3(idx[i] % 2, idx[i] / 2 % 2, idx[i] / 4 % 2) * getSizeAtDepth(startSize, i) / 2;
        }
        return center;
    }

    public float3 calculateCenter(float startSize)
    {
        return calculatePosition(startSize) + getSize(startSize) / 2;
    }

    public OctreeNode getNextNode(byte nextIdx)
    {
        byte[] newIdx = new byte[depth + 1];
        for (int i = 0; i < depth; i++)
        {
            newIdx[i] = idx[i];
        }
        newIdx[depth] = nextIdx;
        return new OctreeNode() { idx = newIdx, depth = depth + 1 };
    }

    public static OctreeNode getRootNode()
    {
        return new OctreeNode() { idx = new byte[0], depth = 0 };
    }
}
