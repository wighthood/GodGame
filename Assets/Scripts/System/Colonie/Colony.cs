using System;
using System.Collections.Generic;
using UnityEngine;

public class Colony : IColony
{
    public int Id;
    public Vector3 Center;
    public int Inhabitants;
    public int MaxInhabitants;
    public int BaseMaxInhabitants;
    public List<GameObject> Buildings;
    public Dictionary<string, int> Resources;
    public float InfluenceRadius;
    private readonly List<IColonyAgent> _members = new();
    public string Species;

    public BlackBoard BlackBoard { get; private set; }

    public int GetId() => Id;
    public Vector3 GetCenter() => Center;
    public int GetInhabitants() => Inhabitants;
    public int GetMaxInhabitants() => MaxInhabitants;
    public IReadOnlyList<IColonyAgent> GetMembers() => _members.AsReadOnly();
    public string GetSpecies() => Species;

    public List<IColonyAgent> Members => _members;

    public static event Func<Vector3, Vector2Int> WorldToCellPos;
    public static event Func<Vector2Int, Vector3> CellToWorld;
    public static event Func<Vector2Int, Cell> GetCell;

    public Colony()
    {
        Buildings = new List<GameObject>();
        Resources = new Dictionary<string, int>();
        BlackBoard = new BlackBoard();
        BaseMaxInhabitants = 0;
        MaxInhabitants = 0;
    }

    public void SetBaseMaxInhabitants(int baseMax)
    {
        BaseMaxInhabitants = Mathf.Max(0, baseMax);
        MaxInhabitants = BaseMaxInhabitants;
        BlackBoard.AddValue("base_max_inhabitants", BaseMaxInhabitants);
    }

    public void AddBuilding(GameObject building)
    {
        if (building == null) return;
        
        if (!Buildings.Contains(building)) BlackBoard.AddValue("building_count", Buildings.Count);
        else
        {           
            BlackBoard.ModifyValue("building_count", Buildings.Count);
        }
    }

    public void RemoveBuilding(GameObject b)
    {
        if (b == null) return;
        if (Buildings.Contains(b)) Buildings.Remove(b);
        BlackBoard.ModifyValue("building_count", Buildings.Count);
    }

    public GameObject GetNearestBuilding(Vector3 position)
    {
        if (BuildingEvents.GetNearestBuilding != null)
        {
            Building build = BuildingEvents.GetNearestBuilding.Invoke(position);
            return build != null ? build.gameObject : null;
        }
        return null;
    }

    public Vector3? GetValidBuildingPosition()
    {
        for (int i = 0; i < 10; i++)
        {
            Vector2 randomPoint = UnityEngine.Random.insideUnitCircle * InfluenceRadius;
            Vector3 candidatePos = Center + new Vector3(randomPoint.x, 0, randomPoint.y);
            
            Vector2Int cellPos = WorldToCellPos.Invoke(candidatePos);
            Vector3 alignedPos = CellToWorld.Invoke(cellPos);
            
            Cell cell = GetCell.Invoke(cellPos);
            if (cell == null || !cell.isWalkable) continue;
            
            if (BuildingEvents.GetNearestBuilding != null)
            {
                Building nearest = BuildingEvents.GetNearestBuilding.Invoke(alignedPos);
                if (nearest != null)
                {
                    if (Vector3.Distance(nearest.transform.position, alignedPos) < 1.0f)
                    {
                        continue;
                    }
                }
            }

            return alignedPos;
        }

        return null;
    }
}
