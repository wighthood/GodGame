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
    }

    private void Start()
    {
        ressources[RessourceType.wood] = new List<Ressource>();
        ressources[RessourceType.food] = new List<Ressource>();
    }

    public void AddNewRessource(RessourceType ressourceType, Vector2 _position)
    {
        GameObject newRessource;
        switch (ressourceType)
        {
            case RessourceType.wood:
                newRessource = Instantiate(ressourcePrefab[0], _position, Quaternion.identity, transform);
                AddRessourceInDictionary(RessourceType.wood,
                newRessource.GetComponent<Ressource>());
                break;
            case RessourceType.food:
                newRessource = Instantiate(ressourcePrefab[1], _position, Quaternion.identity, transform);
                AddRessourceInDictionary(RessourceType.food, newRessource.GetComponent<Ressource>());
                break;
        }
    }

    private void AddRessourceInDictionary(RessourceType type, Ressource ressource)
    {
        if (!ressources.ContainsKey(type))
        {
            ressources[type] = new List<Ressource>();
        }

        ressources[type].Add(ressource);
    }

    public List<Ressource> GetRessources(RessourceType _type)
    {
        if(!ressources.ContainsKey(_type))
        {
            return null;
        }
        return ressources[_type];
    }

    public Transform GetNearestRessource(RessourceType _type)
    {
        List<Ressource> ressources = GetRessources(_type);

        float nearestDistance = float.MaxValue;

        Transform nearestRessource = ressources[0].transform;
        foreach (Ressource ressource in ressources)
        {
            if (ressource.GetRessourceType() == _type && Vector3.Distance(transform.position, ressource.transform.position) < nearestDistance)
            {
                nearestDistance = Vector3.Distance(transform.position, nearestRessource.transform.position);
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
}
