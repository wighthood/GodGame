using System;
using Unity.VisualScripting;
using UnityEngine;

public class MeteoManager : MonoBehaviour, ISaveable
{
    [SerializeField] private Vector2 weatherRange = new(60, 120);
    [SerializeField] private WeatherState currentWeatherState;
    private float timerWeather;

    public static event Action <WeatherState> OnWeatherChanged; 
  
    void Start()
    {
        currentWeatherState = 0;
    }

    void OnEnable()
    {
        SaveEvents.OnRegisterSaveableEvent?.Invoke(this);
    }

    void OnDisable()
    {
        SaveEvents.OnUnregisterSaveableEvent?.Invoke(this);
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
        currentWeatherState = (WeatherState)UnityEngine.Random.Range(0, 4);
        OnWeatherChanged?.Invoke(currentWeatherState);
    }

    public void MeteoChange(WeatherState state)
    {
        OnWeatherChanged?.Invoke(state);
    }

    private void WeatherTime()
    {
        timerWeather = UnityEngine.Random.Range(weatherRange.x, weatherRange.y);
    }   

    // ISaveable Implementation
    public string GetSaveID()
    {
        return "MeteoManager";
    }

    public string CaptureState()
    {
        MeteoSaveData data = new MeteoSaveData
        {
            weatherState = (int)currentWeatherState,
            timer = timerWeather
        };
        return JsonUtility.ToJson(data);
    }

    public void RestoreState(string _state)
    {
        if (string.IsNullOrEmpty(_state)) return;
        MeteoSaveData data = JsonUtility.FromJson<MeteoSaveData>(_state);
        if (data == null) return;

        currentWeatherState = (WeatherState)data.weatherState;
        timerWeather = data.timer;
        
        // Instant visual update
        OnWeatherChanged?.Invoke(currentWeatherState);
    }
}

public enum WeatherState
{
    Sunny = 0,
    Rain,
    Storm,
    Fog,
    /*Poison,
    Care,*/
}