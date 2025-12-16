using UnityEngine;

[CreateAssetMenu(fileName = "BuildFarm", menuName = "Tasks/BuildFarm")]
public class TaskBuildFarm : TaskBuildHouse
{
    public override float GetPriority()
    {
        if (manager.colonieBlackboard == null)
        {
            return 0;
        }

        uint actualFoodStored = actions.GetStorage() != null ? actions.GetStoredRessource(RessourceType.food) : 0;
        int actualFarmBuilded = actions.GetBuildingNumberOfType(BuildType.Farm);
        int actualPop = manager.colonieBlackboard.GetValue<int>("Habitant");
        int foodNeeded = actualPop - ((int)actualFoodStored + actualFarmBuilded);

        return (float)foodNeeded / (float)actualPop;
    }
}
