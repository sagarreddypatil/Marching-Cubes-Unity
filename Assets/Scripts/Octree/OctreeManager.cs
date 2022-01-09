using System.Collections.Generic;
using UnityEngine;
using Unity.Mathematics;
using Unity.Profiling;

[RequireComponent(typeof(OctreeGenerator))]
public class OctreeManager : MonoBehaviour
{
    private Transform player;
    private List<GameObject> spawnedNodes;
    private OctreeGenerator octreeGenerator;

    public GameObject octreeNodePrefab;

    public ProfilerMarker recordNames = new ProfilerMarker("RecordNames");
    public ProfilerMarker checkExisting = new ProfilerMarker("CheckExisting");
    public ProfilerMarker createNew = new ProfilerMarker("CreateNew");

    void Awake()
    {
        player = GameObject.FindGameObjectWithTag("Player").transform;
        octreeGenerator = GetComponent<OctreeGenerator>();
    }

    void Start()
    {
        spawnedNodes = new List<GameObject>();
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
            GameObject spawnedNode = Instantiate(octreeNodePrefab, node.calculatePosition(octreeGenerator.startSize) + (float3)transform.position, Quaternion.identity, transform);
            spawnedNode.name = node.name;
            spawnedNode.GetComponent<BoxOutline>().size = node.getSize(octreeGenerator.startSize);
            spawnedNodes.Add(spawnedNode);
        }
        createNew.End();
    }
}
