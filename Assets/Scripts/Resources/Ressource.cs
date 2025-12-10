using System;
using UnityEngine;

public class Ressource : MonoBehaviour
{
    [SerializeField] private RessourceType ressourceType = RessourceType.none;
    [SerializeField] private int ressourceRemaining = 1;

    public static event Action<Ressource> OnEmptyRessource;

    public RessourceType GetRessourceType()
    {
        return ressourceType;
    }

    public void OnHarvrestingRessource()
    {
        ressourceRemaining--;

        if (ressourceRemaining <= 0)
        {
            OnEmptyRessource?.Invoke(this);
        }
    }
}
