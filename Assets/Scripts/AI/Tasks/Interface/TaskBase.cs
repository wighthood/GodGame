using System;
using UnityEngine;
#if UNITY_EDITOR
using UnityEditor;
#endif

[Serializable]
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

#if UNITY_EDITOR
    public virtual void DrawActionsGizmo()
    {
        GUIStyle style = new GUIStyle();
        style.normal.textColor = Color.green;
        Handles.Label(manager.transform.position + Vector3.up * 0.75f + Vector3.left, $"doing {name}", style);
    }
#endif
}
