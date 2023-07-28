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
    public float persistance;
    public float frequency;
    public float lacunarity;
    public AnimationCurve redistribution;
}
