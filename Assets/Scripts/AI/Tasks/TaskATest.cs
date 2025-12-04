using UnityEngine;

[System.Serializable]
public class TaskATest : TaskBase
{
    Vector2 targetPos = new Vector2(0.5f, 0.5f);
    private bool isFinished;

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
        isFinished = actions.MoveTo(targetPos);
        return FinishCondition();
    }

    public override float GetPriority()
    {
        Vector3 pos = manager.agentBlackboard.GetValue<Transform>("transform").position;
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
        /*Vector3 pos = manager.agentBlackboard.GetValue<Transform>("transform").position;
        bool cond = Vector3.Distance(pos, targetPos) <= 0.5f;*/
        bool cond = isFinished;
        return cond;
    }
}
