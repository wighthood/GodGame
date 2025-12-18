using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

[Serializable]
public struct AgentTypeEntry
{
    public SpeciesType species;
    public GameObject prefab;
}

public class ColonieSystem : MonoBehaviour, ISaveable
{

    public static Action<I_Colony> OnColonyCreatedEvent;
    public static Action<I_Colony> OnColonyDissolvedEvent;

    public static Func<int, Colony> OnRequestColonyByIDEvent;
    [Header("Conditions de création")]
    public int requiredPimusToCreate = 5;
    public float groupingRadius = 5f;

    [Header("Colony defaults")]
    public float defaultInfluenceRadius = 15f;

    [Header("Scan settings")]
    public float scanInterval = 1f;

    [SerializeField] private GameObject colonyPrefab;
    [SerializeField] private List<AgentTypeEntry> agentTypes = new List<AgentTypeEntry>();
    [SerializeField] private Transform agentParent;
    private readonly Dictionary<SpeciesType, GameObject> agentPrefabs = new Dictionary<SpeciesType, GameObject>();
    private readonly Dictionary<I_ColonyAgent, Colony> assignment = new Dictionary<I_ColonyAgent, Colony>();

    private readonly List<Colony> colonies = new List<Colony>();

    private readonly HashSet<I_ColonyAgent> knownAgents = new HashSet<I_ColonyAgent>();

    private int nextColonyId = 1;
    public Action<I_Colony, I_ColonyAgent> OnMemberJoinedEvent;
    public Action<I_Colony, I_ColonyAgent> OnMemberLeftEvent;

    private void Awake()
    {
        BuildingEvents.OnBuildingSpawnedEvent += BuildingSpawned;

        // Init Dictionary
        agentPrefabs.Clear();
        foreach (AgentTypeEntry entry in agentTypes)
        {
            if (entry.prefab != null && !agentPrefabs.ContainsKey(entry.species))
            {
                agentPrefabs.Add(entry.species, entry.prefab);
            }
        }
    }

    private void Start()
    {
        StartCoroutine(Scanning());
    }

    private void OnEnable()
    {
        ColonyEvents.OnRegisterAgentEvent += RegisterAgent;
        ColonyEvents.OnUnregisterAgentEvent += UnregisterAgent;
        SaveEvents.OnRegisterSaveableEvent?.Invoke(this);

        OnRequestColonyByIDEvent = id =>
        {
            return colonies.FirstOrDefault(c => c != null && c.Id == id);
        };
    }

    private void OnDisable()
    {
        BuildingEvents.OnBuildingSpawnedEvent -= BuildingSpawned;
        ColonyEvents.OnRegisterAgentEvent -= RegisterAgent;
        ColonyEvents.OnUnregisterAgentEvent -= UnregisterAgent;
        SaveEvents.OnUnregisterSaveableEvent?.Invoke(this);

        OnRequestColonyByIDEvent = null;
    }

    public string GetSaveID()
    {
        return "ColonieSystem";
    }

    public string CaptureState()
    {
        ColonySystemSaveData systemData = new ColonySystemSaveData();
        systemData.nextColonyId = nextColonyId;

        foreach (Colony col in colonies)
        {
            if (col == null) continue;

            ColonySaveData colData = new ColonySaveData();
            colData.id = col.Id;
            colData.name = col.name;
            colData.position = col.transform.position;
            colData.influenceRadius = col.InfluenceRadius;
            colData.maxPop = col.MaxInhabitants;
            colData.species = col.ColonySpecies.ToString();

            // Blackboard Export
            if (col.BlackBoard != null)
            {
                Dictionary<string, object> bbValues = col.BlackBoard.BbValues();
                foreach (KeyValuePair<string, object> kvp in bbValues)
                {
                    BlackboardEntry entry = new BlackboardEntry { key = kvp.Key };
                    if (kvp.Value is int iVal)
                    {
                        entry.type = "int";
                        entry.intVal = iVal;
                    }
                    else if (kvp.Value is float fVal)
                    {
                        entry.type = "float";
                        entry.floatVal = fVal;
                    }
                    else if (kvp.Value is bool bVal)
                    {
                        entry.type = "bool";
                        entry.boolVal = bVal;
                    }
                    else if (kvp.Value is string sVal)
                    {
                        entry.type = "string";
                        entry.stringVal = sVal;
                    }
                    else continue;
                    colData.blackboard.Add(entry);
                }
            }

            // Agents
            foreach (I_ColonyAgent member in col.Members)
            {
                if (member == null) continue;
                MonoBehaviour memberMono = member as MonoBehaviour;
                if (memberMono == null) continue;

                AgentSaveData agentData = new AgentSaveData();
                agentData.species = member.GetSpecies().ToString();
                agentData.position = memberMono.transform.position;

                AIStats stats = memberMono.GetComponent<AIStats>();
                if (stats != null)
                {
                    agentData.hunger = stats.GetHunger();
                    agentData.health = stats.GetHealth();
                    agentData.maxHealth = stats.maxHealth;
                }

                // Inventory
                AIInventory inventory = memberMono.GetComponent<AIInventory>();
                if (inventory != null && inventory.HasRessource())
                {
                    agentData.carriedItem = new InventoryItemData
                    {
                        type = (int)inventory.GetRessourceType(),
                        amount = (int)inventory.GetRessourceTransportedNumber(),
                    };
                }

                colData.agents.Add(agentData);
            }

            // Diplomacy
            if (col.DiplomaticRelations != null)
            {
                foreach (KeyValuePair<int, ColonyRelation> relKvp in col.DiplomaticRelations)
                {
                    colData.relations.Add(new ColonyRelationData
                    {
                        targetId = relKvp.Key,
                        opinion = relKvp.Value.Opinion,
                        state = relKvp.Value.State,
                    });
                }
            }

            systemData.colonies.Add(colData);
        }

        return JsonUtility.ToJson(systemData);
    }

    public void RestoreState(string _state)
    {
        foreach (I_ColonyAgent agent in knownAgents)
        {
            if (agent is MonoBehaviour m && m != null) Destroy(m.gameObject);
        }
        knownAgents.Clear();
        assignment.Clear();

        foreach (Colony col in colonies)
        {
            if (col != null) Destroy(col.gameObject);
        }
        colonies.Clear();

        if (string.IsNullOrEmpty(_state)) return;

        ColonySystemSaveData data = JsonUtility.FromJson<ColonySystemSaveData>(_state);
        if (data == null) return;

        nextColonyId = data.nextColonyId;

        foreach (ColonySaveData colData in data.colonies)
        {
            Colony col = Instantiate(colonyPrefab, colData.position, Quaternion.identity, transform).GetComponent<Colony>();
            col.InitColony();
            col.Id = colData.id;
            col.name = colData.name;
            col.InfluenceRadius = colData.influenceRadius;
            col.MaxInhabitants = colData.maxPop;
            col.BlackBoard.AddValueOrModify("MaxHabitant", col.MaxInhabitants);

            if (Enum.TryParse(colData.species, out SpeciesType cSpec))
            {
                col.ColonySpecies = cSpec;
            }
            else
            {
                col.ColonySpecies = SpeciesType.Pimu; // Fallback
            }

            // Restore Blackboard
            foreach (BlackboardEntry entry in colData.blackboard)
            {
                if (entry.type == "int") col.BlackBoard.AddValueOrModify(entry.key, entry.intVal);
                else if (entry.type == "float") col.BlackBoard.AddValueOrModify(entry.key, entry.floatVal);
                else if (entry.type == "bool") col.BlackBoard.AddValueOrModify(entry.key, entry.boolVal);
                else if (entry.type == "string") col.BlackBoard.AddValueOrModify(entry.key, entry.stringVal);
            }

            colonies.Add(col);
            OnColonyCreatedEvent?.Invoke(col);

            // Restore Agents
            foreach (AgentSaveData agentData in colData.agents)
            {
                // Parse Species
                if (!Enum.TryParse(agentData.species, out SpeciesType type))
                {
                    Debug.LogWarning($"ColonieSystem: Unknown species '{agentData.species}'. Defaulting to Pimu.");
                    type = SpeciesType.Pimu;
                }

                // Get Prefab
                if (!agentPrefabs.TryGetValue(type, out GameObject prefab) || prefab == null)
                {
                    Debug.LogError($"ColonieSystem: Missing prefab for species '{type}'! Cannot respawn agent.");
                    continue;
                }

                GameObject agentObj = Instantiate(prefab, agentData.position, Quaternion.identity, agentParent);
                I_ColonyAgent agent = agentObj.GetComponent<I_ColonyAgent>();

                if (agent != null)
                {
                    // Update Agent Type
                    if (agentObj.TryGetComponent(out ColonyAgent colAgent))
                    {
                        colAgent.speciesType = type;
                    }

                    // Restore Stats
                    AIStats stats = agentObj.GetComponent<AIStats>();
                    if (stats != null)
                    {
                        stats.hunger = agentData.hunger;
                        stats.SetHealth((int)agentData.health);
                        stats.maxHealth = (int)agentData.maxHealth;
                    }

                    // Restore Inventory
                    if (agentData.carriedItem.amount > 0)
                    {
                        AIInventory inventory = agentObj.GetComponent<AIInventory>();
                        if (inventory != null)
                        {
                            inventory.AddRessources((uint)agentData.carriedItem.amount, (RessourceType)agentData.carriedItem.type);
                        }
                    }

                    // Force Join logic
                    knownAgents.Add(agent);
                    assignment[agent] = col;
                    col.AddMember(agent);

                    OnMemberJoinedEvent?.Invoke(col, agent);
                }
            }

            // Restore Diplomacy
            foreach (ColonyRelationData relData in colData.relations)
            {
                ColonyRelation rel = col.GetRelationData(relData.targetId);
                rel.Opinion = relData.opinion;
                rel.State = relData.state;
            }

            // Sync Inhabitants count
            col.Inhabitants = col.Members.Count;
            col.BlackBoard.AddValueOrModify("Habitant", col.Inhabitants);
        }
    }

    private void BuildingSpawned(BuildType _type, Colony _colony)
    {
        switch (_type)
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
            //if (col.Inhabitants >= col.MaxInhabitants) continue;
            if (col.ColonySpecies != _agent.GetSpecies()) continue;

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

            OnMemberJoinedEvent?.Invoke(best, _agent);
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
            return;
        }

        int rawCount = neighbors.Count;

        neighbors = neighbors
            .Where(g => g != null && !assignment.ContainsKey(g) && g.CanFormColony() && g.GetSpecies() == _candidate.GetSpecies())
            .ToList();

        if (!neighbors.Contains(_candidate)) neighbors.Add(_candidate);

        if (neighbors.Count >= requiredPimusToCreate)
        {
            Vector3 centroid = Vector3.zero;
            foreach (I_ColonyAgent n in neighbors)
            {
                centroid += n.transform.position;
            }
            centroid /= neighbors.Count;

            List<I_ColonyAgent> group = null;
            if (ColonyEvents.OnRequestNeighborsEvent != null)
                group = ColonyEvents.OnRequestNeighborsEvent.Invoke(centroid, groupingRadius);

            if (group != null)
            {
                group = group.Where(g => g != null && !assignment.ContainsKey(g) && g.CanFormColony() && g.GetSpecies() == _candidate.GetSpecies()).ToList();

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

        if (_members.Count > 0 && _members[0] != null)
        {
            colony.ColonySpecies = _members[0].GetSpecies();
        }

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
        {
            OnMemberJoinedEvent?.Invoke(colony, m);
        }

        OnColonyCreatedEvent?.Invoke(colony);
    }

    private void RemoveAgentFromColony(I_ColonyAgent _agent, Colony _colony)
    {
        if (_agent == null || _colony == null) return;
        _colony.RemoveMember(_agent);
        _colony.Inhabitants = _colony.Members.Count;
        _colony.BlackBoard.AddValueOrModify("Habitant", _colony.Inhabitants);

        _agent.SetCurrentColony(null);
        OnMemberLeftEvent?.Invoke(_colony, _agent);
    }
}
