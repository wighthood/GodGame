using UnityEngine;
using System.Collections.Generic;

public interface I_Colony
{
    int GetId();

    Vector3 GetColonyCenter();

    int GetInhabitants();

    int GetMaxInhabitants();

    ColonyRelation GetRelationData(int _otherId);

    float GetInfluenceRadius();
}

public interface I_ColonyAgent
{
    Transform transform { get; }

    bool CanFormColony();

    SpeciesType GetSpecies();

    void SetCurrentColony(I_Colony _colony);
}
