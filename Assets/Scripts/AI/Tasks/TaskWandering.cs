using System.Collections.Generic;
using UnityEditor;
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
        return 0.35f;
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

    public override void DrawActionsGizmo()
    {
        GUIStyle style = new GUIStyle();
        style.normal.textColor = Color.green;
        Handles.Label(manager.transform.position + Vector3.up * 0.5f + Vector3.left, $"doing Wandering task", style);

        if (pathDebug == null || pathDebug.Count == 0)
            return;

        Gizmos.color = Color.green;

        for (int i = 0; i < pathDebug.Count - 1; i++)
        {
            Vector2 firstPos = new();
            firstPos.Set(pathDebug[i].position.x + 0.5f, pathDebug[i].position.y + 0.5f);
            Vector2 secPos = new();
            secPos.Set(pathDebug[i + 1].position.x + 0.5f, pathDebug[i + 1].position.y + 0.5f);
            Gizmos.DrawLine(firstPos, secPos);
        }
    }
}
