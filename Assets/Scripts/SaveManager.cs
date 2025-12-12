using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.Tilemaps;
using System.Collections.Generic;
using System.IO;
using System;

[Serializable]
public class AgentData
{
    public float hunger;
    public int health;
    public int maxHealth;
    public Vector3 agentsPos;
}

[Serializable]
public class GameData
{
    public Vector3 cam;
    public List<AgentData> agentData = new List<AgentData>();
}

[Serializable]
public class TileSaveData
{
    public int x;
    public int y;
    public int tileId;
}

[Serializable]
public class TilemapSave
{
    public List<TileSaveData> tiles = new List<TileSaveData>();
}

[Serializable]
public class RessourceSave
{
    public List<RessourceSaveData> ress = new List<RessourceSaveData>();
}

[Serializable]
public class RessourceSaveData
{
    public Vector3 ressourcePos;
    public RessourceType ressourceType;
}

[Serializable]
public class BlackboardSave
{
    public BlackBoard blackBoard;
}

public class SaveManager : MonoBehaviour
{
    
    public static GameData loadedStats;
    public static TilemapSave loadedTilemap;
    public static RessourceSave loadedRessource;
    public static BlackboardSave loadedBlackBoard;

    [Header("Palette commune pour la tilemap")]
    public TileBase[] tilePalette;

    string StatsPath => Application.persistentDataPath + "/AllData.json";
    string TilemapPath => Application.persistentDataPath + "/tilemap.json";
    string RessourcePath => Application.persistentDataPath + "/ressource.json";
    string BlackBoardPath => Application.persistentDataPath + "/blackboard.json";

    public void OnClickPlay()
    {
        GameModeManager.Instance.currentMode = GameModeManager.GameMode.Play;
        SceneManager.LoadScene("GameScene");
    }

    public void OnClickLoad()
    {
        GameModeManager.Instance.currentMode = GameModeManager.GameMode.Load;

        if (File.Exists(StatsPath))
        {
            string json = File.ReadAllText(StatsPath);
            loadedStats = JsonUtility.FromJson<GameData>(json);
        }
        else
        {
            loadedStats = null;
        }

        if (File.Exists(TilemapPath))
        {
            string json = File.ReadAllText(TilemapPath);
            loadedTilemap = JsonUtility.FromJson<TilemapSave>(json);
        }
        else
        {
            loadedTilemap = null;
        }
        
        if (File.Exists(RessourcePath))
        {
            string json = File.ReadAllText(RessourcePath);
            loadedRessource = JsonUtility.FromJson<RessourceSave>(json);
        }
        else
        {
            loadedRessource = null;
        }

        if (File.Exists(BlackBoardPath))
        {
            string json = File.ReadAllText(BlackBoardPath);
            loadedBlackBoard = JsonUtility.FromJson<BlackboardSave>(json);
        }
        else
        {
            loadedBlackBoard = null;
        }

        SceneManager.LoadScene("GameScene");
    }

    public static void SaveAll(GameData stats, TilemapSave tilemap, RessourceSave ressources, BlackboardSave blackboard)
    {
        string statsPath = Application.persistentDataPath + "/AllData.json";
        string tilePath  = Application.persistentDataPath + "/tilemap.json";
        string ressourcePath  = Application.persistentDataPath + "/ressource.json";
        string blackBoardPath  = Application.persistentDataPath + "/blackboard.json";

        File.WriteAllText(statsPath,  JsonUtility.ToJson(stats));
        File.WriteAllText(tilePath,   JsonUtility.ToJson(tilemap));
        File.WriteAllText(ressourcePath,   JsonUtility.ToJson(ressources));
        File.WriteAllText(blackBoardPath,   JsonUtility.ToJson(blackboard));

        Debug.LogAssertion("Sauvegarde complète effectuée");
    }
}
