using System;
using UnityEngine;

public class Farm : MonoBehaviour
{
    [SerializeField]
    private float farmGrowTime;
    private float currentTimer;

    private GameObject instanciatedFood;

    public static event Func<RessourceType, Vector2, GameObject> spawnFood;

    private void Update()
    {
        if(instanciatedFood != null)
        {
            return;
        }

        currentTimer += Time.deltaTime;
        if(currentTimer >= farmGrowTime)
        {
            instanciatedFood = spawnFood.Invoke(RessourceType.food, transform.position);
            instanciatedFood.GetComponent<SpriteRenderer>().enabled = false;
            currentTimer = 0;
        }
    }
}
