using UnityEngine;

[CreateAssetMenu(fileName = "HarvrestFood", menuName = "Tasks/HarvrestFood")]
public class TaskHarvrestFood : TaskHarverestBase
{
    public override void Init(TaskManager _manager, AgentActions _action, uint _numberMin)
    {
        base.Init(_manager, _action, _numberMin);
        ressource = RessourceType.food;
    }

    public override float GetPriority()
    {
        if (manager.colonieBlackboard == null || actions.GetStorage() == null) { return 0; }

        BlackBoard colonyBlackboard = manager.colonieBlackboard;
        storage = actions.GetStorage();
        uint actualNumberStocked = actions.GetStoredRessource(ressource);
        int actualColonyPop = manager.colonieBlackboard.GetValue<int>("Habitant");

        return ((float)actualColonyPop - (float)actualNumberStocked) / 10;
    }
}
