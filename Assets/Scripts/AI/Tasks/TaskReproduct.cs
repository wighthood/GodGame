using UnityEngine;

[CreateAssetMenu(fileName = "Reproduct", menuName = "Tasks/Reproduct")]
public class TaskReproduct : TaskBase
{
    Transform houseTransform;

    public override bool Do()
    {
        actions.MoveTo(houseTransform);

        return FinishCondition();
    }

    public override float GetPriority()
    {
        if(manager.colonieBlackboard == null || actions.GetStorage() == null) { return 0; }
        
        BlackBoard colonyBlackboard = manager.colonieBlackboard;
        int actualPopulation = colonyBlackboard.GetValue<int>("Habitant");
        int maxPopulation = colonyBlackboard.GetValue<int>("MaxHabitant");
        float populationFactor = 0.8f * (1 - (actualPopulation / maxPopulation));

        int foodStored = (int)actions.GetStoredfood();
        float foodSurplusFactor = 0.2f * (foodStored - actualPopulation);

        return actions.GetNearestHouse() != null ? populationFactor + foodSurplusFactor : 0;
    }

    public override void OnFinish()
    {
        actions.ReproductSelf();
    }

    public override void OnStart()
    {
        houseTransform = actions.GetNearestHouse();
    }

    protected override bool FinishCondition()
    {
        return Vector3.Distance(actions.transform.position, houseTransform.position) < 0.2f;
    }
}
