using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace AI
{
    public class NeedsManager : MonoBehaviour
    {
        [SerializeField] private List<Need> needs = new List<Need>();

        private void Start()
        {
            needs.Add(new Need("Hunger", increaseRate: 2f, criticalThreshold: 70f));
        }

        private void Update()
        {

            foreach (var need in needs)
            {
                need.Update(Time.deltaTime);
            }
        }

        public Need GetMostUrgentNeed()
        {
            if (needs.Count == 0) return null;
            return needs.OrderByDescending(n => n.GetUrgency()).First();
        }

        public List<Need> GetCriticalNeeds()
        {
            return needs.Where(n => n.IsCritical()).ToList();
        }

        public Need GetNeed(string needName)
        {
            return needs.FirstOrDefault(n => n.needName == needName);
        }

        public void ReduceNeed(string needName, float amount)
        {
            var need = GetNeed(needName);
            if (need != null)
            {
                need.Reduce(amount);
            }
        }

        public bool HasCriticalNeeds()
        {
            return needs.Any(n => n.IsCritical());
        }
    }
}
