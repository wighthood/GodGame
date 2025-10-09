using System.Collections;
using System.Collections.Generic;
using AI.Action;
using AI.Jobs;
using UnityEditor.Rendering.Universal;
using UnityEngine;

namespace AI
{
    public class Agent : MonoBehaviour
    {
        [SerializeField] private int _HungerDelay= 10;
        [HideInInspector] public List<ActionBase> actions = new List<ActionBase>();
        private WorldState _worldState = new WorldState();
        private Coroutine _hungerCoroutine;

        private WorldState _currentGoal;
        private WorldState _FeedingGoal;
        private Queue<ActionBase> _currentPlan;
        private Job _currentJob;

        void Start()
        {
            actions.AddRange(GetComponents<ActionBase>());
            _worldState.Set("HungerBar", 0);
        
            _currentGoal = new WorldState();
            _currentGoal.Set("Wander", true);
            
            _hungerCoroutine = StartCoroutine(HungerCoroutine());
        }


        void Update()
        {
            if (_currentPlan == null || _currentPlan.Count == 0)
            {
                if (_currentJob == null)
                {
                    if (_worldState.Get<int>("HungerBar") > 75)
                    {
                        Debug.Log("I'm hungry");
                        _currentGoal = new WorldState();
                        _currentGoal.Set("HungerBar", 0);
                        _currentJob = new Job("eat",_currentGoal,.1f);
                    }
                    else
                    {
                        TryGetJobFromBoard();
                    }
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
                    _currentJob = new Job("wander",_currentGoal);
                }

                _currentPlan = Planner.Plan(_worldState, actions, _currentGoal);
            }
            
            var action = _currentPlan.Peek();
            action.CheckCondition();
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

        private IEnumerator HungerCoroutine()
        {
            while (true)
            {
                yield return new WaitForSeconds(_HungerDelay);

                if (_worldState.Get<int>("HungerBar") >= 100) continue;
                _worldState.Add("HungerBar", 1); ;
            }
        }
        
        private void StopHunger()
        {
            if (_hungerCoroutine == null) return;
            StopCoroutine(_hungerCoroutine);
            _hungerCoroutine = null;
        }

        public void RestartHunger()
        {
            StopHunger();
            _hungerCoroutine = StartCoroutine(HungerCoroutine());
        }

        private void OnDestroy()
        {
            StopHunger();
        }
    }
}
