using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Tilemaps;

public class TilemapManager : MonoBehaviour, ISaveable
{
    public Tilemap tilemap;
    [Tooltip("Palette of tiles used for saving/loading IDs.")]
    public TileBase[] palette;

    private void OnEnable()
    {
        SaveEvents.OnRegisterSaveableEvent?.Invoke(this);
    }

    private void OnDisable()
    {
        SaveEvents.OnUnregisterSaveableEvent?.Invoke(this);
    }

    public string CaptureState()
    {
        TilemapSave save = new TilemapSave();
        
        // BuildTilemapData logic
        foreach (Vector3Int pos in tilemap.cellBounds.allPositionsWithin)
        {
            if (tilemap.HasTile(pos))
            {
                TileBase tile = tilemap.GetTile(pos);
                int id = Array.IndexOf(palette, tile);
                
                if (id != -1)
                {
                    save.tiles.Add(new TileSaveData
                    {
                        x = pos.x,
                        y = pos.y,
                        tileId = id
                    });
                }
                else
                {
                    Debug.LogWarning($"TilemapManager: Tile at {pos} not found in palette! It will not be saved.");
                }
            }
        }

        return JsonUtility.ToJson(save);
    }

    public void RestoreState(string _state)
    {
        if (string.IsNullOrEmpty(_state)) return;

        TilemapSave save = JsonUtility.FromJson<TilemapSave>(_state);
        if (save == null) return;

        // LoadTilemap logic
        tilemap.ClearAllTiles();

        foreach (TileSaveData data in save.tiles)
        {
            if (data.tileId >= 0 && data.tileId < palette.Length)
            {
                tilemap.SetTile(new Vector3Int(data.x, data.y, 0), palette[data.tileId]);
            }
            else
            {
                 Debug.LogWarning($"TilemapManager: TileID {data.tileId} out of range (Palette size: {palette.Length}).");
            }
        }

        tilemap.RefreshAllTiles();
        SaveEvents.OnGraphRefreshRequestedEvent?.Invoke();
    }

    public string GetSaveID()
    {
        return "Tilemap";
    }
}
