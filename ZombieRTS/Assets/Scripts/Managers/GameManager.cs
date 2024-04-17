using UnityEngine;

public class GameManager : MonoBehaviour
{
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
