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
    public bool continousUpdate = false;
    public GameObject chunkPrefab;

    [Header("Mesh Options")]
    public int size = 16;
    public float scale = 1f / 16f;
    public float surfaceLevel = 0f;
    public bool smoothShading = true;

    [Header("Noise Options")]
    [Range(1, 16)]
    public int octaves = 8;
    public float dimension = 3f;
    public float lacunarity = 1.5f;
    public float noiseScale = 1f;
    public float noiseIntensity = 1f;

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
                    chunk.chunkManager.rebuildOnUpdateCount = gridSize * gridSize * gridSize / chunksPerFrame;
                    chunk.chunkManager.rebuildOnUpdate = counter / chunksPerFrame;
                    chunk.gameObject.SetActive(true);

                    chunks[idxId(x, y, z)] = chunk;
                }
            }
        }
    }

    void SetChunkProperties(Chunk chunk)
    {
        chunk.chunkManager.continousUpdate = continousUpdate;

        chunk.meshManager.size = size;
        chunk.meshManager.scale = scale;
        chunk.meshManager.surfaceLevel = surfaceLevel * noiseIntensity;
        chunk.meshManager.smoothShading = smoothShading;

        chunk.voxelManager.octaves = octaves;
        chunk.voxelManager.dimension = dimension;
        chunk.voxelManager.lacunarity = lacunarity;
        chunk.voxelManager.scale = noiseScale;
        chunk.voxelManager.noiseIntensity = noiseIntensity;
    }

    void OnValidate()
    {
        if (chunks != null)
        {
            for (int i = 0; i < chunks.Length; i++)
            {
                Chunk chunk = chunks[i];
                if (chunk.gameObject != null)
                {
                    SetChunkProperties(chunk);
                    chunk.chunkManager.rebuildOnUpdateCount = chunks.Length / chunksPerFrame;
                    chunk.chunkManager.rebuildOnUpdate = i / chunksPerFrame;
                }
            }
        }
    }

    void Update()
    {
    }
}
