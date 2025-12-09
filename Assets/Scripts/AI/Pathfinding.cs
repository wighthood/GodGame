using System;
using System.Collections.Generic;
using UnityEngine;

public class PathFinding
{
    private List<Cell> tempNeighbors = new();
    private List<Cell> path = new();

    private int pathIndex = 0;

    private Vector2Int[] directions =
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

    public static event Func<Vector2Int, Cell> GetCell;
    public static event Func<Vector3, Cell> GetCellFromWorldPos;
    public static event Func<List<Cell>> GetCells;

    private List<Cell> GetNeighbors(Cell cell)
    {
        tempNeighbors.Clear();

        foreach (var d in directions)
        {
            Cell c = GetCell?.Invoke(cell.position + d);
            if (c != null && c.isWalkable)
            {
                tempNeighbors.Add(c);
            }
        }
        return tempNeighbors;
    }

    private int Heuristic(Cell Target, Cell start)
    {
        int dx = Mathf.Abs(Target.position.x - start.position.x);
        int dy = Mathf.Abs(Target.position.y - start.position.y);
        return 10 * (dx + dy) + (4 * Mathf.Min(dx, dy));
    }

    public Cell PeekNextPoint()
    {
        if (path == null || pathIndex >= path.Count) return null;
        return path[pathIndex];
    }

    public void AdvancePoint()
    {
        if (path == null) return;
        pathIndex = Mathf.Min(pathIndex + 1, path.Count);
    }


    public List<Cell> FindPath(Vector2 _startWorld, Vector2 _endWorld)
    {
        Cell start = GetCellFromWorldPos?.Invoke(_startWorld);
        Cell end = GetCellFromWorldPos?.Invoke(_endWorld);

        if (start == null || end == null)
            return null;

        ResetCells();
        PriorityQueue<Cell> open = new PriorityQueue<Cell>();

        start.gCost = 0;
        open.Enqueue(start, Heuristic(start, end));

        while (open.Count > 0)
        {
            Cell current = open.Dequeue();

            if (current.inClosedSet)
                continue;

            if (current == end)
            {
                return BuildPath(end);
            }

            current.inClosedSet = true;

            foreach (Cell neighbor in GetNeighbors(current))
            {
                if (neighbor.inClosedSet) continue;

                int moveCost = (neighbor.position.x != current.position.x &&
                                neighbor.position.y != current.position.y) ? 14 : 10;

                int tentativeG = current.gCost + moveCost;

                if (tentativeG < neighbor.gCost)
                {
                    neighbor.gCost = tentativeG;
                    neighbor.parent = current;
                    int f = neighbor.gCost + Heuristic(neighbor, end);

                    open.Enqueue(neighbor, f);
                }
            }
        }

        return null;
    }

    private List<Cell> BuildPath(Cell end)
    {
        path.Clear();
        Cell c = end;

        while (c != null)
        {
            path.Add(c);
            c = c.parent;
        }

        path.Reverse();
        pathIndex = 0;
        ResetCells();
        return path;
    }

    private void ResetCells()
    {
        foreach (Cell cell in GetCells?.Invoke())
            cell.Reset();
    }
}

public class Cell
{
    public Vector2Int position;
    public int gCost = int.MaxValue;
    public bool isWalkable;
    public Cell parent;
    public bool inClosedSet;

    public Cell(int x, int y, bool walkable)
    {
        position = new Vector2Int(x, y);
        isWalkable = walkable;
    }

    public void Reset()
    {
        gCost = int.MaxValue;
        parent = null;
        inClosedSet = false;
    }
}
