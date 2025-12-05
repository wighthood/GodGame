using System.Collections.Generic;
using UnityEngine;

public class Stockage : MonoBehaviour
{
    // Capacités et état
    public int capaciteMax = 100; // capacité totale (somme de toutes les ressources)

    // Durabilité / points de vie du stockage
    public int pvMax = 10;
    public int pvActuels = 10;

    // Coût en ressources pour créer le stockage (valeur indicative)
    public int coutCreation = 10;

    // Dictionnaire stockant la quantité par type de ressource
    private Dictionary<string, int> ressources = new Dictionary<string, int>();

    // Ajoute une quantité d'un type de ressource. Retourne true si tout a pu être ajouté.
    public bool AjouterRessources(string type, int quantite)
    {
        if (quantite <= 0 || string.IsNullOrEmpty(type)) return false;

        int utilise = GetQuantiteTotale();
        if (utilise + quantite > capaciteMax) return false; // capacité dépassée

        if (ressources.ContainsKey(type)) ressources[type] += quantite;
        else ressources[type] = quantite;

        return true;
    }

    // Retire une quantité d'un type de ressource. Retourne true si l'opération a réussi.
    public bool RetirerRessources(string type, int quantite)
    {
        if (quantite <= 0 || string.IsNullOrEmpty(type)) return false;
        if (!ressources.ContainsKey(type)) return false;
        if (ressources[type] < quantite) return false;

        ressources[type] -= quantite;
        if (ressources[type] == 0) ressources.Remove(type);
        return true;
    }

    // Déposer plusieurs ressources en une fois (par exemple pour désencombrer un agent).
    // items : dictionnaire type->quantité à déposer.
    // Retourne un dictionnaire des restes (ce qui n'a pas pu être déposé à cause de la capacité).
    public Dictionary<string, int> DeposerRessources(Dictionary<string, int> items)
    {
        Dictionary<string, int> reste = new Dictionary<string, int>();
        if (items == null) return reste;

        foreach (KeyValuePair<string, int> kv in items)
        {
            string type = kv.Key;
            int quantite = kv.Value;
            if (quantite <= 0 || string.IsNullOrEmpty(type)) continue;

            int espace = capaciteMax - GetQuantiteTotale();
            if (espace <= 0)
            {
                // plus d'espace, tout reste
                reste[type] = quantite + (reste.ContainsKey(type) ? reste[type] : 0);
                continue;
            }

            int aAjouter = Mathf.Min(espace, quantite);
            AjouterRessources(type, aAjouter);
            int notAdded = quantite - aAjouter;
            if (notAdded > 0) reste[type] = notAdded + (reste.ContainsKey(type) ? reste[type] : 0);
        }

        return reste;
    }

    // Récupère la quantité disponible d'un type de ressource
    public int GetQuantite(string type)
    {
        if (string.IsNullOrEmpty(type)) return 0;
        return ressources.TryGetValue(type, out int q) ? q : 0;
    }

    // Récupère la somme totale des ressources stockées
    public int GetQuantiteTotale()
    {
        int total = 0;
        foreach (KeyValuePair<string, int> kv in ressources) total += kv.Value;
        return total;
    }

    // Capacité restante
    public int GetCapaciteRestante()
    {
        return Mathf.Max(0, capaciteMax - GetQuantiteTotale());
    }

    // Récupère une copie du dictionnaire des ressources
    public Dictionary<string, int> GetToutesRessources()
    {
        return new Dictionary<string, int>(ressources);
    }

    // Répare le stockage (ajoute des PV jusqu'à pvMax)
    public void Reparer(int pv)
    {
        if (pv <= 0) return;
        pvActuels = Mathf.Min(pvMax, pvActuels + pv);
    }

    // Inflige des dégâts au stockage (réduit les PV)
    public void SubirDegats(int degats)
    {
        if (degats <= 0) return;
        pvActuels = Mathf.Max(0, pvActuels - degats);
        if (pvActuels == 0)
        {
            // Optionnel : vider le stockage à la destruction
            ressources.Clear();
            Debug.Log($"Stockage détruit : toutes les ressources perdues sur {gameObject.name}.");
        }
    }

    // Vérification rapide pour savoir si le stockage contient un type
    public bool ContientType(string type)
    {
        return ressources.ContainsKey(type);
    }

    // Vide complètement le stockage (usage de debug ou réinitialisation)
    public void Vider()
    {
        ressources.Clear();
    }
}
