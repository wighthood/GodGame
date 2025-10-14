using System.Collections.Generic;
using NavMeshPlus.Components;
using UnityEngine;
using UnityEngine.Tilemaps;

public class WorldGeneration : MonoBehaviour
{
    [SerializeField] private int mapWidth;
    [SerializeField] private int mapHeight;
    [SerializeField] private int scale;
    [SerializeField] private int octaves;
    [SerializeField,Range(0f,1f)] private float persistence;
    [SerializeField] private float lacunarity;
    [SerializeField] private Vector2 offset;
    [SerializeField] private List<TileWithWeight> tiles = new();
    [SerializeField] private NavMeshSurface navMesh;
    
    
    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        Tilemap tilemap = GetComponent<Tilemap>();
        int seed = Random.Range(0, 1000000);
        float[,] noiseMap = Noise.GenerateNoiseMap(mapWidth, mapHeight, seed,scale, octaves, persistence, lacunarity, offset);
        for (int x = 0; x < mapWidth; x++)
        {
            for (int y = 0; y < mapHeight; y++)
            {
                float weight = 0;
                foreach (var tile in tiles)
                {
                    weight += tile.weight;
                    if (noiseMap[x, y] <= weight)
                    {
                        int Mapx = x - mapWidth / 2;
                        int Mapy = y - mapHeight / 2;

                        tilemap.SetTile(new Vector3Int(Mapx, Mapy, 0), tile.tile);
                        break;
                    } 
                }
            }
        }
        navMesh.BuildNavMesh();
    }
}
