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
    [SerializeField] [Range(0f, 1f)] private float persistence;
    [SerializeField] private float lacunarity;
    [SerializeField] private Vector2 offset;

    [Header("Map Generation Settings")]
    [SerializeField] private List<TileWithWeight> tiles = new List<TileWithWeight>();

    [Header("Resources Generation Settings")]
    [SerializeField] private SO_RessourcesNoiseRules rules;
    [SerializeField] [Range(0, .5f)] private float resourceSpawnRate;

    [SerializeField] private GameObject agentPrefab;
    [SerializeField] private Transform agentParent;
    [SerializeField] private Transform resourceParent;
    public int agentCount;

    [HideInInspector] public List<GameObject> _spawnedAgent = new List<GameObject>();
    private readonly List<GameObject> _spawnedItem = new List<GameObject>();
    private readonly List<(int, int)> _spawnedLocation = new List<(int, int)>();

    private Tilemap _tilemap;
    private readonly Vector3 stoneOffSet = new Vector3(0, -0.29f, 0);

    private readonly Vector3 treeOffSet = new Vector3(0, 0.2f, 0);

    private void Awake()
    {
        _tilemap = GetComponent<Tilemap>();
    }

    private void OnEnable()
    {
        SaveEvents.OnNewGameStartEvent += GenerateWorld;
    }

    private void OnDisable()
    {
        SaveEvents.OnNewGameStartEvent -= GenerateWorld;
    }

    public static event Func<RessourceType, Vector2, GameObject> OnAddNewRessourceEvent;
    public static event Action OnInitGraphEvent;

    private void GenerateWorld()
    {
        MapGeneration();
        OnInitGraphEvent?.Invoke();
        RessourcesGeneration();
        SpawnAgent();
    }

    private void MapGeneration()
    {
        float weight;
        int seed = Random.Range(0, 1000000);
        float[,] noiseMap = Noise.GenerateNoiseMap(mapWidth, mapHeight, seed, scale, octaves, persistence, lacunarity, offset);
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
        float[,] noiseMap = Noise.GenerateNoiseMap(mapWidth, mapHeight, seed, scale, octaves, persistence, lacunarity, offset);
        for (int x = 0; x < mapWidth; x++)
        {
            for (int y = 0; y < mapHeight; y++)
            {
                Vector3Int cell = new Vector3Int(x - mapWidth / 2, y - mapHeight / 2, 0);

                if (_tilemap.GetTile(cell) == tiles[2].tile)
                    continue;

                Vector3 pos = _tilemap.CellToWorld(cell) + new Vector3(.5f, .5f, 0);
                position = (x, y);
                float noiseValue = noiseMap[x, y];

                foreach (RessourceRule rule in rules.ressourceRules)
                {
                    if (noiseValue >= rule.minNoise && noiseValue <= rule.maxNoise
                                                    && !_spawnedLocation.Contains(position))
                    {
                        SpawnResource(rule.type, pos, position);
                        break;
                    }
                }
            }
        }
    }

    private void SpawnResource(RessourceType type, Vector3 pos, (int, int) key)
    {
        if (type == RessourceType.wood)
        {
            _spawnedLocation.Add(key);
            _spawnedItem.Add(OnAddNewRessourceEvent?.Invoke(type, pos + treeOffSet));
        }
        else if (type == RessourceType.stone)
        {
            _spawnedLocation.Add(key);
            _spawnedItem.Add(OnAddNewRessourceEvent?.Invoke(type, pos + stoneOffSet));
        }
        else
        {
            _spawnedLocation.Add(key);
            _spawnedItem.Add(OnAddNewRessourceEvent?.Invoke(type, pos));
        }
    }


    public void SpawnAgent()
    {
        Vector3Int pos = new Vector3Int(Random.Range(-mapWidth / 2, mapWidth / 2), Random.Range(-mapHeight / 2, mapHeight / 2));
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
