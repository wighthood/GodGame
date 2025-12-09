using System;
using System.Collections.Generic;
using UnityEngine;

[DefaultExecutionOrder(-100)]
public class MapBuildingManager : MonoBehaviour
{
    private List<Building> buildings = new List<Building>();

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
        if (buildings.Contains(b)) buildings.Remove(b);
        BuildingEvents.OnBuildingDestroyed?.Invoke(b);
        BuildingEvents.OnBuildingsChanged?.Invoke();
        if (b.gameObject != null) Destroy(b.gameObject);
    }

    private Building HandleGetNearestBuilding(Vector3 pos, string typeFilter)
    {
        Building best = null;
        float bestDist = float.MaxValue;
        foreach (Building b in buildings)
        {
            if (b == null) continue;
            if (!string.IsNullOrEmpty(typeFilter) && !string.Equals(b.Type, typeFilter, StringComparison.OrdinalIgnoreCase)) continue;
            float d = Vector3.Distance(b.transform.position, pos);
            if (d < bestDist) { bestDist = d; best = b; }
        }
        return best;
    }

    public List<Building> GetAllBuildings() => new List<Building>(buildings);
}
