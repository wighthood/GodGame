using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Tilemaps;

[RequireComponent(typeof(Tilemap))]
public class Graph : MonoBehaviour
{
    public static Graph instance;

    [SerializeField] private List<Sprite> notWalkableSprites = new();

    public List<Cell> graph { get; private set; }
    public Dictionary<Vector2Int, Cell> graphDict { get; private set; }

    private Tilemap tilemap;

    private void Awake()
    {
        if (instance != null)
        {
            Destroy(gameObject);
            return;
        }

        instance = this;
        tilemap = GetComponent<Tilemap>();
    }

    public void InitGraph()
    {
        graph = new List<Cell>();
        graphDict = new Dictionary<Vector2Int, Cell>();

        foreach (Vector3Int pos in tilemap.cellBounds.allPositionsWithin)
        {
            if (!tilemap.HasTile(pos)) continue;

            TileBase tile = tilemap.GetTile(pos);
            TileData tileData = new TileData();
            tile.GetTileData(pos, tilemap, ref tileData);

            bool walkable = !notWalkableSprites.Contains(tileData.sprite);

            Cell cell = new Cell(pos.x, pos.y, walkable);
            graph.Add(cell);
            graphDict[new Vector2Int(pos.x, pos.y)] = cell;
        }
    }

    public Vector2Int WorldToCellPos(Vector3 worldPos)
    {
        Vector3Int cell = tilemap.WorldToCell(worldPos);
        return new Vector2Int(cell.x, cell.y);
    }

    public Vector3 CellToWorld(Vector2Int cellPos)
    {
        Vector3 world = tilemap.CellToWorld(new Vector3Int(cellPos.x, cellPos.y, 0));
        return world + new Vector3(0.5f, 0.5f, 0f);
    }

    public Cell GetCellFromWorldPos(Vector3 worldPos)
    {
        Vector2Int cellPos = WorldToCellPos(worldPos);

        if (!graphDict.TryGetValue(cellPos, out Cell cell))
        {
            Debug.LogWarning($"[Graph] No cell found for world {worldPos} -> cell {cellPos}");
            return null;
        }

        return cell;
    }
}
