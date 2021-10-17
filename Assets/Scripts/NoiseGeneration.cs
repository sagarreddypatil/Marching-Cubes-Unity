using System;
using System.Collections;
using System.Collections.Generic;
using Unity.Collections;
using Unity.Jobs;
using Unity.Mathematics;
using UnityEngine;

public class NoiseGeneration : MonoBehaviour
{
    [Range(1, 16)]
    public int octaves = 8;
    [Range(0f, 4f)]
    public float dimension = 2f;
    [Range(0f, 4f)]
    public float lacunarity = 2f;
    [Range(0, 4f)]
    public float noiseScale = 0.25f;

    private int allocatedSize;

    [HideInInspector]
    public NativeArray<float> voxelData;
    private MarchingCubes marchingCubes;

    [HideInInspector]
    public JobHandle currentJobHandle;

    bool generate = true;

    void Start()
    {
        marchingCubes = GetComponent<MarchingCubes>();
        if (voxelData == null || !voxelData.IsCreated)
        {
            AllocateVoxelData();
        }
        GenerateVoxels();

        StartCoroutine("VoxelGenerationCoroutine");
    }

    void AllocateVoxelData()
    {
        int voxelSize = marchingCubes.size + 1;
        voxelData = new NativeArray<float>(voxelSize * voxelSize * voxelSize, Allocator.Persistent, NativeArrayOptions.UninitializedMemory);
        allocatedSize = marchingCubes.size;
    }

    void DisposeVoxelData()
    {
        if (voxelData != null)
        {
            voxelData.Dispose();
        }
        voxelData = default;
    }

    void OnDestroy()
    {
        DisposeVoxelData();
    }

    void OnValidate()
    {
        if (Application.isPlaying)
        {
            if (marchingCubes.size != allocatedSize)
            {
                DisposeVoxelData();
                AllocateVoxelData();
            }
            generate = true;
        }
    }

    IEnumerator VoxelGenerationCoroutine()
    {
        while (true)
        {
            if (generate)
            {
                try
                {
                    GenerateVoxels();
                    // generate = false;
                }
                catch (Exception e)
                {
                    Debug.LogError(e.Message);
                }
            }
            yield return new WaitForSecondsRealtime(1f / 30);
        }
    }
    void GenerateVoxels()
    {
        if (!voxelData.IsCreated)
        {
            AllocateVoxelData();
        }

        int voxelSize = marchingCubes.size + 1;
        var job = new FractalNoiseJob {
            position = transform.position,
            scale = marchingCubes.scale,
            noiseScale = noiseScale,
            size = voxelSize,
            octaves = octaves,
            dimension = dimension,
            lacunarity = lacunarity,
            noiseValues = voxelData
        };

        var handle = job.Schedule(voxelSize * voxelSize * voxelSize, 1);
        currentJobHandle = handle;
        handle.Complete();
    }

    void Update()
    {
    }
}
