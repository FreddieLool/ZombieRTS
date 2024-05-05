using UnityEngine;

public class GameManager : MonoBehaviour
{
    public static GameManager Instance;

    private void Awake()
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
    }

    void Start()
    {
        PlayerData playerData = SaveSystem.LoadPlayerData();
        GameSettings settings = SaveSystem.LoadSettings();

        // Apply loaded settings
        AudioManager.Instance.SetMusicVolume(settings.musicVolume);
        AudioManager.Instance.SetSFXVolume(settings.sfxVolume);
        // Update player stats
        //Player.Instance.Health = playerData.health;
    }

    void Update()
    {

    }
}
