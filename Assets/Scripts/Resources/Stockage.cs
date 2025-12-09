using System.Collections.Generic;
using UnityEngine;

public class Stockage : MonoBehaviour
{
    public int capaciteMax = 100;

    public int pvMax = 10;
    public int pvActuels = 10;

    public int coutCreation = 10;

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

    public Dictionary<RessourceType, int> DeposerRessources(Dictionary<RessourceType, int> items)
    {
        Dictionary<RessourceType, int> reste = new Dictionary<RessourceType, int>();
        if (items == null) return reste;

        foreach (KeyValuePair<RessourceType, int> kv in items)
        {
            RessourceType type = kv.Key;
            int quantite = kv.Value;
            if (quantite <= 0 || type == RessourceType.none) continue;

            int espace = capaciteMax - GetQuantiteTotale();
            if (espace <= 0)
            {
                reste[type] = quantite + (reste.ContainsKey(type) ? reste[type] : 0);
                continue;
            }

            int aAjouter = Mathf.Min(espace, quantite);
            AddRessources(type, aAjouter);
            int notAdded = quantite - aAjouter;
            if (notAdded > 0) reste[type] = notAdded + (reste.ContainsKey(type) ? reste[type] : 0);
        }

        return reste;
    }

    public int GetQuantite(RessourceType type)
    {
        if (type == RessourceType.none) return 0;
        return ressources.TryGetValue(type, out int q) ? q : 0;
    }

    public int GetQuantiteTotale()
    {
        int total = 0;
        foreach (KeyValuePair<RessourceType, int> kv in ressources) total += kv.Value;
        return total;
    }

    public int GetMaxCapacity()
    {
        return Mathf.Max(0, capaciteMax - GetQuantiteTotale());
    }

    public Dictionary<RessourceType, int> GetAllRessources()
    {
        return new Dictionary<RessourceType, int>(ressources);
    }

    public void Clear()
    {
        ressources.Clear();
    }
}
