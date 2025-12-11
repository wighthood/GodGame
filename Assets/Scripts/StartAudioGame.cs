using UnityEngine;
using UnityEngine.UI;

public class StartAudioGame : MonoBehaviour
{
    private AudioManager sound;

    [SerializeField] private Slider _slider;

    void Start()
    {
        sound = AudioManager.Instance;
        //faire un code intermédiare pour conntroler le slider.

        //changer le slider au début
        _slider.value = sound.storedVolume;
    }


    
    public void ChangeVolume(float volume)
    {
        sound.ChangeVolume(_slider.value);
    }
}
