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

    public bool smoothShading = true;

    private MeshFilter meshFilter;
    private MeshCollider meshCollider;
    private Mesh mesh;

    private NativeArray<Triangle> triangleData;
    private NativeArray<Triangle> normalData;
    List<Vector3> meshVertices = new List<Vector3>();
    List<Vector3> meshNormals = new List<Vector3>();
    List<int> meshTriangles = new List<int>();
    Dictionary<float3, int> vertexIdxDict = new Dictionary<float3, int>();

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
        normalData = new NativeArray<Triangle>(size * size * size * 5, Allocator.Persistent, NativeArrayOptions.UninitializedMemory);
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
        if (!triangleData.IsCreated)
        {
            AllocateTriangleData();
        }

        var job = new MarchingCubesJob {
            surfaceLevel = surfaceLevel,
            size = size,
            scale = scale,
            triangles = triangleData,
            normals = normalData,
            voxels = voxelManager.voxelData
        };

        var handle = job.Schedule(size * size * size, 1, dependsOn);
        return handle;
    }

    public void ConstructMesh()
    {
        mesh.indexFormat = UnityEngine.Rendering.IndexFormat.UInt32;

        var trianglesArray = triangleData.ToArray();
        var normalsArray = normalData.ToArray();

        meshVertices.Clear();
        meshNormals.Clear();
        meshTriangles.Clear();
        vertexIdxDict.Clear();

        for (int i = 0; i < trianglesArray.Length; i++)
        {
            if (trianglesArray[i].created)
            {
                for (int j = 0; j < 3; j++)
                {
                    float3 vertex = trianglesArray[i][j];
                    float3 normal = normalsArray[i][j];

                    if (false) // removing duplicate verts is too expensive
                    {
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
                    }
                    else
                    {
                        meshVertices.Add(vertex);
                        meshNormals.Add(normal);
                        meshTriangles.Add(meshVertices.Count - 1);
                    }
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
