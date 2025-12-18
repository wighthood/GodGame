using UnityEngine;

[CreateAssetMenu(fileName = "Reproduct", menuName = "Tasks/Reproduct")]
public class TaskReproduct : TaskBase
{

    private bool hasReproducted;
    private Transform houseTransform;

    public override bool Do()
    {
        actions.MoveTo(houseTransform);

        return FinishCondition();
    }

    public override float GetPriority()
    {
        if (manager.colonieBlackboard == null) return 0;
        if (actions.GetNearestHouse() == null) return 0;

        BlackBoard blackboard = manager.colonieBlackboard;

        int population = blackboard.GetValue<int>("Habitant");
        int maxPopulation = blackboard.GetValue<int>("MaxHabitant");
        if (population >= maxPopulation) return 0;

        int foodStored = (int)actions.GetStoredRessource(RessourceType.food);

        float populationRatio = 1f - population / (float)maxPopulation;
        float foodRatio = Mathf.Clamp01(foodStored / (population * 2f));

        float priority =
            populationRatio * 0.6f +
            foodRatio * 0.4f;

        return actions.GetNearestHouse() != null ? priority : 0;
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
