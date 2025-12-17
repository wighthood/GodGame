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
    public List<LayerMask> ressourcesMask = new List<LayerMask>();

    public static event Func<RessourceType, Vector2, Vector3?> GetRessources;
    public static event Func<Vector2Int, Vector3> CellToWorld;

    ColonyAgent colonyAgent;
    
    public Vector3 Velocity => (transform.position - lastPos) / Time.deltaTime;
    private Coroutine harvrestingCoroutine;

    [SerializeField] private GameObject reproductionFeedback;

    #region values for animations

    private Vector3 lastPos = Vector3.zero;
    public bool isBuilding { get; private set; }
    public bool isHarvesting { get; private set; }

    #endregion

    private void Awake()
    {
        inventory = GetComponent<AIInventory>();
        stats = GetComponent<AIStats>();
        colonyAgent = GetComponent<ColonyAgent>();
        pathFinding = new PathFinding();

        MapEditorScript.OnGraphChange += RebuildPathIfNeeded;
        lastPos = transform.position;
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
        StopAllCoroutines();
        harvrestingCoroutine = null;
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
        newPimus.name = colonyAgent.GetSpecies().ToString();
        Instantiate(reproductionFeedback, transform.position + Vector3.up, Quaternion.identity);
    }

    public void Harvrest(RessourceType _ressource)
    {
        if (harvrestingCoroutine != null) { return; }

        isHarvesting = true;
        harvrestingCoroutine = StartCoroutine(WaitAndHarvrest(0.5f, _ressource));
    }

    private IEnumerator WaitAndHarvrest(float _buildTime, RessourceType _ressource)
    {
        Ressource targetRessource = GetHarvrestRessource(_ressource);
        
        if (targetRessource == null)
        {
            isHarvesting = false;
            harvrestingCoroutine = null;
            yield break;
        }
        
        while (isHarvesting && targetRessource)
        {
            _buildTime -= Time.deltaTime;

            if (_buildTime <= 0)
            {
                isHarvesting = false;
            }

            yield return null;
        }
        
        if (targetRessource != null)
        {
            Harvrest(targetRessource.GetRessourceType());
            targetRessource.isBeeingHarversted = false;
        }
        isHarvesting = false;
        harvrestingCoroutine = null;
    }

    private Ressource GetHarvrestRessource(RessourceType _ressource)
    {
        RaycastHit2D[] hits = Physics2D.CircleCastAll(transform.position, 1, Vector2.zero, 1, ressourcesMask[(int)_ressource - 1]);

        if (hits.Length == 0) return null;

        Ressource ressourceToHarverest = null;

        foreach (RaycastHit2D hit in hits)
        {
            Ressource ressource = hit.collider.GetComponent<Ressource>();
            
            if (ressource.GetRessourceType() == _ressource)
            {
                if (ressourceToHarverest == null)
                {
                    ressourceToHarverest = ressource;
                    continue;
                }
                
                if (Vector3.Distance(ressourceToHarverest.transform.position, transform.position) > Vector3.Distance(ressource.transform.position, transform.position) && !ressource.isBeeingHarversted)
                {
                    ressourceToHarverest = ressource;
                }
            }
        }
        
        if (ressourceToHarverest != null)
        {
            if (inventory.AddRessources(1, ressourceToHarverest.GetRessourceType()))
            {
                ressourceToHarverest.OnHarvrestingRessource();
            }
        }
        
        return ressourceToHarverest;
    }

    public void Eat()
    {
        inventory.RemoveOne();
        stats.SetHungerFull();
    }

    public Storage GetStorage()
    {
        return ((Colony)colonyAgent.GetCurrentColony()).storage;
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

    public bool HasRessourceInColony(RessourceType _ressource)
    {
        return ((Colony)colonyAgent.GetCurrentColony()).storage.HasThisRessource(_ressource);
    }

    public void TakeRessourcesFromStorage(RessourceType _ressourceType, uint _number)
    {
        if (inventory.HasRessource() && inventory.GetRessourceType() != _ressourceType)
        {
            DropRessourcesOnStorage();
        }

        inventory.AddRessources(((Colony)colonyAgent.GetCurrentColony()).storage.GetRessourceNumber(_ressourceType),
            _ressourceType);
    }

    public void DropRessourcesOnStorage()
    {
        ((Colony)colonyAgent.GetCurrentColony()).storage.AddRessources(inventory.GetRessourceType(), inventory.GetRessources().amount);
        inventory.ResetRessource();
    }

    public RessourceType GetRessourceTransported()
    {
        return inventory.GetRessourceType();
    }

    public uint GetRessourceTransportedNumber()
    {
        return inventory.GetRessourceTransportedNumber();
    }

    public uint GetStoredRessource(RessourceType _ressourceType)
    {
        if (!((Colony)colonyAgent.GetCurrentColony()).storage)
        {
            return 0;
        }

        return ((Colony)colonyAgent.GetCurrentColony()).storage.GetRessourceNumber(_ressourceType);
    }

    public Vector3? GetNearestRessource(RessourceType _ressourceType)
    {
        return GetRessources.Invoke(_ressourceType, transform.position);
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

        if (currentNearestHouse == null)
        {
            return null;
        }

        return currentNearestHouse.transform;
    }

    public Vector3? GetValidBuildPosition()
    {
        if (colonyAgent.GetCurrentColony() != null)
        {
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

    public int GetBuildingNumberOfType(BuildType _type)
    {
        Colony colony = (Colony)colonyAgent.GetCurrentColony();

        if(colony == null) { return 0; }

        return BuildingEvents.OnGetBuildingCountOfTypeEvent?.Invoke(_type, colony) ?? 0;
    }
}
