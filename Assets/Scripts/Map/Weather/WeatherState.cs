using UnityEngine;

public class WeatherState : MonoBehaviour
{
    private int currentStateIndex;
    public static State currentWeatherState;
    public delegate void WeatherStateDelegate(State state);
    public event WeatherChangedHandler OnWeatherChanged;

    public enum State
    {
        Sunny,
        Rain,
        Storm,
        Fog,
        Poison,
        Care,
    }

    private void Start()
    {
        currentStateIndex = 0;
        currentWeatherState = WeatherStateOrder[currentStateIndex];
    }

    public void CycleWeatherState()
    {
        currentStateIndex = (currentStateIndex + 1) % WeatherStateOrder.Length;
        currentWeatherState = WeatherStateOrder[currentStateIndex];
        print("The weather is now: " + WeatherStateOrder[currentStateIndex]);

        OnWeatherChanged?.Invoke(currentWeatherState);
    }

    public int GetNextWeatherStateIndex()
    {
        return (currentStateIndex + 1) % WeatherStateOrder.Length;
    }
}
