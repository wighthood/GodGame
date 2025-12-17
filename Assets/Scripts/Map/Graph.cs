using System.Collections.Generic;
using System.Linq;
using UnityEditor;
using UnityEngine;
using UnityEngine.Tilemaps;

[RequireComponent(typeof(Tilemap))]
public class Graph : MonoBehaviour
{
    [SerializeField] private List<TileBase> notWalkableSprites = new();

    public List<Cell> graph { get; private set; }
    public Dictionary<Vector2Int, Cell> graphDict { get; private set; }

    public static System.Func<Graph> OnGetGraph;

    private Tilemap tilemap;

    public static Vector2Int[] directions =
    {
        Vector2Int.up,
        Vector2Int.right,
        Vector2Int.left,
        Vector2Int.down,
        new(1, 1),
        new(1, -1),
        new(-1, 1),
        new(-1, -1),
    };

    private void Awake()
    {
        MapEditorScript.GetCell += GetCellFromWorldPos;
        WorldGeneration.OnInitGraphEvent += InitGraph;
        SaveEvents.OnGraphRefreshRequestedEvent += InitGraph;
        PathFinding.GetCell += GetCell;
        PathFinding.GetCellFromWorldPos += GetCellFromWorldPos;
        PathFinding.GetCells += GetCellsFromDict;
        AgentActions.CellToWorld += CellToWorld;
        Colony.WorldToCellPos += WorldToCellPos;
        Colony.CellToWorld += CellToWorld;
        Colony.GetCell += GetCell;
        AgentActions.GetRessources += GetNearestRessouceLocation;
        MapRessourceManager.ChangeCellRessourceInfos += SetCellRessource;
    }

    private void Start()
    {
        tilemap = GetComponent<Tilemap>();
    }

    public List<Cell> GetCellsFromDict()
    {
        return graphDict.Values.ToList();
    }

    public void InitGraph()
    {
        graph = new List<Cell>();
        graphDict = new Dictionary<Vector2Int, Cell>();
        print("reset graph");

        foreach (Vector3Int pos in tilemap.cellBounds.allPositionsWithin)
        {
            if (!tilemap.HasTile(pos)) continue;

            TileBase tile = tilemap.GetTile(pos);

            bool walkable = !notWalkableSprites.Contains(tile);

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

    public Cell GetCellFromWorldPos(Vector3 _worldPos)
    {
        Vector2Int cellPos = WorldToCellPos(_worldPos);

        if (!graphDict.TryGetValue(cellPos, out Cell cell))
        {
            Debug.LogWarning($"[Graph] No cell found for world {_worldPos} -> cell {cellPos}");
            return null;
        }
        return cell;
    }

    public Cell GetCell(Vector2Int _cellPos)
    {
        graphDict.TryGetValue(_cellPos, out Cell cell);
        return cell;
    }

    private void ResetCells()
    {
        foreach (Cell cell in GetCellsFromDict())
            cell.Reset();
    }

    private Vector3? GetNearestRessouceLocation(RessourceType _ressource, Vector2 _agentPosition)
    {
        Cell start = GetCellFromWorldPos(_agentPosition);

        ResetCells();

        Queue<Cell> open = new Queue<Cell>();
        open.Enqueue(start);

        int security = 0;

        while(open.Count > 0 && security < 10000)
        {
            Cell current = open.Dequeue();
            current.inClosedSet = true;
            if (HasRessource(current, _ressource))
            {
                return CellToWorld(current.position);
            }

            AddAllNeighbors(current.position, open);
            security++;
        }

        return null;
    }

    private void AddAllNeighbors(Vector2Int _cellPosition, Queue<Cell> _cells)
    {
        foreach (Vector2Int d in directions)
        {
            Cell c = GetCell(_cellPosition + d);
            if (c != null && c.isWalkable && !c.inClosedSet)
            {
                _cells.Enqueue(c);
            }
        }
    }

    private bool HasRessource(Cell _cell, RessourceType _ressourceType)
    {
        return _cell.Ressources == (byte)_ressourceType;
    }

    private void SetCellRessource(Vector3 ressourcePosition, RessourceType _type)
    {
        Cell cell = GetCellFromWorldPos(ressourcePosition);

        cell.Ressources = (byte)_type;
    }

    private void OnDrawGizmosSelected()
    {
#if UNITY_EDITOR
        if (graphDict != null)
        {
            Vector3 size = new(0.5f, 0.5f, 0.1f);
            Gizmos.color = Color.blue;
            foreach (Cell cell in graphDict.Values)
            {
                if (cell.isWalkable)
                {
                    if(cell.Ressources > 0)
                    {
                        Gizmos.color = Color.green;
                        GUIStyle style = new GUIStyle();
                        style.normal.textColor = Color.darkGreen;
                        Handles.Label(new Vector3(cell.position.x, cell.position.y) + Vector3.up * 1f, $"ressource : {cell.Ressources} ({(RessourceType)cell.Ressources})", style);
                    }
                    else
                    {
                        Gizmos.color = Color.blue;
                    }
                    Gizmos.DrawCube(CellToWorld(cell.position), size);
                }
                else
                {
                    Gizmos.color = Color.red;
                    Gizmos.DrawCube(CellToWorld(cell.position), size);
                }
            }
        }
#endif
    }


    private void OnDestroy()
    {
        MapEditorScript.GetCell -= GetCellFromWorldPos;
        WorldGeneration.OnInitGraphEvent -= InitGraph;
        SaveEvents.OnGraphRefreshRequestedEvent -= InitGraph;
        PathFinding.GetCell -= GetCell;
        PathFinding.GetCellFromWorldPos -= GetCellFromWorldPos;
        PathFinding.GetCells -= GetCellsFromDict;
        AgentActions.CellToWorld -= CellToWorld;
        Colony.WorldToCellPos -= WorldToCellPos;
        Colony.CellToWorld -= CellToWorld;
        Colony.GetCell -= GetCell;
        AgentActions.GetRessources += GetNearestRessouceLocation;
        MapRessourceManager.ChangeCellRessourceInfos -= SetCellRessource;
    }
}
