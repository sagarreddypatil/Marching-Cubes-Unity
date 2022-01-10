using UnityEngine;
using Unity.Mathematics;
using System.Collections.Generic;

public class OctreeGenerator : MonoBehaviour
{
    public float startSize = 100f;
    public float threshold = 0.4f;
    public float minSize = 1f;

    public List<OctreeNode> generate(float3 relativeTargetPosition)
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
                float3 nodeCenter = node.getCenter(startSize);

                if (math.exp(math.distance(nodeCenter, relativeTargetPosition) / nodeSize) < threshold && nodeSize / 2 > minSize)
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

        return finalNodes;
    }
}
