using System.Collections.Generic;
using AI.Action;
using UnityEngine;

namespace AI
{
    public class Agent : MonoBehaviour
    {
        [HideInInspector] public List<ActionBase> actions = new List<ActionBase>();
        private readonly Dictionary<string, bool> _worldState = new Dictionary<string, bool>();

        private Dictionary<string, bool> _currentGoal;

        private Queue<ActionBase> _currentPlan;

        void Start()
        {
            actions.AddRange(GetComponents<ActionBase>());

            _worldState["hasWood"] = false;
            _worldState["isHungry"] = false;
        
            _currentGoal = new Dictionary<string, bool>() { { "houseBuilt", true } };
        }

        void Update()
        {
            if (_currentPlan == null || _currentPlan.Count == 0)
            {
                _currentPlan = GoapPlanner.Plan(_worldState, actions, _currentGoal);

                if (_currentPlan == null)
                {
                    Debug.Log(name + " : no plan found -> fallback (wander)");
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
            
            //foreach (var eff in action.Effects)
                //_worldState[eff.Key] = eff.Value;

            _currentPlan.Dequeue();
        }
    }
}
