using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[System.Serializable]
public class MapParameters
{
    public string name;
    public int seed;
    public int octaves;
    public float amplitude;
    public float persistence;
    public float frequency;
    public float lacunarity;
    public Vector2[] octaveOffsets;
    public AnimationCurve redistribution;

    public void SetOctaveOffsets()
    {
        octaveOffsets = new Vector2[octaves];
        for (int i = 0; i < octaves; i++)
        {
            octaveOffsets[i] = new Vector2(Random.Range(-10000, 10000), Random.Range(-10000, 10000));
        }
    }
}