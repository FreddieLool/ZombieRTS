using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;
using static Unity.VisualScripting.Member;

public class AudioManager : MonoBehaviour
{
    public static AudioManager Instance;

    // Variables
    #region
    [Header("Sound Manager")]
    [SerializeField] private AudioClip backgroundMusic;
    [SerializeField] private AudioSource musicSource;
    [SerializeField] private AudioSource sfxSource;
    [SerializeField] private AudioSource ambientShort; // short sfx ambient -10s
    [SerializeField] private AudioSource ambientLong; // longer ambients 30s+
    [SerializeField] public AudioSource buildLoopSource;

    [Header("[UI SFX]")]
    [Header("- Menu")]
    [SerializeField] private AudioClip hoverSound;
    [SerializeField] private AudioClip clickSoundUI;
    [SerializeField] private AudioClip hoverAwaySound;
    private float lastHoverTime;
    private bool isUIInteractable = true;  // Flag to manage UI interaction

    [Header("- Game SFX")]
    [SerializeField] AudioClip errorClick;
    [SerializeField] AudioClip clickOnBuilding;
    [SerializeField] AudioClip clickOnEmpty;
    [SerializeField] AudioClip rotateBuilding;
    [SerializeField] AudioClip showBuildingUI, closeBuildingUI;
    [SerializeField] AudioClip newCycle;
    [SerializeField] public AudioClip buildingLoop;
    [SerializeField] AudioClip buildingComplete;

    [Header("- Game Sounds")]
    [SerializeField] private List<AudioClip> shortNightSounds; // Clips less than 10 seconds
    [SerializeField] private List<AudioClip> longNightSounds; // Clips longer than 30 seconds
    private bool isNightSoundScheduled = false;
    private float nightSoundTimer = 0f;

    [Header("- Music")]
    [SerializeField] private AudioClip[] backgroundMusicTracks; // Array of music tracks
    private int currentTrackIndex = 0;
    private Dictionary<SoundEffect, AudioClip> soundEffects = new Dictionary<SoundEffect, AudioClip>();

    [Header("- Unit Actions")]
    [SerializeField] private List<AudioClip> unitSelectedClips = new List<AudioClip>();
    [SerializeField] private List<AudioClip> unitMoveCommandClips = new List<AudioClip>();
    private List<int> unitSelectedOrder = new List<int>();
    private List<int> unitMoveOrder = new List<int>();
    private int unitSelectedIndex = 0;
    private int unitMoveIndex = 0;

    #endregion

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
        RegisterSoundEffects();
        Shuffle(unitSelectedClips, unitSelectedOrder);
        Shuffle(unitMoveCommandClips, unitMoveOrder);
    }

    void Start()
    {
        if (musicSource == null)
        {
            Debug.LogError("Missing AudioSource component on the GameObject");
            return;
        }

        ShuffleMusic();
        PlayNextTrack();
    }

    void Update()
    {
        if(LightingManager.Instance != null)
        {
            float currentTime = LightingManager.Instance.GetCurrentTime(); // Assuming you have a way to get the current game time

            // Handle playing of night sounds based on time
            if (!isNightSoundScheduled && (currentTime >= 20 || currentTime < 3))
            {
                // Schedule short night sounds between 20 and 3
                nightSoundTimer = Random.Range(0, 3600); // Random time within the next hour
                isNightSoundScheduled = true;
                StartCoroutine(PlayNightSoundAfterDelay(shortNightSounds, ambientShort, nightSoundTimer, true));
            }
            else if (!isNightSoundScheduled && (currentTime >= 19 && currentTime < 2))
            {
                // Schedule long night sounds between 19 and 2
                nightSoundTimer = Random.Range(0, 3600); // Random time within the next hour
                isNightSoundScheduled = true;
                StartCoroutine(PlayNightSoundAfterDelay(longNightSounds, ambientLong, nightSoundTimer, false));
            }
        }
    }

    private IEnumerator PlayNightSoundAfterDelay(List<AudioClip> soundList, AudioSource source, float delay, bool isShort)
    {
        yield return new WaitForSeconds(delay);
        PlayNightSound(soundList, source, isShort);
        isNightSoundScheduled = false; // Reset flag to allow scheduling another sound
    }

    private void PlayNightSound(List<AudioClip> soundList, AudioSource source, bool isShort)
    {
        if (soundList.Count == 0)
        {
            Debug.LogWarning("No night sounds assigned for " + (isShort ? "short" : "long") + " sounds.");
            return;
        }

        source.clip = soundList[Random.Range(0, soundList.Count)];
        source.Play();
        StartCoroutine(FadeAudioSource(source, 1f, 1)); // Fade in over 1 second

        // Schedule fade out towards the end of the clip
        StartCoroutine(FadeAudioSource(source, 1f, 0, source.clip.length - 1));
    }

    public static IEnumerator FadeAudioSource(AudioSource audioSource, float duration, float targetVolume, float delay = 0)
    {
        if (delay > 0) yield return new WaitForSeconds(delay);

        float currentTime = 0;
        float startVolume = audioSource.volume;

        while (currentTime < duration)
        {
            currentTime += Time.deltaTime;
            audioSource.volume = Mathf.Lerp(startVolume, targetVolume, currentTime / duration);
            yield return null;
        }

        audioSource.volume = targetVolume;
        if (targetVolume == 0) audioSource.Stop();
    }

    private void ShuffleMusic()
    {
        int n = backgroundMusicTracks.Length;
        while (n > 1)
        {
            n--;
            int k = Random.Range(0, n + 1);
            AudioClip value = backgroundMusicTracks[k];
            backgroundMusicTracks[k] = backgroundMusicTracks[n];
            backgroundMusicTracks[n] = value;
        }
    }

    private IEnumerator WaitForMusicEnd()
    {
        while (musicSource.isPlaying)
        {
            yield return null;
        }

        currentTrackIndex++;
        if (currentTrackIndex >= backgroundMusicTracks.Length)
        {
            currentTrackIndex = 0;
            ShuffleMusic();  // Re-shuffle after all tracks have been played
        }
        PlayNextTrack();
    }

    private void PlayNextTrack()
    {
        musicSource.clip = backgroundMusicTracks[currentTrackIndex];
        musicSource.Play();
        StartCoroutine(WaitForMusicEnd());
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

    public void PlayLoopingSound(AudioClip clip, AudioSource source)
    {
        if (source.isPlaying)
        {
            source.Stop(); 
        }
        source.clip = clip;
        source.loop = true;
        source.Play();
        Debug.Log("Looping sound started.");
    }

    public void StopLoopingSound(AudioSource source)
    {
        if (source.isPlaying)
        {
            source.Stop();
            Debug.Log("Looping sound stopped.");
        }
    }

    public void PlaySoundEffect(SoundEffect effect)
    {
        if (soundEffects.TryGetValue(effect, out AudioClip clip))
        {
            PlayOneShotSFX(clip);
        }
        else
        {
            Debug.LogError($"Sound effect {effect} not found!");
        }
    }

    Coroutine hoverAwayCoroutine;

    public void PlayHoverAwaySound()
    {
        if (!isUIInteractable || hoverAwayCoroutine != null) return;
        hoverAwayCoroutine = StartCoroutine(PlayHoverAwaySoundDelayed(0.11f));
    }

    public void StopCurrentHoverSound()
    {
        if (hoverAwayCoroutine != null)
        {
            StopCoroutine(hoverAwayCoroutine);
            hoverAwayCoroutine = null;
        }
        // reset the timer to prevent delayed execution
        lastHoverTime = Time.time;
    }

    private IEnumerator PlayHoverAwaySoundDelayed(float delay)
    {
        yield return new WaitForSeconds(delay);
        if (!isUIInteractable) yield break;  // Stop execution if UI interaction is disabled
        sfxSource.PlayOneShot(hoverAwaySound);
        hoverAwayCoroutine = null;
    }

    public void PlayHoverSound()
    {
        if (!isUIInteractable) return;
        if (hoverSound == null)
        {
            Debug.LogError("Hover sound clip is not assigned!");
            return;
        }
        CancelScheduledHoverAwaySound();
        sfxSource.PlayOneShot(hoverSound);
    }

    public void CancelScheduledHoverAwaySound()
    {
        if (hoverAwayCoroutine != null)
        {
            StopCoroutine(hoverAwayCoroutine);
            hoverAwayCoroutine = null;
        }
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
        if (errorClick == null)
        {
            Debug.LogError("Error sound clip is not assigned!");
            return;
        }
        sfxSource.PlayOneShot(errorClick);
    }

    // When clicking a button
    public void StopAllUIAudio()
    {
        CancelScheduledHoverAwaySound();
        sfxSource.Stop();
    }

    public void SetUIInteractable(bool state)
    {
        isUIInteractable = state;
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

    public float GetMusicVolume()
    {
        return PlayerPrefs.GetFloat("MusicVolume", 0.8f);
    }

    public float GetSFXVolume()
    {
        return PlayerPrefs.GetFloat("SFXVolume", 0.7f);
    }

    private void LoadVolumeSettings()
    {
        musicSource.volume = PlayerPrefs.GetFloat("MusicVolume", 0.8f);
        sfxSource.volume = PlayerPrefs.GetFloat("SFXVolume", 0.7f);
    }

    public void PlayRandomUnitSelectedSound()
    {
        PlaySoundFromShuffledList(unitSelectedClips, unitSelectedOrder, ref unitSelectedIndex);
    }

    public void PlayRandomUnitMoveCommandSound()
    {
        PlaySoundFromShuffledList(unitMoveCommandClips, unitMoveOrder, ref unitMoveIndex);
    }

    private void RegisterSoundEffects()
    {
        soundEffects.Add(SoundEffect.ErrorClick, errorClick);
        soundEffects.Add(SoundEffect.ClickOnBuilding, clickOnBuilding);
        soundEffects.Add(SoundEffect.ClickOnEmpty, clickOnEmpty);
        soundEffects.Add(SoundEffect.RotateBuilding, rotateBuilding);
        soundEffects.Add(SoundEffect.ShowUIBuilding, showBuildingUI);
        soundEffects.Add(SoundEffect.CloseUIBuilding, closeBuildingUI);
        soundEffects.Add(SoundEffect.HoverAway, hoverAwaySound);
        soundEffects.Add(SoundEffect.BuildingLoop, buildingLoop);
        soundEffects.Add(SoundEffect.BuildingComplete, buildingComplete);
    }

    private void PlaySoundFromShuffledList(List<AudioClip> clips, List<int> order, ref int index)
    {
        if (index >= order.Count)
        {
            Shuffle(clips, order);
            index = 0;
        }
        sfxSource.PlayOneShot(clips[order[index]]);
        index++;
    }

    private void Shuffle(List<AudioClip> clips, List<int> order)
    {
        order.Clear();
        for (int i = 0; i < clips.Count; i++)
        {
            order.Add(i);
        }
        int n = order.Count;
        while (n > 1)
        {
            n--;
            int k = Random.Range(0, n + 1);
            int value = order[k];
            order[k] = order[n];
            order[n] = value;
        }
    }

    public static IEnumerator FadeAudioSource(AudioSource source, float duration, float targetVolume)
    {
        float currentTime = 0;
        float startVolume = source.volume;

        while (currentTime < duration)
        {
            currentTime += Time.deltaTime;
            source.volume = Mathf.Lerp(startVolume, targetVolume, currentTime / duration);
            yield return null;
        }
        source.volume = targetVolume;

        if (targetVolume == 0)
            source.Stop();
    }

    public enum SoundEffect
    {
        ErrorClick,
        ClickOnBuilding,
        ClickOnEmpty,
        RotateBuilding,
        ShowUIBuilding,
        CloseUIBuilding,
        HoverAway,
        BuildingLoop,
        BuildingComplete
    }
}

public static class ExtensionMethods
{
    public static void Shuffle<T>(this IList<T> list)
    {
        int n = list.Count;
        while (n > 1)
        {
            n--;
            int k = Random.Range(0, n + 1);
            T value = list[k];
            list[k] = list[n];
            list[n] = value;
        }
    }
}