using System.Collections.Generic;
using UnityEditor;
using UnityEngine;

[CreateAssetMenu(fileName = "BuildHouse", menuName = "Tasks/BuildHouse")]
public class TaskBuildHouse : TaskBase
{
    [SerializeField] private BuildingTable buildingTable;

    Vector3? batimentPosition;
    public List<Cell> pathDebug = new();
    bool isArrived;
    AIInventory inventory;

    bool isArrivedToRessource;
    Transform targetRessource;
    Transform storagePosition;

    public override bool Do()
    {
        if (batimentPosition == null) { return true; }

        if(actions.GetPath() != null && actions.GetPath().Count > 0)
        {
            pathDebug = actions.GetPath();
        }

        if(!HasEnoughRessources())
        {
            RessourceCollection ressourceCollection =  buildingTable.ressourcesNeeded[0];

            if (manager.colonieBlackboard != null && actions.GetStorage() != null)
            {
                if(storagePosition == null)
                {
                    storagePosition = actions.GetStorage().transform;
                }

                if(isArrivedToRessource)
                {
                    actions.TakeRessourcesFromStorage(ressourceCollection.RessourceType, ressourceCollection.number);
                }
                else
                {
                    isArrivedToRessource = actions.MoveTo(storagePosition);
                }
            }
            else
            {
                if (!targetRessource)
                {
                    targetRessource = actions.GetNearestRessource(ressourceCollection.RessourceType);
                    return false;
                }

                if (isArrivedToRessource)
                {
                    actions.Harvrest(ressourceCollection.RessourceType);
                }
                else
                {
                    isArrivedToRessource = actions.MoveTo(targetRessource.position);
                }
            }

            return false;
        }

        isArrived = actions.MoveTo((Vector2)batimentPosition);
        
        return FinishCondition();
    }

    public override float GetPriority()
    {
        if (manager.colonieBlackboard == null)
        {
            return 0;
        }

        int actualColonyPop = manager.colonieBlackboard.GetValue<int>("Habitant");
        int maxColonyPop = manager.colonieBlackboard.GetValue<int>("MaxHabitant");

        return (float)actualColonyPop / (float)maxColonyPop;
    }

    public override void OnFinish()
    {
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
        isArrivedToRessource = false;

        batimentPosition = actions.GetValidBuildPosition();

        if (batimentPosition == null)
        {
            isArrived = true;
        }
    }

    protected override bool FinishCondition()
    {
        return isArrived;
    }

    private bool HasEnoughRessources()
    {
        foreach (RessourceCollection ressourceCollection in buildingTable.ressourcesNeeded)
        {
            if (inventory.GetRessourceType() != ressourceCollection.RessourceType) { continue; }

            if(inventory.GetRessources().amount == ressourceCollection.number)
            { return true; }
        }

        return false;
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
