using System.Collections.Generic;
using UnityEngine;
using Unity.Mathematics;
using System.Text;

public struct OctreeNode
{
    public byte[] idx;
    public int depth;
    public ulong name
    {
        get {
            if (idx.Length == 0)
                return 0;

            ulong id = (byte)(idx[0] + 1);
            for (int i = 1; i < idx.Length; i++)
            {
                id = id << 4 | (byte)(idx[i] + 1);
            }
            return id;
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
