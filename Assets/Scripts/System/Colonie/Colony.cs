using System;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;
using Random = UnityEngine.Random;

public class Colony : MonoBehaviour, IColony
{
    public int Id;
    public int Inhabitants;
    public int MaxInhabitants;
    public int BaseMaxInhabitants;
    public List<GameObject> Buildings;
    [SerializeField] private GameObject buildParent;
    public Dictionary<RessourceType, int> Resources;
    public float InfluenceRadius;
    private readonly List<ColonyAgent> _members = new List<ColonyAgent>();

    public Storage storage { get; private set; }

    public BlackBoard BlackBoard { get; private set; }

    public int GetId() => Id;
    public Vector3 GetColonyCenter() => transform.position;
    public int GetInhabitants() => Inhabitants;
    public int GetMaxInhabitants() => MaxInhabitants;
    public IReadOnlyList<IColonyAgent> GetMembers() => _members;
    public List<ColonyAgent> Members => _members;

    public Transform GetBuildingParent() => buildParent.transform;

    public static event Func<Vector3, Vector2Int> WorldToCellPos;
    public static event Func<Vector2Int, Vector3> CellToWorld;
    public static event Func<Vector2Int, Cell> GetCell;

    public void InitColony()
    {
        Buildings = new List<GameObject>();
        Resources = new Dictionary<RessourceType, int>();
        BlackBoard = new BlackBoard();
        BaseMaxInhabitants = 5;
        MaxInhabitants = BaseMaxInhabitants;
        BlackBoard.AddValueOrModify("MaxHabitant", MaxInhabitants);
        BlackBoard.AddValue("HasStorage", false);
        BlackBoard.AddValue("StorageTransform", null);
    }

    public void DefineStorage(Storage _storage)
    {
        if (storage != null) { return; }

        storage = _storage;
        BlackBoard.AddValueOrModify("StorageTransform", _storage.transform);
    }

    public void AddMaxPop()
    {
        MaxInhabitants += 2;
        BlackBoard.AddValueOrModify("MaxHabitant", MaxInhabitants);
    }

    public void AddAgentToColony(ColonyAgent _newAgent)
    {
        _members.Add(_newAgent);
        _newAgent.SetCurrentColony(this);
        Inhabitants++;
        BlackBoard.AddValueOrModify("Habitant", Inhabitants);
    }

    public void AddBuilding(GameObject b)
    {
        if (b == null) return;
        if (!Buildings.Contains(b)) Buildings.Add(b);
        BlackBoard.AddValueOrModify("BuildingCount", Buildings.Count);
    }

    public void RemoveBuilding(GameObject b)
    {
        if (b == null) return;
        if (Buildings.Contains(b)) Buildings.Remove(b);
        BlackBoard.AddValueOrModify("BuildingCount", Buildings.Count);
    }

    public int GetAllBuildingOfType(BuildType _buildType)
    {
        int count = 0;
        foreach(GameObject building in Buildings)
        {
            if(building.GetComponent<Building>().Type == _buildType)
            {
                count++;
            }
        }

        return count;
    }

    public GameObject GetNearestBuilding(Vector3 position, BuildType typeFilter)
    {
        if (BuildingEvents.GetNearestBuilding != null)
        {
            Building b = BuildingEvents.GetNearestBuilding.Invoke(position);
            return b != null ? b.gameObject : null;
        }
        return null;
    }

    public Vector3? GetValidBuildingPosition()
    {
        for (int i = 0; i < 100; i++)
        {
            // Pick a random point
            Vector2 randomPoint = Random.insideUnitCircle * InfluenceRadius;
            Vector3 candidatePos = GetColonyCenter() + (Vector3)randomPoint;

            // Align to grid
            Vector2Int cellPos = WorldToCellPos.Invoke(candidatePos);
            Vector3 alignedPos = CellToWorld.Invoke(cellPos);

            // Check if walkable
            Cell cell = GetCell.Invoke(cellPos);
            if (cell == null || !cell.isWalkable) continue;

            // Check if occupied by another building
            if (BuildingEvents.GetNearestBuilding != null)
            {
                Building nearest = BuildingEvents.GetNearestBuilding.Invoke(alignedPos);
                if (nearest != null)
                {
                    // If a building is too close consider it occupied
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

#if UNITY_EDITOR
    private void OnDrawGizmosSelected()
    {
        Gizmos.color = Color.darkRed;
        Gizmos.DrawWireSphere(transform.position, InfluenceRadius);

        GUIStyle style = new GUIStyle();
        style.normal.textColor = Color.darkRed;
        Handles.Label(transform.position + Vector3.up * (InfluenceRadius + 0.5f), $"Colony {Id}, Pop : {Inhabitants} / {MaxInhabitants}", style);
    }

#endif
}
