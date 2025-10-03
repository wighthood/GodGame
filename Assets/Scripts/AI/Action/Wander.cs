using System;
using AI;
using UnityEngine;
using UnityEngine.AI;

public class Wander : ActionBase
{
    private float _wanderRadius = 10f;
    private float _wanderInterval = 5f;

    private NavMeshAgent _agent;
    private float _timer;
    
    private void Start()
    {
        _agent = GetComponent<NavMeshAgent>();
        _timer = _wanderInterval;
    }
    
    public override bool CheckCondition()
    {
        return true;
    }

    public override void DoAction()
    {
        
    }

    public override void ApplyEffect()
    {

    }
}
