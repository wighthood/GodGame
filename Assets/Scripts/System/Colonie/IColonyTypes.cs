using UnityEngine;
using System.Collections.Generic;

public interface IColony
{
    int Id { get; }
    Vector3 Center { get; }
    int Inhabitants { get; }
    int MaxInhabitants { get; }
    IReadOnlyList<IColonyAgent> Members { get; }
    string Species { get; }
}

public interface IColonyAgent
{
    Transform transform { get; }
    GameObject gameObject { get; }
    bool CanFormColony { get; }
    string GetSpecies();
    void SetCurrentColony(IColony colony);
    IColony GetCurrentColony();
}
