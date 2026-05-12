using System.Collections.Generic;
using UnityEngine;

public class AudioManager : MonoBehaviour
{
    public enum SfxType
    {
        RouletteBallSpin,
        WinSound,
        LoseSound,
        // Add other sfx types
    }

    [System.Serializable]
    public struct SoundEffect
    {
        public SfxType type;
        public AudioClip clip;
    }

    [Header("Background Music")]
    [SerializeField] private AudioClip backgroundMusic;
    [SerializeField][Range(0f, 1f)] private float musicVolume = 0.5f;

    [Header("Sound Effects")]
    [SerializeField] private SoundEffect[] soundEffects;

    private AudioSource musicSource;
    private AudioSource sfxSource;

    private Dictionary<SfxType, AudioClip> sfxSounds;
    public IReadOnlyDictionary<SfxType, AudioClip> SfxSounds => sfxSounds;

  
    void Awake()
    {
        musicSource = gameObject.AddComponent<AudioSource>();
        musicSource.loop = true;
        musicSource.volume = musicVolume;
        musicSource.playOnAwake = false;

        sfxSource = gameObject.AddComponent<AudioSource>();
        sfxSource.playOnAwake = false;

        sfxSounds = new Dictionary<SfxType, AudioClip>();
        
        foreach (var sfx in soundEffects)
            sfxSounds[sfx.type] = sfx.clip;
    }

    void Start()
    {
        PlayMusic();
    }
    public void PlayMusic()
    {
        if (backgroundMusic == null) return;
       
        musicSource.clip = backgroundMusic;
        musicSource.Play();
    }

    public void StopMusic() => musicSource.Stop();

    public void PauseMusic() => musicSource.Pause();

    public void PlaySFX(SfxType type)
    {
        if (sfxSounds.TryGetValue(type, out AudioClip clip))
            sfxSource.PlayOneShot(clip);
    }

    public void SetMusicVolume(float volume)
    {
        musicVolume = Mathf.Clamp01(volume);
        musicSource.volume = musicVolume;
    }

    public void SetSFXVolume(float volume)
    {
        sfxSource.volume = Mathf.Clamp01(volume);
    }
}