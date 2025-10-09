using System;
using System.Collections.Generic;

namespace AI
{
    [Serializable]
    public class WorldState
    {
        private Dictionary<string, object> _states = new Dictionary<string, object>();

        public T Get<T>(string key, T defaultValue = default)
        {
            if (_states.ContainsKey(key) && _states[key] is T value)
                return value;
            return defaultValue;
        }

        public void Set(string key, object value)
        {
            _states[key] = value;
        }

        public void Add(string key, object value)
        {
            _states.Add(key, value);
        }

        public bool Has(string key)
        {
            return _states.ContainsKey(key);
        }

        public void Remove(string key)
        {
            _states.Remove(key);
        }

        public Dictionary<string, object> GetAll() => new Dictionary<string, object>(_states);

        public WorldState Clone()
        {
            WorldState clone = new WorldState();
            foreach (var kvp in _states)
            {
                clone._states[kvp.Key] = kvp.Value;
            }
            return clone;
        }

        public bool MeetsGoal(WorldState goal)
        {
            foreach (var kvp in goal._states)
            {
                if (!_states.ContainsKey(kvp.Key) || !_states[kvp.Key].Equals(kvp.Value))
                    return false;
            }
            return true;
        }

    }
}

