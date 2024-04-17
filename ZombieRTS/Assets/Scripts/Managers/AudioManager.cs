using UnityEngine;

public class AudioManager : MonoBehaviour
{
    public static AudioManager Instance;

    [Header("Sound Manager")]
    [SerializeField] private AudioClip backgroundMusic;
    [SerializeField] private AudioSource musicSource;
    [SerializeField] private AudioSource sfxSource;

    [Header("[SFX]")]
    [Header("-> Menu")]
    [SerializeField] private AudioClip hoverSound;
    [SerializeField] private AudioClip clickSoundUI;

    [Header("-> In Game")]
    [SerializeField] AudioClip errorSound;
    [SerializeField] AudioClip clickSoundInGame;
    [SerializeField] AudioClip otherSound;

    [Header("[Music]")]
    [SerializeField] AudioClip[] soundTracks;

    void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
            DontDestroyOnLoad(gameObject);
        }
        else if (Instance != this)
        {
            Destroy(gameObject);
        }

        LoadVolumeSettings();
    }

    void Start()
    {
        if (musicSource == null)
        {
            Debug.LogError("Missing AudioSource component on the GameObject");
            return;
        }

        // Setup and play background music
        if (backgroundMusic == null)
        {
            Debug.LogError("Background music clip is not assigned!");
            return;
        }

        musicSource.clip = backgroundMusic;
        musicSource.loop = true;
        musicSource.Play();
        Debug.Log("Background music should be playing now");
    }

    public void SetMusicVolume(float volume)
    {
        musicSource.volume = volume;
        PlayerPrefs.SetFloat("MusicVolume", volume);
        PlayerPrefs.Save();
    }

    public void SetSFXVolume(float volume)
    {
        sfxSource.volume = volume;
        PlayerPrefs.SetFloat("SFXVolume", volume);
        PlayerPrefs.Save();
    }

    private void LoadVolumeSettings()
    {
        musicSource.volume = PlayerPrefs.GetFloat("MusicVolume", 0.8f); 
        sfxSource.volume = PlayerPrefs.GetFloat("SFXVolume", 0.7f); 
    }

    public float GetMusicVolume()
    {
        return PlayerPrefs.GetFloat("MusicVolume", 0.8f);
    }

    public float GetSFXVolume()
    {
        return PlayerPrefs.GetFloat("SFXVolume", 0.7f);
    }

    public void PlayHoverSound()
    {
        if (hoverSound == null)
        {
            Debug.LogError("Hover sound clip is not assigned!");
            return;
        }
        sfxSource.PlayOneShot(hoverSound);
    }

    public void PlayClickSound()
    {
        if (clickSoundUI == null)
        {
            Debug.LogError("Click sound clip is not assigned!");
            return;
        }
        sfxSource.PlayOneShot(clickSoundUI);
    }

    public void PlayErrorSound()
    {
        if (errorSound == null)
        {
            Debug.LogError("Error sound clip is not assigned!");
            return;
        }
        sfxSource.PlayOneShot(errorSound);
    }

    public void PlayOneShotSFX(AudioClip audioClip)
    {
        if (audioClip == null)
        {
            Debug.LogError($"PlayOneShot clip {audioClip} is not assigned!");
            return;
        }
        sfxSource.PlayOneShot(audioClip);
    }
}