using System.Collections;
using UnityEngine;
using UnityEngine.Rendering.Universal;

public class MeteoEffect : MonoBehaviour
{
    private WeatherState oldState;


    public Light2D mainLight;
    [SerializeField] private GameObject fog;

    private void OnMeteoChange(WeatherState currentState)
    {
        EndWeather();


        switch (currentState)
        {
            case WeatherState.Sunny:
                Debug.Log("Sunny");
                mainLight.intensity = 1f;
                break;
            case WeatherState.Rain:
                Debug.Log("Rain");
                mainLight.intensity = 0.5f;
                break;
            case WeatherState.Storm:
                Debug.Log("Storm");
                mainLight.intensity = 0.5f;
                break;

            case WeatherState.Fog:
                Debug.Log("Fog");
                mainLight.intensity = 0.8f;
                fog.SetActive(true);
                fog.GetComponent<ParticleSystem>().Play();

                break;
            case WeatherState.Poison:
                Debug.Log("Poison");
                mainLight.intensity = 0.8f;
                break;
            case WeatherState.Care:
                Debug.Log("Care");
                mainLight.intensity = 1f;
                break;
        }
        oldState = currentState;
    }

    private void EndWeather()
    {
        switch (oldState)
        {
            case WeatherState.Sunny:
                Debug.Log("fin Sunny");
                fog.GetComponent<ParticleSystem>().Stop();
                break;
            case WeatherState.Rain:
                Debug.Log("fin rain");
                break;
            case WeatherState.Storm:
                Debug.Log("fin Storm");
                break;

            case WeatherState.Fog:

                fog.GetComponent<ParticleSystem>().Stop();


                break;
            case WeatherState.Poison:
                Debug.Log("fin poison");
                break;
            case WeatherState.Care:
                Debug.Log("fin care");
                break;
        }
    }
    private void Start()
    {
        MeteoManager.OnWeatherChanged += OnMeteoChange;  // qd t appele �a alors tu fais �a
        fog.SetActive(false);
    }


}
