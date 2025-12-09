using System;
using System.Collections.Generic;
using UnityEngine;

public class AgentActions : MonoBehaviour
{
    private readonly float moveFactor = 1f;

    private AIInventory inventory;
    private AIStats stats;
    private PathFinding pathFinding;
    private List<Cell> currentPath;

    private Vector2 currentTargetWorld;

    [SerializeField]
    private List<LayerMask> ressourcesMask = new List<LayerMask>();
    private Animator myAnimator;

    public static event Func<RessourceType, List<Ressource>> GetRessources;
    public static event Func<Vector2Int, Vector3> CellToWorld;

    [Header("Building Prefabs")]
    public GameObject storagePrefab;

    private Building _nearestBuilding;
    public Action<Building> OnNearestBuildingChanged;

    private void Awake()
    {
        inventory = GetComponent<AIInventory>();
        stats = GetComponent<AIStats>();
        pathFinding = new PathFinding();
        myAnimator = GetComponent<Animator>();

        MapEditorScript.OnGraphChange += RebuildPathIfNeeded;
    }

    private void RebuildPathIfNeeded(Cell _modifiedCell)
    {
        if(currentPath == null || currentPath.Count == 0 || !currentPath.Contains(_modifiedCell))
        { return; }

        CalculPath();
    }

    private void Start()
    {
        BuildingEvents.OnBuildingSpawned += OnBuildingSpawned;
        BuildingEvents.OnBuildingsChanged += OnBuildingsChanged;

        UpdateNearestBuilding();
    }

    private void OnDestroy()
    {
        BuildingEvents.OnBuildingSpawned -= OnBuildingSpawned;
        BuildingEvents.OnBuildingsChanged -= OnBuildingsChanged;
    }
    
    public Building FindNearestBuilding(string typeFilter = null)
    {
        if (BuildingEvents.GetNearestBuilding != null)
        {
            return BuildingEvents.GetNearestBuilding.Invoke(transform.position, typeFilter);
        }
        return null;
    }

    public Building GetNearestBuilding()
    {
        return _nearestBuilding;
    }

    public List<Cell> GetPath()
    {
        return currentPath;
    }

    private void MoveAgent(Vector2 _dir)
    {
        transform.position = transform.position + (Vector3)(moveFactor * Time.deltaTime * _dir);
    }

    private void CalculPath()
    {
        currentPath = pathFinding.FindPath(transform.position, currentTargetWorld);
    }

    public bool MoveTo(Vector2 _targetWorld)
    {
        if (pathFinding == null)
        {
            Debug.LogError("AgentActions.MoveTo: pathFinding is null. Aborting MoveTo.");
            return true;
        }

        if (myAnimator == null)
        {
            myAnimator = GetComponent<Animator>();
            if (myAnimator == null)
            {
                Debug.LogWarning("AgentActions.MoveTo: Animator not found on agent.");
            }
        }

        if (currentPath == null)
        {
            myAnimator.SetBool("isWalking", false);
            currentTargetWorld = _targetWorld;
            CalculPath();

            if (currentPath == null || currentPath.Count == 0)
            {
                return true;
            }
        }

        Cell nextCell = pathFinding.PeekNextPoint();
        if (nextCell == null)
        {
            if (myAnimator != null) myAnimator.SetBool("isWalking", false);
            currentPath = null;
            return true;
        }

        Vector3 nextWorld;
        if (CellToWorld != null)
        {
            nextWorld = CellToWorld.Invoke(nextCell.position);
        }
        else
        {
            Graph graph = UnityEngine.Object.FindObjectOfType<Graph>();
            if (graph != null)
            {
                nextWorld = graph.CellToWorld(nextCell.position);
            }
            else
            {
                Debug.LogError("AgentActions: No CellToWorld delegate and no Graph found in scene. Using agent position as fallback.");
                nextWorld = transform.position;
            }
        }

        Vector2 dir = ((Vector2)nextWorld - (Vector2)transform.position).normalized;

        MoveAgent(dir);
        if (myAnimator != null) myAnimator.SetBool("isWalking", true);

        if (Vector2.Distance(transform.position, nextWorld) < 0.2f)
        {
            pathFinding.AdvancePoint();
            return false;
        }

        return false;
    }

    public bool MoveTo(Transform _target)
    {
        if (_target == null)
        {
            Debug.LogWarning("AgentActions.MoveTo called with null target; aborting move.");
            return true;
        }
        return MoveTo(_target.position);
    }

    public void ReproductSelf()
    {
        GameObject newPimus = Instantiate(gameObject, transform.position, Quaternion.identity, transform.parent);
        newPimus.name = "Pimus";
    }

    public void HarvrestRessources(RessourceType _ressource)
    {
        switch (_ressource)
        {
            case RessourceType.food:
                HarvrestRessource(0, _ressource);
                break;

            case RessourceType.wood:
                HarvrestRessource(1, _ressource);
                break;
        }

        TryAutoCreateStorage();
    }

    private void HarvrestRessource(int _ressourceIndex, RessourceType _ressource)
    {
        RaycastHit2D[] hits = Physics2D.CircleCastAll(transform.position, 5, Vector2.zero, ressourcesMask[_ressourceIndex]);

        if (hits.Length == 0)
        {
            return;
        }

        foreach (RaycastHit2D hit in hits)
        {
            Ressource ressource = hit.collider.GetComponent<Ressource>();

            if (ressource.GetRessourceType() == _ressource)
            {
                if (inventory.AddRessources(1, ressource.GetRessourceType()))
                {
                    ressource.OnHarvrestingRessource();
                    return;
                }
            }
        }
    }

    private void TryAutoCreateStorage()
    {
        if (storagePrefab == null) return;
        if (inventory == null) return;

        RessourceStockedData data = inventory.GetRessources();
        if (data.ressource == RessourceType.wood && data.amount >= 5)
        {
            inventory.ResetRessource();
            Vector3 spawnPos = transform.position + (Vector3)UnityEngine.Random.insideUnitCircle.normalized * 1.5f;
            Colony owner = null;
            ColonyAgent ca = GetComponent<ColonyAgent>();
            if (ca != null)
            {
                owner = ca.GetCurrentColony() as Colony;
            }
            BuildingEvents.OnSpawnRequested?.Invoke(storagePrefab, spawnPos, Quaternion.identity, owner, "Storage");
        }
    }

    public void Eat()
    {
        inventory.RemoveOne();
        stats.SetHungerFull();
    }

    public bool HasRessource(RessourceType ressource)
    {
        return inventory.GetRessourceType() == ressource;
    }

    public Transform GetNearestFoodRessource(RessourceType _ressourceType)
    {
        return GetRessources.Invoke(_ressourceType, transform);
    }

    private void OnDestroy()
    {
        MapEditorScript.OnGraphChange -= RebuildPathIfNeeded;
    }
    
    private void OnBuildingSpawned(Building b)
    {
        UpdateNearestBuilding();
    }

    private void OnBuildingsChanged()
    {
        UpdateNearestBuilding();
    }

    private void UpdateNearestBuilding()
    {
        Building found = FindNearestBuilding(null);
        if (found != _nearestBuilding)
        {
            _nearestBuilding = found;
            OnNearestBuildingChanged?.Invoke(_nearestBuilding);
        }
    }
}
