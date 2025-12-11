using System.Collections.Generic;
using UnityEngine;

public class MapBuildingManager : MonoBehaviour
{
    private List<Building> buildings = new List<Building>();
    public List<Building> GetAllBuildings() => new List<Building>(buildings);

    [SerializeField] private List<GameObject> building = new List<GameObject>();

    // Spatial Hashing
    private Dictionary<long, List<Building>> _spatialBuckets = new Dictionary<long, List<Building>>();
    private float _cellSize = 20f;

    void Awake()
    {
        BuildingEvents.GetNearestBuilding = GetNearestBuilding;
    }

    void OnDestroy()
    {
        if (BuildingEvents.GetNearestBuilding == GetNearestBuilding)
            BuildingEvents.GetNearestBuilding = null;
    }

    public Building SpawnBuilding(GameObject prefab, Vector3 position, Colony owner, BuildType type)
    {
        if (prefab == null)
            return null;
        
        GameObject buildingObject = Instantiate(prefab, position, Quaternion.identity);
        
        Building building = buildingObject.GetComponent<Building>();
        if (building == null)
            building = buildingObject.AddComponent<Building>();
        
        building.Initialize(type, owner, buildingObject);
        
        if (!buildings.Contains(building))
        {
            buildings.Add(building);
            AddToBucket(building);
        }

        return building;
    }


    public void DestroyBuilding(Building b)
    {
        if (b == null) return;
        if (buildings.Contains(b))
        {
            buildings.Remove(b);
            RemoveFromBucket(b);
        }
        if (b.gameObject != null) Destroy(b.gameObject);
    }
    
    private long GetCellKey(Vector3 worldPosition)
    {
        int cellX = Mathf.FloorToInt(worldPosition.x / _cellSize);
        int cellZ = Mathf.FloorToInt(worldPosition.z / _cellSize);
        
        long cellKey = ((long)cellX << 32) ^ (uint)cellZ;

        return cellKey;
    }


    private void AddToBucket(Building building)
    {
        long cellKey = GetCellKey(building.transform.position);
        
        if (!_spatialBuckets.TryGetValue(cellKey, out List<Building> bucket))
        {
            bucket = new List<Building>();
            _spatialBuckets[cellKey] = bucket;
        }
        if (!bucket.Contains(building))
            bucket.Add(building);
    }
    
    private void RemoveFromBucket(Building building)
    {
        
        long cellKey = GetCellKey(building.transform.position);
        
        if (_spatialBuckets.TryGetValue(cellKey, out List<Building> bucket))
        {
            bucket.Remove(building);
            
            if (bucket.Count == 0)
                _spatialBuckets.Remove(cellKey);
        }
    }


    public Building GetNearestBuilding(Vector3 position)
    {
        Building bestBuilding = null;
        
        float bestDistance = float.MaxValue;
        
        int cellX = Mathf.FloorToInt(position.x / _cellSize);
        int cellZ = Mathf.FloorToInt(position.z / _cellSize);
        
        for (int offsetX = -1; offsetX <= 1; offsetX++)
        {
            for (int offsetZ = -1; offsetZ <= 1; offsetZ++)
            {
                int neighborCellX = cellX + offsetX;
                int neighborCellZ = cellZ + offsetZ;
                
                long cellKey = ((long)neighborCellX << 32) ^ (uint)neighborCellZ;

                if (_spatialBuckets.TryGetValue(cellKey, out List<Building> bucket))
                {
                    foreach (Building b in bucket)
                    {
                        if (b == null) continue;

                        float d = Vector3.Distance(b.transform.position, position);
                        if (d < bestDistance)
                        {
                            bestDistance = d;
                            bestBuilding = b;
                        }
                    }
                }
            }
        }

        return bestBuilding;
    }

}
