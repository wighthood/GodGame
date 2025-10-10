using System.Collections.Generic;

namespace AI
{
    public class WorldState
    {
        private Dictionary<string, object> states = new Dictionary<string, object>();

        public void Set(string key, object value)
        {
            if (states.ContainsKey(key))
                states[key] = value;
            else
                states.Add(key, value);
        }

        public bool Has(string key)
        {
            return states.ContainsKey(key);
        }

        public T Get<T>(string key)
        {
            if (states.ContainsKey(key))
                return (T)states[key];
            return default(T);
        }

        public void Remove(string key)
        {
            if (states.ContainsKey(key))
                states.Remove(key);
        }

        public WorldState Clone()
        {
            WorldState newState = new WorldState();
            foreach (var kvp in states)
            {
                newState.Set(kvp.Key, kvp.Value);
            }
            return newState;
        }

        public bool MeetsGoal(WorldState goal)
        {
            foreach (var g in goal.states)
            {
                if (!Has(g.Key)) return false;

                object ourValue = Get<object>(g.Key);
                if (!g.Value.Equals(ourValue)) return false;
            }
            return true;
        }

        public Dictionary<string, object> GetAllStates()
        {
            return new Dictionary<string, object>(states);
        }
    }
}
