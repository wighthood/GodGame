using System.Collections.Generic;
using System.Linq;
using UnityEngine;

#if ENABLE_INPUT_SYSTEM && !ENABLE_LEGACY_INPUT_MANAGER
using UnityEngine.InputSystem;
#endif

public class DebugSpawnStorage : MonoBehaviour
{
    [Header("Prefab & spawn settings")]
    public GameObject storagePrefab;
    public Transform spawnPoint;
    public KeyCode spawnKey = KeyCode.B;

    [Header("Attach to colony")]
    public bool attachToNearestColony = true;
    public float searchRadius = 10f;

    [Header("Debug resource keys")]
    public KeyCode addWoodKey = KeyCode.Alpha1;
    public KeyCode removeWoodKey = KeyCode.Alpha2;
    public KeyCode addFoodKey = KeyCode.Alpha3;
    public KeyCode removeFoodKey = KeyCode.Alpha4;

    [SerializeField] private MapBuildingManager mbm;
    private MapBuildingManager _mbm;

    void Start()
    {
        if (_mbm == null)
        {
            Debug.LogError("DebugSpawnStorage: MapBuildingManager not found in scene — disabling debug script.");
            enabled = false;
            return;
        }

        if (BuildingEvents.OnSpawnRequested == null) Debug.Log("DebugSpawnStorage: BuildingEvents.OnSpawnRequested is null at Start.");
    }

    void Update()
    {
        if (Keyboard.current == null) return;
        if (Keyboard.current[Key.B].wasPressedThisFrame) SpawnStorage();
        if (Keyboard.current[Key.Digit1].wasPressedThisFrame) ModifyNearestStorage(RessourceType.wood, +1);
        if (Keyboard.current[Key.Digit2].wasPressedThisFrame) ModifyNearestStorage(RessourceType.wood, -1);
        if (Keyboard.current[Key.Digit3].wasPressedThisFrame) ModifyNearestStorage(RessourceType.food, +1);
        if (Keyboard.current[Key.Digit4].wasPressedThisFrame) ModifyNearestStorage(RessourceType.food, -1);
    }

    private void ModifyNearestStorage(RessourceType type, int amount)
    {
        Building nearest = _mbm.FindNearestBuilding(transform.position, "Storage");
        if (nearest == null)
        {
            Debug.LogWarning("DebugSpawnStorage: No Storage building found nearby via MapBuildingManager.");
            return;
        }

        Stockage stockage = nearest.GetComponent<Stockage>();
        if (stockage == null)
        {
            Debug.LogWarning("DebugSpawnStorage: Nearest building has no Stockage component.");
            return;
        }

        bool ok;
        if (amount > 0)
        {
            ok = stockage.AddRessources(type, amount);
            Debug.Log($"DebugSpawnStorage: Added {amount} {type} to storage '{nearest.name}' success={ok}");
        }
        else
        {
            ok = stockage.DelRessources(type, -amount);
            Debug.Log($"DebugSpawnStorage: Removed {-amount} {type} from storage '{nearest.name}' success={ok}");
        }

        Dictionary<RessourceType, int> contents = stockage.GetAllRessources();
        if (contents != null && contents.Count > 0)
        {
            string s = string.Join(", ", contents.Select(kv => kv.Key + ":" + kv.Value));
            Debug.Log($"DebugSpawnStorage: Storage '{nearest.name}' now contains: {s}");
        }
        else
        {
            Debug.Log($"DebugSpawnStorage: Storage '{nearest.name}' is empty.");
        }
    }

    [ContextMenu("Spawn Storage Now")]
    public void SpawnStorage()
    {
        if (_mbm == null)
        {
            Debug.LogError("DebugSpawnStorage: MapBuildingManager not available.");
            return;
        }

        if (storagePrefab == null)
        {
            Debug.LogWarning("DebugSpawnStorage: storagePrefab is not assigned.");
            return;
        }

        Vector3 pos = (spawnPoint != null) ? spawnPoint.position : transform.position;
        Colony owner = null;

        if (attachToNearestColony)
        {
            IColony nearestCol = ColonieSystem.Instance?.GetAllColonies()?.OrderBy(c => Vector3.Distance(c.GetCenter(), pos)).FirstOrDefault();
            if (nearestCol != null && Vector3.Distance(nearestCol.GetCenter(), pos) <= searchRadius)
                owner = nearestCol as Colony;
        }

        _mbm.SpawnBuilding(storagePrefab, pos, Quaternion.identity, owner, "Storage");
        Debug.Log($"DebugSpawnStorage: Spawn requested for storage at {pos} owner={(owner!=null?owner.Id.ToString():"null")}");
    }

    [ContextMenu("Add 1 Wood to Nearest Storage (Inspector)")]
    public void AddWoodNow()
    {
        ModifyNearestStorage(RessourceType.wood, +1);
    }

    [ContextMenu("Remove 1 Wood from Nearest Storage (Inspector)")]
    public void RemoveWoodNow()
    {
        ModifyNearestStorage(RessourceType.wood, -1);
    }

    [ContextMenu("Add 1 Food to Nearest Storage (Inspector)")]
    public void AddFoodNow()
    {
        ModifyNearestStorage(RessourceType.food, +1);
    }

    [ContextMenu("Remove 1 Food from Nearest Storage (Inspector)")]
    public void RemoveFoodNow()
    {
        ModifyNearestStorage(RessourceType.food, -1);
    }

    void OnDrawGizmosSelected()
    {
        Vector3 pos = (spawnPoint != null) ? spawnPoint.position : transform.position;
        Gizmos.color = Color.yellow;
        Gizmos.DrawWireSphere(pos, 0.5f);
    }
}
