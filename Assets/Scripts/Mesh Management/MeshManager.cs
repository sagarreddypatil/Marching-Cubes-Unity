using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Unity.Mathematics;
using Unity.Collections;
using Unity.Jobs;
using Unity.Profiling;

public class MeshManager : MonoBehaviour
{
    public float surfaceLevel = 0f;

    public bool smoothShading = true;

    private MeshFilter meshFilter;
    private MeshCollider meshCollider;
    private MeshRenderer meshRenderer;
    private Mesh mesh;

    private NativeArray<Triangle> triangleData;
    private NativeArray<Triangle> normalData;

    private Triangle[] triangleArray;
    private Triangle[] normalArray;

    private Vector3[] vertices;
    private Vector3[] normals;
    private int[] triangles;

    private ChunkManager chunkManager;
    private VoxelManager voxelManager;

    static readonly ProfilerMarker meshDataPrepMarker = new ProfilerMarker("MeshDataPrep");
    static readonly ProfilerMarker vertexLoopMarker = new ProfilerMarker("VertexLoop");
    static readonly ProfilerMarker vertexDataRetrivalMarker = new ProfilerMarker("VertexDataRetrival");
    static readonly ProfilerMarker vertexAdditionMarker = new ProfilerMarker("VertexAddition");

    void Awake()
    {
        chunkManager = GetComponent<ChunkManager>();
        voxelManager = GetComponent<VoxelManager>();

        meshRenderer = GetComponent<MeshRenderer>();
        meshRenderer.shadowCastingMode = UnityEngine.Rendering.ShadowCastingMode.TwoSided;

        if (meshFilter == null)
        {
            meshFilter = GetComponent<MeshFilter>();

            mesh = new Mesh();
            mesh.indexFormat = UnityEngine.Rendering.IndexFormat.UInt32;
            mesh.MarkDynamic();

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
        int size = chunkManager.resolution;

        triangleData = new NativeArray<Triangle>(size * size * size * 5, Allocator.Persistent, NativeArrayOptions.UninitializedMemory);
        normalData = new NativeArray<Triangle>(size * size * size * 5, Allocator.Persistent, NativeArrayOptions.UninitializedMemory);

        triangleArray = triangleData.ToArray();
        normalArray = normalData.ToArray();

        vertices = new Vector3[size * size * size * 5 * 3];
        normals = new Vector3[size * size * size * 5 * 3];
        triangles = new int[size * size * size * 5 * 3];
    }

    void DisposeTriangleData()
    {
        if (triangleData != null)
        {
            triangleData.Dispose();
            normalData.Dispose();
        }

        triangleData = default;
        normalData = default;

        triangleArray = null;
        normalArray = null;

        vertices = null;
        normals = null;
        triangles = null;
    }

    public JobHandle GenerateTriangles(JobHandle dependsOn = default)
    {
        int resolution = chunkManager.resolution;

        float scale = chunkManager.voxelSize;

        if (!triangleData.IsCreated)
        {
            AllocateTriangleData();
        }
        if (triangleData.Length != resolution * resolution * resolution * 5)
        {
            DisposeTriangleData();
            AllocateTriangleData();
        }

        var job = new MarchingCubesJob {
            surfaceLevel = surfaceLevel,
            resolution = resolution,
            scale = scale,
            triangles = triangleData,
            normals = normalData,
            voxels = voxelManager.voxelData
        };

        var handle = job.Schedule(resolution * resolution * resolution, 1, dependsOn);
        return handle;
    }

    public void ConstructMesh()
    {
        mesh.indexFormat = UnityEngine.Rendering.IndexFormat.UInt32;

        meshDataPrepMarker.Begin();
        triangleData.CopyTo(triangleArray);
        normalData.CopyTo(normalArray);
        meshDataPrepMarker.End();

        int vertexCounter = 0;

        for (int i = 0; i < triangleArray.Length; i++)
        {
            if (triangleArray[i].created)
            {
                for (int j = 0; j < 3; j++)
                {
                    vertexLoopMarker.Begin();

                    float3 vertex = triangleArray[i][j];
                    float3 normal = normalArray[i][j];

                    vertices[vertexCounter] = vertex;
                    normals[vertexCounter] = normal;

                    triangles[vertexCounter] = vertexCounter;
                    vertexCounter += 1;

                    vertexLoopMarker.End();
                }
            }
        }

        mesh.Clear();

        mesh.SetVertices(vertices[0..vertexCounter]);
        mesh.SetTriangles(triangles[0..vertexCounter], 0, true);
        if (smoothShading)
            mesh.SetNormals(normals[0..vertexCounter]);
        else
            mesh.RecalculateNormals();

        meshCollider.sharedMesh = mesh;
    }
}
