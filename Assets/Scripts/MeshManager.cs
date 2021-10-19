using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Unity.Mathematics;
using Unity.Collections;
using Unity.Jobs;

public class MeshManager : MonoBehaviour
{
    public int size = 16;

    public float scale = 0.1f;
    public float surfaceLevel = 0f;

    private MeshFilter meshFilter;
    private MeshCollider meshCollider;
    private Mesh mesh;

    private NativeArray<Triangle> triangleData;

    private VoxelManager voxelManager;

    void Awake()
    {
        voxelManager = GetComponent<VoxelManager>();
        if (meshFilter == null)
        {
            meshFilter = GetComponent<MeshFilter>();

            mesh = new Mesh();
            mesh.indexFormat = UnityEngine.Rendering.IndexFormat.UInt32;

            meshFilter.sharedMesh = mesh;
        }
        if (meshCollider == null)
        {
            meshCollider = GetComponent<MeshCollider>();
            meshCollider.sharedMesh = mesh;
        }
    }

    void OnEnable()
    {
        if (triangleData == null || !triangleData.IsCreated)
        {
            AllocateTriangleData();
        }
    }

    void OnDisable()
    {
        DisposeTriangleData();
    }

    void AllocateTriangleData()
    {
        triangleData = new NativeArray<Triangle>(size * size * size * 5, Allocator.Persistent, NativeArrayOptions.UninitializedMemory);
    }

    void DisposeTriangleData()
    {
        if (triangleData != null)
        {
            triangleData.Dispose();
        }
        triangleData = default;
    }

    public JobHandle GenerateTriangles(JobHandle dependsOn = default)
    {
        if (!triangleData.IsCreated)
        {
            AllocateTriangleData();
        }

        var job = new MarchingCubesJob {
            surfaceLevel = surfaceLevel,
            size = size,
            scale = scale,
            position = int3.zero,
            triangles = triangleData,
            voxels = voxelManager.voxelData
        };

        var handle = job.Schedule(size * size * size, 1, dependsOn);
        return handle;
    }

    public void ConstructMesh()
    {
        Mesh mesh = new Mesh();
        mesh.indexFormat = UnityEngine.Rendering.IndexFormat.UInt32;

        var trianglesArray = triangleData.ToArray();

        var vertexIdxDict = new Dictionary<float3, int>();

        var meshVertices = new List<Vector3>();
        var meshTriangles = new List<int>();

        for (int i = 0; i < trianglesArray.Length; i++)
        {
            if (trianglesArray[i].created)
            {
                for (int j = 0; j < 3; j++)
                {
                    float3 vertex = trianglesArray[i][j];

                    int sharedVertexIdx;
                    if (vertexIdxDict.TryGetValue(vertex, out sharedVertexIdx))
                    {
                        meshTriangles.Add(sharedVertexIdx);
                    }
                    else
                    {
                        meshVertices.Add(vertex);
                        meshTriangles.Add(meshVertices.Count - 1);
                        vertexIdxDict.Add(vertex, meshVertices.Count - 1);
                    }
                }
            }
        }

        mesh.vertices = meshVertices.ToArray();
        mesh.triangles = meshTriangles.ToArray();
        mesh.RecalculateNormals();

        this.mesh = mesh;
        meshFilter.sharedMesh = mesh;
        meshCollider.sharedMesh = mesh;
    }
}
