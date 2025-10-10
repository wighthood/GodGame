using AI;
using AI.Action;
using UnityEngine;

public class Eat : ActionBase
{
    [SerializeField] private int foodAmount = 1;
    [SerializeField] private int hungerReduction = 50;
    [SerializeField] private int eatingDuration;
    
    private float _eatingTimer;

    private void Start()
    {
        actionName = "Eat";
        cost = 1f;

        // Préconditions: besoin de nourriture disponible
        Preconditions.Add("FoodAvailable", true);

        // Effets: satisfait la faim
        Effects.Add("SatisfyHunger", true);
    }

    public override bool CheckCondition()
    {
        Agent agent = GetComponent<Agent>();
        if (agent == null) return false;

        int availableFood = GlobalState.Instance.GetFoodCount();
        if (availableFood < foodAmount) return false;
        return true;
    }

    public override bool DoAction()
    {
        _eatingTimer += Time.deltaTime;

        if (_eatingTimer >= eatingDuration)
        {
            Debug.Log($"{GetComponent<Agent>().name} is eating");
            int currentFood = GlobalState.Instance.GetFoodCount();
            if (currentFood < foodAmount) return false;

            GlobalState.Instance.AddFoodCount(-foodAmount);

            // Réduire la faim de l'agent
            NeedsManager needsManager = GetComponent<NeedsManager>();
            if (needsManager != null)
            {
                needsManager.ReduceNeed("Hunger", hungerReduction);
            }

            _eatingTimer = 0;
            return true;
        }

        return false;
    }
}
