using System;
using System.Collections.Generic;
using System.IO;
using UnityEngine;
using UnityEngine.Tilemaps;

[Serializable]
public class GameData
{
    public Vector3 cam;
    public Tilemap tilemap;
    public List<AgentData> agentData;
}


[Serializable]
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
    public GameObject agentPrefab;
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
        File.WriteAllText(GetPath(), json);
        Debug.Log("Données sauvegardées");
    }

    public void LoadFromJson()
    {
        string filePath = GetPath();
        if (!File.Exists(filePath))
        {
            Debug.LogWarning("Aucun fichier de sauvegarde trouvé");
            return;
        }

        string allData = File.ReadAllText(filePath);
        stats = JsonUtility.FromJson<GameData>(allData);

        Camera.main.transform.position = stats.cam;

        for (int i = agentParent.transform.childCount - 1; i >= 0; i--)
        {
            Destroy(agentParent.transform.GetChild(i).gameObject);
        }

        foreach (AgentData agentData in stats.agentData)
        {
            GameObject agent = Instantiate(agentPrefab, agentData.agentsPos, Quaternion.identity, agentParent.transform);
            AIStats aiStats = agent.GetComponent<AIStats>();

            aiStats.hunger    = agentData.hunger;
            aiStats.health    = agentData.health;
            aiStats.maxHealth = agentData.maxHealth;
        }

        Debug.Log("Données chargées");
    }
}