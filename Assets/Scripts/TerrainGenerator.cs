using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class TerrainGenerator : MonoBehaviour
{
    private PerlinNoiseMap _landMap;

    public Gradient terrainGradient;
    public int seed;
    public int octaves;
    public float scale;
    public float amplitude;
    public float persistance;
    public float frequency;
    public float lacunarity;
    public int chunkSize;
    public AnimationCurve redistribution;

    public GameObject chunkPrefab;
    private Transform _player;
    public int renderDistance;

    private Dictionary<Vector2, Chunk> _chunks = new Dictionary<Vector2, Chunk>();
    void Awake()
    {
        _player = GameObject.FindGameObjectWithTag("Player").transform;
        _landMap = new PerlinNoiseMap(0, octaves, scale, amplitude, persistance, frequency, lacunarity);
    }

    void Update()
    {
        RenderChunks();
    }

    void RenderChunks()
    {
        int mapChunkSize = chunkSize - 1;
        int currChunkX = (int)_player.position.x / mapChunkSize;
        int currChunkZ = (int)_player.position.z / mapChunkSize;

        List<Vector2> tempKeysList = new List<Vector2>();

        for (int x = -renderDistance; x <= renderDistance; x++)
        {
            for (int z = -renderDistance; z <= renderDistance; z++)
            {
                int chunkX = (currChunkX + x) * mapChunkSize;
                int chunkZ = (currChunkZ + z) * mapChunkSize;
                tempKeysList.Add(new Vector2(chunkX, chunkZ));

                if (!_chunks.ContainsKey(new Vector2(chunkX, chunkZ)))
                {
                    SpawnChunk(chunkX, chunkZ);
                }
                else if(!_chunks[new Vector2(chunkX, chunkZ)].Visible)
                {
                    _chunks[new Vector2(chunkX, chunkZ)].UpdateChunk(true);
                }
            }
        }

        foreach(Vector2 key in _chunks.Keys)
        {
            if (!tempKeysList.Contains(key))
            {
                _chunks[key].UpdateChunk(false);
            }
        }
    }

    void OnValidate()
    {
        if (Application.isPlaying && _chunks.Count > 0)
        {
            _landMap = new PerlinNoiseMap(0, octaves, scale, amplitude, persistance, frequency, lacunarity);
            ReloadChunks();
        }
    }
    void ReloadChunks()
    {
        foreach (Chunk chunk in _chunks.Values)
        {
            Vector3[] vertices = CreateVertices(chunk.transform.position.x, chunk.transform.position.z);
            int[] triangles = CreateTriangles();
            Color[] colors = CreateColors(vertices);
            RedistributeVertices(ref vertices);

            chunk.CreateMesh(vertices, triangles, colors);
        }
    }

    void SpawnChunk(float x, float z)
    {
        Vector3[] vertices = CreateVertices(x, z);
        int[] triangles = CreateTriangles();
        Color[] colors = CreateColors(vertices);
        RedistributeVertices(ref vertices);

        GameObject chunk = Instantiate(chunkPrefab, new Vector3(x, 0, z), Quaternion.identity, null);
        chunk.GetComponent<Chunk>().CreateMesh(vertices, triangles, colors);
        _chunks.Add(new Vector2(x, z), chunk.GetComponent<Chunk>());
    }

    void RedistributeVertices(ref Vector3[] vertices)
    {
        for (int i = 0; i < vertices.Length; i++)
        {
            float y = Mathf.InverseLerp(_landMap.MinHeight, _landMap.MaxHeight, vertices[i].y);
            vertices[i].y += redistribution.Evaluate(y) * amplitude;
        }
    }

    Vector3[] CreateVertices(float offsetX, float offsetY)
    {
        Vector3[] vertices = new Vector3[chunkSize * chunkSize];

        for (int i = 0; i < vertices.Length;)
        {
            for (int x = 0; x < chunkSize; x++)
            {
                for (int z = 0; z < chunkSize; z++)
                {
                    float y = _landMap.GetPoint(x + offsetX, z + offsetY);
                    vertices[i] = new Vector3(x, y, z);
                    i++;
                }
            }
        }
        return vertices;
    }

    int[] CreateTriangles()
    {
        int[] triangles = new int[chunkSize * chunkSize * 6];

        // each loop is to generate triangles for each quad
        int currVertex = 0;
        for (int x = 0; x < chunkSize - 1; x++)
        {
            for (int z = 0; z < chunkSize - 1; z++)
            {
                triangles[currVertex] = x * chunkSize + z + chunkSize + 1;
                triangles[currVertex + 1] = x * chunkSize + z + chunkSize;
                triangles[currVertex + 2] = x * chunkSize + z;
                triangles[currVertex + 3] = x * chunkSize + z;
                triangles[currVertex + 4] = x * chunkSize + z + 1;
                triangles[currVertex + 5] = x * chunkSize + z + chunkSize + 1;

                currVertex += 6;
            }
        }
        return triangles;
    }

    Color[] CreateColors(Vector3[] vertices)
    {
        Color[] colors = new Color[chunkSize * chunkSize];

        for (int i = 0; i < colors.Length; i++)
        {
            float height = Mathf.InverseLerp(_landMap.MinHeight, _landMap.MaxHeight, vertices[i].y);
            colors[i] = terrainGradient.Evaluate(height);
        }
        return colors;
    }
}
