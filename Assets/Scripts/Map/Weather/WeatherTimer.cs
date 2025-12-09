using UnityEngine;

public class WeatherTimer : MonoBehaviour
{
    [SerializeField] float timer;
    private int currentStateIndex;
    public float[] timesForWeatherStates = new float[6];

    public delegate void TimerExpireHandler();
    public event TimerExpireHandler OnTimerExpired;

    void Start()
    {
        currentStateIndex = 0;
        timer = timesForWeatherStates[currentStateIndex];
    }

    void Update()
    {
        timer -= Time.deltaTime;

        if (timer <= 0)
        {
            CycleTimer();
            OnTimerExpired?.Invoke();
        }
    }

    public void CycleTimer()
    {
        SetTimer(currentStateIndex + 1);
    }

    public void SetIndexTimer(int newStateIndex)
    {
        currentStateIndex = nextStateIndex % timesForWeatherStates.Length;
        timer = timesForWeatherStates[currentStateIndex];
    }
    
    public void SetTimer(float time)
    {
        timer = time;
    }

    public float GetCurrentTime()
    {
        return timer;
    }
}
