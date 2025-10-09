using UnityEngine;

namespace AI
{
    [System.Serializable]
    public class Need
    {
        public string needName;
        public float currentValue;
        public float maxValue = 100f;
        public float increaseRate = 1f; // Points par seconde
        public float criticalThreshold = 70f; // Seuil où le besoin devient urgent

        public Need(string name, float increaseRate = 1f, float criticalThreshold = 70f)
        {
            this.needName = name;
            this.currentValue = 0f;
            this.increaseRate = increaseRate;
            this.criticalThreshold = criticalThreshold;
        }

        public void Update(float deltaTime)
        {
            currentValue = Mathf.Min(currentValue + (increaseRate * deltaTime), maxValue);
        }

        public void Reduce(float amount)
        {
            currentValue = Mathf.Max(currentValue - amount, 0f);
        }

        public bool IsCritical()
        {
            return currentValue >= criticalThreshold;
        }

        public float GetUrgency()
        {
            return currentValue / maxValue;
        }
    }
}
