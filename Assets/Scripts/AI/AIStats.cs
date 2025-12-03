using System.Collections;
using UnityEngine;

public class AIStats : MonoBehaviour
{
    private BlackBoard blackBoard;

    [SerializeField, Range(0, 1)]
    private float hunger;

    [SerializeField]
    private int health;

    [SerializeField]
    private int maxHealth;

    private void Start()
    {
        blackBoard = GetComponent<BlackBoard>();

        blackBoard.AddValue("hunger", hunger);

        StartCoroutine(Hunger());
    }

    public void TakeDamage(int _amount)
    {
        health -= _amount;
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
            else if (hungerCooldown == 1)
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
