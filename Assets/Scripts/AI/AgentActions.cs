using Mono.Cecil;
using System;
using System.Collections;
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

    public Action<Building> OnNearestBuildingChanged;

    #region values for animations

    private Vector3 lastPos;
    public Vector3 Velocity => (transform.position - lastPos) / Time.deltaTime;

    public bool isBuilding {  get; private set; }
    #endregion

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

        Ressource ressourceToHarverest = null;

        foreach (RaycastHit2D hit in hits)
        {
            Ressource ressource = hit.collider.GetComponent<Ressource>();

            if (ressource.GetRessourceType() == _ressource)
            {
                if(ressourceToHarverest == null) 
                { 
                    ressourceToHarverest = ressource;
                    continue;
                }

                if(Vector3.Distance(ressourceToHarverest.transform.position, transform.position) > Vector3.Distance(ressource.transform.position, transform.position))
                {
                    ressourceToHarverest = ressource;
                }
            }
        }

        if (inventory.AddRessources(1, ressourceToHarverest.GetRessourceType()))
        {
            ressourceToHarverest.OnHarvrestingRessource();
            return;
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

    public Storage GetStorage()
    {
        return ((Colony)colonyAgent.GetCurrentColony()).storage;
    }

    public void TakeRessourcesFromStorage(RessourceType _ressourceType, uint _number)
    {
        if(inventory.HasRessource() && inventory.GetRessourceType() != _ressourceType)
        {
            DropRessourcesOnStorage();
        }

        inventory.AddRessources(((Colony)colonyAgent.GetCurrentColony()).storage.GetRessourceNumber(_ressourceType)
            , _ressourceType);
    }

    public void DropRessourcesOnStorage()
    {
        ((Colony)colonyAgent.GetCurrentColony()).storage.AddRessources(inventory.GetRessourceType(), inventory.GetRessources().amount);
        inventory.ResetRessource();
    }

    public uint GetStoredfood()
    {
        if(((Colony)colonyAgent.GetCurrentColony()).storage)
        {
            return 0;
        }

        return ((Colony)colonyAgent.GetCurrentColony()).storage.GetRessourceNumber(RessourceType.food);
    }

    public bool HasRessource(RessourceType _ressource)
    {
        return inventory.GetRessourceType() == _ressource;
    }

    public Transform GetNearestFoodRessource(RessourceType _ressourceType)
    {
        return GetRessources.Invoke(_ressourceType, transform);
    }

    public Transform GetNearestHouse()
    {
        GameObject currentNearestHouse = null;

        foreach (GameObject building in ((Colony)colonyAgent.GetCurrentColony()).Buildings)
        {
            if(building.GetComponent<Building>().Type == BuildType.House)
            {
                if (currentNearestHouse == null)
                {
                    currentNearestHouse = building;
                }

                if(Vector3.Distance(transform.position, currentNearestHouse.transform.position) > Vector3.Distance(transform.position, building.transform.position))
                {
                    currentNearestHouse = building;
                }
            }
        }

        return currentNearestHouse.transform;
    }

    public Vector3? GetValidBuildPosition()
    {
        if (colonyAgent.GetCurrentColony() != null)
        {
            // return ((Colony)colonyAgent.GetCurrentColony()).GetValidBuildingPosition();
            
            if(BuildingEvents.OnGetBuildPositionEvent != null)
            {
               return BuildingEvents.OnGetBuildPositionEvent.Invoke(transform.position, colonyAgent.GetCurrentColony());
            }
        }

        return null;
    }

    private void Build(BuildType _buildType)
    {
        BuildingEvents.OnSpawnRequestedEvent?.Invoke(_buildType, transform.position, (Colony)colonyAgent.GetCurrentColony());

        BuildingEvents.OnBuildingSpawnedEvent?.Invoke(_buildType, (Colony)colonyAgent.GetCurrentColony());
    }

    public void StartBuild(float _buildTime, BuildType _buildType)
    {
        isBuilding = true;

        StartCoroutine(WaitAndBuild(_buildTime, _buildType));
    }

    private IEnumerator WaitAndBuild(float _buildTime, BuildType _buildType)
    {
        while (isBuilding)
        {
            _buildTime -= Time.deltaTime;

            if( _buildTime <= 0 )
            {
                isBuilding = false;
            }

            yield return null;
        }

        Build(_buildType);
    }
}
