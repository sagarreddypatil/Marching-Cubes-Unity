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
    List<Vector3> meshVertices = new List<Vector3>();
    List<Vector3> meshNormals = new List<Vector3>();
    List<int> meshTriangles = new List<int>();
    Dictionary<float3, int> vertexIdxDict = new Dictionary<float3, int>();

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
    }

    public JobHandle GenerateTriangles(JobHandle dependsOn = default)
    {
        int resolution = chunkManager.resolution;

        float scale = chunkManager.scale;

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

        meshVertices.Clear();
        meshNormals.Clear();
        meshTriangles.Clear();
        vertexIdxDict.Clear();

        for (int i = 0; i < triangleArray.Length; i++)
        {
            if (triangleArray[i].created)
            {
                for (int j = 0; j < 3; j++)
                {
                    vertexLoopMarker.Begin();

                    vertexDataRetrivalMarker.Begin();
                    float3 vertex = triangleArray[i][j];
                    float3 normal = normalArray[i][j];
                    vertexDataRetrivalMarker.End();

                    bool removeDuplicateVerts = false;

                    if (removeDuplicateVerts) // removing duplicate verts is too expensive
                    {
                        vertexAdditionMarker.Begin();

                        int sharedVertexIdx;
                        if (vertexIdxDict.TryGetValue(vertex, out sharedVertexIdx))
                        {
                            meshTriangles.Add(sharedVertexIdx);
                        }
                        else
                        {
                            meshVertices.Add(vertex);
                            meshNormals.Add(normal);
                            meshTriangles.Add(meshVertices.Count - 1);
                            vertexIdxDict.Add(vertex, meshVertices.Count - 1);
                        }

                        vertexAdditionMarker.End();
                    }
                    else
                    {
                        vertexAdditionMarker.Begin();

                        meshVertices.Add(vertex);
                        meshNormals.Add(normal);
                        meshTriangles.Add(meshVertices.Count - 1);

                        vertexAdditionMarker.End();
                    }

                    vertexLoopMarker.End();
                }
            }
        }

        mesh.Clear();

        mesh.SetVertices(meshVertices);
        mesh.SetTriangles(meshTriangles, 0, true);
        if (smoothShading)
            mesh.SetNormals(meshNormals);
        else
            mesh.RecalculateNormals();

        // for some weird reason the code below is slower than the code above, I haven't profiled it yet
        // I think it's beacuse of allocating these massive arrays, but I'm too lazy to global allocate

        // Vector3[] vertices = new Vector3[trianglesArray.Length * 3];
        // Vector3[] normals = new Vector3[trianglesArray.Length * 3];
        // int[] triangles = new int[trianglesArray.Length * 3];

        // for (int i = 0; i < trianglesArray.Length; i++)
        // {
        //     for (int j = 0; j < 3; j++)
        //     {
        //         vertices[i * 3 + j] = triangleData[i][j];
        //         normals[i * 3 + j] = normalData[i][j];
        //         triangles[i * 3 + j] = i * 3 + j;
        //     }
        // }

        // mesh.Clear();
        // mesh.SetVertices(vertices);
        // mesh.SetTriangles(triangles, 0, true);
        // if (smoothShading)
        //     mesh.SetNormals(normals);
        // else
        //     mesh.RecalculateNormals();

        meshFilter.sharedMesh = mesh;
        meshCollider.sharedMesh = mesh;
    }
}
