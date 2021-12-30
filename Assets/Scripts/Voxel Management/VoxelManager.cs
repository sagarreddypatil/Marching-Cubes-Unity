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
        int voxelSize = chunkManager.size + 3;
        voxelData = new NativeArray<float>(voxelSize * voxelSize * voxelSize, Allocator.Persistent, NativeArrayOptions.UninitializedMemory);
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

        int voxelSize = chunkManager.size + 3;
        var job = new FractalNoiseJob {
            position = transform.position,
            chunkScale = chunkManager.scale,
            scale = scale,
            noiseIntensity = noiseIntensity,
            size = voxelSize,
            octaves = octaves,
            dimension = dimension,
            lacunarity = lacunarity,
            noiseValues = voxelData
        };

        var handle = job.Schedule(voxelSize * voxelSize * voxelSize, 8);
        return handle;
    }
}
