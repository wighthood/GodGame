using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Tilemaps;
using System;

public class GameSceneController : MonoBehaviour
{
    [Header("Réfs scène de jeu")]
    public WorldGeneration worldGen;
    public Tilemap tilemap;
    public GameObject agentPrefab;
    [SerializeField] private GameObject agentParent;
    
    [Header("Réfs ressources")]
    [SerializeField] private GameObject ressourceParent;
    [SerializeField] private List<GameObject> ressourcePrefab;
    
    [Header("Réfs IA / Blackboard")]
    public BlackBoard existingBlackboard;
    
    public static event Func<RessourceType, Vector2, GameObject> AddNewRessource;
    public static event Action InitGraph;

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
            LoadStatsAndAgents();
            LoadTilemap();
            LoadRessource();
            LoadBlackBoard();
        }
    }

    public void OnClickSave()
    {
        GameData stats = BuildStatsData();
        TilemapSave tData = BuildTilemapData();
        RessourceSave rData = BuildRessourceData();
        BlackboardSave bbData = BuildBlackBoard();

        SaveManager.SaveAll(stats, tData, rData, bbData);
    }

    GameData BuildStatsData()
    {
        GameData data = new GameData();
        data.cam = Camera.main.transform.position;
        data.agentData = new List<AgentData>();

        for (int i = 0; i < agentParent.transform.childCount; i++)
        {
            Transform child = agentParent.transform.GetChild(i);
            AIStats aiStats = child.GetComponent<AIStats>();

            AgentData a = new AgentData();
            a.hunger    = aiStats.hunger;
            a.health    = aiStats.health;
            a.maxHealth = aiStats.maxHealth;
            a.agentsPos = child.position;

            data.agentData.Add(a);
        }
        return data;
    }

    TilemapSave BuildTilemapData()
    {
        TilemapSave save = new TilemapSave();

        int minX = -worldGen.MapWidth() / 2;
        int maxX =  worldGen.MapWidth() / 2;
        int minY = -worldGen.MapHeight() / 2;
        int maxY =  worldGen.MapHeight() / 2;

        SaveManager menu = FindObjectOfType<SaveManager>();
        TileBase[] palette = menu.tilePalette;

        for (int x = minX; x < maxX; x++)
        {
            for (int y = minY; y < maxY; y++)
            {
                Vector3Int pos = new Vector3Int(x, y, 0);
                TileBase tile = tilemap.GetTile(pos);
                if (tile == null) continue;

                int id = System.Array.IndexOf(palette, tile);
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

    RessourceSave BuildRessourceData()
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

    BlackboardSave BuildBlackBoard()
    {
        BlackboardSave bbSave = new BlackboardSave();
        
        bbSave.blackBoard = existingBlackboard;
        
        return bbSave;
    }

    void LoadStatsAndAgents()
    {
        GameData data = SaveManager.loadedStats;
        if (data == null)
        {
            Debug.LogWarning("Pas de GameData, nouvelle partie");
            return;
        }

        Camera.main.transform.position = data.cam;

        for (int i = agentParent.transform.childCount - 1; i >= 0; i--)
        {
            Destroy(agentParent.transform.GetChild(i).gameObject);
        }

        foreach (AgentData agentData in data.agentData)
        {
            GameObject agent = Instantiate(agentPrefab, agentData.agentsPos, Quaternion.identity, agentParent.transform);
            AIStats aiStats = agent.GetComponent<AIStats>();
            aiStats.hunger    = agentData.hunger;
            aiStats.health    = agentData.health;
            aiStats.maxHealth = agentData.maxHealth;
        }
        
        Debug.Log("Stats et agents bien chargés");
    }

    void LoadTilemap()
    {
        TilemapSave tData = SaveManager.loadedTilemap;
        if (tData == null)
        {
            Debug.LogWarning("Pas de TilemapSave, on garde la tilemap par défaut");
            return;
        }

        tilemap.ClearAllTiles();

        SaveManager menu = FindObjectOfType<SaveManager>();
        TileBase[] palette = menu.tilePalette;

        foreach (TileSaveData data in tData.tiles)
        {
            if (data.tileId < 0 || data.tileId >= palette.Length) continue;

            Vector3Int pos = new Vector3Int(data.x, data.y, 0);
            TileBase tile = palette[data.tileId];
            tilemap.SetTile(pos, tile);
        }
        InitGraph?.Invoke();

        tilemap.RefreshAllTiles();
        Debug.Log("Tilemap chargée");
    }

    void LoadRessource()
    {
        RessourceSave rData = SaveManager.loadedRessource;
        
        if (rData == null)
        {
            Debug.LogWarning("Pas de ressources");
            return;
        }
        
        for (int i = ressourceParent.transform.childCount - 1; i >= 0; i--)
        {
            Destroy(ressourceParent.transform.GetChild(i).gameObject);
        }

        foreach (RessourceSaveData save in rData.ress)
        {
            AddNewRessource.Invoke(save.ressourceType, save.ressourcePos);
        }
        
        Debug.Log("Ressource bien chargée(s)");
    }

    void LoadBlackBoard()
    {
        BlackboardSave bbSave = SaveManager.loadedBlackBoard;

        if (bbSave == null || bbSave.blackBoard == null)
        {
            Debug.LogWarning("Pas de blackboard");
            return;
        }

        existingBlackboard = bbSave.blackBoard;

        Debug.Log("BlackBoard bien chargé");
    }
}
