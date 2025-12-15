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

        bool shouldBuildOne = manager.colonieBlackboard.GetValue<bool>("HasStorage");

        return shouldBuildOne == true ? 0 : 1;
    }

    public override void OnStart()
    {
        manager.colonieBlackboard.ModifyValue("HasStorage", true);

        base.OnStart();
    }
}
