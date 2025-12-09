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
    public Transform spawnPoint; // si null : utilisera la position de cet objet
    public KeyCode spawnKey = KeyCode.B;

    [Header("Attach to colony")]
    public bool attachToNearestColony = true;
    public float searchRadius = 10f;

    [Header("Debug resource keys")]
    public KeyCode addWoodKey = KeyCode.Alpha1;
    public KeyCode removeWoodKey = KeyCode.Alpha2;
    public KeyCode addFoodKey = KeyCode.Alpha3;
    public KeyCode removeFoodKey = KeyCode.Alpha4;

    void Start()
    {
        if (BuildingEvents.GetNearestBuilding == null) Debug.Log("DebugSpawnStorage: BuildingEvents.GetNearestBuilding is null at Start.");
        else Debug.Log("DebugSpawnStorage: BuildingEvents.GetNearestBuilding is present at Start.");
        if (BuildingEvents.OnSpawnRequested == null) Debug.Log("DebugSpawnStorage: BuildingEvents.OnSpawnRequested is null at Start.");
        else Debug.Log("DebugSpawnStorage: BuildingEvents.OnSpawnRequested is present at Start.");
    }

    void Update()
    {
#if ENABLE_INPUT_SYSTEM && !ENABLE_LEGACY_INPUT_MANAGER
        if (Keyboard.current == null) return;
        if (Keyboard.current[Key.B].wasPressedThisFrame) SpawnStorage();
        if (Keyboard.current[Key.Digit1].wasPressedThisFrame) ModifyNearestStorage(RessourceType.wood, +1);
        if (Keyboard.current[Key.Digit2].wasPressedThisFrame) ModifyNearestStorage(RessourceType.wood, -1);
        if (Keyboard.current[Key.Digit3].wasPressedThisFrame) ModifyNearestStorage(RessourceType.food, +1);
        if (Keyboard.current[Key.Digit4].wasPressedThisFrame) ModifyNearestStorage(RessourceType.food, -1);
#else
        if (Input.GetKeyDown(spawnKey)) SpawnStorage();
        if (Input.GetKeyDown(addWoodKey)) ModifyNearestStorage(RessourceType.wood, +1);
        if (Input.GetKeyDown(removeWoodKey)) ModifyNearestStorage(RessourceType.wood, -1);
        if (Input.GetKeyDown(addFoodKey)) ModifyNearestStorage(RessourceType.food, +1);
        if (Input.GetKeyDown(removeFoodKey)) ModifyNearestStorage(RessourceType.food, -1);
#endif
    }

    private void ModifyNearestStorage(RessourceType type, int amount)
    {
        var getter = BuildingEvents.GetNearestBuilding;
        Debug.Log($"DebugSpawnStorage: ModifyNearestStorage called type={type} amount={amount} getter={(getter!=null)}");
        if (getter == null)
        {
            Debug.LogWarning("DebugSpawnStorage: No BuildingEvents.GetNearestBuilding handler available.");
            return;
        }

        Building nearest = getter.Invoke(transform.position, "Storage");
        Debug.Log($"DebugSpawnStorage: nearest building found={(nearest!=null ? nearest.name : "null")}");
        if (nearest == null)
        {
            Debug.Log("DebugSpawnStorage: attempting fallback search for nearest Stockage via FindObjectsOfType...");
            Stockage[] storages = FindObjectsOfType<Stockage>();
            float best = float.MaxValue;
            Stockage bestS = null;
            foreach (var s in storages)
            {
                float d = Vector3.Distance(transform.position, s.transform.position);
                if (d < best)
                {
                    best = d;
                    bestS = s;
                }
            }
            if (bestS != null)
            {
                nearest = bestS.gameObject.GetComponent<Building>();
                Debug.Log($"DebugSpawnStorage: fallback found stockage '{bestS.gameObject.name}' at distance {best}");
            }
        }
        if (nearest == null)
        {
            Debug.LogWarning("DebugSpawnStorage: No Storage building found nearby.");
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

        var contents = stockage.GetAllRessources();
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
        if (storagePrefab == null)
        {
            Debug.LogWarning("DebugSpawnStorage: storagePrefab is not assigned.");
            return;
        }

        Vector3 pos = (spawnPoint != null) ? spawnPoint.position : transform.position;
        Colony owner = null;

        if (attachToNearestColony)
        {
            if (ColonieSystem.Instance != null)
            {
                var colonies = ColonieSystem.Instance.GetAllColonies();
                if (colonies != null && colonies.Count > 0)
                {
                    var nearest = colonies
                        .Where(c => c != null)
                        .OrderBy(c => Vector3.Distance(c.GetCenter(), pos))
                        .FirstOrDefault();

                    if (nearest != null && Vector3.Distance(nearest.GetCenter(), pos) <= searchRadius)
                    {
                        owner = nearest as Colony;
                        if (owner == null)
                        {
                            Debug.Log("DebugSpawnStorage: nearest colony is not a Colony concrete type (owner left null).");
                        }
                    }
                }
            }
            else
            {
                Debug.Log("DebugSpawnStorage: ColonieSystem.Instance is null, cannot attach to colony.");
            }
        }

        if (BuildingEvents.OnSpawnRequested == null)
        {
            Debug.LogWarning("DebugSpawnStorage: no listener for BuildingEvents.OnSpawnRequested found — ensure MapBuildingManager is present in the scene.");
        }

        BuildingEvents.OnSpawnRequested?.Invoke(storagePrefab, pos, Quaternion.identity, owner, "Storage");
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
        if (attachToNearestColony && ColonieSystem.Instance != null)
        {
            var colonies = ColonieSystem.Instance.GetAllColonies();
            if (colonies != null && colonies.Count > 0)
            {
                var nearest = colonies
                    .Where(c => c != null)
                    .OrderBy(c => Vector3.Distance(c.GetCenter(), pos))
                    .FirstOrDefault();

                if (nearest != null)
                {
                    Gizmos.color = Color.cyan;
                    Gizmos.DrawWireSphere(nearest.GetCenter(), 0.5f);
#if UNITY_EDITOR
                    UnityEditor.Handles.Label(nearest.GetCenter() + Vector3.up * 1.2f, $"Nearest Colony Id={nearest.GetId()} dist={(Vector3.Distance(nearest.GetCenter(), pos)):0.0}");
#endif
                }
            }
        }
    }
}
