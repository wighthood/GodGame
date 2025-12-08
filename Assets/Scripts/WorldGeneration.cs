using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Tilemaps;
using Random = UnityEngine.Random;

public class WorldGeneration : MonoBehaviour
{
    [Header("Perlin Noise Settings")]
    [SerializeField] private int mapWidth;
    [SerializeField] private int mapHeight;
    [SerializeField] private int scale;
    [SerializeField] private int octaves;
    [SerializeField,Range(0f,1f)] private float persistence;
    [SerializeField] private float lacunarity;
    [SerializeField] private Vector2 offset;
    
    [Header("Map Generation Settings")]
    [SerializeField] private List<TileWithWeight> tiles = new();
    
    [Header("Resources Generation Settings")]
    [SerializeField,Range(0,.5f)] private float resourceSpawnRate;
    
    [SerializeField] private GameObject agentPrefab;
    [SerializeField] private Transform agentParent;
    [SerializeField] private Transform resourceParent;
    public int agentCount;
    
    private Tilemap _tilemap;
    private List<(int,int)> _spawnedLocation = new();
    private List<GameObject> _spawnedItem = new();
    
    [HideInInspector] public List<GameObject> _spawnedAgent = new();

    public static event Func<RessourceType, Vector2, GameObject> AddNewRessource;

    void Start()
    {
        _tilemap = GetComponent<Tilemap>();
        MapGeneration();
        Graph.instance.InitGraph();
        RessourcesGeneration();
        SpawnAgent();
    }
    
    private void MapGeneration()
    {
        float weight;
        int seed = Random.Range(0, 1000000);
        float[,] noiseMap = Noise.GenerateNoiseMap(mapWidth, mapHeight, seed,scale, octaves, persistence, lacunarity, offset);
        for (int x = 0; x < mapWidth; x++)
        {
            for (int y = 0; y < mapHeight; y++)
            {
                weight = 0;
                foreach (TileWithWeight tile in tiles)
                {
                    weight += tile.weight;
                    if (!(noiseMap[x, y] <= weight)) continue;
                    int mapX = x - mapWidth / 2;
                    int mapY = y - mapHeight / 2;

                    _tilemap.SetTile(new Vector3Int(mapX, mapY, 0), tile.tile);
                    break;
                }
            }
        }
    }
    
    private void RessourcesGeneration()
    {
        (int, int) position;
        int seed = Random.Range(0, 1000000);
        float [,] noiseMap = Noise.GenerateNoiseMap(mapWidth, mapHeight, seed,scale, octaves, persistence, lacunarity, offset);
        for (int x =0; x < mapWidth; x++)
        {
            for (int y = 0; y < mapHeight; y++)
            {
                Debug.Log(noiseMap[x, y]);
                position = (x, y);
                Vector3 pos = _tilemap.CellToWorld(new Vector3Int(x - mapHeight/2, y - mapWidth/2, 0)) + new Vector3(.5f, .5f, 0);
                if (_tilemap.GetTile(new Vector3Int(x-mapWidth/2, y-mapHeight/2, 0)) == tiles[2].tile) continue;
                if (noiseMap[x, y] <= resourceSpawnRate && !_spawnedLocation.Contains(position))
                {
                    _spawnedLocation.Add(position);
                    _spawnedItem.Add(AddNewRessource.Invoke(RessourceType.food, pos));
                }
                else if (noiseMap[x, y] >= 1 - resourceSpawnRate && !_spawnedLocation.Contains(position))
                {
                    _spawnedLocation.Add(position);
                    _spawnedItem.Add(AddNewRessource.Invoke(RessourceType.wood, pos));
                }
            }
        }
    }

    public void SpawnAgent()
    {
        Vector3Int pos = new Vector3Int(Random.Range(-mapWidth/2, mapWidth/2), Random.Range(-mapHeight/2, mapHeight/2));
        if (_tilemap.GetTile(pos) == tiles[2].tile)
        {
            SpawnAgent();
            return;
        }  
        Camera.main.transform.position = _tilemap.CellToWorld(pos) + new Vector3(0, 0, -10);
        
        for (int i = 0; i < agentCount; i++)
        {
            _spawnedAgent.Add(Instantiate(agentPrefab, _tilemap.CellToWorld(pos), Quaternion.identity, agentParent));
        }
    }

    public Tilemap GetTilemaps()
    {
        return _tilemap;
    }

    public int MapWidth()
    {
        return mapWidth;
    }

    public int MapHeight()
    {
        return mapHeight;
    }
}
