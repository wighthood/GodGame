using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class ColonieSystem : MonoBehaviour
{
    [Header("Conditions de création")]
    public int requiredPimusToCreate = 5;
    public float groupingRadius = 5f;

    [Header("Colony defaults")]
    public float defaultInfluenceRadius = 15f;

    [Header("Scan settings")]
    public float scanInterval = 1f;

    [SerializeField] private GameObject colonyPrefab;

    public Action<I_Colony> OnColonyCreated;
    public Action<I_Colony> OnColonyDissolved;
    public Action<I_Colony, I_ColonyAgent> OnMemberJoined;
    public Action<I_Colony, I_ColonyAgent> OnMemberLeft;

    private List<Colony> colonies = new();
    private Dictionary<I_ColonyAgent, Colony> assignment = new();
    
    private HashSet<I_ColonyAgent> knownAgents = new HashSet<I_ColonyAgent>();

    private int nextColonyId = 1;

    void Awake()
    {
        BuildingEvents.OnBuildingSpawnedEvent += BuildingSpawned;
    }

    void OnEnable()
    {
        ColonyEvents.OnRegisterAgentEvent += RegisterAgent;
        ColonyEvents.OnUnregisterAgentEvent += UnregisterAgent;
    }

    void OnDisable()
    {
        BuildingEvents.OnBuildingSpawnedEvent -= BuildingSpawned;
        ColonyEvents.OnRegisterAgentEvent -= RegisterAgent;
        ColonyEvents.OnUnregisterAgentEvent -= UnregisterAgent;
    }

    private void BuildingSpawned(BuildType _type, Colony _colony)
    {
        switch(_type)
        {
            case BuildType.House:
                _colony.AddMaxPop();
                break;
            case BuildType.Farm:
                break;
            case BuildType.Storage:
                break;
        }
    }

    void Start()
    {
        StartCoroutine(Scanning());
    }

    private IEnumerator Scanning()
    {
        WaitForSeconds wait = new WaitForSeconds(scanInterval);
        while (true)
        {
            yield return wait;
            ScanForColonies();
        }
    }

    private void RegisterAgent(I_ColonyAgent _agent)
    {
        if (_agent == null || knownAgents.Contains(_agent)) return;
        knownAgents.Add(_agent);
        
        if (TryJoinNearestColony(_agent)) return;
        CheckNearbyForColony(_agent);
    }

    private void UnregisterAgent(I_ColonyAgent _agent)
    {
        if (_agent == null || !knownAgents.Contains(_agent)) return;

        knownAgents.Remove(_agent);
        
        if (assignment.TryGetValue(_agent, out Colony col))
        {
            assignment.Remove(_agent);
            RemoveAgentFromColony(_agent, col);
            _agent.SetCurrentColony(null);
        }
    }

    private void ScanForColonies()
    {
        foreach (I_ColonyAgent agent in knownAgents.ToList())
        {
            if (agent == null) continue;
            if (assignment.ContainsKey(agent)) continue;

            if (TryJoinNearestColony(agent)) continue;

            CheckNearbyForColony(agent);
        }
    }

    private bool TryJoinNearestColony(I_ColonyAgent _agent)
    {
        if (_agent == null) return false;
        if (assignment.ContainsKey(_agent)) return false;

        Colony best = null;
        float bestDist = float.MaxValue;
        Vector3 pos = _agent.transform.position;

        foreach (Colony col in colonies)
        {
            if (col == null) continue;
            if (col.Inhabitants >= col.MaxInhabitants) continue;

            float d = Vector3.Distance(col.GetColonyCenter(), pos);
            if (d <= col.InfluenceRadius && d < bestDist)
            {
                best = col;
                bestDist = d;
            }
        }

        if (best != null)
        {
            assignment[_agent] = best;
            best.AddMember(_agent);

            OnMemberJoined?.Invoke(best, _agent);
            //Debug.Log($"Agent {_agent.gameObject.GetInstanceID()} joined Colony Id={best.Id} (now {best.Inhabitants}/{best.MaxInhabitants})");
            return true;
        }

        return false;
    }

    public bool TryForceJoinNearestColony(I_ColonyAgent _agent)
    {
        if (_agent == null) return false;
        return TryJoinNearestColony(_agent);
    }

    private void CheckNearbyForColony(I_ColonyAgent _candidate)
    {
        if (_candidate == null) return;
        if (!_candidate.CanFormColony()) return;

        List<I_ColonyAgent> neighbors = null;
        if (ColonyEvents.OnRequestNeighborsEvent != null)
        {
             neighbors = ColonyEvents.OnRequestNeighborsEvent.Invoke(_candidate.transform.position, groupingRadius);
        }
        
        if (neighbors == null) 
        {
            Debug.Log("CheckNearbyForColony: No neighbors returned (Service null or empty).");
            return;
        }

        int rawCount = neighbors.Count;
        
        neighbors = neighbors
            .Where(g => g != null && !assignment.ContainsKey(g) && g.CanFormColony() && string.Equals(g.GetSpecies(), _candidate.GetSpecies(), StringComparison.OrdinalIgnoreCase))
            .ToList();

        if (!neighbors.Contains(_candidate)) neighbors.Add(_candidate);

        //Debug.Log($"Checking Colony for {_candidate.gameObject.name}: Radius={groupingRadius}, RawNeighbors={rawCount}, Filtered={neighbors.Count}, Req={requiredPimusToCreate}");

        if (neighbors.Count >= requiredPimusToCreate)
        {
            Vector3 centroid = Vector3.zero;
            foreach (I_ColonyAgent n in neighbors) centroid += n.transform.position;
            centroid /= neighbors.Count;
            
             List<I_ColonyAgent> group = null;
             if(ColonyEvents.OnRequestNeighborsEvent != null)
                group = ColonyEvents.OnRequestNeighborsEvent.Invoke(centroid, groupingRadius);

             if (group != null)
             {
                 group = group.Where(g => g != null && !assignment.ContainsKey(g) && g.CanFormColony() && string.Equals(g.GetSpecies(), _candidate.GetSpecies(), StringComparison.OrdinalIgnoreCase)).ToList();

//                 Debug.Log($"  > Group Validation at Centroid: Count={group.Count}");

                 if (group.Count >= requiredPimusToCreate)
                 {
                     CreateColony(centroid, group);
                 }
             }
        }
    }

    private void CreateColony(Vector3 _colonySpawnPoint, List<I_ColonyAgent> _members)
    {
        _members = _members.Where(m => m != null && !assignment.ContainsKey(m)).ToList();

        Colony colony = Instantiate(colonyPrefab, _colonySpawnPoint, Quaternion.identity, transform).GetComponent<Colony>();
        colony.InitColony();
        colony.Id = nextColonyId++;

        colony.name = $"Colony {colony.Id}";

        foreach (I_ColonyAgent m in _members)
        {
            assignment[m] = colony;
            colony.AddMember(m);
            m.SetCurrentColony(colony);
        }

        colony.Inhabitants = colony.Members.Count;
        colony.BlackBoard.AddValueOrModify("Habitant", colony.Inhabitants);

        colony.InfluenceRadius = defaultInfluenceRadius;

        colonies.Add(colony);

        foreach (I_ColonyAgent m in colony.Members)
            OnMemberJoined?.Invoke(colony, m);

        OnColonyCreated?.Invoke(colony);
    }


    public I_Colony GetColonyForAgent(I_ColonyAgent _agent)
    {
        if (_agent == null) return null;
        if (assignment.TryGetValue(_agent, out Colony col)) return col;
        return null;
    }

    public List<I_Colony> GetAllColonies()
    {
        return colonies.Cast<I_Colony>().ToList();
    }

    private void RemoveAgentFromColony(I_ColonyAgent _agent, Colony _colony)
    {
        if (_agent == null || _colony == null) return;
        _colony.RemoveMember(_agent);
        _colony.Inhabitants = _colony.Members.Count;
        _colony.BlackBoard.AddValueOrModify("Habitant", _colony.Inhabitants);

        _agent.SetCurrentColony(null);
        OnMemberLeft?.Invoke(_colony, _agent);
        //Debug.Log($"Agent {_agent.gameObject.GetInstanceID()} left Colony Id={_colony.Id} (now {_colony.Inhabitants}/{_colony.MaxInhabitants})");
    }
}
