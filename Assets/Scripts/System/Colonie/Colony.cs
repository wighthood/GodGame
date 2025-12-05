using System.Collections.Generic;
using UnityEngine;

public class Colony : IColony
{
    public int Id;
    public Vector3 Center;
    public int Inhabitants;
    public int MaxInhabitants;
    public List<GameObject> Buildings;
    public Dictionary<string, int> Resources;
    public float InfluenceRadius;

    
    private readonly List<IColonyAgent> _members = new List<IColonyAgent>();
    public string Species;

    public int GetId() => Id;
    public Vector3 GetCenter() => Center;
    public int GetInhabitants() => Inhabitants;
    public int GetMaxInhabitants() => MaxInhabitants;
    public IReadOnlyList<IColonyAgent> GetMembers() => _members.AsReadOnly();
    public string GetSpecies() => Species;

    public List<IColonyAgent> Members => _members;
}
