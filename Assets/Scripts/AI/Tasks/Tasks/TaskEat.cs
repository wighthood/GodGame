using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "Eat", menuName = "Tasks/Eat")]
public class TaskEat : TaskBase
{
    private bool isArrivedToRessource;
    public List<Cell> path = new List<Cell>();
    private Transform storageTransform;
    private Vector3? targetFoodSource;
    private Transform transform;

    public override void Init(TaskManager _manager, AgentActions _actions)
    {
        base.Init(_manager, _actions);
        transform = _manager.agentBlackboard.GetValue<Transform>("transform");
    }

    private void GetNearestFoodIfExiste()
    {
        Vector3? target = actions.GetNearestRessource(RessourceType.food);

        if (target == null)
        {
            return;
        }
        targetFoodSource = (Vector3)target;
    }

    public override bool Do()
    {
        if (FinishCondition())
        {
            return true;
        }
        if (manager.colonieBlackboard != null && actions.GetStorage() != null && actions.HasRessourceInColony(RessourceType.food))
        {
            if (storageTransform == null)
            {
                storageTransform = actions.GetStorage().transform;
                return false;
            }

            if (isArrivedToRessource)
            {
                actions.TakeRessourcesFromStorage(RessourceType.food, 1);
                return FinishCondition();
            }
            isArrivedToRessource = actions.MoveTo(storageTransform);
            return false;
        }
        if (targetFoodSource == null)
        {
            GetNearestFoodIfExiste();
            if (targetFoodSource == null)
            {
                return false;
            }
        }
        else if (Vector3.Distance(transform.position, (Vector3)targetFoodSource) < 0.5f)
        {
            if (IsNextToRessource())
            {
                actions.Harvrest(RessourceType.food);
            }
            else
            {
                GetNearestFoodIfExiste();
                if (targetFoodSource == null)
                {
                    return false;
                }
            }
        }
        else
        {
            path = actions.GetPath();
            actions.MoveTo((Vector3)targetFoodSource);

            if (path == null)
            {
                GetNearestFoodIfExiste();
            }
        }

        return false;
    }

    private bool IsNextToRessource()
    {
        return Physics2D.CircleCast(transform.position, 0.5f, Vector2.zero, 0.5f, actions.ressourcesMask[(int)RessourceType.food - 1]);
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
        targetFoodSource = null;
    }

    protected override bool FinishCondition()
    {
        return actions.HasRessource(RessourceType.food);
    }

    #if UNITY_EDITOR
    public override void DrawActionsGizmo()
    {
        base.DrawActionsGizmo();

        if (path == null || path.Count == 0)
            return;

        Gizmos.color = Color.green;

        for (int i = 0; i < path.Count - 1; i++)
        {
            Vector2 firstPos = new Vector2();
            firstPos.Set(path[i].position.x + 0.5f, path[i].position.y + 0.5f);
            Vector2 secPos = new Vector2();
            secPos.Set(path[i + 1].position.x + 0.5f, path[i + 1].position.y + 0.5f);
            Gizmos.DrawLine(firstPos, secPos);
        }
    }
    #endif
}
