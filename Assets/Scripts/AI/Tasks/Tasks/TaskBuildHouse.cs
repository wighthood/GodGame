using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "BuildHouse", menuName = "Tasks/BuildHouse")]
public class TaskBuildHouse : TaskBase
{
    [SerializeField] private BuildingTable buildingTable;

    Vector3? batimentPosition;
    public List<Cell> pathDebug = new();
    AIInventory inventory;

    Transform targetRessource;
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

        if (actions.GetRessourceTransported() == buildingTable.ressourcesNeeded[0].RessourceType && actions.GetRessourceTransportedNumber() >= buildingTable.ressourcesNeeded[0].number)
        {
            pathDebug = actions.GetPath();
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
            else
            {
                if (targetRessource != null && Vector3.Distance(transform.position, targetRessource.position) < 0.5f)
                {
                    actions.Harvrest(buildingTable.ressourcesNeeded[0].RessourceType);
                    if (!CanBuild())
                    {
                        return false;
                    }
                }
                else
                {
                    pathDebug = actions.GetPath();
                    actions.MoveTo(targetRessource);
                }
            }
        }

        return false;
    }

    private void GetNearestIfExiste()
    {
        Transform target = actions.GetNearestRessource(buildingTable.ressourcesNeeded[0].RessourceType);

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
        if(storage == null && actions.GetStorage())
        {
            storage = actions.GetStorage();
        }

        int actualColonyPop = manager.colonieBlackboard.GetValue<int>("Habitant");
        int maxColonyPop = manager.colonieBlackboard.GetValue<int>("MaxHabitant");

        return (float)actualColonyPop / (float)maxColonyPop;
    }

    public override void OnFinish()
    {
        if(!CanBuild())
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

        Debug.Log($"build position {(Vector2)batimentPosition}");
    }

    protected override bool FinishCondition()
    {
        return Vector3.Distance(transform.position, (Vector2)batimentPosition) < 0.25f && CanBuild();
    }

    public override void DrawActionsGizmo()
    {
        base.DrawActionsGizmo();

        if (pathDebug == null || pathDebug.Count == 0)
            return;

        Gizmos.color = Color.green;

        for (int i = 0; i < pathDebug.Count - 1; i++)
        {
            Vector2 firstPos = new();
            firstPos.Set(pathDebug[i].position.x + 0.5f, pathDebug[i].position.y + 0.5f);
            Vector2 secPos = new();
            secPos.Set(pathDebug[i + 1].position.x + 0.5f, pathDebug[i + 1].position.y + 0.5f);
            Gizmos.DrawLine(firstPos, secPos);
        }
    }
}
