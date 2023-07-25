using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public sealed class PerlinNoiseMap
{
    private int _seed;
    private int _octaves;
    private float _scale;
    private float _amplitude;
    private float _persistence;
    private float _frequency;
    private float _lacunarity;
    private float _minHeight;
    private float _maxHeight;
    private Vector2[] _octaveOffsets;

    public PerlinNoiseMap(int seed, int octaves, float scale, float amplitude, float persistence, float frequency, float lacunarity, float minHeight, float maxHeight)
    {
        _seed = seed;
        _octaves = octaves;
        _scale = scale;
        _amplitude = amplitude;
        _frequency = frequency;
        _lacunarity = lacunarity;
        _minHeight = minHeight;
        _maxHeight = maxHeight;
        _persistence = persistence;
        _octaveOffsets = new Vector2[octaves];

        for (int i = 0; i < _octaves; i++)
        {
            _octaveOffsets[i] = new Vector2(Random.Range(-10000, 10000), Random.Range(-10000, 10000));
        }
        _persistence = persistence;
    }

    public float GetPoint(float x, float y)
    {
        float height = 0;
        float tempAmplitude = _amplitude;
        float tempFrequency = _frequency;

        for (int i = 0; i < _octaves; i++)
        {
            float offsetX = x / _scale * tempFrequency + _octaveOffsets[i].x;
            float offsetY = y / _scale * tempFrequency + _octaveOffsets[i].y;

            height += (Mathf.PerlinNoise(_seed + offsetX + 0.5f, _seed + offsetY + 0.5f) * 2 - 1) * tempAmplitude;
            tempAmplitude *= _persistence;
            tempFrequency *= _lacunarity;
        }
        
        return Mathf.Clamp(height, _minHeight, _maxHeight);
    }
}
