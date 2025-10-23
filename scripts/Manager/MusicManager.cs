using UnityEngine;

public class MusicManager : MonoBehaviour
{
    public static MusicManager Instance;

    [Header("Music Tracks")]
    [Tooltip("Music that plays during daytime")]
    public AudioClip dayMusic;
    [Tooltip("Music that plays during nighttime")]
    public AudioClip nightMusic;

    [Header("Audio Settings")]
    [Range(0f, 1f)]
    public float musicVolume = 0.5f;
    [Tooltip("Time in seconds to fade between tracks")]
    public float crossfadeDuration = 2f;

    private AudioSource dayAudioSource;
    private AudioSource nightAudioSource;
    private bool isCurrentlyNight = false;
    private bool isCrossfading = false;

    void Awake()
    {
        // Singleton pattern
        if (Instance == null)
        {
            Instance = this;
            DontDestroyOnLoad(gameObject);
        }
        else
        {
            Destroy(gameObject);
            return;
        }

        // Create two audio sources for crossfading
        dayAudioSource = gameObject.AddComponent<AudioSource>();
        nightAudioSource = gameObject.AddComponent<AudioSource>();

        // Configure audio sources
        SetupAudioSource(dayAudioSource, dayMusic);
        SetupAudioSource(nightAudioSource, nightMusic);
    }

    void SetupAudioSource(AudioSource source, AudioClip clip)
    {
        source.clip = clip;
        source.loop = true;
        source.playOnAwake = false;
        source.volume = 0f;
    }

    void Start()
    {
        // Start playing the appropriate music based on initial time
        if (GameManager.Instance != null)
        {
            isCurrentlyNight = GameManager.Instance.isNight;

            if (isCurrentlyNight)
            {
                PlayNightMusic(immediate: true);
            }
            else
            {
                PlayDayMusic(immediate: true);
            }
        }
    }

    void Update()
    {
        // Check if day/night state has changed
        if (GameManager.Instance != null)
        {
            bool gameIsNight = GameManager.Instance.isNight;

            // Transition occurred
            if (gameIsNight != isCurrentlyNight && !isCrossfading)
            {
                isCurrentlyNight = gameIsNight;

                if (isCurrentlyNight)
                {
                    PlayNightMusic();
                }
                else
                {
                    PlayDayMusic();
                }
            }
        }
    }

    void PlayDayMusic(bool immediate = false)
    {
        if (immediate)
        {
            dayAudioSource.volume = musicVolume;
            nightAudioSource.volume = 0f;
            dayAudioSource.Play();
            nightAudioSource.Stop();
        }
        else
        {
            StartCoroutine(CrossfadeMusic(nightAudioSource, dayAudioSource));
        }
    }

    void PlayNightMusic(bool immediate = false)
    {
        if (immediate)
        {
            nightAudioSource.volume = musicVolume;
            dayAudioSource.volume = 0f;
            nightAudioSource.Play();
            dayAudioSource.Stop();
        }
        else
        {
            StartCoroutine(CrossfadeMusic(dayAudioSource, nightAudioSource));
        }
    }

    System.Collections.IEnumerator CrossfadeMusic(AudioSource fadeOut, AudioSource fadeIn)
    {
        isCrossfading = true;

        // Start playing the fade-in track if it's not already playing
        if (!fadeIn.isPlaying)
        {
            fadeIn.Play();
        }

        float elapsed = 0f;
        float startVolumeOut = fadeOut.volume;
        float startVolumeIn = fadeIn.volume;

        while (elapsed < crossfadeDuration)
        {
            elapsed += Time.deltaTime;
            float t = elapsed / crossfadeDuration;

            // Smooth fade using sine curve for more natural transition
            float fadeOutCurve = Mathf.Cos(t * Mathf.PI * 0.5f);
            float fadeInCurve = Mathf.Sin(t * Mathf.PI * 0.5f);

            fadeOut.volume = startVolumeOut * fadeOutCurve;
            fadeIn.volume = musicVolume * fadeInCurve;

            yield return null;
        }

        // Ensure final volumes are set
        fadeOut.volume = 0f;
        fadeIn.volume = musicVolume;
        fadeOut.Stop();

        isCrossfading = false;
    }

    // Public methods for manual control
    public void SetMusicVolume(float volume)
    {
        musicVolume = Mathf.Clamp01(volume);

        if (isCurrentlyNight)
        {
            nightAudioSource.volume = musicVolume;
        }
        else
        {
            dayAudioSource.volume = musicVolume;
        }
    }

    public void StopAllMusic()
    {
        dayAudioSource.Stop();
        nightAudioSource.Stop();
    }

    public void PauseMusic()
    {
        dayAudioSource.Pause();
        nightAudioSource.Pause();
    }

    public void ResumeMusic()
    {
        if (isCurrentlyNight)
        {
            nightAudioSource.UnPause();
        }
        else
        {
            dayAudioSource.UnPause();
        }
    }
}