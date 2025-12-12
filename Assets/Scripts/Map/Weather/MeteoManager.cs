using System;
using Unity.VisualScripting;
using UnityEngine;

public class MeteoManager : MonoBehaviour
{
    [SerializeField] private Vector2 weatherRange = new(60, 120);

    [SerializeField] private WeatherState currentWeatherState;
    private float timerWeather;

    public static event Action <WeatherState> OnWeatherChanged;  //Lorsque WeatherState est appelé, alors 
  
    void Start()
    {
        currentWeatherState = 0;
    }

    private void Update()
    {
        if (timerWeather < 0)
        {
            WeatherTime();
            MeteoChange();
        }
        else 
        {
            timerWeather -= Time.deltaTime;
        }
    }

    private void MeteoChange()
    {
        currentWeatherState = (WeatherState)UnityEngine.Random.Range(0, 3);
        OnWeatherChanged.Invoke(currentWeatherState);
    }

    public void MeteoChange(WeatherState state)
    {
        OnWeatherChanged.Invoke(state);
    }

    private void WeatherTime()
    {
        timerWeather = UnityEngine.Random.Range(weatherRange.x, weatherRange.y);
    }      
}

public enum WeatherState
{
    Sunny = 0,
    Rain,
    Fog,
    /*Storm,
    Poison,
    Care,*/
}