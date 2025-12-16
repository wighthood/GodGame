using System.Collections.Generic;
using UnityEngine;

public enum RessourceType
{
    none = 0,
    wood = 1,
    food = 2,
    stone = 3,
}

public class MapRessourceManager : MonoBehaviour, ISaveable
{
    private Dictionary<RessourceType, List<Ressource>> ressources = new();

    [SerializeField]
    private List<GameObject> ressourcePrefab = new();

    private void Awake()
    {
        Ressource.OnEmptyRessource += RemoveFromListForDestroy;
        AgentActions.GetRessources += GetNearestRessource;
        MapEditorScript.AddNewRessource += AddNewRessource;
        WorldGeneration.OnAddNewRessourceEvent += AddNewRessource;
    }

    private void OnEnable()
    {
        SaveEvents.OnRegisterSaveableEvent?.Invoke(this);
    }

    private void OnDisable()
    {
        SaveEvents.OnUnregisterSaveableEvent?.Invoke(this);
    }

    public GameObject AddNewRessource(RessourceType _ressourceType, Vector2 _position)
    {
        GameObject newRessource = null;
        newRessource = Instantiate(ressourcePrefab[(int)_ressourceType], _position, Quaternion.identity, transform);
        AddRessourceInDictionary(newRessource.GetComponent<Ressource>());
        return newRessource;
    }

    private void AddRessourceInDictionary(Ressource _ressource)
    {
        if (!ressources.ContainsKey(_ressource.GetRessourceType()))
        {
            ressources[_ressource.GetRessourceType()] = new List<Ressource>() { _ressource };
            return;
        }

        ressources[_ressource.GetRessourceType()].Add(_ressource);
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

        if (ressourcesList == null || ressourcesList.Count == 0) return null;

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

    private void RemoveFromListForDestroy(Ressource _ressource)
    {
        if (!ressources.ContainsKey(_ressource.GetRessourceType())) return;
        ressources[_ressource.GetRessourceType()].Remove(_ressource);
        Destroy(_ressource.gameObject);
    }

    private void OnDestroy()
    {
        Ressource.OnEmptyRessource -= RemoveFromListForDestroy;
        AgentActions.GetRessources -= GetNearestRessource;
        MapEditorScript.AddNewRessource -= AddNewRessource;
        WorldGeneration.OnAddNewRessourceEvent -= AddNewRessource;
    }

    public string CaptureState()
    {
        RessourceSave save = new RessourceSave();
        foreach (KeyValuePair<RessourceType, List<Ressource>> kvp in ressources)
        {
            foreach (Ressource r in kvp.Value)
            {
                if (r == null) continue;
                save.ress.Add(new RessourceSaveData
                {
                    ressourceType = r.GetRessourceType(),
                    ressourcePos = r.transform.position
                });
            }
        }
        return JsonUtility.ToJson(save);
    }

    public void RestoreState(string _state)
    {
        foreach (KeyValuePair<RessourceType, List<Ressource>> kvp in ressources)
        {
            foreach (Ressource r in kvp.Value)
            {
                if (r != null) Destroy(r.gameObject);
            }
        }
        ressources.Clear();
        
        if (string.IsNullOrEmpty(_state)) return;

        RessourceSave save = JsonUtility.FromJson<RessourceSave>(_state);
        if (save != null && save.ress != null)
        {
            foreach (RessourceSaveData data in save.ress)
            {
                AddNewRessource(data.ressourceType, data.ressourcePos);
            }
        }
    }

    public string GetSaveID()
    {
        return "Ressources";
    }
}


