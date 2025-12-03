using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

#if UNITY_EDITOR
using UnityEditor;
#endif

using Agents;

// Classe représentant une colonie
public class Colony
{
    public int Id; // identifiant unique
    public Vector3 Center;
    public int Inhabitants;
    public int MaxInhabitants; // fixé à la création
    public List<GameObject> Buildings;
    public Dictionary<string, int> Resources;
    public float InfluenceRadius;
    public List<ColonyAgent> Members;
    public string Species;
}

public class ColonieSystem : MonoBehaviour
{
    [Header("Conditions de création")]
    public int requiredPimusToCreate = 5;
    public float groupingRadius = 5f;

    [Header("Colony defaults")]
    public float defaultInfluenceRadius = 15f;
    public int defaultMaxPerPimu = 2; // multiplicateur utilisé au moment de la création

    [Header("Scan settings")]
    public float scanInterval = 1f;

    // Singleton
    public static ColonieSystem Instance { get; private set; }

    // Events
    public Action<Colony> OnColonyCreated;
    public Action<Colony> OnColonyDissolved;
    public Action<Colony, ColonyAgent> OnMemberJoined;
    public Action<Colony, ColonyAgent> OnMemberLeft;

    // Internal storage
    private List<Colony> _colonies = new List<Colony>();
    private Dictionary<ColonyAgent, Colony> _assignment = new Dictionary<ColonyAgent, Colony>();

    // Spatial hash
    private HashSet<ColonyAgent> _registeredAgents = new HashSet<ColonyAgent>();
    private Dictionary<long, List<ColonyAgent>> _spatialBuckets = new Dictionary<long, List<ColonyAgent>>();
    private float _cellSize = 5f;

    // next colony id
    private int _nextColonyId = 1;

    private float _scanTimer;

    void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(this);
            return;
        }
        Instance = this;
        _cellSize = Mathf.Max(0.1f, groupingRadius);
    }

    void OnDestroy()
    {
        if (Instance == this) Instance = null;
    }

    void Start()
    {
        _scanTimer = scanInterval;
    }

    void Update()
    {
        _scanTimer += Time.deltaTime;
        if (_scanTimer >= scanInterval)
        {
            _scanTimer = 0f;
            ScanForColonies();
        }
    }

    // Public API pour les agents
    public void RegisterAgent(ColonyAgent agent)
    {
        if (agent == null) return;
        if (_registeredAgents.Contains(agent)) return;
        _registeredAgents.Add(agent);
        AddToBucket(agent);

        // Tenter immédiatement de rejoindre une colonie existante
        if (TryJoinNearestColony(agent)) return;

        // Sinon, tenter de créer une nouvelle colony à partir des voisins
        CheckNearbyForColony(agent);
    }

    public void UnregisterAgent(ColonyAgent agent)
    {
        if (agent == null) return;
        if (!_registeredAgents.Contains(agent)) return;

        _registeredAgents.Remove(agent);
        RemoveFromBucket(agent);

        if (_assignment.TryGetValue(agent, out Colony col))
        {
            _assignment.Remove(agent);
            RemoveAgentFromColony(agent, col);
            agent.SetCurrentColony(null);
        }
    }

    public void UpdateAgentCell(ColonyAgent agent, Vector3 previousPosition)
    {
        if (agent == null) return;
        RemoveFromBucket(agent, previousPosition);
        AddToBucket(agent);
    }

    // Spatial helpers
    private long GetCellKey(Vector3 pos)
    {
        int x = Mathf.FloorToInt(pos.x / _cellSize);
        int z = Mathf.FloorToInt(pos.z / _cellSize);
        return ((long)x << 32) ^ (uint)z;
    }

    private void AddToBucket(ColonyAgent a)
    {
        long key = GetCellKey(a.transform.position);
        if (!_spatialBuckets.TryGetValue(key, out List<ColonyAgent> list))
        {
            list = new List<ColonyAgent>();
            _spatialBuckets[key] = list;
        }
        if (!list.Contains(a)) list.Add(a);
    }

    private void RemoveFromBucket(ColonyAgent a)
    {
        RemoveFromBucket(a, a.transform.position);
    }

    private void RemoveFromBucket(ColonyAgent a, Vector3 fromPosition)
    {
        long key = GetCellKey(fromPosition);
        if (_spatialBuckets.TryGetValue(key, out List<ColonyAgent> list))
        {
            list.Remove(a);
            if (list.Count == 0) _spatialBuckets.Remove(key);
        }
    }

    // Scanning
    private void ScanForColonies()
    {
        foreach (ColonyAgent agent in _registeredAgents.ToList())
        {
            if (agent == null) continue;
            if (_assignment.ContainsKey(agent)) continue;

            // essayer de rejoindre une colonie existante
            if (TryJoinNearestColony(agent)) continue;

            // sinon, tenter de créer
            CheckNearbyForColony(agent);
        }
    }

    // Renvoie true si l'agent a rejoint une colony existante
    private bool TryJoinNearestColony(ColonyAgent agent)
    {
        if (agent == null) return false;
        if (_assignment.ContainsKey(agent)) return false;

        Colony best = null;
        float bestDist = float.MaxValue;
        Vector3 pos = agent.transform.position;

        foreach (Colony col in _colonies)
        {
            if (col == null) continue;
            if (!string.Equals(col.Species, agent.GetSpecies(), StringComparison.OrdinalIgnoreCase)) continue;
            if (col.Inhabitants >= col.MaxInhabitants) continue; // plein

            float d = Vector3.Distance(col.Center, pos);
            if (d <= col.InfluenceRadius && d < bestDist)
            {
                best = col;
                bestDist = d;
            }
        }

        if (best != null)
        {
            _assignment[agent] = best;
            if (best.Members == null) best.Members = new List<ColonyAgent>();
            best.Members.Add(agent);
            best.Inhabitants = best.Members.Count;

            agent.SetCurrentColony(best);
            OnMemberJoined?.Invoke(best, agent);
            Debug.Log($"Agent {agent.GetInstanceID()} joined Colony Id={best.Id} (now {best.Inhabitants}/{best.MaxInhabitants})");
            return true;
        }

        return false;
    }

    // Vérifie un groupe autour d'un agent et crée une colony si nécessaire
    private void CheckNearbyForColony(ColonyAgent candidate)
    {
        if (candidate == null) return;
        if (!candidate.canFormColony) return;

        List<ColonyAgent> neighbors = GetNeighborsFromBuckets(candidate.transform.position)
            .Where(g => g != null && !_assignment.ContainsKey(g) && g.canFormColony && string.Equals(g.GetSpecies(), candidate.GetSpecies(), StringComparison.OrdinalIgnoreCase))
            .ToList();

        if (!neighbors.Contains(candidate)) neighbors.Add(candidate);

        if (neighbors.Count >= requiredPimusToCreate)
        {
            Vector3 centroid = Vector3.zero;
            foreach (ColonyAgent n in neighbors) centroid += n.transform.position;
            centroid /= neighbors.Count;

            List<ColonyAgent> group = GetNeighborsFromBuckets(centroid)
                .Where(g => g != null && !_assignment.ContainsKey(g) && g.canFormColony && string.Equals(g.GetSpecies(), candidate.GetSpecies(), StringComparison.OrdinalIgnoreCase) && Vector3.Distance(g.transform.position, centroid) <= groupingRadius)
                .ToList();

            if (group.Count >= requiredPimusToCreate)
            {
                CreateColony(centroid, group);
            }
        }
    }

    private List<ColonyAgent> GetNeighborsFromBuckets(Vector3 position)
    {
        List<ColonyAgent> results = new List<ColonyAgent>();
        int cx = Mathf.FloorToInt(position.x / _cellSize);
        int cz = Mathf.FloorToInt(position.z / _cellSize);

        for (int dx = -1; dx <= 1; dx++)
            for (int dz = -1; dz <= 1; dz++)
            {
                int nx = cx + dx;
                int nz = cz + dz;
                long key = ((long)nx << 32) ^ (uint)nz;
                if (_spatialBuckets.TryGetValue(key, out List<ColonyAgent> list))
                {
                    foreach (ColonyAgent go in list)
                        if (go != null && !results.Contains(go)) results.Add(go);
                }
            }

        results = results.Where(g => g != null && Vector3.Distance(g.transform.position, position) <= groupingRadius).ToList();
        return results;
    }

    // Création : fixe MaxInhabitants à la création et ne le modifie plus
    private void CreateColony(Vector3 center, List<ColonyAgent> members)
    {
        members = members.Where(m => m != null && !_assignment.ContainsKey(m)).ToList();

        Colony colony = new Colony();
        colony.Id = _nextColonyId++;
        colony.Center = center;
        colony.Members = new List<ColonyAgent>();

        foreach (ColonyAgent m in members)
        {
            _assignment[m] = colony;
            colony.Members.Add(m);
            m.SetCurrentColony(colony);
        }

        colony.Inhabitants = colony.Members.Count;
        // MaxInhabitants fixé AU MOMENT DE LA CREATION et non recalculé ensuite
        int computedMax = colony.Inhabitants * defaultMaxPerPimu;
        colony.MaxInhabitants = computedMax > colony.Inhabitants ? computedMax : colony.Inhabitants;

        colony.Buildings = new List<GameObject>();
        colony.Resources = new Dictionary<string, int>();
        colony.InfluenceRadius = defaultInfluenceRadius;
        colony.Species = members.Count > 0 ? members[0].GetSpecies() : "Unknown";

        _colonies.Add(colony);

        // Fire member joined events for initial members
        foreach (ColonyAgent m in colony.Members)
            OnMemberJoined?.Invoke(colony, m);

        Debug.Log($"Colony created (Id={colony.Id}) at {center} with {colony.Inhabitants} habitants (max {colony.MaxInhabitants}) species={colony.Species}");
        OnColonyCreated?.Invoke(colony);
    }

    public Colony GetColonyForAgent(ColonyAgent agent)
    {
        if (agent == null) return null;
        _assignment.TryGetValue(agent, out Colony colony);
        return colony;
    }

    private void DissolveColony(Colony colony)
    {
        if (colony == null) return;

        List<ColonyAgent> members = colony.Members != null ? colony.Members.ToList() : new List<ColonyAgent>();
        foreach (ColonyAgent m in members)
        {
            if (m == null) continue;
            _assignment.Remove(m);
            try { m.SetCurrentColony(null); } catch { }
            OnMemberLeft?.Invoke(colony, m);
        }

        _colonies.Remove(colony);
        Debug.Log($"Colony dissolved (Id={colony.Id}) species={colony.Species}");
        OnColonyDissolved?.Invoke(colony);
    }

    private void RemoveAgentFromColony(ColonyAgent agent, Colony colony)
    {
        if (agent == null || colony == null) return;

        if (colony.Members != null && colony.Members.Contains(agent)) colony.Members.Remove(agent);

        colony.Inhabitants = colony.Members != null ? colony.Members.Count : Math.Max(0, colony.Inhabitants - 1);

        // Ne pas modifier MaxInhabitants : fixé à la création

        OnMemberLeft?.Invoke(colony, agent);

        if (colony.Inhabitants < requiredPimusToCreate)
            DissolveColony(colony);
    }

    public List<Colony> GetAllColonies() => _colonies;

    // Debug drawing
    public void DebugDrawColonies()
    {
        foreach (Colony colony in _colonies)
        {
            if (colony == null) continue;
            // wire sphere influence
            Gizmos.color = new Color(0f, 0.6f, 1f, 1f);
            Gizmos.DrawWireSphere(colony.Center, colony.InfluenceRadius);
            // center marker
            Gizmos.color = new Color(1f, 0.85f, 0f, 0.12f);
            Gizmos.DrawSphere(colony.Center, 0.12f);
        }
    }

    private void OnDrawGizmos()
    {
        if (!Application.isPlaying) DebugDrawColonies();
    }

    private void OnDrawGizmosSelected()
    {
        DebugDrawColonies();
    }
}
