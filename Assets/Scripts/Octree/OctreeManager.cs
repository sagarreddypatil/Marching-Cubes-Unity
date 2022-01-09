using System.Collections.Generic;
using UnityEngine;
using Unity.Mathematics;

struct OctreeNode
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

public class OctreeManager : MonoBehaviour
{
    private Transform player;
    private List<GameObject> spawnedNodes;

    public GameObject octreeNodePrefab;
    public float startSize = 100f;
    public float threshold = 0.4f;
    public float minSize = 1f;

    void Start()
    {
        player = GameObject.FindGameObjectWithTag("Player").transform;
        spawnedNodes = new List<GameObject>();
    }

    void Update()
    {
        var finalNodes = new List<OctreeNode>();

        var currentNodes = new List<OctreeNode>();
        var nextNodes = new List<OctreeNode>();

        currentNodes.Add(OctreeNode.getRootNode());

        while (currentNodes.Count > 0)
        {
            foreach (OctreeNode node in currentNodes)
            {
                float nodeSize = node.getSize(startSize);
                float3 nodeCenter = node.calculateCenter(startSize) + (float3)transform.position;

                if (math.distancesq(nodeCenter, player.position) < math.pow(nodeSize * threshold, 2) && nodeSize / 2 > minSize)
                {
                    for (byte i = 0; i < 8; i++)
                    {
                        nextNodes.Add(node.getNextNode(i));
                    }
                }
                else
                {
                    finalNodes.Add(node);
                }
            }
            currentNodes = nextNodes;
            nextNodes = new List<OctreeNode>();
        }

        var finalNodeNames = new List<string>();
        foreach (OctreeNode node in finalNodes)
        {
            finalNodeNames.Add(node.name);
        }

        for (int i = 0; i < spawnedNodes.Count; i++)
        {
            GameObject spawnedNode = spawnedNodes[i];

            int idx = finalNodeNames.IndexOf(spawnedNode.name);

            if (idx == -1)
            {
                // Destroy(spawnedNode);
                spawnedNode.SetActive(false);
                spawnedNodes.RemoveAt(i);
            }
            else
            {
                spawnedNode.SetActive(true);
                finalNodeNames.RemoveAt(idx);
                finalNodes.RemoveAt(idx);
            }
        }

        foreach (OctreeNode node in finalNodes)
        {
            GameObject spawnedNode = Instantiate(octreeNodePrefab, node.calculatePosition(startSize) + (float3)transform.position, Quaternion.identity, transform);
            spawnedNode.name = node.name;

            BoxOutline boxOutline = spawnedNode.GetComponent<BoxOutline>();
            boxOutline.size = node.getSize(startSize);

            spawnedNodes.Add(spawnedNode);
        }
    }
}
