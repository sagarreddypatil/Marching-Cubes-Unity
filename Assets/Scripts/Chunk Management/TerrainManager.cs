using UnityEngine;
using Unity.Mathematics;
using System.Collections.Generic;

struct Chunk
{
    public GameObject gameObject;
    public MeshManager meshManager;
    public VoxelManager voxelManager;
    public ChunkManager chunkManager;
}

[RequireComponent(typeof(OctreeGenerator))]
public class TerrainManager : MonoBehaviour
{
    [Header("Terrain Options")]
    public int gridSize = 3;
    public int chunksPerFrame = 1;
    public GameObject chunkPrefab;

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

    float idxToFloat(int idx)
    {
        return (float)idx * size;
    }

    int idxId(int x, int y, int z)
    {
        return x + y * gridSize + z * gridSize * gridSize;
    }

    void Awake()
    {
        octreeGenerator = GetComponent<OctreeGenerator>();
        player = GameObject.FindGameObjectWithTag("Player").transform;
    }

    void Start()
    {
        chunks = new List<Chunk>();

        int counter = 0;

        for (int x = 0; x < gridSize; x++)
        {
            for (int y = 0; y < gridSize; y++)
            {
                for (int z = 0; z < gridSize; z++)
                {
                    Vector3 location = new Vector3(idxToFloat(x), idxToFloat(y), idxToFloat(z));
                    GameObject newChunk = Instantiate(chunkPrefab, location, Quaternion.identity, transform);
                    newChunk.name = $"Chunk ({x}, {y}, {z})";

                    var chunk = new Chunk {
                        gameObject = newChunk,
                        meshManager = newChunk.GetComponent<MeshManager>(),
                        voxelManager = newChunk.GetComponent<VoxelManager>(),
                        chunkManager = newChunk.GetComponent<ChunkManager>()
                    };

                    SetChunkProperties(chunk);

                    chunk.chunkManager.rebuildOnUpdateCount = gridSize * gridSize * gridSize / chunksPerFrame;
                    chunk.chunkManager.rebuildOnUpdate = counter / chunksPerFrame;
                    chunk.gameObject.SetActive(true);

                    chunks[idxId(x, y, z)] = chunk;

                    counter++;
                }
            }
        }
    }

    void GenerateOctreeChunks()
    {
        var nodes = octreeGenerator.generate(player.position - transform.position);
    }

    void SetChunkProperties(Chunk chunk)
    {
        chunk.chunkManager.continousUpdate = continousUpdate;
        chunk.chunkManager.resolution = resolution;
        chunk.chunkManager.size = size;

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

    void Update()
    {
    }
}
