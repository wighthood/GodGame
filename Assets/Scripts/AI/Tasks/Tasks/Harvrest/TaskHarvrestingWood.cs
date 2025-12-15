using UnityEngine;

[CreateAssetMenu(fileName = "HarvrestWood", menuName = "Tasks/HarvrestWood")]
public class TaskHarvrestingWood : TaskHarverestBase
{
    public override void Init(TaskManager _manager, AgentActions _action, uint _numberMin)
    {
        base.Init(_manager, _action, _numberMin);
        ressource = RessourceType.wood;
    }
}
