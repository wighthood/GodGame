using UnityEngine;
using UnityEngine.Tilemaps;

[System.Serializable]
public class GameData
{
    public float hunger;
    public int health;
    public int maxHealth;
    
    public Vector3 cam;
    
    public Tilemap tilemap;
}

public class SaveData : MonoBehaviour
{
    public AIStats aiStats;
    public WorldGeneration worldGen;
    
    string GetPath()
    {
        return Application.persistentDataPath + "/AllData.json";
    }
    
    public void StatsToJson()
    {
        GameData stats = new GameData();
        
        stats.hunger = aiStats.hunger;
        stats.health = aiStats.health;
        stats.maxHealth = aiStats.maxHealth;
        
        stats.cam = Camera.main.transform.position;
        
        stats.tilemap = worldGen.GetTilemaps();
        
        string json = JsonUtility.ToJson(stats);
        System.IO.File.WriteAllText(GetPath(), json);
        
        Debug.Log("Données sauvegardées");
    }

    public void Save()
    {
        StatsToJson();
    }
}
