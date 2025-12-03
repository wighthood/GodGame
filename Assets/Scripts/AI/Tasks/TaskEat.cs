using Unity.VisualScripting.FullSerializer;
using UnityEngine;

public class TaskEat : TaskBase
{
    private Transform targetFoodSource;

    public TaskEat(TaskManager _manager, AgentActions _actions)
    {
        Init(_manager, _actions);
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
            Vector3 selfPosition = manager.agentBlackboard.GetValue<Vector3>("position");
            if (Vector2.Distance(selfPosition, targetFoodSource.position) < 1)
            {
                actions.HarvrestRessources(RessourceType.food);
            }
            else
            {
                actions.MoveTo(targetFoodSource);
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
