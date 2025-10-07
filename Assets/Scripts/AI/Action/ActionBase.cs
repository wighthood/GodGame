using System.Collections.Generic;
using UnityEngine;

namespace AI.Action
{
    public abstract class ActionBase : MonoBehaviour
    {
        public string actionName = "Default";
        public float cost = 1f;
        
        public Dictionary<string, float> Preconditions = new();
        public Dictionary<string, float> Effects = new ();
        
        
        public abstract bool CheckCondition();
        
        public abstract bool DoAction();
        
    }
}