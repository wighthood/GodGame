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

    public bool AddRessources(int _amount, RessourceType ressource)
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

    public void RemoveOne()
    {
        ressourceStockedData.amount--;
    }
}

[System.Serializable]
public struct RessourceStockedData
{
    public RessourceType ressource;
    public int amount;
}