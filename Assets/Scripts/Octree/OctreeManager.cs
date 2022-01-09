using System.Collections.Generic;
using UnityEngine;
using Unity.Mathematics;
using Unity.Profiling;

public class OctreeManager : MonoBehaviour
{
    private Transform player;
    private List<GameObject> spawnedNodes;
    private OctreeGenerator octreeGenerator;

    public GameObject octreeNodePrefab;
    public float startSize = 100f;
    public float threshold = 0.4f;
    public float minSize = 1f;

    public ProfilerMarker recordNames = new ProfilerMarker("RecordNames");
    public ProfilerMarker checkExisting = new ProfilerMarker("CheckExisting");
    public ProfilerMarker createNew = new ProfilerMarker("CreateNew");

    void Start()
    {
        player = GameObject.FindGameObjectWithTag("Player").transform;
        spawnedNodes = new List<GameObject>();
        octreeGenerator = new OctreeGenerator(startSize, threshold, minSize);
    }

    void OnValidate()
    {
        if (octreeGenerator != null)
        {
            octreeGenerator.startSize = startSize;
            octreeGenerator.threshold = threshold;
            octreeGenerator.minSize = minSize;
        }
    }

    void Update()
    {
        List<OctreeNode> finalNodes = octreeGenerator.generate(player.position - transform.position);

        recordNames.Begin();
        var finalNodeNames = new List<string>();
        foreach (OctreeNode node in finalNodes)
        {
            finalNodeNames.Add(node.name);
        }
        recordNames.End();

        checkExisting.Begin();
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
        checkExisting.End();

        createNew.Begin();
        foreach (OctreeNode node in finalNodes)
        {
            GameObject spawnedNode = Instantiate(octreeNodePrefab, node.calculateCenter(startSize) + (float3)transform.position, Quaternion.identity, transform);
            spawnedNode.name = node.name;
            spawnedNode.transform.localScale = new float3(1, 1, 1) * node.getSize(startSize);
            spawnedNodes.Add(spawnedNode);
        }
        createNew.End();
    }
}
