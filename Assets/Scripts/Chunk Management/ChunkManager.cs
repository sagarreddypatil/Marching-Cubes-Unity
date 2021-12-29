using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Unity.Mathematics;
using Unity.Jobs;

public class ChunkManager : MonoBehaviour
{
    private VoxelManager voxelManager;
    private MeshManager meshManager;

    public int rebuildOnUpdate = -1;
    public bool continousUpdate = false;

    void Awake()
    {
        voxelManager = GetComponent<VoxelManager>();
        meshManager = GetComponent<MeshManager>();
    }

    void OnEnable()
    {
    }

    void OnDisable()
    {
    }

    void Update()
    {
        if (isActiveAndEnabled)
        {
            int frameCount = Time.frameCount;
            if (rebuildOnUpdate != -1 && frameCount % math.max(1, rebuildOnUpdate) == 0)
            {
                var voxelHandle = voxelManager.GenerateVoxels();
                var meshHandle = meshManager.GenerateTriangles(voxelHandle);

                meshHandle.Complete();
                meshManager.ConstructMesh();

                if (rebuildOnUpdate != 0 && !continousUpdate)
                {
                    rebuildOnUpdate = -1;
                }
            }
        }
    }
}
