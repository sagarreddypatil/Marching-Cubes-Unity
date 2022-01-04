using UnityEngine;
using Unity.Mathematics;

public class ChunkManager : MonoBehaviour
{
    private VoxelManager voxelManager;
    private MeshManager meshManager;
    public int resolution = 16;
    public float size = 10f;

    public float voxelSize
    {
        get {
            return size / resolution;
        }
    }

    public int rebuildOnUpdate = -1;
    public int rebuildOnUpdateCount = 1;
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
            if (Time.frameCount % math.max(1, rebuildOnUpdateCount) == rebuildOnUpdate)
            {
                var voxelHandle = voxelManager.GenerateVoxels();
                var meshHandle = meshManager.GenerateTriangles(voxelHandle);

                meshHandle.Complete();
                meshManager.ConstructMesh();

                if (!continousUpdate)
                {
                    rebuildOnUpdate = -1;
                }
            }
        }
    }
}
