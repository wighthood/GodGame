using System.Collections;
using UnityEngine;

public class AIStats : MonoBehaviour
{

    [Range(0, 1)]
    public float hunger;

    public int health;

    public int maxHealth;
    private BlackBoard blackBoard;

    private void Start()
    {
        blackBoard = GetComponent<TaskManager>().agentBlackboard;

        blackBoard.AddValue("hunger", hunger);

        StartCoroutine(Hunger());
    }

    public void TakeDamage(int _amount)
    {
        health -= _amount;
        if (health <= 0) { Death(); }
    }

    public void SetHealth(int _health)
    {
        health = _health;
    }

    public int GetHealth()
    {
        return health;
    }

    public float GetHunger()
    {
        return hunger;
    }

    public void SetHungerFull()
    {
        hunger = 0;
        blackBoard.ModifyValue("hunger", hunger);
    }

    private void Death()
    {
        Destroy(gameObject);
    }

    private IEnumerator Hunger()
    {
        int hungerCooldown = 0;
        while (true)
        {
            if (hunger >= 1)
            {
                TakeDamage(1);
                yield return new WaitForSeconds(1);
            }
            else if (hungerCooldown == 20)
            {
                hungerCooldown = 0;
                hunger += 0.1f;
                blackBoard.ModifyValue("hunger", hunger);
                yield return new WaitForSeconds(1);
            }
            else
            {
                hungerCooldown++;
                yield return new WaitForSeconds(1);
            }
        }
    }
}
