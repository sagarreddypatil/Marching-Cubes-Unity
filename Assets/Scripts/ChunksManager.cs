using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ChunksManager : MonoBehaviour
{
    [Header("Chunk Options")]
    public int size = 16;
    public float scale = 1f / 16f;
    public float surfaceLevel = -1f;
    public int gridSizeXZ = 3;
    public int gridSizeY = 3;
    public GameObject chunkPrefab;

    [Header("Terrain Options")]
    public int octaves = 4;
    public float dimension = 4f;
    public float lacunarity = 2f;
    public float noiseScale = 0.5f;

    private List<GameObject> chunks;
    private List<MarchingCubes> marchingCubes;
    private List<NoiseGeneration> noiseGenerators;

    void Awake()
    {
        chunks = new List<GameObject>();
        marchingCubes = new List<MarchingCubes>();
        noiseGenerators = new List<NoiseGeneration>();

        for (int x = 0; x < gridSizeXZ; x++)
        {
            for (int y = 0; y < gridSizeY; y++)
            {
                for (int z = 0; z < gridSizeXZ; z++)
                {
                    Vector3 pos = new Vector3(x - (float)gridSizeXZ / 2f, y - (float)gridSizeY / 2f, z - (float)gridSizeXZ / 2f);

                    GameObject newChunk = Instantiate(chunkPrefab, pos * size * scale, Quaternion.identity, transform);
                    chunks.Add(newChunk);
                    marchingCubes.Add(newChunk.GetComponent<MarchingCubes>());
                    noiseGenerators.Add(newChunk.GetComponent<NoiseGeneration>());
                }
            }
        }

        ApplySettings(true);
    }

    void ApplySettings(bool first)
    {
        for (int i = 0; i < chunks.Count; i++)
        {
            marchingCubes[i].size = size;
            marchingCubes[i].scale = scale;
            marchingCubes[i].surfaceLevel = surfaceLevel;

            noiseGenerators[i].octaves = octaves;
            noiseGenerators[i].dimension = dimension;
            noiseGenerators[i].lacunarity = lacunarity;
            noiseGenerators[i].scale = noiseScale;

            if (first)
            {
                StartCoroutine("RegenerateAll", i);
            }
            else
            {
                noiseGenerators[i].OnValidate();
                marchingCubes[i].OnValidate();
            }
        }
    }

    void RegenerateNoise(int i)
    {
        noiseGenerators[i].GenerateVoxels();
    }

    void RegenerateMesh(int i)
    {
        marchingCubes[i].GenerateMesh();
    }

    void RegenerateAll(int i)
    {
        RegenerateNoise(i);
        RegenerateMesh(i);
    }

    void OnValidate()
    {
        if (Application.isPlaying)
        {
            ApplySettings(false);
        }
    }
}
