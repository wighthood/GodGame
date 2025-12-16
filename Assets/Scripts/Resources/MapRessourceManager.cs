using System.Collections.Generic;
using UnityEngine;

public enum RessourceType
{
    none = 0,
    wood = 1,
    food = 2,
    stone = 3,
}

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
        GameSceneController.AddNewRessource += AddNewRessource;
        Farm.spawnFood += SpawnFoodOnFarm;
    }

    public GameObject AddNewRessource(RessourceType _ressourceType, Vector2 _position)
    {
        GameObject newRessource = null;
        newRessource = Instantiate(ressourcePrefab[(int)_ressourceType], _position, Quaternion.identity, transform);
        AddRessourceInDictionary(newRessource.GetComponent<Ressource>());
        return newRessource;
    }

    private GameObject SpawnFoodOnFarm(Vector3 _position)
    {
        GameObject spawnFood = null;
        spawnFood = Instantiate(ressourcePrefab[^1], _position, Quaternion.identity, transform);
        AddRessourceInDictionary(spawnFood.GetComponent<Ressource>());
        return spawnFood;
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

        float nearestDistance = float.MaxValue;

        Transform nearestRessource = ressourcesList[0].transform;
        foreach (Ressource ressource in ressourcesList)
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
        if (ressource == null || !ressources.ContainsKey(ressource.GetRessourceType())) return;

        ressources[ressource.GetRessourceType()].Remove(ressource);

        if(ressource == null) { return; }

        Destroy(ressource.gameObject);
    }

    private void OnDestroy()
    {
        Ressource.OnEmptyRessource -= RemoveFromListForDestroy;
        AgentActions.GetRessources -= GetNearestRessource;
        MapEditorScript.AddNewRessource -= AddNewRessource;
        WorldGeneration.AddNewRessource -= AddNewRessource;
        GameSceneController.AddNewRessource -= AddNewRessource;
        Farm.spawnFood -= SpawnFoodOnFarm;
    }
}
