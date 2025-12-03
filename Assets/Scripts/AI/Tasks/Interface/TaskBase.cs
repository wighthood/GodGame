[System.Serializable]
public abstract class TaskBase
{
    protected TaskManager manager;
    protected AgentActions actions;

    public void Init(TaskManager _manager, AgentActions _actions)
    {
        manager = _manager; 
        actions = _actions;
    }

    protected abstract bool FinishCondition();

    public abstract void OnStart();

    public abstract bool Do();

    public abstract void OnFinish();

    public virtual void Cancel()
    {
        manager.ResetTask();
    }

    public abstract float GetPriority();
}
