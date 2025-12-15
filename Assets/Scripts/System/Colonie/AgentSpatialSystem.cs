using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class AgentSpatialSystem : MonoBehaviour
{
    public static AgentSpatialSystem Instance { get; private set; }

    private float cellSize = 5f;
    private Dictionary<long, List<I_ColonyAgent>> spatialBuckets = new Dictionary<long, List<I_ColonyAgent>>();
    private HashSet<I_ColonyAgent> registeredAgents = new HashSet<I_ColonyAgent>();

    private void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(this);
            return;
        }
        Instance = this;
        DontDestroyOnLoad(gameObject);
    }

    private void OnEnable()
    {
        ColonyEvents.OnRegisterAgentEvent += RegisterAgent;
        ColonyEvents.OnUnregisterAgentEvent += UnregisterAgent;
        ColonyEvents.OnUpdateAgentPositionEvent += UpdateAgentPosition;
        ColonyEvents.OnRequestNeighborsEvent += GetNeighbors;
    }

    private void OnDisable()
    {
        ColonyEvents.OnRegisterAgentEvent -= RegisterAgent;
        ColonyEvents.OnUnregisterAgentEvent -= UnregisterAgent;
        ColonyEvents.OnUpdateAgentPositionEvent -= UpdateAgentPosition;
        ColonyEvents.OnRequestNeighborsEvent -= GetNeighbors;
    }

    private void RegisterAgent(I_ColonyAgent _agent)
    {
        if (_agent == null || registeredAgents.Contains(_agent)) return;
        registeredAgents.Add(_agent);
        AddToBucket(_agent);
    }

    private void UnregisterAgent(I_ColonyAgent _agent)
    {
        if (_agent == null || !registeredAgents.Contains(_agent)) return;
        registeredAgents.Remove(_agent);
        RemoveFromBucket(_agent);
    }

    private void UpdateAgentPosition(I_ColonyAgent _agent, Vector3 _previousPos)
    {
        if (_agent == null) return;
        RemoveFromBucket(_agent, _previousPos);
        AddToBucket(_agent);
    }

    private long GetCellKey(Vector3 _pos)
    {
        int x = Mathf.FloorToInt(_pos.x / cellSize);
        int z = Mathf.FloorToInt(_pos.z / cellSize);
        return ((long)x << 32) ^ (uint)z;
    }

    private void AddToBucket(I_ColonyAgent _a)
    {
        long key = GetCellKey(_a.transform.position);
        if (!spatialBuckets.TryGetValue(key, out var list))
        {
            list = new List<I_ColonyAgent>();
            spatialBuckets[key] = list;
        }
        if (!list.Contains(_a)) list.Add(_a);
    }

    private void RemoveFromBucket(I_ColonyAgent _a)
    {
        RemoveFromBucket(_a, _a.transform.position);
    }

    private void RemoveFromBucket(I_ColonyAgent _a, Vector3 _fromPos)
    {
        long key = GetCellKey(_fromPos);
        if (spatialBuckets.TryGetValue(key, out var list))
        {
            list.Remove(_a);
            if (list.Count == 0) spatialBuckets.Remove(key);
        }
    }

    public List<I_ColonyAgent> GetNeighbors(Vector3 _position, float _radius)
    {
        List<I_ColonyAgent> results = new List<I_ColonyAgent>();
        int cx = Mathf.FloorToInt(_position.x / cellSize);
        int cz = Mathf.FloorToInt(_position.z / cellSize);
        int range = Mathf.CeilToInt(_radius / cellSize);

        for (int dx = -range; dx <= range; dx++)
        {
            for (int dz = -range; dz <= range; dz++)
            {
                int nx = cx + dx;
                int nz = cz + dz;
                long key = ((long)nx << 32) ^ (uint)nz;

                if (spatialBuckets.TryGetValue(key, out var list))
                {
                    foreach (var agent in list)
                    {
                        if (agent != null && Vector3.Distance(agent.transform.position, _position) <= _radius)
                        {
                            results.Add(agent);
                        }
                    }
                }
            }
        }
        return results;
    }
}
