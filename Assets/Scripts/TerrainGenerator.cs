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
    float maxHeight;
    float minHeight;
    public Gradient terrainGradient;
    int seed;

    public GameObject water;

    enum Biomes
    {
        Plains, 
        Mountains, 
        Ocean
    }

    void Awake()
    {
        meshFilter = GetComponent<MeshFilter>();
        meshCollider = GetComponent<MeshCollider>();
        meshFilter.mesh = CreateMesh();
    }

    private void Update()
    {
        if (Input.GetKeyDown(KeyCode.E))
        {
            meshFilter.mesh.Clear();
            seed = Random.Range(0, 10000);
            meshCollider.sharedMesh = meshFilter.mesh = CreateMesh();
            meshFilter.mesh.RecalculateNormals();
        }
    }

    private void OnValidate()
    {
        if (!meshFilter) return;
        /*
        meshFilter.mesh.Clear();
        seed = Random.Range(0,10000);
        meshCollider.sharedMesh = meshFilter.mesh = CreateMesh();
        meshFilter.mesh.RecalculateNormals();*/
    }

    Mesh CreateMesh()
    {
        maxHeight = -99999;
        minHeight = 999999;
        Mesh mesh = new Mesh();
        mesh.vertices = vertices = CreateVertices();
        mesh.triangles = triangles = CreateTriangles();
        mesh.colors = CreateColors();

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
                    vertices[i] = new Vector3(x, GetHeight(x,z), z);
                    if (vertices[i].y > maxHeight) maxHeight = vertices[i].y;
                    if (vertices[i].y < minHeight) minHeight = vertices[i].y;
                    i++;
                }
            }
        }
        float waterLevel = (maxHeight + minHeight) * 0.40f;
        water.transform.position = new Vector3(100f, waterLevel, 100f);
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

    Color[] CreateColors()
    {
        Color[] colors = new Color[size * size];

        for (int i = 0; i < colors.Length; i++)
        {
            float height = Mathf.InverseLerp(minHeight, maxHeight, vertices[i].y);
            colors[i] = terrainGradient.Evaluate(height);
        }
        return colors;
    }

    float GetHeight(int x, int z)
    {
        float biomeY = Mathf.PerlinNoise(x * 0.01f + seed, z * 0.01f + seed);
        return Mathf.PerlinNoise(x * 0.6f + seed, z * 0.6f + seed) * 2f + GetHeight(biomeY) * 50;
    }

    float GetHeight(float x)
    {
        if (x < 0.15) return 0.45f;
        else return 179.3f * Mathf.Pow(x, 7) - 400.38f * Mathf.Pow(x, 6) + 27.3f * Mathf.Pow(x, 5) + 592.5f * Mathf.Pow(x, 4) - 600f * Mathf.Pow(x, 3) + 240f * Mathf.Pow(x, 2) - 40f * x + 2.73f;

    }

}
