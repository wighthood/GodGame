using System.Collections.Generic;
using UnityEngine;

public class TaskEat : TaskBase
{
    private Transform targetFoodSource;
    private bool isArrive;
    public List<Cell> pathDebug = new();
    Transform transform;

    public TaskEat(TaskManager _manager, AgentActions _actions)
    {
        Init(_manager, _actions);
        transform = manager.agentBlackboard.GetValue<Transform>("transform");
    }

    private void GetNearestFoodIfExiste()
    {
        Transform target = actions.GetNearestFoodRessource(RessourceType.food);

        if (target == null)
        {
            Cancel();
            return;
        }
        targetFoodSource = target;
    }

    public override void Cancel()
    {
        base.Cancel();
        Debug.Log("Ya pas à manger");
    }

    public override bool Do()
    {
        if (FinishCondition())
        {
            return true;
        }
        //TODO check in colonie inventory if there is food, if yes go take it
        else
        {
            if (targetFoodSource == null)
            {
                GetNearestFoodIfExiste();
            }
            Vector3 selfPosition = transform.position;
            if (isArrive)
            {
                actions.HarvrestRessources(RessourceType.food);
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
        Debug.Log("eat priority : " + Mathf.Sqrt(hunger));
        return Mathf.Sqrt(hunger);
    }

    public override void OnFinish()
    {
        actions.Eat();
    }

    public override void OnStart()
    {
        Debug.Log("Hungry !");
    }

    protected override bool FinishCondition()
    {
        return actions.HasRessource(RessourceType.food);
    }
}
