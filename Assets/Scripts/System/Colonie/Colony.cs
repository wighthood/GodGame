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
    private readonly List<IColonyAgent> _members = new List<IColonyAgent>();
    public string Species;

    public BlackBoard BlackBoard { get; private set; }

    public int GetId() => Id;
    public Vector3 GetCenter() => Center;
    public int GetInhabitants() => Inhabitants;
    public int GetMaxInhabitants() => MaxInhabitants;
    public IReadOnlyList<IColonyAgent> GetMembers() => _members.AsReadOnly();
    public string GetSpecies() => Species;

    public List<IColonyAgent> Members => _members;

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
        BlackBoard.AddValueOrModify("base_max_inhabitants", BaseMaxInhabitants);
    }

    public void AddBuilding(GameObject b)
    {
        if (b == null) return;
        if (!Buildings.Contains(b)) Buildings.Add(b);
        BlackBoard.AddValueOrModify("building_count", Buildings.Count);
    }

    public void RemoveBuilding(GameObject b)
    {
        if (b == null) return;
        if (Buildings.Contains(b)) Buildings.Remove(b);
        BlackBoard.AddValueOrModify("building_count", Buildings.Count);
    }

    public GameObject GetNearestBuilding(Vector3 position, string typeFilter = null)
    {
        if (BuildingEvents.GetNearestBuilding != null)
        {
            Building b = BuildingEvents.GetNearestBuilding.Invoke(position, typeFilter);
            return b != null ? b.gameObject : null;
        }
        return null;
    }
}
