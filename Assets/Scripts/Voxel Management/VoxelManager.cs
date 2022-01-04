using System;
using System.Collections;
using System.Collections.Generic;
using Unity.Collections;
using Unity.Jobs;
using Unity.Mathematics;
using UnityEngine;

public class VoxelManager : MonoBehaviour
{
    [Range(1, 16)]
    public int octaves = 8;
    public float dimension = 2f;
    public float lacunarity = 2f;
    public float scale = 0.25f;
    public float noiseIntensity = 1f;

    [HideInInspector]
    public NativeArray<float> voxelData;
    private ChunkManager chunkManager;

    void Awake()
    {
        chunkManager = GetComponent<ChunkManager>();
    }

    void OnEnable()
    {
        if (voxelData == null || !voxelData.IsCreated)
        {
            AllocateVoxelData();
        }
    }

    void OnDisable()
    {
        DisposeVoxelData();
    }

    void AllocateVoxelData()
    {
        int resolution = chunkManager.resolution + 3;
        voxelData = new NativeArray<float>(resolution * resolution * resolution, Allocator.Persistent, NativeArrayOptions.UninitializedMemory);
    }

    void DisposeVoxelData()
    {
        if (voxelData != null)
        {
            voxelData.Dispose();
        }
        voxelData = default;
    }

    public void ReallocateVoxelData()
    {
        DisposeVoxelData();
        AllocateVoxelData();
    }

    public JobHandle GenerateVoxels()
    {
        if (!voxelData.IsCreated)
        {
            AllocateVoxelData();
        }

        int resolution = chunkManager.resolution + 3;
        if (voxelData.Length != resolution * resolution * resolution)
        {
            DisposeVoxelData();
            AllocateVoxelData();
        }

        var job = new FractalNoiseJob {
            position = transform.position,
            voxelScale = chunkManager.scale,
            noiseScale = scale,
            noiseIntensity = noiseIntensity,
            resolution = resolution,
            octaves = octaves,
            dimension = dimension,
            lacunarity = lacunarity,
            noiseValues = voxelData
        };

        var handle = job.Schedule(resolution * resolution * resolution, 8);
        return handle;
    }
}
