using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Unity.Mathematics;
using Unity.Collections;
using Unity.Jobs;

public class MarchingCubes : MonoBehaviour
{
    public int size = 16;
    public float scale = 0.1f;
    public float surfaceLevel = 0f;

    private MeshFilter meshFilter;
    private Mesh mesh;

    void Start()
    {
        Initialize();
        GenerateMesh();
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
    }

    void GenerateMesh()
    {
        var result = new NativeArray<Triangle>(size * size * size * 5, Allocator.Persistent, NativeArrayOptions.ClearMemory);
        var job = new MarchingCubesJob { surfaceLevel = surfaceLevel,
                                         size = size,
                                         position = float3.zero,
                                         scale = scale,
                                         triangles = result };

        var handle = job.Schedule(size * size * size, 1);
        handle.Complete();

        var triangleStructs = result.ToArray();
        result.Dispose();

        var vertices = new List<Vector3>();
        var triangles = new List<int>();

        for (int i = 0; i < triangleStructs.Length; i++)
        {
            if (triangleStructs[i].created)
            {
                vertices.Add(triangleStructs[i].a);
                triangles.Add(vertices.Count - 1);

                vertices.Add(triangleStructs[i].b);
                triangles.Add(vertices.Count - 1);

                vertices.Add(triangleStructs[i].c);
                triangles.Add(vertices.Count - 1);
            }
        }

        mesh.vertices = vertices.ToArray();
        mesh.triangles = triangles.ToArray();
        mesh.RecalculateNormals();
    }

    void Update()
    {
    }
}
