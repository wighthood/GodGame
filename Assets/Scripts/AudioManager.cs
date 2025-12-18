using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public class AudioManager : MonoBehaviour
{
    public enum SoundType
    {
        Music_Menu,
        Music_game,
        //pour ajouter des son, ajouter des SoundType ici.
    }

    //Singleton
    public static AudioManager Instance;
    [SerializeField] private AudioClip[] audioClips;

    //tout les sont et leus types, a mettre dans l'inspecteur
    public Sound[] AllSound;

    public SoundType SelectedSound;
    private AudioSource _musicSource;

    //Runtime collection
    private readonly Dictionary<SoundType, Sound> _soundDictionary = new Dictionary<SoundType, Sound>();
    public float storedVolume { get; private set; } = 0.5f;

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
        foreach (Sound s in AllSound)
        {
            _soundDictionary[s.Type] = s;
        }
        _musicSource = GetComponent<AudioSource>();
        ChangeMusic(SoundType.Music_Menu);
    }

    private void Update()
    {
        if (SceneManager.GetActiveScene() == SceneManager.GetSceneByName("GameScene") && !_musicSource.isPlaying)
        {
            if (_musicSource.clip != audioClips[0])
            {
                _musicSource.clip = audioClips[0];
            }
            else
            {
                _musicSource.clip = audioClips[1];
            }
            _musicSource.Play();
        }
    }

    //appel pour jouer un son
    public void play(SoundType type)
    {
        if (!_soundDictionary.TryGetValue(type, out Sound s))
        {
            Debug.LogWarning($"Sound type {type} not found!");
            return;
        }

        //Cr�e un nouvel objet Son
        GameObject soundObj = new GameObject($"Sound_{type}");
        AudioSource audioSrc = soundObj.AddComponent<AudioSource>();

        //propri�t� du son
        audioSrc.clip = s.Clip;
        audioSrc.volume = s.volume * storedVolume; // times storedVolume

        //play the sound
        audioSrc.Play();
        //Destroy the object
        Destroy(soundObj, s.Clip.length);
    }

    //change les musiques
    public void ChangeMusic(SoundType type)
    {
        if (!_soundDictionary.TryGetValue(type, out Sound track))
        {
            Debug.LogWarning($"music track {type} not found!");
            return;
        }

        _musicSource.loop = !_musicSource.loop;

        _musicSource.Stop();
        _musicSource.clip = track.Clip;
        _musicSource.Play();
    }

    public void ChangeVolume(float volume)
    {
        storedVolume = volume;
        _musicSource.volume = volume;
    }

    [Serializable]
    public class Sound
    {
        public SoundType Type;
        public AudioClip Clip;


        [Range(0f, 1f)]
        public float volume = 1f;

        [HideInInspector]
        public AudioSource Source;
    }
}
