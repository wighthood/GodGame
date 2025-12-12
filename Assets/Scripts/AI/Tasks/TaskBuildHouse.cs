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

    public override bool Do()
    {
        if (batimentPosition == null) { return true; }

        if(actions.GetPath() != null && actions.GetPath().Count > 0)
        {
            pathDebug = actions.GetPath();
        }

        if(!HasEnoughRessources())
        {
            //TODO check dans le stockage

            if(!targetRessource)
            {
                targetRessource = actions.GetNearestFoodRessource(RessourceType.wood);
                return false;
            }

            if(isArrivedToRessource)
            {
                actions.HarvrestRessources(RessourceType.wood);
            }
            else
            {
                isArrivedToRessource = actions.MoveTo(targetRessource.position);
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

        //Debug.Log($"build priority : {(float)actualColonyPop / (float)maxColonyPop}");

        return (float)actualColonyPop / (float)maxColonyPop;
    }

    public override void OnFinish()
    {
        inventory.RemoveRessources(5);
        actions.StartBuild(2f, BuildType.House);
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
        GUIStyle style = new GUIStyle();
        style.normal.textColor = Color.green;
        Handles.Label(manager.transform.position + Vector3.up * 0.5f, $"doing BuildHouse task", style);

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
