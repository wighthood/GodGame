using System.Collections.Generic;
using UnityEngine;

public class AgentActions : MonoBehaviour
{
    private readonly float moveFactor = 0.01f;

    private AIInventory inventory;
    private AIStats stats;

    [SerializeField]
    private GameObject pimusPrefab;

    [SerializeField]
    private List<LayerMask> ressourcesMask = new List<LayerMask>();

    [HideInInspector]
    public BlackBoard agentBlackboard;

    private void Awake()
    {
        agentBlackboard = GetComponent<BlackBoard>();
        inventory = GetComponent<AIInventory>();
        stats = GetComponent<AIStats>();
    }

    private void Start()
    {
        agentBlackboard.AddValue("position", transform.position);
    }

    private void MoveAgent(Vector2 _movementAddition)
    {
        transform.position = (Vector2)transform.position + _movementAddition;
        agentBlackboard.ModifyValue("position", transform.position);
    }

    public void MoveTo(Vector2 _point)
    {
        Vector2 dir = (_point - (Vector2)transform.position).normalized;
        MoveAgent(dir * moveFactor);
    }

    public void MoveTo(Transform _target)
    {
        MoveTo(_target.position);
    }

    public void ReproductSelf()
    {
        GameObject newPimus = Instantiate(pimusPrefab, transform.position, Quaternion.identity, transform.parent);
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

        if(inventory.AddRessources(1, ressource.GetRessourceType()))
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
        foreach(Ressource ressource in ressources)
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
