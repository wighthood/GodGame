using System.Collections.Generic;
using UnityEngine;

public class AgentActions : MonoBehaviour
{
    private readonly float moveFactor = 1f;

    private AIInventory inventory;
    private AIStats stats;
    private PathFinding pathFinding;
    private List<Cell> currentPath;

    [SerializeField]
    private List<LayerMask> ressourcesMask = new List<LayerMask>();

    private void Awake()
    {
        inventory = GetComponent<AIInventory>();
        stats = GetComponent<AIStats>();
        pathFinding = new();
    }

    public List<Cell> GetPath()
    {
        return currentPath;
    }

    private void MoveAgent(Vector2 _dir)
    {
        transform.position = transform.position + (Vector3)(_dir * moveFactor * Time.deltaTime);
    }

    public bool MoveTo(Vector2 targetWorld)
    {
        currentPath = pathFinding.FindPath(transform.position, targetWorld);

        if (currentPath == null || currentPath.Count == 0)
        {
            return true;
        }

        if(currentPath.Count == 1)
        {
            Vector3 finalPos = Graph.instance.CellToWorld(currentPath[0].position);
            if (Vector2.Distance(transform.position, finalPos) < 0.1f)
            {
                currentPath = null;
                return true;
            }
        }

        print(currentPath.Count);

        Cell nextCell = currentPath[0];
        Vector3 nextWorld = Graph.instance.CellToWorld(nextCell.position);

        Vector2 dir = ((Vector2)nextWorld - (Vector2)transform.position).normalized;

        MoveAgent(dir);

        if (Vector2.Distance(transform.position, nextWorld) < 0.1f)
        {
            pathFinding.GoToNextPoint();
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

    public void HarvrestRessources(RessourceType ressource)
    {
        switch (ressource)
        {
            default:
                break;

            case RessourceType.food:
                HarvrestRessource(0);
                break;

            case RessourceType.wood:
                HarvrestRessource(1);
                break;
        }
    }

    private void HarvrestRessource(int ressourceIndex)
    {
        RaycastHit2D hit = Physics2D.CircleCast(transform.position, 5, Vector2.zero, ressourcesMask[ressourceIndex]);

        if (!hit || hit.collider == null || !hit.collider.GetComponent<Ressource>())
        {
            return;
        }

        Ressource ressource = hit.collider.GetComponent<Ressource>();

        if (inventory.AddRessources(1, ressource.GetRessourceType()))
        { ressource.OnHarvrestingRessource(); }
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

    public Transform GetNearestFoodRessource(RessourceType ressourceType)
    {
        float nearestDistance = float.MaxValue;
        List<Ressource> ressources = MapRessourceManager.Get().GetRessources(ressourceType);
        Transform nearestRessource = ressources[0].transform;
        foreach (Ressource ressource in ressources)
        {
            if (ressource.GetRessourceType() == ressourceType && Vector3.Distance(transform.position, ressource.transform.position) < nearestDistance)
            {
                nearestDistance = Vector3.Distance(transform.position, nearestRessource.transform.position);
                nearestRessource = ressource.transform;
            }
        }

        return nearestRessource;
    }
}
