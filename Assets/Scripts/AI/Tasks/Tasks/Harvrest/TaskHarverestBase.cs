using System.Collections.Generic;
using Unity.VisualScripting.FullSerializer;
using UnityEditor;
using UnityEngine;

public abstract class TaskHarverestBase : TaskBase
{
    protected RessourceType ressource;
    protected uint numberMin;
    protected Transform targetRessource;
    protected Transform transform;
    protected Storage storage;
    public List<Cell> pathDebug = new();

    public virtual void Init(TaskManager _manager, AgentActions _action, uint _numberMin)
    {
        base.Init(_manager, _action);
        transform = _action.transform;
        numberMin = _numberMin;
    }

    private void GetNearestIfExiste()
    {
        Transform target = actions.GetNearestRessource(ressource);

        if (target == null)
        {
            return;
        }
        targetRessource = target;
    }

    public override float GetPriority()
    {
        if (manager.colonieBlackboard == null || actions.GetStorage() == null) { return 0; }

        BlackBoard colonyBlackboard = manager.colonieBlackboard;
        storage = actions.GetStorage();
        uint actualNumberStocked = actions.GetStoredRessource(ressource);

        return 1 - ((float)actualNumberStocked / (float)numberMin);
    }

    public override bool Do()
    {
        if (actions.GetRessourceTransported() == ressource && actions.GetRessourceTransportedNumber() >= 5)
        {
            pathDebug = actions.GetPath();
            actions.MoveTo(storage.transform);
            return FinishCondition();
        }
        else
        {
            if (targetRessource == null)
            {
                GetNearestIfExiste();
                if (targetRessource == null)
                {
                    return true;
                }
            }
            else
            {
                if (targetRessource != null && Vector3.Distance(transform.position, targetRessource.position) < 0.5f)
                {
                    actions.Harvrest(ressource);
                    if (actions.GetRessourceTransportedNumber() < 5)
                    {
                        return false;
                    }
                }
                else
                {
                    pathDebug = actions.GetPath();
                    actions.MoveTo(targetRessource);
                }
            }
        }

        return false;
    }

    public override void OnFinish()
    {
        if (actions.GetRessourceTransported() == ressource)
        {
            actions.DropRessourcesOnStorage();
        }
    }

    public override void OnStart()
    {
        targetRessource = null;
    }

    protected override bool FinishCondition()
    {
        return Vector3.Distance(transform.position, storage.transform.position) < 0.5f;
    }

    public override void DrawActionsGizmo()
    {
        GUIStyle style = new GUIStyle();
        style.normal.textColor = Color.green;
        Handles.Label(manager.transform.position + Vector3.up * 0.5f + Vector3.left, $"doing Eat task", style);

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
