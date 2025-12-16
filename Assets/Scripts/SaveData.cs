using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Tilemaps;

public class GameSceneController : MonoBehaviour
{
    [Header("Réfs scène de jeu")]
    [SerializeField] private WorldGeneration worldGen;
    [SerializeField] private SaveManager saveManager;
    [SerializeField] private Tilemap tilemap;
    [SerializeField] private TileBase[] tilePalette;
    [SerializeField] private GameObject agentPrefab;
    public GameObject saveButton;
    public GameObject panelPauseMenu;

    [Header("Parents")]
    [SerializeField] private Transform colonyParent;
    [SerializeField] private Transform agentParent;

    [Header("Réfs ressources")]
    [SerializeField] private GameObject ressourceParent;
    [SerializeField] private List<GameObject> ressourcePrefab;

    [Header("Réfs IA / Blackboard")]
    public BlackBoard existingBlackboard;

    public static event Action<GameData> SaveGame;

    public static event Func<RessourceType, Vector2, GameObject> AddNewRessource;
    public static event Action InitGraph;
    public static event Action<Vector3, List<ColonyAgent>> spawnColony;
    public static event Func<int, Colony> getColony;
    public static event Func<WeatherState> GetWeather;
    public static event Func<float> getRemainingTime;
    public static event Action<WeatherState, float> SetWeather;

    void Start()
    {
        if (GameModeManager.Instance == null ||
            GameModeManager.Instance.currentMode == GameModeManager.GameMode.Play)
        {
            worldGen.enabled = true;
        }
        else
        {
            worldGen.enabled = false;
            LoadGame(SaveManager.loadedGameData);
        }
    }

    public void OnClickSave()
    {
        SaveGame.Invoke(SaveGameData());
    }

    GameData SaveGameData()
    {
        return new GameData
        {
            camPos = Camera.main.transform.position,
            tilemap = SaveTilemapData(),
            ressources = SaveRessourceData(),
            colonies = SaveColonies(),
            noColonyAgents = SaveAgentsWithoutColony(),
            weather = GetCurrentWeather()
        };
    }

    private void LoadGame(GameData _data)
    {
        Camera.main.transform.position = _data.camPos;
        LoadTilemap(_data.tilemap);
        LoadRessource(_data.ressources);
        LoadColonies(_data.colonies);
        LoadAgentsWithoutColonies(_data.noColonyAgents);
        LoadWeather(_data.weather);
    }

    #region Colonies

    //save
    List<ColonySaveData> SaveColonies()
    {
        List<ColonySaveData> savedColonies = new List<ColonySaveData>();

        foreach (Transform colonyTransform in colonyParent.transform)
        {
            Colony currentColony = colonyTransform.GetComponent<Colony>();

            savedColonies.Add(
            new ColonySaveData
            {
                colonyPos = colonyTransform.position,
                buildings = SaveColonyBuilding(currentColony),
                agentData = SaveAgentOfColony(currentColony),
            });
        }

        return savedColonies;
    }

    private List<BuildingSaveData> SaveColonyBuilding(Colony _colony)
    {
        List<BuildingSaveData> savedBuildings = new();

        foreach (GameObject building in _colony.Buildings)
        {
            savedBuildings.Add(new BuildingSaveData
            {
                buildingPos = building.transform.position,
                type = building.GetComponent<Building>().Type
            });
        }

        return savedBuildings;
    }

    //load

    private void LoadColonies(List<ColonySaveData> _coloniesData)
    {
        foreach(ColonySaveData data in _coloniesData)
        {
            List<ColonyAgent> agents = LoadAgentsFormColony(data);

            spawnColony.Invoke(data.colonyPos, agents);

            Colony createdColony = getColony.Invoke(_coloniesData.IndexOf(data));

            LoadBuildingsOfColony(createdColony, data);
        }
    }

    private void LoadBuildingsOfColony(Colony _createdColony, ColonySaveData _colonyData)
    {
        foreach(BuildingSaveData buildingData in _colonyData.buildings)
        {
            BuildingEvents.OnSpawnRequested.Invoke(buildingData.type, buildingData.buildingPos, _createdColony);
        }
    }

    #endregion

    #region Agents

    //save
    List<AgentData> SaveAgentOfColony(Colony _colony)
    {
        List<AgentData> agents = new List<AgentData>();
        foreach (ColonyAgent agent in _colony.Members)
        {
            AIStats aiStats = agent.GetComponent<AIStats>();

            agents.Add(new AgentData
            {
                agentsPos = agent.transform.position,
                hunger = aiStats.hunger,
                health = aiStats.health,
                maxHealth = aiStats.maxHealth,
            });
        }

        return agents;
    }

    private List<AgentData> SaveAgentsWithoutColony()
    {
        List<AgentData> agents = new List<AgentData>();

        foreach(Transform agent in agentParent)
        {
            AIStats stats = agent.GetComponent<AIStats>();
            if (!stats) { continue; }

            BlackBoard blackboard = agent.GetComponent<TaskManager>().agentBlackboard;

            agents.Add(new AgentData
            {
                agentsPos = agent.position,
                hunger = stats.hunger,
                health = stats.health,
                maxHealth = stats.maxHealth,
            });
        }

        return agents;
    }

    //Load
    private void LoadAgentsWithoutColonies(List<AgentData> _agentsToLoad)
    {
        foreach (AgentData agentData in _agentsToLoad)
        {
            GameObject agent = Instantiate(agentPrefab, agentData.agentsPos, Quaternion.identity, agentParent);
            AIStats aiStats = agent.GetComponent<AIStats>();
            aiStats.hunger = agentData.hunger;
            aiStats.health = agentData.health;
            aiStats.maxHealth = agentData.maxHealth;
        }
    }

    private List<ColonyAgent> LoadAgentsFormColony(ColonySaveData _colonyData)
    {
        List<ColonyAgent> agents = new();

        foreach (AgentData agentData in _colonyData.agentData)
        {
            GameObject agent = Instantiate(agentPrefab, agentData.agentsPos, Quaternion.identity, agentParent);
            AIStats aiStats = agent.GetComponent<AIStats>();
            aiStats.hunger = agentData.hunger;
            aiStats.health = agentData.health;
            aiStats.maxHealth = agentData.maxHealth;
            agents.Add(agent.GetComponent<ColonyAgent>());
        }

        return agents;
    }

    #endregion

    #region Tilemap

    //save
    TilemapSave SaveTilemapData()
    {
        TilemapSave save = new TilemapSave();

        int minX = -worldGen.MapWidth() / 2;
        int maxX = worldGen.MapWidth() / 2;
        int minY = -worldGen.MapHeight() / 2;
        int maxY = worldGen.MapHeight() / 2;

        for (int x = minX; x < maxX; x++)
        {
            for (int y = minY; y < maxY; y++)
            {
                Vector3Int pos = new Vector3Int(x, y, 0);
                TileBase tile = tilemap.GetTile(pos);
                if (tile == null) continue;

                int id = Array.IndexOf(tilePalette, tile);
                if (id < 0) continue;

                TileSaveData data = new TileSaveData
                {
                    x = x,
                    y = y,
                    tileId = id
                };

                save.tiles.Add(data);
            }
        }
        return save;
    }

    //load
    void LoadTilemap(TilemapSave _data)
    {
        tilemap.ClearAllTiles();

        foreach (TileSaveData data in _data.tiles)
        {
            if (data.tileId < 0 || data.tileId >= tilePalette.Length) continue;

            Vector3Int pos = new Vector3Int(data.x, data.y, 0);
            TileBase tile = tilePalette[data.tileId];
            tilemap.SetTile(pos, tile);
        }
        InitGraph?.Invoke();

        tilemap.RefreshAllTiles();
    }
    #endregion

    #region ressources
    //save
    RessourceSave SaveRessourceData()
    {
        RessourceSave save = new RessourceSave();
        save.ress = new List<RessourceSaveData>();

        for (int i = 0; i < ressourceParent.transform.childCount; i++)
        {
            Transform child = ressourceParent.transform.GetChild(i);

            Ressource res = child.GetComponent<Ressource>();
            if (res == null) continue;

            RessourceSaveData saveData = new RessourceSaveData();
            saveData.ressourcePos = child.position;
            saveData.ressourceType = res.GetRessourceType();

            save.ress.Add(saveData);
        }
        return save;
    }

    //load
    void LoadRessource(RessourceSave _data)
    {
        for (int i = ressourceParent.transform.childCount - 1; i >= 0; i--)
        {
            Destroy(ressourceParent.transform.GetChild(i).gameObject);
        }

        foreach (RessourceSaveData save in _data.ress)
        {
            AddNewRessource.Invoke(save.ressourceType, save.ressourcePos);
        }
    }
    #endregion

    #region Weather

    //save
    private WeatherData GetCurrentWeather()
    {
        return new WeatherData{
            weatherState = (int)GetWeather.Invoke(),
            weatherTime = getRemainingTime.Invoke(),
        };
    }

    //load
    private void LoadWeather(WeatherData _weatherState)
    {
        SetWeather.Invoke((WeatherState)_weatherState.weatherState, _weatherState.weatherTime);
    }

    #endregion

    public void SendAlertSave()
    {
        saveButton.SetActive(true);
    }
}
