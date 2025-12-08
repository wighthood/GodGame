using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "Wandering", menuName = "Tasks/Wandering")]
public class TaskWandering : TaskBase
{
    Vector3 targetPos;
    private bool isFinished;
    public List<Cell> pathDebug = new();
    Transform transform;

    public override void Init(TaskManager _manager, AgentActions _actions)
    {
        base.Init(_manager, _actions);
        transform = _manager.agentBlackboard.GetValue<Transform>("transform");
    }

    public override bool Do()
    {
        isFinished = actions.MoveTo(targetPos);
        pathDebug = actions.GetPath();
        return FinishCondition();
    }

    public override float GetPriority()
    {
        return 0.5f;
    }

    public override void OnFinish()
    {
        
    }

    public override void OnStart()
    {
        Vector3 pos = transform.position;
        targetPos.Set(pos.x + Random.Range(-8, 8), pos.y + Random.Range(-8, 8), 0);
    }

    protected override bool FinishCondition()
    {
        bool cond = isFinished;
        return cond;
    }
}
