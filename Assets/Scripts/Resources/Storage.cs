using System.Collections.Generic;
using UnityEngine;

public class Storage : MonoBehaviour
{
    private readonly List<RessourceCollection> ressourcesStocked = new();

    public void AddRessources(RessourceType _type, uint _numberToAdd)
    {
        RessourceCollection ressourceCollection = getCollectionOfType(_type);

        if (ressourceCollection == null)
        {
            ressourcesStocked.Add(new RessourceCollection { RessourceType = _type, number = _numberToAdd });
            return;
        }

        ressourceCollection.number += _numberToAdd;
    }

    public uint TakeRessources(RessourceType _type)
    {
        RessourceCollection ressourceCollection = getCollectionOfType(_type);

        if(ressourceCollection == null) {  return 0; }

        return ressourceCollection.number;
    }

    public bool HasThisRessource(RessourceType _type)
    {
        foreach (RessourceCollection ressourceCol in ressourcesStocked)
        {
            if (ressourceCol.RessourceType == _type)
            {
                return true;
            }
        }

        return false;
    }

    private RessourceCollection getCollectionOfType(RessourceType _type)
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
}
