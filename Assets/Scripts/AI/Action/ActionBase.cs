using System.Collections.Generic;
using UnityEngine;

namespace AI.Action
{
    public abstract class ActionBase : MonoBehaviour
    {
        public string actionName = "Default";
        public float cost = 1f;
        
        public Dictionary<string, bool> Preconditions = new Dictionary<string, bool>();
        public Dictionary<string, bool> Effects = new Dictionary<string, bool>();
        public abstract bool CheckCondition();
        
        public abstract bool DoAction();
        
    }
}