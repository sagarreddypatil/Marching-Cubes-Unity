using System;
using System.Collections;
using System.Collections.Generic;
using Unity.Collections;
using Unity.Jobs;
using Unity.Mathematics;
using UnityEngine;

public class VoxelManager : MonoBehaviour
{
    [HideInInspector]
    public NativeArray<float> voxelData;
    private ChunkManager chunkManager;

    public VoxelJob[] voxelGenerationPipeline;

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

    public JobHandle ApplyJob(VoxelJob job, JobHandle dependsOn = default)
    {
        job.voxelData = voxelData;
        job.position = transform.position; // TODO: Change this to local position if needed
        job.resolution = chunkManager.resolution + 3;
        job.voxelScale = chunkManager.scale;

        return job.GenerateVoxels(dependsOn);
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

        JobHandle previousJob = default;
        for (int i = 0; i < voxelGenerationPipeline.Length; i++)
        {
            VoxelJob job = voxelGenerationPipeline[i];
            previousJob = ApplyJob(job, previousJob);
        }

        return previousJob;
    }
}
