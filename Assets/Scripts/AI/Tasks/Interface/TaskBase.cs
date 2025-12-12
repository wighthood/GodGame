using UnityEditor;
using UnityEngine;

[System.Serializable]
public abstract class TaskBase : ScriptableObject
{
    protected TaskManager manager;
    protected AgentActions actions;

    public virtual void Init(TaskManager _manager, AgentActions _actions)
    {
        manager = _manager; 
        actions = _actions;
    }

    protected abstract bool FinishCondition();

    public abstract void OnStart();

    public abstract bool Do();

    public abstract void OnFinish();

    public abstract float GetPriority();

    public virtual void DrawActionsGizmo()
    {
        
    }
}
