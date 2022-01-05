using System.Collections.Generic;
using UnityEngine;
using Unity.Mathematics;

public class OctreeManager : MonoBehaviour
{
    public GameObject octreeNodePrefab;
    private Transform player;
    public float threshold = 0.4f;
    public float minSize = 1f;

    private List<GameObject> allNodes = new List<GameObject>();

    void Start()
    {
        player = GameObject.FindGameObjectWithTag("Player").transform;
    }
    void Update()
    {
        if (Time.frameCount % 2 == 0)
            return;

        foreach (GameObject node in allNodes)
        {
            Destroy(node);
        }
        allNodes.Clear();

        GameObject rootNode = Instantiate(octreeNodePrefab, new Vector3(-50f, -50f, -50f), Quaternion.identity, transform);
        rootNode.name = "Root";
        rootNode.GetComponent<OctreeNode>().size = 100f;

        List<GameObject> nodes = new List<GameObject>();
        List<GameObject> nextNodes = new List<GameObject>();

        nodes.Add(rootNode);
        allNodes.Add(rootNode);

        while (nodes.Count != 0)
        {
            foreach (GameObject node in nodes)
            {
                OctreeNode currNode = node.GetComponent<OctreeNode>();
                Vector3 nodeCenter = node.transform.position + new Vector3(1, 1, 1) * currNode.size / 2;

                if (Vector3.Distance(nodeCenter, player.position) < currNode.size * threshold)
                {
                    if (currNode.size >= 2 * minSize)
                    {
                        for (int i = 0; i < 8; i++)
                        {
                            Vector3 childPos = new Vector3(i % 2, i / 2 % 2, i / 4 % 2) * currNode.size / 2;
                            GameObject newNode = Instantiate(octreeNodePrefab, node.transform.position + childPos, Quaternion.identity, node.transform);
                            newNode.name = "Node " + i;
                            newNode.GetComponent<OctreeNode>().size = currNode.size / 2;
                            nextNodes.Add(newNode);
                            allNodes.Add(newNode);
                        }
                    }
                }
            }

            nodes = nextNodes;
            nextNodes = new List<GameObject>();
        }
    }
}
