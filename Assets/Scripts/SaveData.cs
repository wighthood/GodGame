using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.Tilemaps;

[System.Serializable]
public class GameData
{
    public Vector3 cam;
    public Tilemap tilemap;
    public List<AgentData> agentData;
}

[System.Serializable]
public class AgentData
{
    public float hunger;
    public int health;
    public int maxHealth;
    public Vector3 agentsPos;
}

public class SaveData : MonoBehaviour
{
    public WorldGeneration worldGen;   
    [SerializeField] private GameObject agentParent;
    GameData stats = new GameData();

    string GetPath()
    {
        return Application.persistentDataPath + "/AllData.json";
    }

    public void SaveToJson()
    {

        stats.cam = Camera.main.transform.position;
        stats.tilemap = worldGen.GetTilemaps();
        stats.agentData = new List<AgentData>();
        
        for (int i = 0; i < agentParent.transform.childCount; i++)
        {
            Transform child = agentParent.transform.GetChild(i);
            AIStats aiStats = child.GetComponent<AIStats>();

            AgentData agentData = new AgentData();
            agentData.hunger    = aiStats.hunger;
            agentData.health    = aiStats.health;
            agentData.maxHealth = aiStats.maxHealth;

            agentData.agentsPos = child.position;

            stats.agentData.Add(agentData);
        }

        string json = JsonUtility.ToJson(stats);
        System.IO.File.WriteAllText(GetPath(), json);
        Debug.Log("Données sauvegardées");
    }

    public void LoadFromJson()
    {
        string filePath = Application.persistentDataPath + "/AllData.json";
        string allData = System.IO.File.ReadAllText(filePath);
        
        stats = JsonUtility.FromJson<GameData>(allData);
        SceneManager.LoadScene("GameScene");
        // gameObject.GetComponent<WorldGeneration>().enabled = false;
    }
}