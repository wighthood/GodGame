using UnityEngine;
using System.Collections.Generic;


public interface IColony
{
    int GetId();
    
    Vector3 GetColonyCenter();
    
    int GetInhabitants();
   
    int GetMaxInhabitants();
    
    IReadOnlyList<IColonyAgent> GetMembers();
    
    string GetSpecies();
}

public interface IColonyAgent
{
  
    Transform GetTransform();
    
    GameObject GetGameObject();
    
    bool CanFormColony();
    
    string GetSpecies();
    
    void SetCurrentColony(IColony colony);
    
    IColony GetCurrentColony();
}
