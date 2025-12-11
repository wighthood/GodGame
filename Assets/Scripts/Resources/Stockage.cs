using System.Collections.Generic;
using UnityEngine;

public class Stockage : MonoBehaviour
{
    public int capaciteMax = 100;

    private Dictionary<RessourceType, int> ressources = new Dictionary<RessourceType, int>();

    public bool AddRessources(RessourceType type, int quantite)
    {
        if (quantite <= 0 || type == RessourceType.none) return false;

        int utilise = GetQuantiteTotale();
        if (utilise + quantite > capaciteMax) return false;

        if (ressources.ContainsKey(type)) ressources[type] += quantite;
        else ressources[type] = quantite;

        return true;
    }

    public bool DelRessources(RessourceType type, int quantite)
    {
        if (quantite <= 0 || type == RessourceType.none) return false;
        if (!ressources.ContainsKey(type)) return false;
        if (ressources[type] < quantite) return false;

        ressources[type] -= quantite;
        if (ressources[type] == 0) ressources.Remove(type);
        return true;
    }
    public int GetQuantiteTotale()
    {
        int total = 0;
        foreach (KeyValuePair<RessourceType, int> kv in ressources) total += kv.Value;
        return total;
    }

    public Dictionary<RessourceType, int> GetAllRessources()
    {
        return new Dictionary<RessourceType, int>(ressources);
    }
}
