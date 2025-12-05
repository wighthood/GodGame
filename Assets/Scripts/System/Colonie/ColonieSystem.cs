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
    public int defaultMaxPerPimu = 2;

    [Header("Scan settings")]
    public float scanInterval = 1f;

    public static ColonieSystem Instance { get; private set; }

    public Action<IColony> OnColonyCreated;
    public Action<IColony> OnColonyDissolved;
    public Action<IColony, IColonyAgent> OnMemberJoined;
    public Action<IColony, IColonyAgent> OnMemberLeft;

    private List<Colony> _colonies = new List<Colony>();
    private Dictionary<IColonyAgent, Colony> _assignment = new Dictionary<IColonyAgent, Colony>();

    private HashSet<IColonyAgent> _registeredAgents = new HashSet<IColonyAgent>();
    private Dictionary<long, List<IColonyAgent>> _spatialBuckets = new Dictionary<long, List<IColonyAgent>>();
    private float _cellSize = 5f;

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

    public void RegisterAgent(IColonyAgent agent)
    {
        if (agent == null) return;
        if (_registeredAgents.Contains(agent)) return;
        _registeredAgents.Add(agent);
        AddToBucket(agent);

        if (TryJoinNearestColony(agent)) return;

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

    private long GetCellKey(Vector3 pos)
    {
        int x = Mathf.FloorToInt(pos.x / _cellSize);
        int z = Mathf.FloorToInt(pos.z / _cellSize);
        return ((long)x << 32) ^ (uint)z;
    }

    private void AddToBucket(IColonyAgent a)
    {
        long key = GetCellKey(a.GetTransform().position);
        if (!_spatialBuckets.TryGetValue(key, out var list))
        {
            list = new List<IColonyAgent>();
            _spatialBuckets[key] = list;
        }
        if (!list.Contains(a)) list.Add(a);
    }

    private void RemoveFromBucket(IColonyAgent a)
    {
        RemoveFromBucket(a, a.GetTransform().position);
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

    private void ScanForColonies()
    {
        foreach (var agent in _registeredAgents.ToList())
        {
            if (agent == null) continue;
            if (_assignment.ContainsKey(agent)) continue;

            if (TryJoinNearestColony(agent)) continue;

            CheckNearbyForColony(agent);
        }
    }

    private bool TryJoinNearestColony(IColonyAgent agent)
    {
        if (agent == null) return false;
        if (_assignment.ContainsKey(agent)) return false;

        Colony best = null;
        float bestDist = float.MaxValue;
        Vector3 pos = agent.GetTransform().position;

        foreach (var col in _colonies)
        {
            if (col == null) continue;
            if (!string.Equals(col.Species, agent.GetSpecies(), StringComparison.OrdinalIgnoreCase)) continue;
            if (col.Inhabitants >= col.MaxInhabitants) continue;

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
            best.Members.Add(agent);
            best.Inhabitants = best.Members.Count;

            agent.SetCurrentColony(best);
            OnMemberJoined?.Invoke(best, agent);
            Debug.Log($"Agent {agent.GetGameObject().GetInstanceID()} joined Colony Id={best.Id} (now {best.Inhabitants}/{best.MaxInhabitants})");
            return true;
        }

        return false;
    }

    private void CheckNearbyForColony(IColonyAgent candidate)
    {
        if (candidate == null) return;
        if (!candidate.CanFormColony()) return;

        var neighbors = GetNeighborsFromBuckets(candidate.GetTransform().position)
            .Where(g => g != null && !_assignment.ContainsKey(g) && g.CanFormColony() && string.Equals(g.GetSpecies(), candidate.GetSpecies(), StringComparison.OrdinalIgnoreCase))
            .ToList();

        if (!neighbors.Contains(candidate)) neighbors.Add(candidate);

        if (neighbors.Count >= requiredPimusToCreate)
        {
            Vector3 centroid = Vector3.zero;
            foreach (var n in neighbors) centroid += n.GetTransform().position;
            centroid /= neighbors.Count;

            var group = GetNeighborsFromBuckets(centroid)
                .Where(g => g != null && !_assignment.ContainsKey(g) && g.CanFormColony() && string.Equals(g.GetSpecies(), candidate.GetSpecies(), StringComparison.OrdinalIgnoreCase) && Vector3.Distance(g.GetTransform().position, centroid) <= groupingRadius)
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

        results = results.Where(g => g != null && Vector3.Distance(g.GetTransform().position, position) <= groupingRadius).ToList();
        return results;
    }

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
        int computedMax = colony.Inhabitants * defaultMaxPerPimu;
        colony.MaxInhabitants = computedMax > colony.Inhabitants ? computedMax : colony.Inhabitants;

        colony.Buildings = new List<GameObject>();
        colony.Resources = new Dictionary<string, int>();
        colony.InfluenceRadius = defaultInfluenceRadius;
        colony.Species = members.Count > 0 ? members[0].GetSpecies() : "Unknown";

        _colonies.Add(colony);

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
            try { m.SetCurrentColony(null); } catch (Exception ex) { Debug.LogError($"Error while unassigning member from colony: {ex}"); }
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

        OnMemberLeft?.Invoke(colony, agent);

        if (colony.Inhabitants < requiredPimusToCreate)
            DissolveColony(colony);
    }

    public List<IColony> GetAllColonies() => _colonies.Cast<IColony>().ToList();

    public void DebugDrawColonies()
    {
        foreach (Colony colony in _colonies)
        {
            if (colony == null) continue;
            Gizmos.color = new Color(0f, 0.6f, 1f, 0.5f);
            Gizmos.DrawWireSphere(colony.Center, colony.InfluenceRadius);
            Gizmos.color = new Color(1f, 0.85f, 0f, 0.06f);
            Gizmos.DrawSphere(colony.Center, 0.12f);

#if UNITY_EDITOR
            UnityEditor.Handles.Label(colony.Center + Vector3.up * 1.5f, $"Id={colony.Id} {colony.Species} {colony.Inhabitants}/{colony.MaxInhabitants}");
#endif
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
