using System;
using System.Collections.Generic;
using UnityEngine;

public class MapBuildingManager : MonoBehaviour
{
    private List<Building> buildings = new List<Building>();
    public List<Building> GetAllBuildings() => new List<Building>(buildings);

    // Spatial Hashing
    private Dictionary<long, List<Building>> _spatialBuckets = new Dictionary<long, List<Building>>();
    private float _cellSize = 20f;

    void Awake()
    {
        BuildingEvents.OnSpawnRequested += SpawnRequested;
        BuildingEvents.GetNearestBuilding = HandleGetNearestBuilding;
    }

    void OnDestroy()
    {
        BuildingEvents.OnSpawnRequested -= SpawnRequested;
        if (BuildingEvents.GetNearestBuilding == HandleGetNearestBuilding)
            BuildingEvents.GetNearestBuilding = null;
    }

    private void SpawnRequested(GameObject prefab, Vector3 position, Quaternion rotation, Colony owner, string type)
    {
        SpawnBuilding(prefab, position, rotation, owner, type);
    }

    public Building SpawnBuilding(GameObject prefab, Vector3 position, Quaternion rotation, Colony owner, string type)
    {
        if (prefab == null) return null;
        
        GameObject go = Instantiate(prefab, position, rotation);
        
        Building b = go.GetComponent<Building>();
        
        if (b == null)
        {
            b = go.AddComponent<Building>();
        }

        b.Initialize(type, owner, go);

        if (!buildings.Contains(b))
        {
            buildings.Add(b);
            AddToBucket(b);
            BuildingEvents.OnBuildingSpawned?.Invoke(b);
            BuildingEvents.OnBuildingsChanged?.Invoke();
        }

        return b;
    }

    public Building FindNearestBuilding(Vector3 pos, string typeFilter)
    {
        return HandleGetNearestBuilding(pos, typeFilter);
    }

    public void DestroyBuilding(Building b)
    {
        if (b == null) return;
        if (buildings.Contains(b))
        {
            buildings.Remove(b);
            RemoveFromBucket(b);
        }
        BuildingEvents.OnBuildingDestroyed?.Invoke(b);
        BuildingEvents.OnBuildingsChanged?.Invoke();
        if (b.gameObject != null) Destroy(b.gameObject);
    }

    // Spatial Hashing Logic

    private long GetCellKey(Vector3 pos)
    {
        int x = Mathf.FloorToInt(pos.x / _cellSize);
        int z = Mathf.FloorToInt(pos.z / _cellSize);
        return ((long)x << 32) ^ (uint)z;
    }

    private void AddToBucket(Building b)
    {
        long key = GetCellKey(b.transform.position);
        if (!_spatialBuckets.TryGetValue(key, out List<Building> list))
        {
            list = new List<Building>();
            _spatialBuckets[key] = list;
        }
        if (!list.Contains(b)) list.Add(b);
    }

    private void RemoveFromBucket(Building b)
    {
        long key = GetCellKey(b.transform.position);
        if (_spatialBuckets.TryGetValue(key, out List<Building> list))
        {
            list.Remove(b);
            if (list.Count == 0) _spatialBuckets.Remove(key);
        }
    }

    private Building HandleGetNearestBuilding(Vector3 pos, string typeFilter)
    {
        Building best = null;
        float bestDist = float.MaxValue;
        
        int cx = Mathf.FloorToInt(pos.x / _cellSize);
        int cz = Mathf.FloorToInt(pos.z / _cellSize);

        for (int dx = -1; dx <= 1; dx++)
        {
            for (int dz = -1; dz <= 1; dz++)
            {
                int nx = cx + dx;
                int nz = cz + dz;
                long key = ((long)nx << 32) ^ (uint)nz;

                if (_spatialBuckets.TryGetValue(key, out List<Building> list))
                {
                    foreach (Building b in list)
                    {
                        if (b == null) continue;
                        if (!string.IsNullOrEmpty(typeFilter) && !string.Equals(b.Type, typeFilter, StringComparison.OrdinalIgnoreCase)) continue;

                        float d = Vector3.Distance(b.transform.position, pos);
                        if (d < bestDist)
                        {
                            bestDist = d;
                            best = b;
                        }
                    }
                }
            }
        }

        return best;
    }
}
