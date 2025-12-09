using UnityEngine;
using Random = System.Random;

public static class Noise {

    /// <summary>
    /// 
    /// </summary>
    /// <param name="mapWidth">Width of the noise map you want to generate</param>
    /// <param name="mapHeight">Height of the noise map you want to generate</param>
    /// <param name="seed">seed for random generation</param>
    /// <param name="scale">how homogenous your noise map is</param>
    /// <param name="octaves">amount of perlin noise</param>
    /// <param name="persistance"></param>
    /// <param name="lacunarity"></param>
    /// <param name="offset">move noise map according to offset</param>
    /// <returns></returns>
    public static float[,] GenerateNoiseMap(int mapWidth, int mapHeight, int seed, float scale, int octaves, float persistance, float lacunarity, Vector2 offset) {
        float[,] noiseMap = new float[mapWidth,mapHeight];
        Random rndValue = new Random (seed);
        Vector2[] octaveOffsets = new Vector2[octaves];
        
        float maxPossibleHeight = 0;
        //float minPossibleHeight = 0;
        
        float amplitude = 1;
        float frequency = 1;
        
        for (int i = 0; i < octaves; i++) {
            float offsetX = rndValue.Next (-100000, 100000) + offset.x;
            float offsetY = rndValue.Next (-100000, 100000) - offset.y;
            octaveOffsets [i] = new Vector2 (offsetX, offsetY);

            maxPossibleHeight += amplitude;
            amplitude *= persistance;
        }

        if (scale <= 0) {
            scale = 0.0001f;
        }

        float maxLocalNoiseHeight = float.MinValue;
        float minLocalNoiseHeight = float.MaxValue;

        // Permet que le paramètre noise provoque un zoom au centre de la carte
        float halfWidth = mapWidth / 2f;
        float halfHeight = mapHeight / 2f;


        for (int y = 0; y < mapHeight; y++) {
            for (int x = 0; x < mapWidth; x++) {
        
                amplitude = 1;
                frequency = 1;
                float noiseHeight = 0;

                for (int i = 0; i < octaves; i++) {
                    float sampleX = (x-halfWidth+ octaveOffsets[i].x) / scale * frequency ;
                    float sampleY = (y-halfHeight+ octaveOffsets[i].y) / scale * frequency ;

                    float perlinValue = Mathf.PerlinNoise (sampleX, sampleY) * 2 - 1;
                    noiseHeight += perlinValue * amplitude;

                    amplitude *= persistance;
                    frequency *= lacunarity;
                }

                // Garde une trace des valeurs minimale et maximale générées
                if (noiseHeight > maxLocalNoiseHeight) {
                    maxLocalNoiseHeight = noiseHeight;
                } else if (noiseHeight < minLocalNoiseHeight) {
                    minLocalNoiseHeight = noiseHeight;
                }
                noiseMap [x, y] = noiseHeight;
            }
        }

        // Normalisation
        for (int y = 0; y < mapHeight; y++) {
            for (int x = 0; x < mapWidth; x++) {
                noiseMap [x, y] = Mathf.InverseLerp (minLocalNoiseHeight, maxLocalNoiseHeight, noiseMap [x, y]);
            }
        }

        return noiseMap;
    }

}