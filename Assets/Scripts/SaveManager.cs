using System;
using UnityEngine;
using UnityEngine.SceneManagement;
using System.Collections.Generic;
using System.IO;
using UnityEngine.Tilemaps;

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
    public static GameData LoadedData;
    public static TilemapSave loadedTilemap;
    public static SaveData saveData;
    
    public TileBase[] tilePalette;
    public static SaveManager instance;

    private void Awake()
    {
        instance = this;
    }


    string GetStatsPath()
    {
        return Application.persistentDataPath + "/AllData.json";
    }
    
    string GetTilemapPath()
    {
        return Application.persistentDataPath + "/tilemap.json";
    }
    
    public void SaveAll(UnityEngine.Tilemaps.Tilemap tilemap)
    {
        SaveStats();
        SaveTilemap(tilemap);
    }
    
    void SaveStats()
    {
        saveData.SaveToJson();
    }
    
    void SaveTilemap(UnityEngine.Tilemaps.Tilemap tilemap)
    {
        TilemapSave save = new TilemapSave();

        BoundsInt bounds = tilemap.cellBounds;
        foreach (Vector3Int pos in bounds.allPositionsWithin)
        {
            TileBase tile = tilemap.GetTile(pos);
            if (tile == null) continue;

            int id = System.Array.IndexOf(tilePalette, tile);
            if (id < 0) continue;

            TileSaveData data = new TileSaveData
            {
                x = pos.x,
                y = pos.y,
                tileId = id
            };
            save.tiles.Add(data);
        }

        string json = JsonUtility.ToJson(save);
        File.WriteAllText(GetTilemapPath(), json);
        Debug.Log("Tilemap sauvegardée");
    }

    public void LoadGame()
    {
        string statsPath = GetStatsPath();
        if (File.Exists(statsPath))
        {
            string allData = File.ReadAllText(statsPath);
            LoadedData = JsonUtility.FromJson<GameData>(allData);
        }

        string tilePath = GetTilemapPath();
        if (File.Exists(tilePath))
        {
            string json = File.ReadAllText(tilePath);
            loadedTilemap = JsonUtility.FromJson<TilemapSave>(json);
        }

        UnityEngine.SceneManagement.SceneManager.LoadScene("SaveSystem");
    }
}