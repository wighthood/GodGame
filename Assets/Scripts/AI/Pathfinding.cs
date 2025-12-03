using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Tilemaps;
using UnityEngine.AI;

public class Pathfinding : MonoBehaviour
{
    [SerializeField] private int cellWidth = 1;
    [SerializeField] private int cellHeight = 1;
    [SerializeField] private int gridWidth = 100;
    [SerializeField] private int gridHeight = 100;

    [SerializeField] private bool newPath;
    [SerializeField] private bool visualiseGrid;
    [SerializeField] private bool showTexts;

    [SerializeField] private Transform textPrefab;
    [SerializeField] private Transform textParent;

    [SerializeField] private Transform startCell;
    [SerializeField] private Transform endCell;

    public List<Vector3> linePoints = new List<Vector3>();
    
    public Tilemap groundTilemap;

    private Dictionary<Vector2Int, Cell> cells;
    private bool gridGenerated;

    private Vector3[] navMeshCorners;

    private void Update()
    {
        if (newPath && !gridGenerated)
        {
            GenerateGrid();
            ComputeNavMeshPath();
            
            gridGenerated = true;
        }
        else if (!newPath)
        {
            gridGenerated = false;
        }
    }

    private void GenerateGrid()
    {
        cells = new Dictionary<Vector2Int, Cell>();

        for (int x = -50; x < gridWidth; x++)
        {
            for (int y = -50; y < gridHeight; y++)
            {
                Vector2Int gridPos = new Vector2Int(x, y);
                Vector3Int tileCell = new Vector3Int(x, y, 0);

                Vector3 worldPos = groundTilemap != null
                    ? groundTilemap.GetCellCenterWorld(tileCell)
                    : new Vector3(x * cellWidth, y * cellHeight, 0f);

                bool walkable = IsOnNavMesh(worldPos);

                var cell = new Cell((Vector2)worldPos)
                {
                    isWall = !walkable
                };

                cells.Add(gridPos, cell);
            }
        }
    }

    private bool IsOnNavMesh(Vector3 worldPos, float maxDistance = 0.2f)
    {
        NavMeshHit hit;
        return NavMesh.SamplePosition(worldPos, out hit, maxDistance, NavMesh.AllAreas);
    }

    private void ComputeNavMeshPath()
    {
        navMeshCorners = null;
        linePoints.Clear();

        if (startCell == null || endCell == null) return;

        NavMeshPath path = new NavMeshPath();

        NavMeshHit h1, h2;
        if (!NavMesh.SamplePosition(startCell.position, out h1, 0.3f, NavMesh.AllAreas)) return;
        if (!NavMesh.SamplePosition(endCell.position,   out h2, 0.3f, NavMesh.AllAreas)) return;

        if (!NavMesh.CalculatePath(h1.position, h2.position, NavMesh.AllAreas, path)) return;
        if (path.status != NavMeshPathStatus.PathComplete) return;

        navMeshCorners = path.corners;

        if (navMeshCorners == null || navMeshCorners.Length < 2) return;

        linePoints.AddRange(navMeshCorners);

        for (int i = 1; i < navMeshCorners.Length - 1; i++)
        {
            Vector3 prev = navMeshCorners[i]     - navMeshCorners[i - 1];
            Vector3 next = navMeshCorners[i + 1] - navMeshCorners[i];

            prev.z = 0f;
            next.z = 0f;

            if (prev.sqrMagnitude < 0.0001f || next.sqrMagnitude < 0.0001f)
                continue;

            prev.Normalize();
            next.Normalize();

            float dot = Vector3.Dot(prev, next);
            if (dot < 0.999f)
            {
                linePoints.Add(navMeshCorners[i]);
            }
        }

        linePoints.Add(navMeshCorners[navMeshCorners.Length - 1]);


    }

    private void OnDrawGizmos()
    {
        if (visualiseGrid && cells != null)
        {
            foreach (KeyValuePair<Vector2Int, Cell> kvp in cells)
            {
                Gizmos.color = kvp.Value.isWall ? Color.black : Color.white;
                float gizmoSize = showTexts ? 0.2f : 1f;

                Gizmos.DrawCube(
                    kvp.Value.position,
                    new Vector3(cellWidth, cellHeight, 0f) * gizmoSize
                );
            }
        }

        if (startCell != null)
        {
            Gizmos.color = Color.blue;
            Gizmos.DrawSphere(startCell.position, 0.2f);
        }

        if (endCell != null)
        {
            Gizmos.color = Color.red;
            Gizmos.DrawSphere(endCell.position, 0.2f);
        }

        if (navMeshCorners != null && navMeshCorners.Length > 1)
        {
            Gizmos.color = Color.green;
            for (int i = 0; i < navMeshCorners.Length - 1; i++)
            {
                Gizmos.DrawLine(navMeshCorners[i], navMeshCorners[i + 1]);
            }
        }
    }

    private class Cell
    {
        public Vector2 position;
        public bool isWall;

        public Cell(Vector2 pos)
        {
            position = pos;
        }
    }
}

