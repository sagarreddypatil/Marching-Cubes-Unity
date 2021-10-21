using System.Collections;
using System.Collections.Generic;
using UnityEngine;

struct Chunk
{
    public GameObject gameObject;
    public MeshManager meshManager;
    public VoxelManager voxelManager;
    public ChunkManager chunkManager;
}

public class TerrainManager : MonoBehaviour
{
    [Header("Terrain Options")]
    public int gridSize = 3;
    public int chunksPerFrame = 1;
    public GameObject chunkPrefab;

    [Header("Mesh Options")]
    public int size = 16;
    public float scale = 1f / 16f;
    public float surfaceLevel = 0f;

    [Header("Noise Options")]
    [Range(1, 16)]
    public int octaves = 8;
    public float dimension = 3f;
    public float lacunarity = 1.5f;
    public float noiseScale = 1f;
    public float noiseFactor = 1f;

    private Chunk[] chunks;

    float idxToFloat(int idx)
    {
        return (float)idx * scale * size - 0.5f * gridSize;
    }

    int idxId(int x, int y, int z)
    {
        return x + y * gridSize + z * gridSize * gridSize;
    }

    void Start()
    {
        int counter = 0;
        chunks = new Chunk[gridSize * gridSize * gridSize];
        for (int x = 0; x < gridSize; x++)
        {
            for (int y = 0; y < gridSize; y++)
            {
                for (int z = 0; z < gridSize; z++)
                {
                    Vector3 location = new Vector3(idxToFloat(x), idxToFloat(y), idxToFloat(z));
                    GameObject newChunk = Instantiate(chunkPrefab, location, Quaternion.identity, transform);
                    var chunk = new Chunk {
                        gameObject = newChunk,
                        meshManager = newChunk.GetComponent<MeshManager>(),
                        voxelManager = newChunk.GetComponent<VoxelManager>(),
                        chunkManager = newChunk.GetComponent<ChunkManager>()
                    };

                    SetChunkProperties(chunk);

                    counter++;
                    chunk.chunkManager.rebuildOnUpdate = counter / chunksPerFrame;
                    chunk.gameObject.SetActive(true);

                    chunks[idxId(x, y, z)] = chunk;
                }
            }
        }
    }

    void SetChunkProperties(Chunk chunk)
    {
        chunk.meshManager.size = size;
        chunk.meshManager.scale = scale;
        chunk.meshManager.surfaceLevel = surfaceLevel * noiseFactor;

        chunk.voxelManager.octaves = octaves;
        chunk.voxelManager.dimension = dimension;
        chunk.voxelManager.lacunarity = lacunarity;
        chunk.voxelManager.scale = noiseScale;
        chunk.voxelManager.noiseFactor = noiseFactor;
    }

    void OnValidate()
    {
        if (chunks != null)
        {
            foreach (Chunk chunk in chunks)
            {
                if (chunk.gameObject != null)
                {
                    SetChunkProperties(chunk);
                }
            }
        }
    }

    void Update()
    {
    }
}
