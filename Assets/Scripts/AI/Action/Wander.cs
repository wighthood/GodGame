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
        private Transform _spriteTransform;

    
        private void Start()
        {
            actionName = "Wander";
            cost = 100f; 

            _agent = GetComponent<NavMeshAgent>();
            _spriteTransform = transform;
            
            Effects.Add("Wander", true);
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
            
            if (_agent.velocity.sqrMagnitude > 0.01f)
            {
                Vector3 scale = _spriteTransform.localScale;
                scale.x = _agent.velocity.x > 0 ? -Mathf.Abs(scale.x) : Mathf.Abs(scale.x);
                _spriteTransform.localScale = scale;
            }
            
            if (_agent.pathPending || !(_agent.remainingDistance <= _distanceThreshold)) return false;
            _agent.ResetPath();
            return true;

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
