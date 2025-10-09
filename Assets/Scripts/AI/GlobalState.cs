using UnityEngine;

namespace AI
{
    public class GlobalState : MonoBehaviour
    {
        public static GlobalState Instance { get; private set; }

        [SerializeField] private int foodCount = 10;

        private void Awake()
        {
            if (Instance == null)
            {
                Instance = this;
                DontDestroyOnLoad(gameObject);
            }
            else
            {
                Destroy(gameObject);
            }
        }

        public int GetFoodCount()
        {
            return foodCount;
        }

        public void AddFoodCount(int amount)
        {
            foodCount += amount;
            Debug.Log($"[GlobalState] Food count: {foodCount}");
        }
    }
}
