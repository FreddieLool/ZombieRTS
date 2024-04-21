using System.IO;
using UnityEngine;

[System.Serializable]
public class PlayerData
{
    public int health;
    public int experience;
}

[System.Serializable]
public class GameSettings
{
    public float musicVolume;
    public float sfxVolume;
}


public static class SaveSystem
{
    private static string playerDataPath = Path.Combine(Application.persistentDataPath, "playerData.json");
    private static string settingsDataPath = Path.Combine(Application.persistentDataPath, "settingsData.json");

    public static void SavePlayerData(PlayerData data)
    {
        string json = JsonUtility.ToJson(data);
        File.WriteAllText(playerDataPath, json);
    }

    public static PlayerData LoadPlayerData()
    {
        if (File.Exists(playerDataPath))
        {
            string json = File.ReadAllText(playerDataPath);
            return JsonUtility.FromJson<PlayerData>(json);
        }
        return new PlayerData(); // Return new data if nothing is saved
    }

    public static void SaveSettings(GameSettings settings)
    {
        PlayerPrefs.SetFloat("MusicVolume", settings.musicVolume);
        PlayerPrefs.SetFloat("SFXVolume", settings.sfxVolume);
        PlayerPrefs.Save();
    }

    public static GameSettings LoadSettings()
    {
        GameSettings settings = new GameSettings
        {
            musicVolume = PlayerPrefs.GetFloat("MusicVolume", 0.8f), // Default value if not set
            sfxVolume = PlayerPrefs.GetFloat("SFXVolume", 0.7f),
        };
        return settings;
    }
}
