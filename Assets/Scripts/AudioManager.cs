using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;

public class AudioManager : MonoBehaviour
{
    public enum SoundType
    {
        Music_Menu,
        Music_game
        //pour ajouter des son, ajouter des SoundType ici.
    }

    [System.Serializable]
    public class Sound
    {
        public SoundType Type;
        public AudioClip Clip;

        [Range(0f, 1f)]
        public float volume = 1f;

        [HideInInspector]
        public AudioSource Source;
    }

    //Singleton
    public static AudioManager Instance;

    //tout les sont et leus types, a mettre dans l'inspecteur
    public Sound[] AllSound;

    //Runtime collection
    private Dictionary<SoundType, Sound> _soundDictionary = new Dictionary<SoundType, Sound>();
    private AudioSource _musicSource;

    private void Awake()
    {
        if (Instance != null)
        {
            Destroy(this);
            return;
        }
        //assignage du Singletone
        Instance = this;
        DontDestroyOnLoad(this);

        //mise en place des sons
        foreach (var s in AllSound)
        {
            _soundDictionary[s.Type] = s;
        }
        _musicSource = GetComponent<AudioSource>();
        ChangeMusic(SoundType.Music_Menu);
    }

    public SoundType SelectedSound;

    //appel pour jouer un son
    public void play(SoundType type)
    {
        if (!_soundDictionary.TryGetValue(type, out Sound s))
        {
            Debug.LogWarning($"Sound type {type} not found!");
            return;
        }

        //Crée un nouvel objet Son
        var soundObj = new GameObject($"Sound_{type}");
        var audioSrc = soundObj.AddComponent<AudioSource>();

        //propriété du son
        audioSrc.clip = s.Clip;
        audioSrc.volume = s.volume;

        //play the sound
        audioSrc.Play();

        //Destroy the object
        Destroy(soundObj, s.Clip.length);
    }

    //change les musiques
    public void ChangeMusic(SoundType type)
    {
        if (!_soundDictionary.TryGetValue(type,out Sound track))
        {
            Debug.LogWarning($"music track {type} not found!");
            return; 
        }

        _musicSource.Stop();
        _musicSource.loop = true;
        _musicSource.clip = track.Clip;
        _musicSource.Play();
    }
}
