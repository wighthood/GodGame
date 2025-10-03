using UnityEngine;

namespace AI
{
    public abstract class ActionBase : MonoBehaviour
    {
        public string actionName = "Default";

        public abstract bool CheckCondition();
        
        public abstract void DoAction();
        
        public abstract void ApplyEffect();
        
    }
}
