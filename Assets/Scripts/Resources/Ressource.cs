using System;
using UnityEngine;

public class Ressource : MonoBehaviour
{
    [SerializeField] private SO_Ressource ressource;
    [SerializeField] private int ressourceRemaining;
    
    public static event Action<Ressource> OnEmptyRessource;
    public RessourceType GetRessourceType()
    {
        return ressource.ressourceType;
    }

    public void OnHarvrestingRessource()
    {
        ressourceRemaining--;

        if (ressourceRemaining == 0)
        {
            OnEmptyRessource?.Invoke(this);
        }
    }
}
