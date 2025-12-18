using UnityEngine;
using UnityEngine.UI;

public class StartAudioGame : MonoBehaviour
{

    [SerializeField] private Slider _slider;
    private AudioManager sound;

    private void Start()
    {
        sound = AudioManager.Instance;
        //faire un code interm�diare pour conntroler le slider.

        //changer le slider au d�but
        _slider.value = sound.storedVolume;
    }



    public void ChangeVolume(float volume)
    {
        sound.ChangeVolume(_slider.value);
    }
}
