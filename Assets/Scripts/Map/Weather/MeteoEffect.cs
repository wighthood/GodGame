using System.Collections;
using UnityEngine;
using UnityEngine.Rendering.Universal;

public class MeteoEffect : MonoBehaviour
{
    private WeatherState oldState;

    public Light2D mainLight;

    [SerializeField] private GameObject fog;

    private ParticleSystem particuleSystem;

    Coroutine testCoroutine;


    private void Awake()
    {
        if (!particuleSystem)
            particuleSystem = fog.GetComponent<ParticleSystem>();
    }

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
                StopAllCoroutines();
                particuleSystem.Play();

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
                
                break;
            case WeatherState.Rain:
                Debug.Log("fin rain");
                break;
            case WeatherState.Storm:
                Debug.Log("fin Storm");
                break;

            case WeatherState.Fog:
                testCoroutine = StartCoroutine(WeatherFade());
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
        MeteoManager.OnWeatherChanged += OnMeteoChange;  
        fog.SetActive(false);
    }

    private void OnDestroy()
    {
        StopAllCoroutines();
        MeteoManager.OnWeatherChanged -= OnMeteoChange;  
    }

    private IEnumerator WeatherFade()
    {
        particuleSystem.Stop(); 

        int maxParticles = particuleSystem.main.maxParticles;
        ParticleSystem.Particle[] particles = new ParticleSystem.Particle[maxParticles];

        int count = particuleSystem.GetParticles(particles);

        while (count > 0)
        {
            for (int i = 0; i < count; i++)
            {
                Color color = particles[i].startColor;
                color.a -= 0.01f; 
                color.a = Mathf.Max(color.a, 0f);
                particles[i].startColor = color;

                
                if (color.a <= 0f)
                    particles[i].remainingLifetime = 0f;
            }

            particuleSystem.SetParticles(particles, count);

            yield return null;

            count = particuleSystem.GetParticles(particles);
        }

        fog.SetActive(false);

    }
}
