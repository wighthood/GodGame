using System.Collections.Generic;
using UnityEngine;

public class Storage : MonoBehaviour
{
    private readonly List<RessourceCollection> ressourcesStocked = new();

    public void AddRessources(RessourceType _type, uint _numberToAdd)
    {
        RessourceCollection ressourceCollection = GetCollectionOfType(_type);

        if (ressourceCollection == null)
        {
            ressourcesStocked.Add(new RessourceCollection { RessourceType = _type, number = _numberToAdd });
            return;
        }

        ressourceCollection.number += _numberToAdd;
    }

    public uint GetRessourceNumber(RessourceType _type)
    {
        RessourceCollection ressourceCollection = GetCollectionOfType(_type);

        if(ressourceCollection == null) {  return 0; }

        return ressourceCollection.number;
    }

    public bool HasThisRessource(RessourceType _type)
    {
        if(ressourcesStocked.Count == 0) { return false; }

        foreach (RessourceCollection ressourceCol in ressourcesStocked)
        {
            if (ressourceCol.RessourceType == _type)
            {
                return true;
            }
        }

        return false;
    }

    public RessourceCollection GetCollectionOfType(RessourceType _type)
    {
        foreach (RessourceCollection ressourceCol in ressourcesStocked)
        {
            if (ressourceCol.RessourceType == _type)
            {
                return ressourceCol;
            }
        }

        return null;
    }
    public List<RessourceCollection> GetStockedResources()
    {
        return new List<RessourceCollection>(ressourcesStocked);
    }

    public void ClearAndSetResources(List<InventoryItemData> items)
    {
        ressourcesStocked.Clear();
        if (items == null) return;

        foreach (InventoryItemData item in items)
        {
            AddRessources((RessourceType)item.type, (uint)item.amount);
        }
    }
}
