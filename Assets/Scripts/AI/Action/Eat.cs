using System;
using AI;
using AI.Action;
using UnityEngine;
using UnityEngine.Serialization;

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
        
        Preconditions.Add("Food", foodAmount);
        Effects.Add("Hunger", -hungerReduction);
    }

    public override bool CheckCondition()
    {
        Agent agent = GetComponent<Agent>();
        if (agent == null) return false;

        return true;
    }

    public override bool DoAction()
    {
        throw new System.NotImplementedException();
    }
}
