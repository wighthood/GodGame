using UnityEngine;

public abstract class TaskHarverestBase : TaskBase
{
    protected RessourceType ressource;
    protected uint numberMin;
    protected Transform targetRessource;
    protected Transform transform;
    protected Storage storage;

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
        uint actualNumberStocked = actions.GetStoredRessource(RessourceType.food);

        return 1 - ((float)actualNumberStocked / (float)numberMin);
    }

    public override bool Do()
    {
        if (actions.GetRessourceTransported() == ressource && actions.GetRessourceTransportedNumber() < 5)
        {
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
                if (Vector3.Distance(transform.position, targetRessource.position) < 0.5f)
                {
                    actions.Harvrest(ressource);
                    if(actions.GetRessourceTransportedNumber() < 5)
                    {
                        return false;
                    }
                }
                else
                {
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
}
