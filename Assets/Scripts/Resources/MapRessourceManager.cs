using System;
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

    public static event Action<Vector3, RessourceType> ChangeCellRessourceInfos;

    private void Awake()
    {
        Ressource.OnEmptyRessource += RemoveFromListForDestroy;
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

        Ressource ressource = newRessource.GetComponent<Ressource>();
        ChangeCellRessourceInfos?.Invoke(ressource.transform.position, ressource.GetRessourceType());
        AddRessourceInDictionary(ressource);
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

    private void RemoveFromListForDestroy(Ressource _ressource)
    {
        if (!ressources.ContainsKey(_ressource.GetRessourceType())) return;
        ressources[_ressource.GetRessourceType()].Remove(_ressource);
        ChangeCellRessourceInfos?.Invoke(_ressource.transform.position, RessourceType.none);
        Destroy(_ressource.gameObject);
    }

    private void OnDestroy()
    {
        Ressource.OnEmptyRessource -= RemoveFromListForDestroy;
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
                    ressourcePos = r.transform.position,
                    remainingAmount = r.GetRemaining()
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
                GameObject resObj = AddNewRessource(data.ressourceType, data.ressourcePos);
                if (resObj != null)
                {
                    Ressource r = resObj.GetComponent<Ressource>();
                    if (r != null)
                    {
                        r.SetRemaining(data.remainingAmount);
                    }
                }
            }
        }
    }

    public string GetSaveID()
    {
        return "Ressources";
    }
}


