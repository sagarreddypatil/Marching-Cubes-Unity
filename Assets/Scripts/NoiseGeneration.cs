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
    public float scale = 0.25f;

    [HideInInspector]
    public NativeArray<float> voxelData;
    private MarchingCubes marchingCubes;

    [HideInInspector]
    public JobHandle currentJobHandle;

    private bool init = false;

    void Awake()
    {
        marchingCubes = GetComponent<MarchingCubes>();
        if (voxelData == null || !voxelData.IsCreated)
        {
            AllocateVoxelData();
        }
        GenerateVoxels();

        init = true;
    }

    void AllocateVoxelData()
    {
        int voxelSize = marchingCubes.size + 1;
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

    void OnDestroy()
    {
        DisposeVoxelData();
    }

    public void ReallocateVoxelData()
    {
        DisposeVoxelData();
        AllocateVoxelData();
        GenerateVoxels();
    }

    public void OnValidate()
    {
        if (init && Application.isPlaying)
        {
            GenerateVoxels();
        }
    }

    public void GenerateVoxels()
    {
        if (!voxelData.IsCreated)
        {
            AllocateVoxelData();
        }

        int voxelSize = marchingCubes.size + 1;
        var job = new FractalNoiseJob {
            position = transform.localPosition,
            meshScale = marchingCubes.scale,
            scale = scale,
            size = voxelSize,
            octaves = octaves,
            dimension = dimension,
            lacunarity = lacunarity,
            noiseValues = voxelData
        };

        var handle = job.Schedule(voxelSize * voxelSize * voxelSize, 8);
        currentJobHandle = handle;
        handle.Complete();
    }

    void Update()
    {
    }
}
