using System;
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
        AgentActions.GetRessources += GetRessources;
    }

    private void Start()
    {
        ressources[RessourceType.wood] = new List<Ressource>();
        ressources[RessourceType.food] = new List<Ressource>();

        //TODO remove this v
        AddNewRessource(1, new Vector2(0, 10));
        AddNewRessource(1, new Vector2(-10, 10));
        AddNewRessource(1, new Vector2(8, 10));
        AddNewRessource(1, new Vector2(-3, 5));
    }

    public void AddNewRessource(int _ressourceIndex, Vector2 _position)
    {
        GameObject newRessource = Instantiate(ressourcePrefab[_ressourceIndex], _position, Quaternion.identity, transform);
        switch(_ressourceIndex)
        {
            case 0:
                AddRessourceInDictionary(RessourceType.wood, newRessource.GetComponent<Ressource>());
                break;
            case 1:
                AddRessourceInDictionary(RessourceType.food, newRessource.GetComponent<Ressource>());
                break;
        }
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

    private void RemoveFromListForDestroy(Ressource ressource)
    {
        ressources[ressource.GetRessourceType()].Remove(ressource);
        Destroy(ressource.gameObject);
    }
}
