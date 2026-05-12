using System.Collections.Generic;
using UnityEngine;

namespace RouletteGame.Core.Audio
{
    //////////////////////////////////////////////////////////////////////////
    // Central audio system responsible for background music playback
    // and sound effect management using a simple lookup-based SFX registry.
    //////////////////////////////////////////////////////////////////////////
    public class AudioManager : MonoBehaviour
    {
        //////////////////////////////////////////////////////////////////////////
        public enum SfxType
        {
            RouletteBallSpin,
            WinSound,
            LoseSound
        }

        //////////////////////////////////////////////////////////////////////////

        [System.Serializable]
        public struct SoundEffect
        {
            public SfxType type;
            public AudioClip clip;
        }

        //////////////////////////////////////////////////////////////////////////
        
        // Inspector refs
        [Header("Background Music")]
        [SerializeField] private AudioClip backgroundMusic;
        [SerializeField][Range(0f, 1f)] private float musicVolume = 0.5f;

        [Header("Sound Effects")]
        [SerializeField] private SoundEffect[] soundEffects;

        //////////////////////////////////////////////////////////////////////////
        
        private AudioSource musicSource;
        private AudioSource sfxSource;
        private Dictionary<SfxType, AudioClip> sfxSounds;

        //////////////////////////////////////////////////////////////////////////
        // Initialize music and SFX audio sources and build lookup dictionary.
        void Awake()
        {
            musicSource = gameObject.AddComponent<AudioSource>();
            musicSource.loop = true;
            musicSource.volume = musicVolume;
            musicSource.playOnAwake = false;

            sfxSource = gameObject.AddComponent<AudioSource>();
            sfxSource.playOnAwake = false;

            sfxSounds = new Dictionary<SfxType, AudioClip>();

            // Build runtime lookup table for fast SFX retrieval.
            foreach (var sfx in soundEffects)
            {
                sfxSounds[sfx.type] = sfx.clip;
            }
        }

        void Start()
        {
            PlayMusic();
        }

        public void PlayMusic()
        {
            if (backgroundMusic == null)
            {
                Debug.LogError("[AudioManager] Failed to play music." +
                    " Background music is invalid.");
                return;
            }

            musicSource.clip = backgroundMusic;
            musicSource.Play();
        }

        public void StopMusic() => musicSource.Stop();

        public void PauseMusic() => musicSource.Pause();

        public void PlaySFX(SfxType type)
        {
            if (sfxSounds.TryGetValue(type, out AudioClip clip))
            {
                sfxSource.PlayOneShot(clip);
                return;
            }

            Debug.LogError($"[AudioManager] Failed to play {type} sfx.");
            return;
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
}