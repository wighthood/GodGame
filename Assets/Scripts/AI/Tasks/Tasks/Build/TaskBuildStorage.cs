using UnityEngine;

[CreateAssetMenu(fileName = "BuildStorage", menuName = "Tasks/BuildStorage")]
public class TaskBuildStorage : TaskBuildHouse
{
    public override float GetPriority()
    {
        if (manager.colonieBlackboard == null)
        {
            return 0;
        }

        Colony colony = (Colony)(manager.GetComponent<ColonyAgent>().GetCurrentColony());
        bool shouldBuildOne = !colony.storage;

        return shouldBuildOne == true ? 0 : 1;
    }

    public override void OnStart()
    {
        manager.colonieBlackboard.ModifyValue("HasStorage", true);

        base.OnStart();
    }
}
