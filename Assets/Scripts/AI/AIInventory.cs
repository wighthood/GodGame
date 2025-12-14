using UnityEngine;

public class AIInventory : MonoBehaviour
{
    [SerializeField]
    private RessourceStockedData ressourceStockedData;


    public RessourceStockedData GetRessources()
    {
        return ressourceStockedData;
    }

    public RessourceType GetRessourceType()
    {
        return ressourceStockedData.ressource;
    }

    public bool HasRessource()
    {
        return !(ressourceStockedData.ressource == RessourceType.none);
    }

    public bool AddRessources(uint _amount, RessourceType ressource)
    {
        if(HasRessource() && GetRessourceType() != ressource)
        {
            return false;
        }

        if (!HasRessource())
        {
            ressourceStockedData.ressource = ressource;
            ressourceStockedData.amount += _amount;
            return true;
        }

        ressourceStockedData.amount += _amount;
        return true;
    }

    public void ResetRessource()
    {
        ressourceStockedData.amount = 0;
        ressourceStockedData.ressource = RessourceType.none;
    }

    public void RemoveRessources(uint _amount)
    {
        ressourceStockedData.amount -= _amount;

        if(ressourceStockedData.amount <= 0)
        {
            ResetRessource();
        }
    }

    public void RemoveOne()
    {
        RemoveRessources(1);
    }
}

[System.Serializable]
public struct RessourceStockedData
{
    public RessourceType ressource;
    public uint amount;
}