using System.Collections.Generic;
using UnityEngine;

public class MapRessourceManager : MonoBehaviour
{
    private Dictionary<RessourceType, List<Ressource>> ressources = new Dictionary<RessourceType, List<Ressource>>();

    [SerializeField]
    private List<GameObject> ressourcePrefab = new List<GameObject>();

    private void Awake()
    {
        Ressource.OnEmptyRessource += RemoveFromListForDestroy;
        AgentActions.GetRessources += GetNearestRessource;
        MapEditorScript.AddNewRessource += AddNewRessource;
        WorldGeneration.AddNewRessource += AddNewRessource;
    }

    private void Start()
    {
        ressources[RessourceType.wood] = new List<Ressource>();
        ressources[RessourceType.food] = new List<Ressource>();
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
        List<Ressource> ressources = GetRessources(_type);

        float nearestDistance = float.MaxValue;

        Transform nearestRessource = ressources[0].transform;
        foreach (Ressource ressource in ressources)
        {
            if (ressource.GetRessourceType() == _type && Vector3.Distance(_fromEntity.position, ressource.transform.position) < nearestDistance)
            {
                nearestDistance = Vector3.Distance(_fromEntity.position, nearestRessource.transform.position);
                nearestRessource = ressource.transform;
            }
        }

        return nearestRessource;
    }

    private void RemoveFromListForDestroy(Ressource ressource)
    {
        ressources[ressource.GetRessourceType()].Remove(ressource);
        Destroy(ressource.gameObject);
    }

    private void OnDestroy()
    {
        Ressource.OnEmptyRessource -= RemoveFromListForDestroy;
        AgentActions.GetRessources -= GetNearestRessource;
        MapEditorScript.AddNewRessource -= AddNewRessource;
        WorldGeneration.AddNewRessource -= AddNewRessource;
    }
}
