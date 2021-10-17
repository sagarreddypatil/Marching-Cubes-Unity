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
    private NativeArray<Triangle> jobResult;

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

        if (jobResult == null || !jobResult.IsCreated)
        {
            AllocateJobResult();
            allocatedSize = size;
        }
    }

    void AllocateJobResult()
    {
        jobResult = new NativeArray<Triangle>(size * size * size * 5, Allocator.Persistent, NativeArrayOptions.ClearMemory);
    }
    void DisposeJobResult()
    {
        jobResult.Dispose();
        jobResult = default;
    }

    void OnDestroy()
    {
        if (jobResult != null)
        {
            DisposeJobResult();
        }
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
            DisposeJobResult();
            AllocateJobResult();
        }
    }

    void GenerateMesh()
    {
        if (!jobResult.IsCreated)
        {
            AllocateJobResult();
        }

        var job = new MarchingCubesJob { surfaceLevel = surfaceLevel,
                                         size = size,
                                         scale = scale,
                                         position = int3.zero,
                                         triangles = jobResult };

        var handle = job.Schedule(size * size * size, 1);
        handle.Complete();

        var triangleStructs = jobResult.ToArray();

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
        // GenerateMesh();
    }
}
