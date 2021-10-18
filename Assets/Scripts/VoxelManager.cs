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
    [Range(0f, 4f)]
    public float dimension = 2f;
    [Range(0f, 4f)]
    public float lacunarity = 2f;
    [Range(0, 4f)]
    public float scale = 0.25f;

    [HideInInspector]
    public NativeArray<float> voxelData;
    private MeshManager meshManager;

    void Awake()
    {
        meshManager = GetComponent<MeshManager>();
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
        int voxelSize = meshManager.size + 1;
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

        int voxelSize = meshManager.size + 1;
        var job = new FractalNoiseJob {
            position = transform.localPosition,
            meshScale = meshManager.scale,
            scale = scale,
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
