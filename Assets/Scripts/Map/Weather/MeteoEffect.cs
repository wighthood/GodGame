using System.Collections;
using UnityEngine;
using UnityEngine.Rendering.Universal;

public class MeteoEffect : MonoBehaviour
{
    [HideInInspector] public WeatherState oldState;

    public Light2D mainLight;
    private ParticleSystem particuleSystem;
    private Coroutine stormCoroutine;

    [SerializeField] private GameObject fog;
    [SerializeField] private GameObject rain;
    [SerializeField] private Animator animatorRain;
    [SerializeField] private float lightningMinDelay = 3f; 
    [SerializeField] private float lightningMaxDelay = 8f;



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
                rain.SetActive(true);
                animatorRain.SetBool("IsActive", true);
                
                break;
           case WeatherState.Storm:
                Debug.Log("Storm");
                rain.SetActive(true);
                animatorRain.SetBool("IsActive", true);
                if (stormCoroutine != null)
                    StopCoroutine(stormCoroutine);

                stormCoroutine = StartCoroutine(StormRoutine());
                mainLight.intensity = 0.5f;
                break;

            case WeatherState.Fog:
                Debug.Log("Fog");
                mainLight.intensity = 0.8f;
                fog.SetActive(true);
                StopAllCoroutines();
                particuleSystem.Play();

                break;
            /*case WeatherState.Poison:
                Debug.Log("Poison");
                mainLight.intensity = 0.8f;
                break;*/
           /* case WeatherState.Care:
                Debug.Log("Care");
                mainLight.intensity = 1f;
                break;*/
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
                animatorRain.SetBool("IsActive", false);
                rain.SetActive(false);
                break;
            case WeatherState.Storm:
                Debug.Log("fin Storm");

                if (stormCoroutine != null)
                {
                    StopCoroutine(stormCoroutine);
                    stormCoroutine = null;
                }

                mainLight.intensity = 1f;
                animatorRain.SetBool("IsActive", false);
                rain.SetActive(false);

                break;

            case WeatherState.Fog:
                StartCoroutine(WeatherFade());
                break;

           /* case WeatherState.Poison:
                Debug.Log("fin poison");
                break;*/
            
           /* case WeatherState.Care:
                Debug.Log("fin care");
                break;*/
        }
    }
  
    private void Start()
    {
        MeteoManager.OnWeatherChanged += OnMeteoChange;  
        fog.SetActive(false);
        rain.SetActive(false);
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

    private IEnumerator StormRoutine()
    {
        while (true)
        {
            float baseIntensity = mainLight.intensity;

            yield return new WaitForSeconds(Random.Range(lightningMinDelay, lightningMaxDelay));
        
            mainLight.intensity = 1.5f;
            yield return new WaitForSeconds(0.05f);

            mainLight.intensity = baseIntensity;
            yield return new WaitForSeconds(0.08f);

            mainLight.intensity = 1.2f;
            yield return new WaitForSeconds(0.03f);

            mainLight.intensity = baseIntensity;
        }
    }
}
