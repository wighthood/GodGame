using System.Collections.Generic;
using UnityEngine;

public enum RessourceType
{
    none = 0,
    wood = 1,
    food = 2,
}

public class MapRessourceManager : MonoBehaviour
{
    private Dictionary<RessourceType, List<Ressource>> ressources = new Dictionary<RessourceType, List<Ressource>>();

    [SerializeField]
    private List<GameObject> ressourcePrefab = new List<GameObject>();

    private void Awake()
    {
        Ressource.OnEmptyRessource += RemoveFromListForDestroy;
        AgentActions.GetRessources += GetRessources;
        MapEditorScript.AddNewRessource += AddNewRessource;
        WorldGeneration.AddNewRessource += AddNewRessource;
    }

    public GameObject AddNewRessource(RessourceType ressourceType, Vector2 _position)
    {
        GameObject newRessource = null;
        switch (ressourceType)
        {
            case RessourceType.wood:
                newRessource = Instantiate(ressourcePrefab[0], _position, Quaternion.identity, transform);
                AddRessourceInDictionary(newRessource.GetComponent<Ressource>());
                break;
            case RessourceType.food:
                newRessource = Instantiate(ressourcePrefab[1], _position, Quaternion.identity, transform);
                AddRessourceInDictionary(newRessource.GetComponent<Ressource>());
                break;
        }

        return newRessource;
    }

    private void AddRessourceInDictionary(Ressource ressource)
    {
        if (!ressources.ContainsKey(ressource.GetRessourceType()))
        {
            ressources[ressource.GetRessourceType()] = new List<Ressource>() { ressource };
            return;
        }

        ressources[ressource.GetRessourceType()].Add(ressource);
    }

    public List<Ressource> GetRessources(RessourceType _type)
    {
        if(!ressources.ContainsKey(_type))
        {
            return null;
        }
        return ressources[_type];
    }

    public Transform GetNearestRessource(RessourceType _type, Transform _fromEntity)
    {
        List<Ressource> ressourcesList = GetRessources(_type);
        if (ressourcesList == null || ressourcesList.Count == 0 || _fromEntity == null) return null;

        float bestDist = float.MaxValue;
        Transform bestTransform = null;

        foreach (Ressource ressource in ressourcesList)
        {
            if (ressource == null) continue;
            float d = Vector3.Distance(_fromEntity.position, ressource.transform.position);
            if (d < bestDist)
            {
                bestDist = d;
                bestTransform = ressource.transform;
            }
        }

        return bestTransform;
    }

    private void RemoveFromListForDestroy(Ressource ressource)
    {
        if (!ressources.ContainsKey(ressource.GetRessourceType())) return;
        ressources[ressource.GetRessourceType()].Remove(ressource);
        Destroy(ressource.gameObject);
    }

    private void OnDestroy()
    {
        Ressource.OnEmptyRessource -= RemoveFromListForDestroy;
        AgentActions.GetRessources -= GetRessources;
        MapEditorScript.AddNewRessource -= AddNewRessource;
        WorldGeneration.AddNewRessource -= AddNewRessource;
    }
}
