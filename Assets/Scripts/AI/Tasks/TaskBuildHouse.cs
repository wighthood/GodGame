using System.Collections.Generic;
using UnityEditor;
using UnityEngine;

[CreateAssetMenu(fileName = "BuildHouse", menuName = "Tasks/BuildHouse")]
public class TaskBuildHouse : TaskBase
{
    Vector3? batimentPosition;
    public List<Cell> pathDebug = new();
    bool isArrived;

    public override bool Do()
    {
        if(batimentPosition == null) { return true; }

        isArrived = actions.MoveTo((Vector2)batimentPosition);
        pathDebug = actions.GetPath();
        return FinishCondition();
    }

    public override float GetPriority()
    {
        if (manager.colonieBlackboard == null)
        {
            return 0;
        }

        int actualColonyPop = manager.colonieBlackboard.GetValue<int>("Habitant");
        int maxColonyPop = manager.colonieBlackboard.GetValue<int>("MaxHabitant");

        //Debug.Log($"build priority : {(float)actualColonyPop / (float)maxColonyPop}");

        return (float)actualColonyPop / (float)maxColonyPop;
    }

    public override void OnFinish()
    {
        actions.StartBuild(2f, BuildType.House);
    }

    public override void OnStart()
    {
        batimentPosition = actions.GetValidBuildPosition();

        if (batimentPosition == null)
        {
            isArrived = true;
        }
    }

    protected override bool FinishCondition()
    {
        return isArrived;
    }

    public override void DrawActionsGizmo()
    {
        GUIStyle style = new GUIStyle();
        style.normal.textColor = Color.green;
        Handles.Label(manager.transform.position + Vector3.up * 0.5f, $"doing BuildHouse task", style);

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
