using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[RequireComponent(typeof(MeshFilter))]
public class Chunk : MonoBehaviour
{
    private Mesh _mesh;
    private Vector3[] _vertices;
    private int[] _triangles;
    private MeshFilter _meshFilter;
    public bool Visible { get; private set; }

    void Awake()
    {
        _meshFilter = GetComponent<MeshFilter>();
    }

    public void CreateMesh(Vector3[] vertices, int[] triangles, Color[] colors)
    {
        _mesh = new Mesh();
        _mesh.vertices = _vertices = vertices;
        _mesh.triangles = _triangles = triangles;
        _mesh.colors = colors;
        _mesh.RecalculateNormals();
        _meshFilter.mesh = _mesh;
    }

    public void UpdateChunk(bool state)
    {
        Visible = state;
        gameObject.SetActive(state);
    }
}
