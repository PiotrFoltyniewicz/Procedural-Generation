using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class TerrainGenerator : MonoBehaviour
{
    private PerlinNoiseMap _landMap;
    private PerlinNoiseMap _erosionMap;
    private PerlinNoiseMap _mountainsnessMap;
    //private BiomeManager _biomeManager;

    private float _maxHeight;
    private float _minHeight;

    public Gradient terrainGradient;
    public float scale;
    public int chunkSize;

    public GameObject chunkPrefab;
    private Transform _player;
    public int renderDistance;

    private Dictionary<Vector2, Chunk> _chunks = new Dictionary<Vector2, Chunk>();

    public List<MapParameters> parameters;
    void Awake()
    {
        //_biomeManager = GetComponent<BiomeManager>();
        _player = GameObject.FindGameObjectWithTag("Player").transform;
        _landMap = new PerlinNoiseMap(0, parameters[0].octaves, scale, parameters[0].amplitude, parameters[0].persistance, parameters[0].frequency, parameters[0].lacunarity);
        _erosionMap = new PerlinNoiseMap(0, parameters[1].octaves, scale, parameters[1].amplitude, parameters[1].persistance, parameters[1].frequency, parameters[1].lacunarity);
        _mountainsnessMap = new PerlinNoiseMap(0, parameters[2].octaves, scale, parameters[2].amplitude, parameters[2].persistance, parameters[2].frequency, parameters[2].lacunarity);
        
        _maxHeight = _landMap.MaxHeight;
        _minHeight = _landMap.MinHeight;
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
            _landMap = new PerlinNoiseMap(0, parameters[0].octaves, scale, parameters[0].amplitude, parameters[0].persistance, parameters[0].frequency, parameters[0].lacunarity);
            _erosionMap = new PerlinNoiseMap(0, parameters[1].octaves, scale, parameters[1].amplitude, parameters[1].persistance, parameters[1].frequency, parameters[1].lacunarity);
            _mountainsnessMap = new PerlinNoiseMap(0, parameters[2].octaves, scale, parameters[2].amplitude, parameters[2].persistance, parameters[2].frequency, parameters[2].lacunarity);

            _maxHeight = _landMap.MaxHeight;
            _minHeight = _landMap.MinHeight;
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

            chunk.CreateMesh(vertices, triangles, colors);
        }
    }
    
    void SpawnChunk(float x, float z)
    {
        //Biome[] biomes = new Biome[chunkSize * chunkSize];
        Vector3[] vertices = CreateVertices(x, z);
        int[] triangles = CreateTriangles();
        Color[] colors = CreateColors(vertices);

        GameObject chunk = Instantiate(chunkPrefab, new Vector3(x, 0, z), Quaternion.identity, null);
        chunk.GetComponent<Chunk>().CreateMesh(vertices, triangles, colors);
        _chunks.Add(new Vector2(x, z), chunk.GetComponent<Chunk>());
    }

    /*
    void RedistributeVertices(ref Vector3[] vertices, Biome[] biomes)
    {
        for (int i = 0; i < vertices.Length; i++)
        {
            float y = Mathf.InverseLerp(_landMap.MinHeight, _landMap.MaxHeight, vertices[i].y);
            vertices[i].y = biomes[i].redistribution.Evaluate(y) * amplitude;
        }
    }
    */
    Vector3[] CreateVertices(float offsetX, float offsetZ)
    {
        Vector3[] vertices = new Vector3[chunkSize * chunkSize];

        for (int i = 0; i < vertices.Length;)
        {
            for (int x = 0; x < chunkSize; x++)
            {
                for (int z = 0; z < chunkSize; z++)
                {

                    float height = GetHeight(x + offsetX,z + offsetZ);
                    vertices[i] = new Vector3(x, height, z);
                    i++;
                }
            }
        }
        return vertices;
    }

    float GetHeight(float x, float z)
    {
        float landHeight = _landMap.GetPoint(x, z);
        float clampedLandHeight = Mathf.InverseLerp(_landMap.MinHeight, _landMap.MaxHeight, landHeight);
        float height = landHeight;
        float erosion = _erosionMap.GetPoint(x, z);
        float mountainsness = _mountainsnessMap.GetPoint(x, z);

        
        float mountainsnessStrength = mountainsness * parameters[1].redistribution.Evaluate(clampedLandHeight);
        float erosionStrength = erosion * parameters[0].redistribution.Evaluate(mountainsness);

        height *= parameters[0].redistribution.Evaluate(clampedLandHeight);
        height += parameters[2].redistribution.Evaluate(mountainsnessStrength) * 10;
        height -= parameters[1].redistribution.Evaluate(erosion) * erosionStrength;

        return height;
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
