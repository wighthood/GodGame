using System;
using System.Collections.Generic;
using System.Linq;
using UnityEditor;
using UnityEngine;
using UnityEngine.Tilemaps;

[RequireComponent(typeof(Tilemap))]
public class Graph : MonoBehaviour
{

    public static Func<Graph> OnGetGraph;

    public static Vector2Int[] directions =
    {
        Vector2Int.up,
        Vector2Int.right,
        Vector2Int.left,
        Vector2Int.down, new Vector2Int(1, 1), new Vector2Int(1, -1), new Vector2Int(-1, 1), new Vector2Int(-1, -1),
    };
    [SerializeField] private List<TileBase> notWalkableSprites = new List<TileBase>();

    private int currentSearchId;

    private Tilemap tilemap;

    public List<Cell> graph { get; private set; }
    public Dictionary<Vector2Int, Cell> graphDict { get; private set; }

    private void Awake()
    {
        graph = new List<Cell>();
        graphDict = new Dictionary<Vector2Int, Cell>();
    }

    private void Start()
    {
        tilemap = GetComponent<Tilemap>();
    }

    private void OnEnable()
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

    private void OnDisable()
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
        AgentActions.GetRessources -= GetNearestRessouceLocation;
        MapRessourceManager.ChangeCellRessourceInfos -= SetCellRessource;
    }

    private void OnDrawGizmosSelected()
    {
        #if UNITY_EDITOR
        if (graphDict != null)
        {
            Vector3 size = new Vector3(0.5f, 0.5f, 0.1f);
            Gizmos.color = Color.blue;
            foreach (Cell cell in graphDict.Values)
            {
                if (cell.isWalkable)
                {
                    if (cell.Ressources > 0)
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

    public List<Cell> GetCellsFromDict()
    {
        return graphDict.Values.ToList();
    }

    public void InitGraph()
    {
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

    public Vector2Int WorldToCellPos(Vector3 _worldPos)
    {
        Vector3Int cell = tilemap.WorldToCell(_worldPos);
        return new Vector2Int(cell.x, cell.y);
    }

    public Vector3 CellToWorld(Vector2Int _cellPos)
    {
        Vector3 world = tilemap.CellToWorld(new Vector3Int(_cellPos.x, _cellPos.y, 0));
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

    private Vector3? GetNearestRessouceLocation(RessourceType _ressource, Vector2 _agentPosition)
    {
        Cell start = GetCellFromWorldPos(_agentPosition);
        if (start == null) return null;

        currentSearchId++;

        Queue<Cell> open = new Queue<Cell>();
        start.visitedId = currentSearchId;
        open.Enqueue(start);

        while (open.Count > 0)
        {
            Cell current = open.Dequeue();

            if (current.Ressources == (byte)_ressource)
                return CellToWorld(current.position);

            foreach (Vector2Int d in directions)
            {
                Cell cell = GetCell(current.position + d);
                if (cell == null || !cell.isWalkable) continue;
                if (cell.visitedId == currentSearchId) continue;

                cell.visitedId = currentSearchId;
                open.Enqueue(cell);
            }
        }

        return null;
    }

    private void SetCellRessource(Vector3 ressourcePosition, RessourceType _type)
    {
        Cell cell = GetCellFromWorldPos(ressourcePosition);

        cell.Ressources = (byte)_type;
    }
}
