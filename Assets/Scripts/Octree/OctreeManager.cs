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
        var finalNodes = octreeGenerator.generate(player.position - transform.position);
        var nodes = new Dictionary<ulong, OctreeNode>();

        recordNames.Begin();
        foreach (OctreeNode node in finalNodes)
        {
            try
            {
                nodes.Add(node.name, node);
            }
            catch (System.Exception)
            {
                Debug.Log("Duplicate node: " + node.name);
            }
        }
        recordNames.End();

        checkExisting.Begin();
        for (int i = 0; i < spawnedNodes.Count; i++)
        {
            GameObject spawnedNode = spawnedNodes[i];
            ulong name = ulong.Parse(spawnedNode.name);
            if (nodes.ContainsKey(name))
            {
                nodes.Remove(name);
            }
            else
            {
                Destroy(spawnedNode);
                spawnedNodes.RemoveAt(i);
                i--;
            }
        }
        checkExisting.End();

        createNew.Begin();
        foreach (OctreeNode node in nodes.Values)
        {
            GameObject spawnedNode = Instantiate(octreeNodePrefab, node.calculatePosition(octreeGenerator.startSize) + (float3)transform.position, Quaternion.identity, transform);
            spawnedNode.name = node.name.ToString();

            var boxOutline = spawnedNode.GetComponent<BoxOutline>();
            boxOutline.size = node.getSize(octreeGenerator.startSize);
            boxOutline.depth = node.depth;

            spawnedNodes.Add(spawnedNode);
        }
        createNew.End();
    }
}
