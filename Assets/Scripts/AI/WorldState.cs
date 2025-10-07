using System;
using System.Collections.Generic;

namespace AI
{
    [Serializable]
    public class WorldState
    {
        private Dictionary<string, object> states = new Dictionary<string, object>();

        public T Get<T>(string key, T defaultValue = default)
        {
            if (states.ContainsKey(key) && states[key] is T value)
                return value;
            return defaultValue;
        }

        public void Set(string key, object value)
        {
            states[key] = value;
        }

        public bool Has(string key)
        {
            return states.ContainsKey(key);
        }

        public void Remove(string key)
        {
            states.Remove(key);
        }

        public Dictionary<string, object> GetAll() => new Dictionary<string, object>(states);

        public WorldState Clone()
        {
            WorldState clone = new WorldState();
            foreach (var kvp in states)
            {
                clone.states[kvp.Key] = kvp.Value;
            }
            return clone;
        }

        public bool MeetsGoal(WorldState goal)
        {
            foreach (var kvp in goal.states)
            {
                if (!states.ContainsKey(kvp.Key) || !states[kvp.Key].Equals(kvp.Value))
                    return false;
            }
            return true;
        }

    }
}

