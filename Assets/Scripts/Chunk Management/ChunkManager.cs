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
        DrawBox();

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

    void DrawBox()
    {
        return;

        Color boxColor = size > 0 ? Color.green : Color.red;

        Debug.DrawLine(transform.position + new Vector3(0, 0, 0), transform.position + new Vector3(size, 0, 0), boxColor);
        Debug.DrawLine(transform.position + new Vector3(size, 0, 0), transform.position + new Vector3(size, size, 0), boxColor);
        Debug.DrawLine(transform.position + new Vector3(size, size, 0), transform.position + new Vector3(0, size, 0), boxColor);
        Debug.DrawLine(transform.position + new Vector3(0, size, 0), transform.position + new Vector3(0, 0, 0), boxColor);

        Debug.DrawLine(transform.position + new Vector3(0, 0, size), transform.position + new Vector3(size, 0, size), boxColor);
        Debug.DrawLine(transform.position + new Vector3(size, 0, size), transform.position + new Vector3(size, size, size), boxColor);
        Debug.DrawLine(transform.position + new Vector3(size, size, size), transform.position + new Vector3(0, size, size), boxColor);
        Debug.DrawLine(transform.position + new Vector3(0, size, size), transform.position + new Vector3(0, 0, size), boxColor);

        Debug.DrawLine(transform.position + new Vector3(0, 0, 0), transform.position + new Vector3(0, 0, size), boxColor);
        Debug.DrawLine(transform.position + new Vector3(size, 0, 0), transform.position + new Vector3(size, 0, size), boxColor);
        Debug.DrawLine(transform.position + new Vector3(size, size, 0), transform.position + new Vector3(size, size, size), boxColor);
        Debug.DrawLine(transform.position + new Vector3(0, size, 0), transform.position + new Vector3(0, size, size), boxColor);
    }
}
