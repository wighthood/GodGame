using UnityEngine;

[CreateAssetMenu(fileName = "Reproduct", menuName = "Tasks/Reproduct")]
public class TaskReproduct : TaskBase
{
    Transform houseTransform;

    private bool hasReproducted;

    Storage storage;

    public override bool Do()
    {
        actions.MoveTo(houseTransform);

        return FinishCondition();
    }

    public override float GetPriority()
    {
        if (storage == null && actions.GetStorage())
        {
            storage = actions.GetStorage();
        }

        if (manager.colonieBlackboard == null || actions.GetStorage() == null) { return 0; }
        
        BlackBoard colonyBlackboard = manager.colonieBlackboard;
        int actualPopulation = colonyBlackboard.GetValue<int>("Habitant");
        int maxPopulation = colonyBlackboard.GetValue<int>("MaxHabitant");

        if(actualPopulation >= maxPopulation) { return 0; }

        float populationFactor = 0.8f * (1.0f - ((float)actualPopulation / (float)maxPopulation));

        int foodStored = (int)actions.GetStoredRessource(RessourceType.food);
        float foodSurplusFactor = 0.2f * ((float)foodStored - (float)actualPopulation);

        return actions.GetNearestHouse() != null ? populationFactor + foodSurplusFactor : 0;
    }

    public override void OnFinish()
    {
        if (hasReproducted) { return; }
        hasReproducted = true;
        manager.colonieBlackboard.AddValueOrModify("Habitant", manager.colonieBlackboard.GetValue<int>("Habitant") - 1);
        actions.ReproductSelf();
    }

    public override void OnStart()
    {
        manager.colonieBlackboard.AddValueOrModify("Habitant", manager.colonieBlackboard.GetValue<int>("Habitant") + 1);
        hasReproducted = false;
        houseTransform = actions.GetNearestHouse();
    }

    protected override bool FinishCondition()
    {
        return Vector3.Distance(actions.transform.position, houseTransform.position) < 0.2f;
    }
}
