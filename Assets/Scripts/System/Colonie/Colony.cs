using System.Collections.Generic;
using UnityEngine;

// Classe représentant une colonie (implémente IColony)
public class Colony : IColony
{
    public int Id; // identifiant unique
    public Vector3 Center;
    public int Inhabitants;
    public int MaxInhabitants; // fixé à la création
    public List<GameObject> Buildings;
    public Dictionary<string, int> Resources;
    public float InfluenceRadius;
    public List<IColonyAgent> _members = new List<IColonyAgent>();
    public string Species;

    // IColony implementation
    int IColony.Id => Id;
    Vector3 IColony.Center => Center;
    int IColony.Inhabitants => Inhabitants;
    int IColony.MaxInhabitants => MaxInhabitants;
    IReadOnlyList<IColonyAgent> IColony.Members => _members.AsReadOnly();
    string IColony.Species => Species;

    // helper to work with concrete members internally
    public List<IColonyAgent> Members => _members;
}
