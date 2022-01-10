using System.Collections.Generic;
using UnityEngine;
using Unity.Mathematics;
using System.Text;

public struct OctreeNode
{
    public byte[] pos; // position in terms of the cube index at each depth level
    public int depth;  // depth of the node
    public ulong id
    {
        get {
            if (pos.Length == 0)
                return 0;

            ulong id = (byte)(pos[0] + 1);
            for (int i = 1; i < pos.Length; i++)
            {
                id = id << 4 | (byte)(pos[i] + 1);
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

    public float3 getPosition(float startSize) // calculates the position the bottom left corner
    {
        float3 center = new float3(0, 0, 0);
        for (int i = 0; i < depth; i++)
        {
            center += new float3(pos[i] % 2, pos[i] / 2 % 2, pos[i] / 4 % 2) * getSizeAtDepth(startSize, i) / 2;
        }
        return center;
    }

    public float3 getCenter(float startSize)
    {
        return getPosition(startSize) + getSize(startSize) / 2;
    }

    public OctreeNode getNextNode(byte nextIdx)
    {
        byte[] newIdx = new byte[depth + 1];
        for (int i = 0; i < depth; i++)
        {
            newIdx[i] = pos[i];
        }
        newIdx[depth] = nextIdx;
        return new OctreeNode() { pos = newIdx, depth = depth + 1 };
    }

    public static OctreeNode getRootNode()
    {
        return new OctreeNode() { pos = new byte[0], depth = 0 };
    }
}
