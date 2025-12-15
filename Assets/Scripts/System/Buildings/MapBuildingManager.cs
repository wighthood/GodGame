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
        BuildingEvents.OnSpawnRequested += SpawnRequested;
        BuildingEvents.GetNearestBuilding = HandleGetNearestBuilding;
    }

    void OnDestroy()
    {
        BuildingEvents.OnSpawnRequested -= SpawnRequested;
        if (BuildingEvents.GetNearestBuilding == HandleGetNearestBuilding)
            BuildingEvents.GetNearestBuilding = null;
    }

    private void SpawnRequested(BuildType _type, Vector3 _position, Colony _owner)
    {
        SpawnBuilding(building[(int)_type], _position, _owner, _type);
    }

    public void SpawnBuilding(GameObject prefab, Vector3 position, Colony owner, BuildType type)
    {
        if (prefab == null) return;
        
        GameObject BuildGameObject = Instantiate(prefab, position, Quaternion.identity, owner.GetBuildingParent());
        
        Building building = BuildGameObject.GetComponent<Building>();

        building.Initialize(type, owner, BuildGameObject);

        if (!buildings.Contains(building))
        {
            buildings.Add(building);
            AddToBucket(building);
            //BuildingEvents.OnBuildingsChanged?.Invoke();
        }

        if(building.TryGetComponent(out Storage storage))
        {
            owner.DefineStorage(storage);
        }
    }

    public Building FindNearestBuilding(Vector3 pos)
    {
        return HandleGetNearestBuilding(pos);
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
        //BuildingEvents.OnBuildingsChanged?.Invoke();
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

    private Building HandleGetNearestBuilding(Vector3 _pos)
    {
        Building best = null;
        float bestDist = float.MaxValue;
        
        int cx = Mathf.FloorToInt(_pos.x / _cellSize);
        int cz = Mathf.FloorToInt(_pos.z / _cellSize);

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

                        float d = Vector3.Distance(b.transform.position, _pos);
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
