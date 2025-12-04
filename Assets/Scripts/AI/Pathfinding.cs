using System.Collections.Generic;
using UnityEngine;

public class PathFinding
{
    private List<Cell> tempNeighbors = new();
    private List<Cell> usedCells = new();
    private List<Cell> path = new();

    private Vector2Int[] directions =
    {
        Vector2Int.up,
        Vector2Int.right,
        Vector2Int.left,
        Vector2Int.down,
        new Vector2Int(1, 1),
        new Vector2Int(1, -1),
        new Vector2Int(-1, 1),
        new Vector2Int(-1, -1),
    };

    private List<Cell> GetNeighbors(Cell cell)
    {
        tempNeighbors.Clear();

        foreach (var d in directions)
        {
            if (Graph.instance.graphDict.TryGetValue(cell.position + d, out Cell n)
                && n.isWalkable)
            {
                tempNeighbors.Add(n);
            }
        }
        return tempNeighbors;
    }

    private int Heuristic(Cell a, Cell b)
    {
        return Mathf.Abs(a.position.x - b.position.x) +
               Mathf.Abs(a.position.y - b.position.y);
    }

    public void GoToNextPoint()
    {
        if (path.Count > 0)
            path.RemoveAt(0);
    }

    private bool IsPathValid(Cell _endPoint)
    {
        return path.Count > 0 && path.TrueForAll(c => c.isWalkable) && path[^1] == _endPoint; 
    }

    public List<Cell> FindPath(Vector2 startWorld, Vector2 endWorld)
    {
        Cell start = Graph.instance.GetCellFromWorldPos(startWorld);
        Cell end = Graph.instance.GetCellFromWorldPos(endWorld);

        if (start == null || end == null)
            return null;

        if(IsPathValid(end))
        {
            return path;
        }

        ResetUsedCells();

        PriorityQueue<Cell> open = new PriorityQueue<Cell>();

        start.gCost = 0;
        start.parent = null;
        open.Enqueue(start, Heuristic(start, end));

        while (open.Count > 0)
        {
            Cell current = open.Dequeue();

            if (current == end)
                return BuildPath(end);

            current.inClosedSet = true;

            foreach (Cell neighbor in GetNeighbors(current))
            {
                if (neighbor.inClosedSet) continue;

                int tentativeG = current.gCost + 1;

                if (tentativeG < neighbor.gCost || !open.Contains(neighbor))
                {
                    neighbor.gCost = tentativeG;
                    neighbor.parent = current;
                    int f = neighbor.gCost + Heuristic(neighbor, end);

                    open.Enqueue(neighbor, f);
                    AddToUsed(neighbor);
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
        return path;
    }

    private void AddToUsed(Cell c)
    {
        if (!usedCells.Contains(c))
            usedCells.Add(c);
    }

    private void ResetUsedCells()
    {
        foreach (Cell c in usedCells)
            c.Reset();

        usedCells.Clear();
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
