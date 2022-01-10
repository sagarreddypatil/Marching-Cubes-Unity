using UnityEngine;
using Unity.Mathematics;
using System.Collections.Generic;

struct Chunk
{
    public OctreeNode octreeNode;
    public GameObject gameObject;
    public MeshManager meshManager;
    public VoxelManager voxelManager;
    public ChunkManager chunkManager;
}

[RequireComponent(typeof(OctreeGenerator))]
public class TerrainManager : MonoBehaviour
{
    [Header("Terrain Options")]
    public int chunksPerFrame = 1;
    public GameObject chunkPrefab;
    public int numChunks;

    [Header("Chunk Options")]
    public int resolution = 16;
    public float size = 1f / 16f;
    public bool continousUpdate = false;

    [Header("Mesh Options")]
    public float surfaceLevel = 0f;
    public bool smoothShading = true;

    [Header("Noise Options")]
    public float noiseScale = 1f;
    public VoxelJob[] voxelGenerationPipeline;

    private List<Chunk> chunks;
    private OctreeGenerator octreeGenerator;
    private Transform player;

    void Awake()
    {
        octreeGenerator = GetComponent<OctreeGenerator>();
        player = GameObject.FindGameObjectWithTag("Player").transform;
        chunks = new List<Chunk>();
    }

    void Update()
    {
        var finalNodes = octreeGenerator.generate(player.position - transform.position);
        var nodes = new Dictionary<ulong, OctreeNode>();

        foreach (OctreeNode node in finalNodes)
        {
            nodes.Add(node.id, node);
        }

        for (int i = 0; i < chunks.Count; i++)
        {
            var chunk = chunks[i];
            var id = chunk.octreeNode.id;

            if (nodes.ContainsKey(id))
            {
                nodes.Remove(id);
            }
            else
            {
                Destroy(chunk.gameObject);
                chunks.RemoveAt(i);
                i--;
            }
        }

        int counter = 0;
        foreach (var node in nodes.Values)
        {
            Vector3 location = node.getPosition(octreeGenerator.startSize) + (float3)transform.position;
            GameObject newChunk = Instantiate(chunkPrefab, location, Quaternion.identity, transform);

            var chunk = new Chunk {
                octreeNode = node,
                gameObject = newChunk,
                meshManager = newChunk.GetComponent<MeshManager>(),
                voxelManager = newChunk.GetComponent<VoxelManager>(),
                chunkManager = newChunk.GetComponent<ChunkManager>()
            };

            SetChunkProperties(chunk);

            chunk.chunkManager.rebuildOnUpdateCount = nodes.Count;
            chunk.chunkManager.rebuildOnUpdate = counter / chunksPerFrame;
            chunk.gameObject.SetActive(true);

            chunks.Add(chunk);
            counter++;
        }

        numChunks = chunks.Count;
    }

    void GenerateOctreeChunks()
    {
        var nodes = octreeGenerator.generate(player.position - transform.position);
    }

    void SetChunkProperties(Chunk chunk)
    {
        chunk.gameObject.name = chunk.octreeNode.id.ToString();

        chunk.chunkManager.continousUpdate = continousUpdate;
        chunk.chunkManager.resolution = resolution;
        chunk.chunkManager.size = chunk.octreeNode.getSize(octreeGenerator.startSize);

        chunk.meshManager.surfaceLevel = surfaceLevel;
        chunk.meshManager.smoothShading = smoothShading;

        chunk.voxelManager.noiseScale = noiseScale;
        chunk.voxelManager.voxelGenerationPipeline = voxelGenerationPipeline;
    }

    void OnValidate()
    {
        if (chunks != null)
        {
            for (int i = 0; i < chunks.Count; i++)
            {
                Chunk chunk = chunks[i];
                if (chunk.gameObject != null)
                {
                    SetChunkProperties(chunk);
                    chunk.chunkManager.rebuildOnUpdateCount = chunks.Count / chunksPerFrame;
                    chunk.chunkManager.rebuildOnUpdate = i / chunksPerFrame;
                }
            }
        }
    }
}
