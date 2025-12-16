using System;
using System.Collections.Generic;
using System.IO;
using UnityEngine;
using UnityEngine.SceneManagement;

[Serializable]
public class GameData
{
    public Vector3 camPos;
    public TilemapSave tilemap;
    public RessourceSave ressources;
    public List<ColonySaveData> colonies;
    public List<AgentData> noColonyAgents;
    public WeatherData weather;
}

[Serializable]
public class WeatherData
{
    public int weatherState;
    public float weatherTime;
}

[Serializable]
public class AgentData
{
    public Vector3 agentsPos;
    public float hunger;
    public int health;
    public int maxHealth;
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
public class ColonySaveData
{
    public Vector3 colonyPos;
    public List<BuildingSaveData> buildings = new List<BuildingSaveData>();
    public List<AgentData> agentData = new List<AgentData>();
}

[Serializable]
public class BuildingSaveData
{
    public Vector3 buildingPos;
    public BuildType type;
}

public class SaveManager : MonoBehaviour
{
    public static GameData loadedGameData;

    private string savePath => Application.persistentDataPath + "/Save.json";

    private void Awake()
    {
        GameSceneController.SaveGame += SaveToJSON;
    }

    private void OnDestroy()
    {
        GameSceneController.SaveGame -= SaveToJSON;
    }

    public void OnClickPlay()
    {
        GameModeManager.Instance.currentMode = GameModeManager.GameMode.Play;
        SceneManager.LoadScene("GameScene");
    }

    public void LoadAll()
    {
        GameModeManager.Instance.currentMode = GameModeManager.GameMode.Load;

        if (File.Exists(savePath))
        {
            string json = File.ReadAllText(savePath);
            loadedGameData = JsonUtility.FromJson<GameData>(json);
        }
        else
        {
            loadedGameData = null;
        }

        SceneManager.LoadScene("GameScene");
    }

    private void SaveToJSON(GameData _data)
    {
        File.WriteAllText(savePath,  JsonUtility.ToJson(_data));

        Debug.Log("Sauvegarde complète effectuée");
    }
}
