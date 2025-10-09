using System;
using UnityEngine;

namespace AI
{
    public class GlobalState : MonoBehaviour
    {
        public static GlobalState Instance { get; private set; }
        
        private WorldState _sharedState = new WorldState();

        private void Awake()
        {
            if (Instance == null)
            {
                Instance = this;
            }
            else
            {
                Destroy(gameObject);
            }
        }
        
        public WorldState GetSharedState() => _sharedState;
        
        public int GetFoodCount()=> _sharedState.Get<int>("Food");
        public void SetFoodCount(int count) => _sharedState.Set("Food", count);
        public void AddFoodCount(int count) => _sharedState.Add("Food", count);
    }
}
