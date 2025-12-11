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

    public static event Func<RessourceType, Transform, Transform> GetRessources;
    public static event Func<Vector2Int, Vector3> CellToWorld;

    ColonyAgent colonyAgent;

    [Header("Building Prefabs")]
    public GameObject storagePrefab;

    public Action<Building> OnNearestBuildingChanged;

    private Vector3 lastPos;
    public Vector3 Velocity => (transform.position - lastPos) / Time.deltaTime;

    private void Awake()
    {
        inventory = GetComponent<AIInventory>();
        stats = GetComponent<AIStats>();
        colonyAgent = GetComponent<ColonyAgent>();
        pathFinding = new PathFinding();

        MapEditorScript.OnGraphChange += RebuildPathIfNeeded;
    }

    private void RebuildPathIfNeeded(Cell _modifiedCell)
    {
        if (currentPath == null || currentPath.Count == 0 || !currentPath.Contains(_modifiedCell))
        { return; }

        CalculPath();
    }

    private void OnDestroy()
    {
        MapEditorScript.OnGraphChange -= RebuildPathIfNeeded;
    }

    public List<Cell> GetPath()
    {
        return currentPath;
    }

    private void MoveAgent(Vector2 _dir)
    {
        lastPos = transform.position;
        transform.position = transform.position + (Vector3)(moveFactor * Time.deltaTime * _dir);
    }

    private void CalculPath()
    {
        currentPath = pathFinding.FindPath(transform.position, currentTargetWorld);
    }

    public bool MoveTo(Vector2 _targetWorld)
    {
        if (currentPath == null)
        {
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
            currentPath = null;
            return true;
        }

        Vector3 nextWorld;
        nextWorld = CellToWorld.Invoke(nextCell.position);
        Vector2 dir = ((Vector2)nextWorld - (Vector2)transform.position).normalized;

        MoveAgent(dir);

        if (Vector2.Distance(transform.position, nextWorld) < 0.2f)
        {
            pathFinding.AdvancePoint();
            return false;
        }

        return false;
    }

    public bool MoveTo(Transform _target)
    {
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

        //TryAutoCreateStorage();
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

    /*private void TryAutoCreateStorage()
    {
        if (storagePrefab == null) return;

        RessourceStockedData data = inventory.GetRessources();
        if (data.ressource == RessourceType.wood && data.amount >= 5)
        {
            inventory.ResetRessource();
            Vector3 spawnPos = transform.position + (Vector3)UnityEngine.Random.insideUnitCircle.normalized * 1.5f;
            Colony owner = null;
            ColonyAgent agent = GetComponent<ColonyAgent>();
            if (agent != null)
            {
                owner = agent.GetCurrentColony() as Colony;
            }
            BuildingEvents.OnSpawnRequested?.Invoke(storagePrefab, spawnPos, Quaternion.identity, owner, "Storage");
        }
    }*/

    public void Eat()
    {
        inventory.RemoveOne();
        stats.SetHungerFull();
    }

    public bool HasRessource(RessourceType _ressource)
    {
        return inventory.GetRessourceType() == _ressource;
    }

    public Transform GetNearestFoodRessource(RessourceType _ressourceType)
    {
        return GetRessources.Invoke(_ressourceType, transform);
    }

    public Vector3? GetValidBuildPosition()
    {
        return null;
    }
}
