using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Unity.Mathematics;
using Unity.Collections;
using Unity.Jobs;

public class MarchingCubes : MonoBehaviour
{
    public int size = 16;
    private int allocatedSize;

    public float scale = 0.1f;
    public float surfaceLevel = 0f;

    private MeshFilter meshFilter;
    private Mesh mesh;

    public bool generate = false;
    private NativeArray<Triangle> triangleData;

    void Start()
    {
        Initialize();
        StartCoroutine("MeshGenerationCoroutine");
        generate = true;
    }

    void Initialize()
    {
        if (meshFilter == null)
        {
            meshFilter = GetComponent<MeshFilter>();

            mesh = new Mesh();
            mesh.indexFormat = UnityEngine.Rendering.IndexFormat.UInt32;

            meshFilter.sharedMesh = mesh;
        }

        if (triangleData == null || !triangleData.IsCreated)
        {
            AllocateTriangleData();
            allocatedSize = size;
        }
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

    void OnDestroy()
    {
        DisposeTriangleData();
    }

    IEnumerator MeshGenerationCoroutine()
    {
        while (true)
        {
            if (generate)
            {
                try
                {
                    GenerateMesh();
                }
                catch (Exception e)
                {
                    Debug.LogError(e.Message);
                }
            }
            yield return new WaitForSecondsRealtime(1f / 30);
        }
    }

    void OnValidate()
    {
        if (Application.isPlaying)
        {
            DisposeTriangleData();
            AllocateTriangleData();
        }
    }

    void GenerateMesh()
    {
        if (!triangleData.IsCreated)
        {
            AllocateTriangleData();
        }

        var job = new MarchingCubesJob { surfaceLevel = surfaceLevel,
                                         size = size,
                                         scale = scale,
                                         position = int3.zero,
                                         triangles = triangleData };

        var handle = job.Schedule(size * size * size, 1);
        handle.Complete();

        var trianglesArray = triangleData.ToArray();

        var meshVertices = new List<Vector3>();
        var meshTriangles = new List<int>();

        for (int i = 0; i < trianglesArray.Length; i++)
        {
            if (trianglesArray[i].created)
            {
                meshVertices.Add(trianglesArray[i].a);
                meshTriangles.Add(meshVertices.Count - 1);

                meshVertices.Add(trianglesArray[i].b);
                meshTriangles.Add(meshVertices.Count - 1);

                meshVertices.Add(trianglesArray[i].c);
                meshTriangles.Add(meshVertices.Count - 1);
            }
        }

        mesh.vertices = meshVertices.ToArray();
        mesh.triangles = meshTriangles.ToArray();
        mesh.RecalculateNormals();
    }

    void Update()
    {
        // GenerateMesh();
    }
}
