using System.Collections.Generic;
using UnityEditor;
using UnityEngine;

[CreateAssetMenu(fileName = "Eat", menuName = "Tasks/Eat")]
public class TaskEat : TaskBase
{
    private Transform targetFoodSource;
    private bool isArrive;
    public List<Cell> pathDebug = new();
    Transform transform;
    private bool isArrivedToRessource;
    private Transform storageTransform;

    public override void Init(TaskManager _manager, AgentActions _actions)
    {
        base.Init(_manager, _actions);
        transform = _manager.agentBlackboard.GetValue<Transform>("transform");
    }

    private void GetNearestFoodIfExiste()
    {
        Transform target = actions.GetNearestRessource(RessourceType.food);

        if (target == null)
        {
            return;
        }
        targetFoodSource = target;
    }

    public override bool Do()
    {
        if (FinishCondition())
        {
            return true;
        }
        else if (manager.colonieBlackboard != null && actions.GetStorage() != null && actions.HasRessourceInColony(RessourceType.food))
        {
            if (storageTransform == null)
            {
                storageTransform = actions.GetStorage().transform;
            }

            if (isArrivedToRessource)
            {
                actions.TakeRessourcesFromStorage(RessourceType.food, 1);
                return FinishCondition();
            }
            else
            {
                isArrivedToRessource = actions.MoveTo(storageTransform);
                return false;
            }
        }
        else
        {
            if (targetFoodSource == null)
            {
                GetNearestFoodIfExiste();
                if (targetFoodSource == null)
                {
                    return true;
                }
            }

            Vector3 selfPosition = transform.position;
            if (isArrive)
            {
                actions.Harvrest(RessourceType.food);
            }
            else
            {
                isArrive = actions.MoveTo(targetFoodSource);
                pathDebug = actions.GetPath();
            }
            return false;
        }
    }

    public override float GetPriority()
    {
        float hunger = manager.agentBlackboard.GetValue<float>("hunger");
        return Mathf.Sqrt(hunger);
    }

    public override void OnFinish()
    {
        actions.Eat();
    }

    public override void OnStart()
    {
        //Debug.Log("Hungry !");
    }

    protected override bool FinishCondition()
    {
        return actions.HasRessource(RessourceType.food);
    }

    public override void DrawActionsGizmo()
    {
        GUIStyle style = new GUIStyle();
        style.normal.textColor = Color.green;
        Handles.Label(manager.transform.position + Vector3.up * 0.5f + Vector3.left, $"doing Eat task", style);

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
