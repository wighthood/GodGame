using System.Collections.Generic;
using UnityEngine;

public class MapRessourceManager : MonoBehaviour
{
    private static MapRessourceManager instance;

    private Dictionary<RessourceType, List<Ressource>> ressources = new Dictionary<RessourceType, List<Ressource>>();

    [SerializeField]
    private List<GameObject> ressourcePrefab = new List<GameObject>();

    private void Awake()
    {
        if (instance != null)
        { 
            Destroy(gameObject);
        }

        instance = this;
    }

    private void Start()
    {
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

    public static MapRessourceManager Get() => instance;

    public List<Ressource> GetRessources(RessourceType type)
    {
        if(!ressources.ContainsKey(type))
        {
            return null;
        }
        return ressources[type];
    }

    public void RemoveFromListForDestroy(Ressource ressource)
    {
        ressources[ressource.GetRessourceType()].Remove(ressource);
    }
}
