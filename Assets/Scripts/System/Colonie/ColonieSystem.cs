using System;
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
    public int defaultMaxPerPimu = 2; // multiplicateur utilisé au moment de la création

    [Header("Scan settings")]
    public float scanInterval = 1f;

    // Singleton
    public static ColonieSystem Instance { get; private set; }

    // Events
    public Action<IColony> OnColonyCreated;
    public Action<IColony> OnColonyDissolved;
    public Action<IColony, IColonyAgent> OnMemberJoined;
    public Action<IColony, IColonyAgent> OnMemberLeft;

    // Internal storage
    private List<Colony> _colonies = new List<Colony>();
    private Dictionary<IColonyAgent, Colony> _assignment = new Dictionary<IColonyAgent, Colony>();

    // Spatial hash (bucket stores IColonyAgent)
    private HashSet<IColonyAgent> _registeredAgents = new HashSet<IColonyAgent>();
    private Dictionary<long, List<IColonyAgent>> _spatialBuckets = new Dictionary<long, List<IColonyAgent>>();
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

    // Public API pour les agents — utiliser IColonyAgent
    public void RegisterAgent(IColonyAgent agent)
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

    public void UnregisterAgent(IColonyAgent agent)
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

    public void UpdateAgentCell(IColonyAgent agent, Vector3 previousPosition)
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

    private void AddToBucket(IColonyAgent a)
    {
        long key = GetCellKey(a.transform.position);
        if (!_spatialBuckets.TryGetValue(key, out var list))
        {
            list = new List<IColonyAgent>();
            _spatialBuckets[key] = list;
        }
        if (!list.Contains(a)) list.Add(a);
    }

    private void RemoveFromBucket(IColonyAgent a)
    {
        RemoveFromBucket(a, a.transform.position);
    }

    private void RemoveFromBucket(IColonyAgent a, Vector3 fromPosition)
    {
        long key = GetCellKey(fromPosition);
        if (_spatialBuckets.TryGetValue(key, out var list))
        {
            list.Remove(a);
            if (list.Count == 0) _spatialBuckets.Remove(key);
        }
    }

    // Scanning
    private void ScanForColonies()
    {
        foreach (var agent in _registeredAgents.ToList())
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
    private bool TryJoinNearestColony(IColonyAgent agent)
    {
        if (agent == null) return false;
        if (_assignment.ContainsKey(agent)) return false;

        Colony best = null;
        float bestDist = float.MaxValue;
        Vector3 pos = agent.transform.position;

        foreach (var col in _colonies)
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
            // Members is exposed as a read-only list; use Add on the internal list
            best.Members.Add(agent);
            best.Inhabitants = best.Members.Count;

            agent.SetCurrentColony(best);
            OnMemberJoined?.Invoke(best, agent);
            Debug.Log($"Agent {agent.gameObject.GetInstanceID()} joined Colony Id={best.Id} (now {best.Inhabitants}/{best.MaxInhabitants})");
            return true;
        }

        return false;
    }

    // Vérifie un groupe autour d'un agent et crée une colony si nécessaire
    private void CheckNearbyForColony(IColonyAgent candidate)
    {
        if (candidate == null) return;
        if (!candidate.CanFormColony) return;

        var neighbors = GetNeighborsFromBuckets(candidate.transform.position)
            .Where(g => g != null && !_assignment.ContainsKey(g) && g.CanFormColony && string.Equals(g.GetSpecies(), candidate.GetSpecies(), StringComparison.OrdinalIgnoreCase))
            .ToList();

        if (!neighbors.Contains(candidate)) neighbors.Add(candidate);

        if (neighbors.Count >= requiredPimusToCreate)
        {
            Vector3 centroid = Vector3.zero;
            foreach (var n in neighbors) centroid += n.transform.position;
            centroid /= neighbors.Count;

            var group = GetNeighborsFromBuckets(centroid)
                .Where(g => g != null && !_assignment.ContainsKey(g) && g.CanFormColony && string.Equals(g.GetSpecies(), candidate.GetSpecies(), StringComparison.OrdinalIgnoreCase) && Vector3.Distance(g.transform.position, centroid) <= groupingRadius)
                .ToList();

            if (group.Count >= requiredPimusToCreate)
            {
                CreateColony(centroid, group);
            }
        }
    }

    private List<IColonyAgent> GetNeighborsFromBuckets(Vector3 position)
    {
        var results = new List<IColonyAgent>();
        int cx = Mathf.FloorToInt(position.x / _cellSize);
        int cz = Mathf.FloorToInt(position.z / _cellSize);

        for (int dx = -1; dx <= 1; dx++)
            for (int dz = -1; dz <= 1; dz++)
            {
                int nx = cx + dx;
                int nz = cz + dz;
                long key = ((long)nx << 32) ^ (uint)nz;
                if (_spatialBuckets.TryGetValue(key, out var list))
                {
                    foreach (var go in list)
                        if (go != null && !results.Contains(go)) results.Add(go);
                }
            }

        results = results.Where(g => g != null && Vector3.Distance(g.transform.position, position) <= groupingRadius).ToList();
        return results;
    }

    // Création : fixe MaxInhabitants à la création et ne le modifie plus
    private void CreateColony(Vector3 center, List<IColonyAgent> members)
    {
        members = members.Where(m => m != null && !_assignment.ContainsKey(m)).ToList();

        var colony = new Colony();
        colony.Id = _nextColonyId++;
        colony.Center = center;
        colony.Members.Clear();

        foreach (var m in members)
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
        foreach (var m in colony.Members)
            OnMemberJoined?.Invoke(colony, m);

        Debug.Log($"Colony created (Id={colony.Id}) at {center} with {colony.Inhabitants} habitants (max {colony.MaxInhabitants}) species={colony.Species}");
        OnColonyCreated?.Invoke(colony);
    }

    public IColony GetColonyForAgent(IColonyAgent agent)
    {
        if (agent == null) return null;
        _assignment.TryGetValue(agent, out Colony colony);
        return colony;
    }

    private void DissolveColony(Colony colony)
    {
        if (colony == null) return;

        List<IColonyAgent> members = colony.Members != null ? colony.Members.ToList() : new List<IColonyAgent>();
        foreach (IColonyAgent m in members)
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

    private void RemoveAgentFromColony(IColonyAgent agent, Colony colony)
    {
        if (agent == null || colony == null) return;

        if (colony.Members != null && colony.Members.Contains(agent)) colony.Members.Remove(agent);

        colony.Inhabitants = colony.Members != null ? colony.Members.Count : Math.Max(0, colony.Inhabitants - 1);

        // Ne pas modifier MaxInhabitants : fixé à la création

        OnMemberLeft?.Invoke(colony, agent);

        if (colony.Inhabitants < requiredPimusToCreate)
            DissolveColony(colony);
    }

    public List<IColony> GetAllColonies() => _colonies.Cast<IColony>().ToList();

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
