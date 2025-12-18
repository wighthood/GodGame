using System;
using System.Collections.Generic;
using UnityEngine;

public class Colony : MonoBehaviour, I_Colony
{

    public static Func<Vector3, Vector2Int> WorldToCellPos;
    public static Func<Vector2Int, Vector3> CellToWorld;
    public static Func<Vector2Int, Cell> GetCell;
    [Header("Core")]
    public int Id;
    public float InfluenceRadius;
    public SpeciesType ColonySpecies;

    [Header("Population")]
    public int Inhabitants;
    public int MaxInhabitants;
    public int BaseMaxInhabitants;
    public List<GameObject> Buildings;
    [SerializeField] private GameObject buildParent;
    private readonly List<I_ColonyAgent> members = new List<I_ColonyAgent>();

    public IReadOnlyList<I_ColonyAgent> Members
    {
        get { return members; }
    }
    public Storage storage { get; private set; }

    public BlackBoard BlackBoard { get; private set; }
    [field: Header("Diplomacy")]
    public Dictionary<int, ColonyRelation> DiplomaticRelations { get; private set; } = new Dictionary<int, ColonyRelation>();

    private void OnDrawGizmosSelected()
    {
        #if unity_editor
        Gizmos.color = Color.darkRed;
        Gizmos.DrawWireSphere(transform.position, InfluenceRadius);

        GUIStyle style = new GUIStyle();
        style.normal.textColor = Color.darkRed;
        Handles.Label(transform.position + Vector3.up * (InfluenceRadius + 0.5f), $"Colony {Id}, Pop : {Inhabitants} / {MaxInhabitants}", style);
        #endif
    }

    public int GetId()
    {
        return Id;
    }
    public Vector3 GetColonyCenter()
    {
        return transform.position;
    }
    public int GetInhabitants()
    {
        return Inhabitants;
    }
    public int GetMaxInhabitants()
    {
        return MaxInhabitants;
    }
    public float GetInfluenceRadius()
    {
        return InfluenceRadius;
    }

    public ColonyRelation GetRelationData(int _otherId)
    {
        if (!DiplomaticRelations.ContainsKey(_otherId))
        {
            DiplomaticRelations[_otherId] = new ColonyRelation();
        }
        return DiplomaticRelations[_otherId];
    }
    public void AddMember(I_ColonyAgent _agent)
    {
        if (!members.Contains(_agent))
        {
            members.Add(_agent);
            Inhabitants = members.Count;
            if (BlackBoard != null) BlackBoard.AddValueOrModify("Habitant", Inhabitants);
            _agent.SetCurrentColony(this);
        }
    }

    public void RemoveMember(I_ColonyAgent _agent)
    {
        if (members.Contains(_agent))
        {
            members.Remove(_agent);
            Inhabitants = members.Count;
            if (BlackBoard != null) BlackBoard.AddValueOrModify("Habitant", Inhabitants);
            _agent.SetCurrentColony(null);
        }
    }
    public IReadOnlyList<I_ColonyAgent> GetMembers()
    {
        return members.AsReadOnly();
    }

    public Transform GetBuildingParent()
    {
        return buildParent.transform;
    }

    public void InitColony()
    {
        Buildings = new List<GameObject>();
        DiplomaticRelations = new Dictionary<int, ColonyRelation>();
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
}
