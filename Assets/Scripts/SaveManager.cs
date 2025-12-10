using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.Tilemaps;
using System.Collections.Generic;
using System.IO;

[System.Serializable]
public class AgentData
{
    public float hunger;
    public int health;
    public int maxHealth;
    public Vector3 agentsPos;
}

[System.Serializable]
public class GameData
{
    public Vector3 cam;
    public List<AgentData> agentData = new List<AgentData>();
}

[System.Serializable]
public class TileSaveData
{
    public int x;
    public int y;
    public int tileId;
}

[System.Serializable]
public class TilemapSave
{
    public List<TileSaveData> tiles = new List<TileSaveData>();
}

public class SaveManager : MonoBehaviour
{
    [SerializeField] private GameModeManager gameModeManager;
    
    public static GameData loadedStats;
    public static TilemapSave loadedTilemap;

    [Header("Palette commune pour la tilemap")]
    public TileBase[] tilePalette;

    string StatsPath => Application.persistentDataPath + "/AllData.json";
    string TilemapPath => Application.persistentDataPath + "/tilemap.json";

    public void OnClickPlay()
    {
        gameModeManager.currentMode = GameModeManager.GameMode.Play;
        SceneManager.LoadScene("GameScene");
    }

    public void OnClickLoad()
    {
        gameModeManager.currentMode = GameModeManager.GameMode.Load;

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

        SceneManager.LoadScene("GameScene");
    }

    public static void SaveAll(GameData stats, TilemapSave tilemap)
    {
        string statsPath = Application.persistentDataPath + "/AllData.json";
        string tilePath  = Application.persistentDataPath + "/tilemap.json";

        File.WriteAllText(statsPath,  JsonUtility.ToJson(stats));
        File.WriteAllText(tilePath,   JsonUtility.ToJson(tilemap));

        Debug.Log("Sauvegarde complète effectuée");
    }
}
