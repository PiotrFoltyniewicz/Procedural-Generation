using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[RequireComponent(typeof(MeshFilter))]
public class TerrainGenerator : MonoBehaviour
{

    Vector3[] vertices;
    int[] triangles;
    MeshFilter meshFilter;
    MeshCollider meshCollider;
    public int size;

    void Awake()
    {
        meshFilter = GetComponent<MeshFilter>();
        meshCollider = GetComponent<MeshCollider>();
        meshFilter.mesh = CreateMesh();
    }

    private void OnValidate()
    {
        if (!meshFilter) return;
        meshFilter.mesh.Clear();
        meshCollider.sharedMesh = meshFilter.mesh = CreateMesh();
        meshFilter.mesh.RecalculateNormals();
    }

    Mesh CreateMesh()
    {
        Mesh mesh = new Mesh();
        mesh.vertices = vertices = CreateVertices();
        mesh.triangles = triangles = CreateTriangles();

        return mesh;
    }

    Vector3[] CreateVertices()
    {
        Vector3[] vertices = new Vector3[size * size];

        for(int i = 0; i < vertices.Length;)
        {
            for(int x = 0; x < size; x++)
            {
                for (int z = 0; z < size; z++)
                {
                    vertices[i] = new Vector3(x, Mathf.PerlinNoise(x * 0.3f, z * 0.3f) * 2f, z);
                    i++;
                }
            }
        }
        return vertices;
    }

    int[] CreateTriangles()
    {
        int[] triangles = new int[size * size * 6];

        // each loop is to generate triangles for each quad
        int currVertex = 0;
        for(int x = 0; x < size - 1; x++)
        {
            for(int z = 0; z < size - 1; z++)
            {
                triangles[currVertex] = x * size + z + size + 1;
                triangles[currVertex + 1] = x * size + z + size;
                triangles[currVertex + 2] = x * size + z;
                triangles[currVertex + 3] = x * size + z ;
                triangles[currVertex + 4] = x * size + z + 1;
                triangles[currVertex + 5] = x * size + z + size + 1;

                currVertex += 6;
            }
        }
        return triangles;
    }



}
