using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Linq;

public class TerrainGenerator : MonoBehaviour
{
    public ComputeShader chunkGenerator;

    private float _maxHeight;
    private float _minHeight;

    public Gradient terrainGradient;
    public float scale;
    public int chunkSize;

    public GameObject chunkPrefab;
    private Transform _player;
    public int renderDistance;

    int[] chunkTriangles;

    private Dictionary<Vector2, Chunk> _chunks = new Dictionary<Vector2, Chunk>();

    public List<MapParameters> parameters;
    void Awake()
    {
        _player = GameObject.FindGameObjectWithTag("Player").transform;
       
        chunkTriangles = CreateTriangles();
        parameters[0].SetOctaveOffsets();
        CalculateHeights(parameters[0]);

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
    void SpawnChunk(float x, float z)
    {
        //Biome[] biomes = new Biome[chunkSize * chunkSize];
        Vector3[] vertices = CreateVertices(x, z);
        int[] triangles = chunkTriangles;
        Color[] colors = CreateColors(vertices);

        GameObject chunk = Instantiate(chunkPrefab, new Vector3(x, 0, z), Quaternion.identity, null);
        chunk.GetComponent<Chunk>().CreateMesh(vertices, triangles, colors);
        _chunks.Add(new Vector2(x, z), chunk.GetComponent<Chunk>());

    }

    Vector3[] CreateVertices(float offsetX, float offsetZ)
    {
        Vector3[] vertices = new Vector3[chunkSize * chunkSize];
        int kernel = chunkGenerator.FindKernel("CSMain");
        ComputeBuffer verticesBuffer = new ComputeBuffer(vertices.Length, sizeof(float) * 3);
        ComputeBuffer octaveOffsetsBuffer = new ComputeBuffer(parameters[0].octaves, sizeof(float) * 2);

        chunkGenerator.SetBuffer(kernel, "verticesBuffer", verticesBuffer);
        chunkGenerator.SetBuffer(kernel, "octaveOffsetsBuffer", octaveOffsetsBuffer);
        chunkGenerator.SetFloat("chunkOffsetX", offsetX);
        chunkGenerator.SetFloat("chunkOffsetZ", offsetZ);
        chunkGenerator.SetFloat("scale", scale);
        chunkGenerator.SetInt("seed", parameters[0].seed);
        chunkGenerator.SetInt("octaves", parameters[0].octaves);
        chunkGenerator.SetFloat("amplitude", parameters[0].amplitude);
        chunkGenerator.SetFloat("lacunarity", parameters[0].lacunarity);
        chunkGenerator.SetFloat("frequency", parameters[0].frequency);
        chunkGenerator.SetFloat("persistence", parameters[0].persistence);
        chunkGenerator.SetFloat("oceanLevel", (_minHeight - _maxHeight) * 0.224f);
        octaveOffsetsBuffer.SetData(parameters[0].octaveOffsets);
        chunkGenerator.Dispatch(kernel,1,1,1);
        
        verticesBuffer.GetData(vertices);
        verticesBuffer.Release();
        octaveOffsetsBuffer.Release();
        
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
            float height = Mathf.InverseLerp(_minHeight, _maxHeight, vertices[i].y);
            colors[i] = terrainGradient.Evaluate(height);
        }
        return colors;
    }

    void CalculateHeights(MapParameters parameters)
    {
        float tempAmplitude = parameters.amplitude;
        for(int i = 0; i < parameters.octaves; i++)
        {
            _maxHeight += tempAmplitude;
            _minHeight -= tempAmplitude;

            tempAmplitude *= parameters.persistence;
        }
    }
}
