using System.Collections.Generic;
using UnityEngine;

public class MapBuildingManager : MonoBehaviour, ISaveable
{
    private List<Building> buildings = new ();
    public List<Building> GetAllBuildings() => new(buildings);

    [SerializeField] private List<GameObject> building = new();

    // Spatial Hashing
    private Dictionary<long, List<Building>> spatialBuckets = new();
    private float _cellSize = 20f;

    void OnEnable()
    {
        SaveEvents.OnRegisterSaveableEvent?.Invoke(this);
    }

    void OnDisable()
    {
        SaveEvents.OnUnregisterSaveableEvent?.Invoke(this);
    }

    void Awake()
    {
        BuildingEvents.OnSpawnRequestedEvent += SpawnRequested;
        BuildingEvents.OnGetNearestBuildingEvent = HandleGetNearestBuilding;
        BuildingEvents.OnGetBuildPositionEvent += GetValidBuildingPosition;
    }

    void OnDestroy()
    {
        BuildingEvents.OnSpawnRequestedEvent -= SpawnRequested;
        if (BuildingEvents.OnGetNearestBuildingEvent == HandleGetNearestBuilding)
            BuildingEvents.OnGetNearestBuildingEvent = null;
        BuildingEvents.OnGetBuildPositionEvent -= GetValidBuildingPosition;
    }

    private Vector3? GetValidBuildingPosition(Vector3 _searchCenter, I_Colony _colony)
    {
        if (_colony == null) return null;

        for (int i = 0; i < 100; i++)
        {
            // Pick a random point
            Vector2 randomPoint = UnityEngine.Random.insideUnitCircle * _colony.GetInfluenceRadius();
            
            Vector3 offset = new Vector3(randomPoint.x, 0f, randomPoint.y);
            
            Vector3 candidatePos = _colony.GetColonyCenter() + offset;

            // Align to grid (Assuming we have access to these static events or utils)
            Vector2Int cellPos = Colony.WorldToCellPos.Invoke(candidatePos);
            Vector3 alignedPos = Colony.CellToWorld.Invoke(cellPos);

            // Check if walkable
            Cell cell = Colony.GetCell.Invoke(cellPos);
            if (cell == null || !cell.isWalkable) continue;

            // Check if occupied by another building using internal spatial methods
            Building nearest = HandleGetNearestBuilding(alignedPos);
            if (nearest != null)
            {
               if (Vector3.Distance(nearest.transform.position, alignedPos) < 1.0f)
               {
                   continue;
               }
            }

            return alignedPos;
        }
        return null;
    }

    private void SpawnRequested(BuildType _type, Vector3 _position, Colony _owner)
    {
        int index = (int)_type;
        if (building == null || index < 0 || index >= building.Count)
        {
            Debug.LogError($"MapBuildingManager: Missing prefab for BuildType {_type} (index {index}). Check 'Building' list in Inspector.");
            return;
        }
        SpawnBuilding(building[index], _position, _owner, _type);
    }

    public void SpawnBuilding(GameObject _prefab, Vector3 _position, Colony _owner, BuildType _type)
    {
        if (_prefab == null) return;
        
        GameObject BuildGameObject = Instantiate(_prefab, _position, Quaternion.identity, _owner.GetBuildingParent());
        
        Building building = BuildGameObject.GetComponent<Building>();

        building.Initialize(_type, _owner, BuildGameObject);

        if (!buildings.Contains(building))
        {
            buildings.Add(building);
            AddToBucket(building);
            BuildingEvents.OnBuildingsChangedEvent?.Invoke();
        }

        if(building.TryGetComponent(out Storage storage))
        {
            _owner.DefineStorage(storage);
        }
    }

    public Building FindNearestBuilding(Vector3 _pos)
    {
        return HandleGetNearestBuilding(_pos);
    }

    public void DestroyBuilding(Building _b)
    {
        if (_b == null) return;
        if (buildings.Contains(_b))
        {
            buildings.Remove(_b);
            RemoveFromBucket(_b);
        }
        BuildingEvents.OnBuildingDestroyedEvent?.Invoke(_b);
        BuildingEvents.OnBuildingsChangedEvent?.Invoke();
        if (_b.gameObject != null) Destroy(_b.gameObject);
    }

    // Spatial Hashing Logic

    private long GetCellKey(Vector3 _pos)
    {
        int x = Mathf.FloorToInt(_pos.x / _cellSize);
        int z = Mathf.FloorToInt(_pos.z / _cellSize);
        return ((long)x << 32) ^ (uint)z;
    }

    private void AddToBucket(Building _b)
    {
        long key = GetCellKey(_b.transform.position);
        if (!spatialBuckets.TryGetValue(key, out List<Building> list))
        {
            list = new List<Building>();
            spatialBuckets[key] = list;
        }
        if (!list.Contains(_b)) list.Add(_b);
    }

    private void RemoveFromBucket(Building _b)
    {
        long key = GetCellKey(_b.transform.position);
        if (spatialBuckets.TryGetValue(key, out List<Building> list))
        {
            list.Remove(_b);
            if (list.Count == 0) spatialBuckets.Remove(key);
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

                if (spatialBuckets.TryGetValue(key, out List<Building> list))
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

    public string GetSaveID()
    {
        return "MapBuildingManager";
    }

    public string CaptureState()
    {
        MapBuildingSystemSaveData data = new MapBuildingSystemSaveData();

        foreach (var b in buildings)
        {
            if (b == null) continue;
            
            BuildingSaveData bData = new BuildingSaveData();
            bData.position = b.transform.position;
            bData.buildTypeId = (int)b.Type;
            
            if (b.Owner != null)
            {
                bData.ownerColonyId = b.Owner.GetId();
            }
            else
            {
                bData.ownerColonyId = -1;
            }
            
            data.buildings.Add(bData);
        }

        return JsonUtility.ToJson(data);
    }

    public void RestoreState(string _state)
    {
        foreach (var b in buildings)
        {
            if (b != null && b.gameObject != null) Destroy(b.gameObject);
        }
        buildings.Clear();
        spatialBuckets.Clear();

        if (string.IsNullOrEmpty(_state)) return;

        MapBuildingSystemSaveData data = JsonUtility.FromJson<MapBuildingSystemSaveData>(_state);
        if (data == null) return;
        
        foreach (var bData in data.buildings)
        {
            Colony owner = null;
            if (bData.ownerColonyId != -1)
            {
               // Helper to find colony by ID without direct reference
               if (ColonieSystem.OnRequestColonyByIDEvent != null)
               {
                   owner = ColonieSystem.OnRequestColonyByIDEvent.Invoke(bData.ownerColonyId);
               }
            }
            
            // Resolve Prefab
            if (bData.buildTypeId < 0 || bData.buildTypeId >= building.Count) continue;
            GameObject prefab = building[bData.buildTypeId];
            
            if (owner != null)
            {
                 SpawnBuilding(prefab, bData.position, owner, (BuildType)bData.buildTypeId);
            }
            else
            {
                // Debug.LogWarning($"Building at {bData.position} skipped because owner colony {bData.ownerColonyId} not found (via Event).");
            }
        }
    }
}
