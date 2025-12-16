using System;
using UnityEngine;

public class Farm : MonoBehaviour
{
    [SerializeField]
    private float farmGrowTime;
    private float currentTimer;

    [SerializeField]
    private GameObject foodPrefab;

    private GameObject instanciatedFood;

    public static event Func<Vector3, GameObject> spawnFood;

    private void Update()
    {
        if(instanciatedFood != null)
        {
            return;
        }

        currentTimer += Time.deltaTime;
        if(currentTimer >= farmGrowTime)
        {
            instanciatedFood = spawnFood.Invoke(transform.position);
            currentTimer = 0;
        }
    }
}
