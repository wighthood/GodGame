using UnityEngine;

public class MeteoEffect : MonoBehaviour
{
    
    private void OnMeteoChange(WeatherState currentState)
    {
        print(currentState);


    }

    private void Start()
    {
        MeteoManager.OnWeatherChanged += OnMeteoChange;  // qd t appele ça alors tu fais ça
    }

}
