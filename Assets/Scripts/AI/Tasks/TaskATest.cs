using UnityEngine;

[System.Serializable]
public class TaskATest : TaskBase
{
    public TaskATest(TaskManager _manager, AgentActions _actions)
    {
        Init(_manager, _actions);
    }

    public override void Cancel()
    {
        throw new System.NotImplementedException();
    }

    public override bool Do()
    {
        Debug.Log("Doing A");
        actions.MoveTo(new Vector2(0, 0));
        return FinishCondition();
    }

    public override float GetPriority()
    {
        Vector3 pos = manager.agentBlackboard.GetValue<Vector3>("position");
        float distancePriorityFactory = Vector2.Distance(pos, new Vector2(0, 0)) - 0.5f;
        return Mathf.Clamp(distancePriorityFactory, 0, 1);
    }

    public override void OnFinish()
    {
    }

    public override void OnStart()
    {
    }

    protected override bool FinishCondition()
    {
        Vector3 pos = manager.agentBlackboard.GetValue<Vector3>("position");
        Vector3 targetPos = new Vector3(0, 0, 0);
        bool cond = (int)pos.x <= (int)targetPos.x;
        return cond;
    }
}
