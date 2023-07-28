using System.Collections;
using System.Collections.Generic;
using UnityEngine;


public class BiomeManager: MonoBehaviour
{
    public List<Biome> biomes;
    public static BiomeManager _instance;

    public BiomeManager()
    {
        _instance = this;
    }
    public Biome GetBiome(float height)
    {
        if(height < 0.4f)
        {
            return biomes[1];
        } 
        else if(height < 0.43f)
        {
            return biomes[2];
        }
        else if (height < 0.7f)
        {
            return biomes[0];
        }
        else
        {
            return biomes[3];
        }
    }
}
