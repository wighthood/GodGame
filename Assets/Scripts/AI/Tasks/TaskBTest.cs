using System.Collections.Generic;
using UnityEngine;

[System.Serializable]
public class TaskBTest : TaskBase
{
    Vector3 targetPos;
    private bool isFinished;
    public List<Cell> pathDebug = new();
    Transform transform;

    public TaskBTest(TaskManager _manager, AgentActions _actions)
    {
        Init(_manager, _actions);
        transform = manager.agentBlackboard.GetValue<Transform>("transform");
    }

    public override bool Do()
    {
        Debug.Log("Doing B");
        isFinished = actions.MoveTo(targetPos);
        pathDebug = actions.GetPath();
        return FinishCondition();
    }

    public override float GetPriority()
    {
        Vector3 pos = transform.position;
        float distancePriorityFactory = Vector2.Distance(pos, targetPos) - 0.5f;
        return Mathf.Clamp(distancePriorityFactory, 0, 1);
    }

    public override void OnFinish()
    {
        //actions.ReproductSelf();
    }

    public override void OnStart()
    {
        Vector3 pos = transform.position;
        targetPos.Set(pos.x + Random.Range(-5, 5), pos.y + Random.Range(-5, 5), 0);
    }

    protected override bool FinishCondition()
    {
        bool cond = isFinished;
        return cond;
    }
}
