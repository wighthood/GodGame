using System.Collections.Generic;
using UnityEngine;

public class TaskWandering : TaskBase
{
    Vector3 targetPos;
    private bool isFinished;
    public List<Cell> pathDebug = new();
    Transform transform;

    public TaskWandering(TaskManager _manager, AgentActions _actions)
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
        return 0.2f;
    }

    public override void OnFinish()
    {
        
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
