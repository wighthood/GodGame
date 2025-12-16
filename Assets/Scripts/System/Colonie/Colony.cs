using System;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;

public class Colony : MonoBehaviour, I_Colony
{
    [Header("Core")]
    public int Id;
    public float InfluenceRadius;

    [Header("Population")]
    public int Inhabitants;
    public int MaxInhabitants;
    public int BaseMaxInhabitants; 
    private readonly List<I_ColonyAgent> members = new();
    public IReadOnlyList<I_ColonyAgent> Members => members;
    public void AddMember(I_ColonyAgent _agent)
    {
        if (!members.Contains(_agent))
        {
            members.Add(_agent);
            Inhabitants = members.Count;
            if(BlackBoard != null) BlackBoard.AddValueOrModify("Habitant", Inhabitants);
            _agent.SetCurrentColony(this);
        }
    }

    public void RemoveMember(I_ColonyAgent _agent)
    {
        if (members.Contains(_agent))
        {
            members.Remove(_agent);
            Inhabitants = members.Count;
            if(BlackBoard != null) BlackBoard.AddValueOrModify("Habitant", Inhabitants);
            _agent.SetCurrentColony(null);
        }
    }
    public List<GameObject> Buildings;
    [SerializeField] private GameObject buildParent;
    public Storage storage {  get; private set; }
    
    public BlackBoard BlackBoard { get; private set; }

    [Header("Diplomacy")]
    private Dictionary<int, ColonyRelation> diplomaticRelations = new();

    public int GetId() => Id;
    public Vector3 GetColonyCenter() => transform.position;
    public int GetInhabitants() => Inhabitants;
    public int GetMaxInhabitants() => MaxInhabitants;
    public IReadOnlyList<I_ColonyAgent> GetMembers() => members.AsReadOnly();
    public float GetInfluenceRadius() => InfluenceRadius;

    public Transform GetBuildingParent() => buildParent.transform;

    public static Func<Vector3, Vector2Int> WorldToCellPos;
    public static Func<Vector2Int, Vector3> CellToWorld;
    public static Func<Vector2Int, Cell> GetCell;

    public void InitColony()
    {
        Buildings = new List<GameObject>();
        diplomaticRelations = new Dictionary<int, ColonyRelation>();
        BlackBoard = new BlackBoard();
        BaseMaxInhabitants = 5;
        MaxInhabitants = BaseMaxInhabitants;
        BlackBoard.AddValueOrModify("MaxHabitant", MaxInhabitants);
        BlackBoard.AddValue("HasStorage", false);
        BlackBoard.AddValue("StorageTransform", null);
    }

    public void DefineStorage(Storage _storage)
    {
        if(storage != null) { return; }

        storage = _storage;
        BlackBoard.AddValueOrModify("StorageTransform", _storage.transform);
    }

    public void AddMaxPop()
    {
        MaxInhabitants += 2;
        BlackBoard.AddValueOrModify("MaxHabitant", MaxInhabitants);
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

    public ColonyRelation GetRelationData(int _otherId)
    {
        if (!diplomaticRelations.ContainsKey(_otherId))
        {
            diplomaticRelations[_otherId] = new ColonyRelation();
        }
        return diplomaticRelations[_otherId];
    }

    private void OnDrawGizmosSelected()
    {
        Gizmos.color = Color.darkRed;
        Gizmos.DrawWireSphere(transform.position, InfluenceRadius);

        GUIStyle style = new GUIStyle();
        style.normal.textColor = Color.darkRed;
        Handles.Label(transform.position + Vector3.up * (InfluenceRadius + 0.5f), $"Colony {Id}, Pop : {Inhabitants} / {MaxInhabitants}", style);
    }
}
