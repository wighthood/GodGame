using System.Collections.Generic;
using AI.Action;
using AI.Jobs;
using UnityEngine;

namespace AI
{
    public class Agent : MonoBehaviour
    {
        [HideInInspector] public List<ActionBase> actions = new List<ActionBase>();
        private WorldState _worldState = new WorldState();

        private WorldState _currentGoal;
        private Queue<ActionBase> _currentPlan;
        private Job _currentJob;

        void Start()
        {
            actions.AddRange(GetComponents<ActionBase>());
            
            _worldState.Set("HungerBar", 0);
        
            _currentGoal = new WorldState();
            _currentGoal.Set("Wander", true);
        }


        void Update()
        {
            if (_currentPlan == null || _currentPlan.Count == 0)
            {
                if (_currentJob == null)
                {
                    TryGetJobFromBoard();
                }

                if (_currentJob != null)
                {
                    _currentGoal = _currentJob.requiredGoal;
                    _currentJob.status = JobStatus.InProgress;
                }
                else
                {
                    _currentGoal = new WorldState();
                    _currentGoal.Set("Wander", true);
                }
                
                _currentPlan = Planner.Plan(_worldState, actions, _currentGoal);
                
                
                if (_currentPlan == null)
                {
                    Debug.Log(name + " : no plan found -> fallback (wander)");
                    
                    if (_currentJob != null)
                    {
                        JobBoard.Instance?.FailJob(_currentJob);
                        _currentJob = null;
                    }
                    
                    var wander = GetComponent<Wander>();
                    if (wander == null) return;
                    wander.CheckCondition();
                    wander.DoAction();

                    return;
                }
            }
        
            var action = _currentPlan.Peek();
            bool finished = action.DoAction();

            if (!finished) return;
            Debug.Log(name + " : action terminée -> " + action.actionName);
            
            foreach (var eff in action.Effects)
                _worldState.Set(eff.Key, eff.Value);

            _currentPlan.Dequeue();
            if (_currentPlan.Count == 0 && _currentJob != null)
            {
                JobBoard.Instance?.CompleteJob(_currentJob);
                _currentJob = null;
            }
            _worldState.Set("HungerBar", _worldState.Get<int>("HungerBar") +1);
        }

        private void TryGetJobFromBoard()
        {
            if (JobBoard.Instance == null) return;
            Job job = JobBoard.Instance.GetBestAvailableJob(this);
            if (job != null && JobBoard.Instance.AssignJob(job, this))
            {
                _currentJob = job;
            }
        }

        public void PostJob(string jobType, WorldState goal, float priority = 1f)
        {
            if (JobBoard.Instance == null) return;
            JobBoard.Instance.PostJob(jobType, goal, priority);
        }
    }
}
