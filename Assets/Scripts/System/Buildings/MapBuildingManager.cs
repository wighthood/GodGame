using System.Collections.Generic;
using UnityEngine;

public class MapBuildingManager : MonoBehaviour, ISaveable
{

    [SerializeField] private List<GameObject> building = new List<GameObject>();

    [Header("Obstacle Detection")]
    [SerializeField] private LayerMask obstacleLayers;
    [SerializeField] private Vector2 checkSize = new Vector2(0.9f, 0.9f);
    private readonly float _cellSize = 20f;
    private readonly List<Building> buildings = new List<Building>();
    private bool lastDebugHit;

    private Vector3 lastDebugPos;

    // Spatial Hashing
    private readonly Dictionary<long, List<Building>> spatialBuckets = new Dictionary<long, List<Building>>();

    private void Awake()
    {
        BuildingEvents.OnSpawnRequestedEvent += SpawnRequested;
        BuildingEvents.OnGetNearestBuildingEvent = HandleGetNearestBuilding;
        BuildingEvents.OnGetBuildPositionEvent += GetValidBuildingPosition;
        BuildingEvents.OnGetBuildingCountOfTypeEvent += GetBuildingCountOfType;
    }

    private void OnEnable()
    {
        SaveEvents.OnRegisterSaveableEvent?.Invoke(this);
    }

    private void OnDisable()
    {
        SaveEvents.OnUnregisterSaveableEvent?.Invoke(this);
    }

    private void OnDestroy()
    {
        BuildingEvents.OnSpawnRequestedEvent -= SpawnRequested;
        if (BuildingEvents.OnGetNearestBuildingEvent == HandleGetNearestBuilding)
            BuildingEvents.OnGetNearestBuildingEvent = null;
        BuildingEvents.OnGetBuildPositionEvent -= GetValidBuildingPosition;
        BuildingEvents.OnGetBuildingCountOfTypeEvent -= GetBuildingCountOfType;
    }

    private void OnDrawGizmos()
    {
        Gizmos.color = lastDebugHit ? Color.red : Color.green;
        Gizmos.DrawWireCube(lastDebugPos, checkSize);
    }

    public string GetSaveID()
    {
        return "MapBuildingManager";
    }

    public string CaptureState()
    {
        MapBuildingSystemSaveData data = new MapBuildingSystemSaveData();

        foreach (Building b in buildings)
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

            // Save Storage
            if (b.TryGetComponent(out Storage storage))
            {
                List<RessourceCollection> stockpiled = storage.GetStockedResources();
                if (stockpiled != null)
                {
                    foreach (RessourceCollection res in stockpiled)
                    {
                        bData.storedItems.Add(new InventoryItemData
                        {
                            type = (int)res.RessourceType,
                            amount = (int)res.number,
                        });
                    }
                }
            }

            data.buildings.Add(bData);
        }

        return JsonUtility.ToJson(data);
    }

    public void RestoreState(string _state)
    {
        foreach (Building b in buildings)
        {
            if (b != null && b.gameObject != null) Destroy(b.gameObject);
        }
        buildings.Clear();
        spatialBuckets.Clear();

        if (string.IsNullOrEmpty(_state)) return;

        MapBuildingSystemSaveData data = JsonUtility.FromJson<MapBuildingSystemSaveData>(_state);
        if (data == null) return;

        foreach (BuildingSaveData bData in data.buildings)
        {
            Colony owner = null;
            if (bData.ownerColonyId != -1)
            {
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
                Building newBuilding = SpawnBuilding(prefab, bData.position, owner, (BuildType)bData.buildTypeId);
                if (newBuilding != null && newBuilding.TryGetComponent(out Storage storage))
                {
                    storage.ClearAndSetResources(bData.storedItems);
                }
            }
            // Debug.LogWarning($"Building at {bData.position} skipped because owner colony {bData.ownerColonyId} not found (via Event).");
        }
    }

    private Vector3? GetValidBuildingPosition(Vector3 _searchCenter, I_Colony _colony)
    {
        if (_colony == null) return null;

        for (int i = 0; i < 100; i++)
        {
            Vector2 randomPoint = Random.insideUnitCircle * _colony.GetInfluenceRadius();
            Vector3 offset = new Vector3(randomPoint.x, randomPoint.y, 0f);
            Vector3 candidatePos = _colony.GetColonyCenter() + offset;

            Vector2Int cellPos = Colony.WorldToCellPos.Invoke(candidatePos);
            Vector3 alignedPos = Colony.CellToWorld.Invoke(cellPos);

            Cell cell = Colony.GetCell.Invoke(cellPos);
            if (cell == null || !cell.isWalkable) continue;

            lastDebugPos = alignedPos;
            Collider2D hit = Physics2D.OverlapBox(alignedPos, checkSize, 0, obstacleLayers);

            if (hit != null)
            {
                lastDebugHit = true;
                continue;
            }
            lastDebugHit = false;

            return alignedPos;
        }

        Debug.LogWarning("Aucune position de construction valide trouvée après 100 essais ! Vérifiez les Layers ou la densité d'obstacles.");
        return null;
    }

    private int GetBuildingCountOfType(BuildType _type, Colony _colony)
    {
        if (_colony == null) return 0;
        int count = 0;
        foreach (Building b in buildings)
        {
            if (b != null && b.Type == _type && b.Owner == _colony)
            {
                count++;
            }
        }
        return count;
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

    public Building SpawnBuilding(GameObject _prefab, Vector3 _position, Colony _owner, BuildType _type)
    {
        if (_prefab == null) return null;

        Vector2Int cellPos = Colony.WorldToCellPos.Invoke(_position);
        Vector3 centeredPos = Colony.CellToWorld.Invoke(cellPos);

        Collider2D obstacle = Physics2D.OverlapBox(centeredPos, checkSize, 0, obstacleLayers);
        if (obstacle != null)
        {
            Debug.LogWarning($"[SPAWN BLOCKED] Construction annulée en {centeredPos}. La place a été prise entre temps par : {obstacle.name}");
            return null;
        }

        GameObject BuildGameObject = Instantiate(_prefab, centeredPos, Quaternion.identity, _owner.GetBuildingParent());

        Building building = BuildGameObject.GetComponent<Building>();

        building.Initialize(_type, _owner, BuildGameObject);

        if (!buildings.Contains(building))
        {
            buildings.Add(building);
            AddToBucket(building);
            BuildingEvents.OnBuildingsChangedEvent?.Invoke();
        }

        if (building.TryGetComponent(out Storage storage))
        {
            _owner.DefineStorage(storage);
        }

        return building;
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

    private long GetCellKey(Vector3 pos)
    {
        int x = Mathf.FloorToInt(pos.x / _cellSize);
        int y = Mathf.FloorToInt(pos.y / _cellSize);

        return (long)x << 32 ^ (uint)y;
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
        int cy = Mathf.FloorToInt(_pos.y / _cellSize);

        for (int dx = -1; dx <= 1; dx++)
        {

            for (int dy = -1; dy <= 1; dy++)
            {
                int nx = cx + dx;
                int ny = cy + dy;

                long key = (long)nx << 32 ^ (uint)ny;

                if (spatialBuckets.TryGetValue(key, out List<Building> list))
                {
                    foreach (Building b in list)
                    {
                        if (b == null) continue;

                        float d = Vector2.Distance(b.transform.position, _pos);
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
