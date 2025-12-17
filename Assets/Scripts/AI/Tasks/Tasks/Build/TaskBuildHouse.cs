using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "BuildHouse", menuName = "Tasks/BuildHouse")]
public class TaskBuildHouse : TaskBase
{
    [SerializeField] private BuildingTable buildingTable;

    Vector3? batimentPosition;
    public List<Cell> path = new();
    AIInventory inventory;

    Vector3? targetRessource;
    Transform transform;
    protected Storage storage;

    public override void Init(TaskManager _manager, AgentActions _actions)
    {
        base.Init(_manager, _actions);
        transform = actions.transform;
    }

    public override bool Do()
    {
        if (batimentPosition == null) { return true; }

        if (CanBuild())
        {
            path = actions.GetPath();
            actions.MoveTo((Vector2)batimentPosition);
            return FinishCondition();
        }
        else
        {
            if (targetRessource == null)
            {
                GetNearestIfExiste();
                if (targetRessource == null)
                {
                    return true;
                }
            }
            else if (Vector3.Distance(transform.position, (Vector3)targetRessource) < 0.5f)
            {
                if(IsNextToRessource())
                {
                    actions.Harvrest(buildingTable.ressourcesNeeded[0].RessourceType);
                    if (!CanBuild())
                    {
                        return false;
                    }
                }
                else
                {
                    GetNearestIfExiste();
                    if (targetRessource == null)
                    {
                        return true;
                    }
                }
                    
            }
            else
            {
                path = actions.GetPath();
                actions.MoveTo((Vector3)targetRessource);

                if (path == null)
                {
                    GetNearestIfExiste();
                }
            }
        }

        return false;
    }

    private bool IsNextToRessource()
    {
        return Physics2D.CircleCast(transform.position, 0.5f, Vector2.zero, 0.5f, actions.ressourcesMask[(int)buildingTable.ressourcesNeeded[0].RessourceType - 1]);
    }

    private void GetNearestIfExiste()
    {
        Vector3? target = actions.GetNearestRessource(buildingTable.ressourcesNeeded[0].RessourceType);

        if (target == null)
        {
            return;
        }
        targetRessource = target;
    }

    private bool CanBuild()
    {
        return actions.GetRessourceTransported() == buildingTable.ressourcesNeeded[0].RessourceType && actions.GetRessourceTransportedNumber() >= buildingTable.ressourcesNeeded[0].number;
    }

    public override float GetPriority()
    {
        if (manager.colonieBlackboard == null)
        {
            return 0;
        }
        if (storage == null && actions.GetStorage())
        {
            storage = actions.GetStorage();
        }

        int actualColonyPop = manager.colonieBlackboard.GetValue<int>("Habitant");
        int maxColonyPop = manager.colonieBlackboard.GetValue<int>("MaxHabitant");

        return (float)actualColonyPop / (float)maxColonyPop;
    }

    public override void OnFinish()
    {
        if (!CanBuild())
        {
            return;
        }

        inventory.RemoveRessources(buildingTable.ressourcesNeeded[0].number);
        actions.StartBuild(2f, buildingTable.buildType);
    }

    public override void OnStart()
    {
        if (!inventory)
        {
            inventory = manager.GetComponent<AIInventory>();
        }

        targetRessource = null;

        batimentPosition = actions.GetValidBuildPosition();
    }

    protected override bool FinishCondition()
    {
        return Vector3.Distance(transform.position, (Vector2)batimentPosition) < 0.25f && CanBuild();
    }

#if UNITY_EDITOR
    public override void DrawActionsGizmo()
    {
        base.DrawActionsGizmo();

        if (targetRessource != null)
        {
            Gizmos.color = Color.red;
            Gizmos.DrawCube((Vector3)targetRessource, new Vector3(0.5f, 0.5f, 0.1f));
        }

        if (path == null || path.Count == 0)
            return;

        Gizmos.color = Color.green;

        for (int i = 0; i < path.Count - 1; i++)
        {
            Vector2 firstPos = new();
            firstPos.Set(path[i].position.x + 0.5f, path[i].position.y + 0.5f);
            Vector2 secPos = new();
            secPos.Set(path[i + 1].position.x + 0.5f, path[i + 1].position.y + 0.5f);
            Gizmos.DrawLine(firstPos, secPos);
        }
    }
#endif
}
