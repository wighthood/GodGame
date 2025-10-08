using UnityEngine;
using UnityEngine.AI;

namespace AI.Action
{
    public class Wander : ActionBase
    {
        private readonly float _wanderRadius = 8f;
        private readonly float _distanceThreshold = .5f;

        private NavMeshAgent _agent;
        private Vector3 _target;

    
        private void Start()
        {
            actionName = "Wander";
            cost = 100f; 

            _agent = GetComponent<NavMeshAgent>();
        }
    
        public override bool CheckCondition()
        {
            _target = RandomNavSphere(transform.position, _wanderRadius);
            return true;
        }

        public override bool DoAction()
        {
            if (_agent == null) return true;
            
            if (!_agent.hasPath)
            {
                _agent.SetDestination(_target);
            }
            
            if (!_agent.pathPending && _agent.remainingDistance <= _distanceThreshold)
            {
                _agent.ResetPath();
            }
            
            return false;
        }
    
        private static Vector3 RandomNavSphere(Vector3 origin, float dist)
        {
            Vector3 randomDirection = Random.insideUnitSphere * dist;
            randomDirection += origin;

            NavMesh.SamplePosition(randomDirection, out var navHit, dist, NavMesh.AllAreas);

            return navHit.position;
        }
    }
}
