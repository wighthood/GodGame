using UnityEngine;
using UnityEngine.Tilemaps;
using System.IO;

public class TilemapSaver : MonoBehaviour
{
    public Tilemap tilemap;
    public TileBase[] tilePalette;

    public void SaveTilemap()
    {
        TilemapSave save = new TilemapSave();

        BoundsInt bounds = tilemap.cellBounds;
        foreach (Vector3Int pos in bounds.allPositionsWithin)
        {
            TileBase tile = tilemap.GetTile(pos);
            if (tile == null) continue;

            int id = System.Array.IndexOf(tilePalette, tile);
            if (id < 0)
            {
                Debug.Log(tilePalette);
                continue;
            }

            TileSaveData data = new TileSaveData
            {
                x = pos.x,
                y = pos.y,
                tileId = id
            };
            save.tiles.Add(data);
        }
        

        string json = JsonUtility.ToJson(save);
        File.WriteAllText(Application.persistentDataPath + "/tilemap.json", json);
        Debug.Log("Tilemap sauvegardée");
    }
}