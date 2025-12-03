using System.Collections.Generic;
using AI.Action;
using UnityEngine;

namespace AI
{
    public class Agent : MonoBehaviour
    {
        [Header("Agent Configuration")]
        [SerializeField] private float planningInterval = 2f;

        [HideInInspector] public List<ActionBase> actions = new List<ActionBase>();

        private Queue<ActionBase> _currentPlan;
        private ActionBase _currentAction;
        private NeedsManager _needsManager;
        private WorldState _worldState;
        private float _planningTimer;

        void Start()
        {
            actions.AddRange(GetComponents<ActionBase>());
            _needsManager = GetComponent<NeedsManager>();

            if (_needsManager == null)
            {
                _needsManager = gameObject.AddComponent<NeedsManager>();
            }

            _worldState = new WorldState();
            UpdateWorldState();
        }

        void Update()
        {
            _planningTimer += Time.deltaTime;
            
            if (_currentPlan == null || _currentPlan.Count == 0 || _planningTimer >= planningInterval)
            {
                _planningTimer = 0f;
                CreateNewPlan();
            }


            if (_currentPlan == null || _currentPlan.Count <= 0) return;
            if (_currentAction == null)
            {
                _currentAction = _currentPlan.Dequeue();
            }
                
            if (!_currentAction.CheckCondition())
            {
                PostRequestForFailedAction(_currentAction);
                _currentAction = null;
                _currentPlan = null;
                return;
            }

            bool finished = _currentAction.DoAction();

            if (!finished) return;
            _currentAction = null;
            _currentPlan = null;
            UpdateWorldState();
        }

        private void CreateNewPlan()
        {
            UpdateWorldState();
            
            Need urgentNeed = _needsManager.GetMostUrgentNeed();
            if (urgentNeed == null || !urgentNeed.IsCritical())
            {
                CreateDefaultPlan();
                return;
            }

            WorldState goal = new WorldState();
            goal.Set($"Satisfy{urgentNeed.needName}", true);

            _currentPlan = Planner.Plan(_worldState, actions, goal);

            if (_currentPlan == null || _currentPlan.Count == 0)
            {
                PostRequestForUnsatisfiedNeed(urgentNeed);
                
                CreateDefaultPlan();
            }
        }

        private void CreateDefaultPlan()
        {
            ActionBase defaultAction = actions.Find(a => a.actionName == "Wander");

            if (defaultAction != null)
            {
                _currentPlan = new Queue<ActionBase>();
                _currentPlan.Enqueue(defaultAction);
            }
        }

        private void UpdateWorldState()
        {
            _worldState = new WorldState();
            
            if (GlobalState.Instance != null)
            {
                _worldState.Set("FoodAvailable", GlobalState.Instance.GetFoodCount() > 0);
            }

            foreach (Need need in _needsManager.GetCriticalNeeds())
            {
                _worldState.Set(need.needName, need.currentValue);
            }
        }

        private void PostRequestForUnsatisfiedNeed(Need need)
        {
            if (RequestBoard.Instance == null) return;
            
            Request request = new Request(
                this,
                RequestType.NeedResource,
                $"{name} needs help with {need.needName}",
                need.GetUrgency()
            );

            request.resourceName = need.needName;
            RequestBoard.Instance.PostRequest(request);
        }

        private void PostRequestForFailedAction(ActionBase action)
        {
            if (RequestBoard.Instance == null) return;

            Request request = new Request(
                this,
                RequestType.NeedAction,
                $"{name} cannot perform {action.actionName}",
                0.8f
            );

            RequestBoard.Instance.PostRequest(request);
        }
    }
}
